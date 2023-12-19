using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class critterBase : MonoBehaviour
{
    [Header("BaseClass")]
    public bool bFriendly;
    public bool bMouseOver;

    //Fields for controller
    public battleMaster BattleMaster { get; private set; }
    public lineScript redLine { get; private set; }
    public  lineScript greenLine { get; private set; }
    public Animator critterAnimator { get; private set; }
    public  Camera renderCamera { get; private set; }

    [Header("Health Bar")]
    [SerializeField] private GameObject healthNotif;
    [SerializeField] private GameObject hitParticleEffect;
    private Image bar;
    private TMP_Text healthText;
    private TMP_Text nameText;
    bool bShattered;

    [Header("Critter Reference")]
    public critter myCritter;
    private SpriteRenderer critterGraphic;
    private bool bAttack;
    private Transform originalParent;

    private Transform HUD;
    private Vector3 originalHUD;
    private Vector3 originalHUDSize;

    public void setCritter(critter reference)
    {
        myCritter = reference;
        critterGraphic = gameObject.transform.Find("critterGraphic").GetComponent<SpriteRenderer>(); // flips the sprite
        critterGraphic.sprite = reference.stages[0]; // this is incorrect if they have levels
        for (int i = 0; i < 3; ++i) { if (critterGraphic.sprite == myCritter.stages[i] && myCritter.bFlipSprite[i] == true) critterGraphic.flipX = !critterGraphic.flipX; }
        if (bFriendly == false) critterGraphic.flipX = !critterGraphic.flipX;

        originalParent = transform.parent;

        myCritter.Set_Initial_Stats(); // this is incorrect past the first round
        healthText.text = myCritter.getCurrentHealth() + "";
        healthText.color = (myCritter.getHealthPercentage() >= 0.7f) ? new Vector4(0, 255, 0, 255) : new Vector4(255, 180, 180, 255);
    }

    private void Awake()
    {
        BattleMaster = GameObject.FindGameObjectWithTag("BattleField").GetComponent<battleMaster>();
        redLine = GameObject.Find("redLineRender").GetComponent<lineScript>();
        greenLine = GameObject.Find("greenLineRender").GetComponent<lineScript>();
        critterAnimator = GetComponent<Animator>();
        renderCamera = GameObject.Find("lineRenderCamera").GetComponent<Camera>();

        HUD = gameObject.transform.Find("HUD").GetComponent<Transform>();

        nameText = HUD.Find("textCanvas").transform.Find("name").GetComponent<TextMeshProUGUI>();
        healthText = HUD.Find("textCanvas").transform.Find("healthText").GetComponent<TextMeshProUGUI>();
        bar = HUD.Find("textCanvas").transform.Find("bar").GetComponent<Image>();

        originalHUD = HUD.localPosition;
        originalHUDSize = HUD.localScale;
        
    }

    private void Update()
    {
        HUDUpdate();
        attackUpdate();
        healthUpdate();
    }

    private void HUDUpdate()
    {
        Vector3 HUDLerp = (bAttack) ? new Vector3(0, 33, 0) : originalHUD;
        Vector3 HUDLerpSize = (bAttack) ? new Vector3(0.5f, 0.5f, 0.5f) : originalHUDSize;

        HUD.transform.localPosition = Vector3.Lerp(HUD.transform.localPosition, HUDLerp, 4 * Time.deltaTime);
        HUD.transform.localScale = Vector3.Lerp(HUD.transform.localScale, HUDLerpSize, 4 * Time.deltaTime);
    }

    private void attackUpdate()
    {
        Vector3 attackLerp = Vector3.zero;
        if (bAttack) attackLerp.x = (bFriendly) ? -40 : 40;
        transform.localPosition = Vector3.Lerp(transform.localPosition, attackLerp, 5 * Time.deltaTime);
    }

    private void healthUpdate()
    {
        bar.fillAmount = Mathf.Lerp(bar.fillAmount, myCritter.getHealthPercentage(), 4 * Time.deltaTime);
    }

    public void attackMove(bool state, Transform target)
    {
        bAttack = state;

        if(state)
        {
            transform.SetParent(target);
        }
        else
        {
            transform.SetParent(originalParent);
        }
    }

    private void healthParticles(int change)
    {
        GameObject popUp = Instantiate(healthNotif, transform.position, transform.rotation);
        Animator popANIM = popUp.transform.Find("damageText").GetComponent<Animator>();

        popANIM.SetTrigger("pop" + Random.Range(0, 3)); // do 1 over the amount you want. So (0, 3) is really (0, 2)
        TextMeshProUGUI popText = popUp.transform.Find("damageText").GetComponent<TextMeshProUGUI>();

        popText.text = change + "";

        GameObject hitParticles = Instantiate(hitParticleEffect); //rotate it here
        hitParticles.transform.SetParent(transform);
        hitParticles.transform.localScale = new Vector3(1, 1, 1);
        hitParticles.transform.localPosition = Vector3.zero;
        hitParticles.transform.rotation = (bFriendly) ? Quaternion.Euler(0, 0, 90) : Quaternion.Euler(0, 0,-90);
        ParticleSystem hit = hitParticles.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule mainModule;
        mainModule = hit.main;
        mainModule.maxParticles = (Mathf.Abs(change));
        mainModule.startColor = new Color(myCritter.matchingColor.r, myCritter.matchingColor.g, myCritter.matchingColor.b, 255);
        StartCoroutine(hitDelay(popUp, hitParticles));
    }

    private IEnumerator hitDelay(GameObject hitPopup, GameObject hitPrefab)
    {
        yield return new WaitForSeconds(.43f);
        // hit animation

        yield return new WaitForSeconds(.30f);
        if (myCritter.getHealthPercentage() == 0 && bShattered == false)
        {
            //death animation
            bShattered = true;
            float force = (float)Random.Range(-0.05f, 0.05f);
            gameObject.transform.Find("healthBar").transform.GetChild(0).gameObject.SetActive(false);
            explodeHealthBar(force);
            BattleMaster.removeFromCritterBase(this);
            GetComponent<Animator>().SetTrigger("die");
            Destroy(this, 0.6f);
            StartCoroutine(explodeDelay());
        }

        Destroy(hitPopup);
        Destroy(hitPrefab);
    }

    private IEnumerator explodeDelay()
    {
        yield return new WaitForSeconds(.4f);
        nameText.text = "";
        healthText.text = "";
        float force = (float)Random.Range(10f, 15f);
        explodeHealthBar(force);
    }

    public void explodeHealthBar(float force)
    {
        foreach (Transform child in gameObject.transform.Find("healthBar").transform)
        {
            Rigidbody2D childRB = child.GetComponent<Rigidbody2D>();

            Vector2 direction = new Vector2((float)Random.Range(-500, 500), (float)Random.Range(-500, 500));

            childRB.gravityScale = 0f;
            childRB.AddForce(direction * force);
        }
    }

    public void recieveAttack(int change)
    {
        //for testing
        myCritter.changeHealth(change);
        healthText.text = myCritter.getCurrentHealth() + "";
        healthText.color = (myCritter.getHealthPercentage() >= 0.7f) ? new Vector4(0, 255, 0, 255) : new Vector4(255, 180, 180, 255);
        healthParticles(change);
    }

    public void OnMouseOver()
    {
        if (BattleMaster.bRendering == false || bMouseOver == true) return;

        bMouseOver = true;
        BattleMaster.targetedCritter = myCritter;

        if(bFriendly)
        {
            redLine.enable(false, Vector2.zero);
            greenLine.enable(true, renderCamera.ScreenToWorldPoint(BattleMaster.touchedPos));
            greenLine.focusTarget(true, gameObject.transform);

        }
        else
        {
            greenLine.enable(false, Vector2.zero);
            redLine.enable(true, renderCamera.ScreenToWorldPoint(BattleMaster.touchedPos));
            redLine.focusTarget(true, gameObject.transform);
        }
    }

    public void OnMouseExit()
    {
        if (bMouseOver == false) return;

        bMouseOver = false;

        if(bFriendly)
        {
            greenLine.focusTarget(false, gameObject.transform);
        }
        else
        {
            redLine.focusTarget(false, gameObject.transform);
        }
    }
}

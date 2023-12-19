using System.Collections;
using System.Collections.Generic;
using TMPro;
using TMPro.Examples;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class enemyBase : MonoBehaviour
{
    private Animator enemy_Animator;

    private bool bMouseOn;
    private bool bCanTarget;
    private bool bExploded;
    private GameObject healthMask;
    private TextMeshProUGUI healthText;

    [SerializeField] private GameObject popUpPrefab;
    [SerializeField] private GameObject deathSmokePrefab;
    [SerializeField] private GameObject hitPrefab;

    public int maxHealth;
    public int currentHealth;
    public string enemyName;
    public Color hitColor;

    private void Awake()
    {
        enemy_Animator = GetComponent<Animator>();
    }

    void Start()
    {
        healthMask = gameObject.transform.Find("healthBar").transform.GetChild(1).gameObject;

        gameObject.transform.Find("enemyCanvas").transform.Find("name").GetComponent<TextMeshProUGUI>().text = enemyName;
        healthText = gameObject.transform.Find("enemyCanvas").transform.Find("healthText").GetComponent<TextMeshProUGUI>();

        changeHealth(maxHealth);

    }


    void Update()
    {

        float value = (currentHealth * 1.0f / maxHealth);
        value *= 4.53f;
        value += 4.62f;

        Vector2 barLerp = Vector2.Lerp(healthMask.transform.localPosition, new Vector2(value, healthMask.transform.localPosition.y), 5 * Time.deltaTime);

        healthMask.transform.localPosition = barLerp;

        if(currentHealth == 0 && bExploded == false)
        {
            bExploded = true;
            float force = (float)Random.Range(-0.05f, 0.05f);
            gameObject.transform.Find("healthBar").transform.GetChild(0).gameObject.SetActive(false);
            explodeHealthBar(force);
            StartCoroutine(explodeDelay());
        }
    }

    public void dead()
    {
        GameObject deathSmoke = Instantiate(deathSmokePrefab, transform.GetChild(0).transform.position, transform.GetChild(0).transform.rotation);
        deathSmoke.transform.localScale = gameObject.transform.localScale;
        ParticleSystem smoke = deathSmoke.GetComponent<ParticleSystem>();
        ParticleSystem.ShapeModule shapeModule;
        ParticleSystem.MainModule mainModule;
        shapeModule = smoke.shape;
        mainModule = smoke.main;
        mainModule.maxParticles = (int)(5 * transform.localScale.x);
        shapeModule.radius = (transform.localScale.x / 3);
        Destroy(this.gameObject);
    }

    void changeHealth(int change)
    {
        currentHealth += change;

        if(change <= -1)
        {
            enemy_Animator.SetTrigger("hit");
            damageParticles(change * -1);
        }

        if (currentHealth >= maxHealth) currentHealth = maxHealth;

        healthText.text = currentHealth + "";
        
        healthText.color = (currentHealth >= maxHealth) ? new Vector4(0, 255, 0, 255) : new Vector4(255, 180, 180, 255);
    }

    void damageParticles(int damage)
    {
        GameObject popUp = Instantiate(popUpPrefab, transform.position, transform.rotation);
        Animator popANIM = popUp.transform.Find("damageText").GetComponent<Animator>();

        popANIM.SetTrigger("pop" + Random.Range(0, 3)); // do 1 over the amount you want. So (0, 3) is really (0, 2)
        TextMeshProUGUI popText = popUp.transform.Find("damageText").GetComponent<TextMeshProUGUI>();
        
        popText.text = damage + "";

        GameObject hitParticles = Instantiate(hitPrefab, new Vector2(transform.position.x + 1, transform.position.y), hitPrefab.transform.rotation);
        ParticleSystem hit = hitParticles.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule mainModule;
        mainModule = hit.main;
        mainModule.maxParticles = damage;
        mainModule.startColor = hitColor;
        StartCoroutine(hitDelay(popUp, hitParticles));
    }

    private IEnumerator hitDelay(GameObject hitPopup, GameObject hitPrefab)
    {
        yield return new WaitForSeconds(.43f);
        enemy_Animator.ResetTrigger("hit");

        yield return new WaitForSeconds(.30f);
        if(currentHealth <= 0) enemy_Animator.SetTrigger("death");
        Destroy(hitPopup);
        Destroy(hitPrefab);
    }

    private IEnumerator explodeDelay()
    {
        yield return new WaitForSeconds(1.5f);
        gameObject.transform.Find("enemyCanvas").transform.Find("name").GetComponent<TextMeshProUGUI>().text = "";
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

    public void canTarget(bool state)
    {
        bCanTarget = state;
    }

    public void OnMouseOver()
    {
        bMouseOn = true;

        if (bCanTarget)
        {
            gameObject.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    public void OnMouseExit()
    {
        bMouseOn = false;

        gameObject.transform.Find("attackIndicator").gameObject.SetActive(false);

        changeHealth(-5);
    }
}

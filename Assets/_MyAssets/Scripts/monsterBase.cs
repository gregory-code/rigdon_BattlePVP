using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class monsterBase : MonoBehaviour
{
    [Header("BaseClass")]
    [SerializeField] bool bMouseOver;
    
    public GameMaster gameMaster { get; private set; }
    public lineScript redLine { get; private set; }
    public lineScript greenLine { get; private set; }
    public Camera renderCamera { get; private set; }

    [Header("Monster Prefab")]
    [SerializeField] monster myMonster;
    private SpriteRenderer monsterSprite;
    private Image health;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI nameText;
    private Transform HUD;
    private Animator monsterAnimator;
    private Transform attackPoint;
    private damagePopScript damagePop;

    public int attackMultiplier = 100;

    public void Init(monster myMonster, GameMaster gameMaster, lineScript redLine, lineScript greenLine, Camera renderCamera, damagePopScript damagePop)
    {
        this.myMonster = myMonster;
        this.gameMaster = gameMaster;
        this.redLine = redLine;
        this.greenLine = greenLine;
        this.renderCamera = renderCamera;
        this.damagePop = damagePop;

        myMonster.myBase = this;

        monsterDependecy dependecies = GetComponent<monsterDependecy>();
        GrabDependecies(dependecies);

        int stage = myMonster.GetSpriteIndexFromLevel();
        monsterSprite.sprite = myMonster.stages[stage];
        if (myMonster.bFlipSprite[stage] == true)
            monsterSprite.flipX = !monsterSprite.flipX;

        if(myMonster.isMine() == false)
        {
            Vector3 flipRotation = new Vector3(0, 180, 0);
            transform.localEulerAngles = flipRotation;
            HUD.localEulerAngles = flipRotation;
        }

        myMonster.onTakeDamage += takeDamage;
        myMonster.onAnimPlayed += playAnimation;
        myMonster.onProjectileShot += ShootProjectile;
        myMonster.onApplyStatus += applyStatus;
        myMonster.onProcStatus += procStatus;

        nameText.text = myMonster.GetMonsterNickname();
        healthText.text = myMonster.GetCurrentHealth() + "";
        healthText.color = (myMonster.getHealthPercentage() >= 0.7f) ? new Vector4(0, 255, 0, 255) : new Vector4(255, 180, 180, 255);
    }

    private void GrabDependecies(monsterDependecy dependecy)
    {
        monsterSprite = dependecy.GetMonsterSprite();
        health = dependecy.GetHealthBar();
        healthText = dependecy.GetHealthText();
        nameText = dependecy.GetNameText();
        HUD = dependecy.GetHUD();
        monsterAnimator = dependecy.GetAnimator(myMonster.GetMonsterID(), myMonster.GetSpriteIndexFromLevel());
        attackPoint = dependecy.GetAttackPoint();
        statusProcPrefabs = dependecy.GetStatusPrefabs();
    }

    void Update()
    {
        health.fillAmount = Mathf.Lerp(health.fillAmount, myMonster.getHealthPercentage(), 4 * Time.deltaTime);
    }

    public monster GetMyMonster()
    {
        return myMonster;
    }

    public void SetIsMouseOver(bool state)
    {
        bMouseOver = state;
    }

    private void playAnimation(string animName)
    {
        monsterAnimator.SetTrigger(animName);
    }

    private void takeDamage(int change, bool died, bool bMine, int userIndex)
    {
        healthText.text = myMonster.GetCurrentHealth() + "";
        healthText.color = (myMonster.getHealthPercentage() >= 0.7f) ? new Vector4(0, 255, 0, 255) : new Vector4(255, 180, 180, 255);
        healthParticles(change);

        if (died)
        {
            monsterAnimator.SetTrigger("dead");
            gameMaster.removeFromCritterBase(this);
            return;
        }

        monsterAnimator.SetTrigger("damaged");
    }

    GameObject[] statusPrefabs = new GameObject[3];
    GameObject[] statusProcPrefabs = new GameObject[3];

    private void applyStatus(int whichStatus, GameObject statusPrefab, int statusCounter)
    {
        statusPrefabs[whichStatus] = Instantiate(statusPrefab, transform);
        statusPrefabs[whichStatus].transform.localPosition = Vector3.one;

        if(whichStatus == 2)
            SetTaunt();
    }

    private void procStatus(bool shouldDestroy, int whichStatus, bool triggerProc)
    {
        if(shouldDestroy)
            Destroy(statusPrefabs[whichStatus]);

        if (whichStatus == 2)
            RemoveTaunt();

        if (triggerProc == false)
            return;

        GameObject proc = Instantiate(statusProcPrefabs[whichStatus], transform);
        proc.transform.localPosition = Vector3.one;
        Destroy(proc, 1f);
    }

    private void SetTaunt()
    {
        monster[] myTeam = gameMaster.GetMonstersTeam(myMonster);
        for (int i = 0; i < myTeam.Length; i++)
        {
            if (myTeam[i].hasStatus[2] == false)
            {
                myTeam[i].isTargetable = false;
            }
        }
    }

    private void RemoveTaunt()
    {
        monster[] myTeam = gameMaster.GetMonstersTeam(myMonster);
        bool someoneIsTaunting = false;
        for (int i = 0; i < myTeam.Length; i++)
        {
            if (myTeam[i].hasStatus[2] == true)
            {
                someoneIsTaunting = true;
            }
        }

        if (someoneIsTaunting)
        {
            myMonster.isTargetable = true;
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            myTeam[i].isTargetable = true;
        }
    }

    private void ShootProjectile(projectileScript projectilePrefab, Transform target)
    {
        projectileScript newProjectile = Instantiate(projectilePrefab, attackPoint);
        newProjectile.Init(target);
    }

    private void healthParticles(int change)
    {
        damagePopScript popUp = Instantiate(damagePop, transform.position, transform.rotation);
        popUp.Init(change);

        /*GameObject hitParticles = Instantiate(hitParticleEffect); //rotate it here
        hitParticles.transform.SetParent(transform);
        hitParticles.transform.localScale = new Vector3(1, 1, 1);
        hitParticles.transform.localPosition = Vector3.zero;
        hitParticles.transform.rotation = (bFriendly) ? Quaternion.Euler(0, 0, 90) : Quaternion.Euler(0, 0, -90);
        ParticleSystem hit = hitParticles.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule mainModule;
        mainModule = hit.main;
        mainModule.maxParticles = (Mathf.Abs(change));
        mainModule.startColor = new Color(myCritter.matchingColor.r, myCritter.matchingColor.g, myCritter.matchingColor.b, 255);
        StartCoroutine(hitDelay(popUp, hitParticles));*/
    }

    /*private IEnumerator hitDelay(GameObject hitPopup, GameObject hitPrefab)
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
    }*/

    public int GetMoveDamage(int moveContentID, int moveScaleID)
    {
        int tier = GetMyMonster().GetSpriteIndexFromLevel();
        return GetCurrentMove(moveContentID).GetScaleValues(moveScaleID)[tier];
    }

    public int GetMultiplierDamage(int damage)
    {
        float Multiplier = damage * (1f * attackMultiplier / 100f);
        return Mathf.RoundToInt(Multiplier);
    }

    public monster GetTargetedMonster()
    {
        return gameMaster.targetedMonster.GetMyMonster();
    }

    public moveContent GetCurrentMove(int whichMove)
    {
        moveContent[] moves = GetMyMonster().GetMoveContents();
        return moves[whichMove];
    }

    public void OnMouseOver()
    {
        if (myMonster.isTargetable == false)
            return;

        if (gameMaster.bRendering == false || bMouseOver == true) return;

        bMouseOver = true;
        gameMaster.targetedMonster = this;

        if (myMonster.isMine())
        {
            redLine.enable(false, Vector2.zero);
            greenLine.enable(true, renderCamera.ScreenToWorldPoint(gameMaster.touchedPos));
            greenLine.focusTarget(true, gameObject.transform);

        }
        else
        {
            greenLine.enable(false, Vector2.zero);
            redLine.enable(true, renderCamera.ScreenToWorldPoint(gameMaster.touchedPos));
            redLine.focusTarget(true, gameObject.transform);
        }
    }

    public void OnMouseExit()
    {
        if (bMouseOver == false) 
            return;

        bMouseOver = false;

        if (myMonster.isMine())
        {
            greenLine.focusTarget(false, gameObject.transform);
        }
        else
        {
            redLine.focusTarget(false, gameObject.transform);
        }
    }
}

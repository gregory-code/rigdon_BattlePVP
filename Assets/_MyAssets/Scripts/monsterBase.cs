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
    bool loadingInfoScreen;
    
    public GameMaster gameMaster { get; private set; }
    public lineScript redLine { get; private set; }
    public lineScript greenLine { get; private set; }
    public Camera renderCamera { get; private set; }

    [Header("Monster Prefab")]
    [SerializeField] monster myMonster;
    private SpriteRenderer monsterSprite;
    private SpriteRenderer monsterShadow;
    private Image health;
    private Image tempHealth;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI nameText;
    private Transform HUD;
    private Animator monsterAnimator;
    private Transform effectSpawn;
    private damagePopScript damagePop;

    private Transform attackPoint;
    private Vector3 lerpLocation;

    public bool destroyShields = false;

    public delegate void OnOpenInfo();
    public event OnOpenInfo onOpenInfo;

    public void Init(monster myMonster, GameMaster gameMaster, lineScript redLine, lineScript greenLine, Camera renderCamera, damagePopScript damagePop, Transform spawnLocation)
    {
        this.myMonster = myMonster;
        this.gameMaster = gameMaster;
        this.redLine = redLine;
        this.greenLine = greenLine;
        this.renderCamera = renderCamera;
        this.damagePop = damagePop;
        myMonster.spawnLocation = spawnLocation;
        lerpLocation.x = spawnLocation.transform.position.x;
        lerpLocation.y = spawnLocation.transform.position.y;

        myMonster.gameMaster = gameMaster;
        myMonster.ownerTransform = transform;
        myMonster.GetExpHold(0).GainExp();
        myMonster.GetExpHold(0).GainExp();

        monsterDependecy dependecies = GetComponent<monsterDependecy>();
        GrabDependecies(dependecies);

        myMonster.attackPoint = this.attackPoint;

        int stage = myMonster.GetSpriteIndexFromLevel();
        SetImage(monsterSprite, myMonster.stages[stage], stage);
        SetImage(monsterShadow, myMonster.stages[stage], stage);

        if (myMonster.GetOwnership() == false)
        {
            Vector3 flipRotation = new Vector3(0, 180, 0);
            transform.localEulerAngles = flipRotation;
            HUD.localEulerAngles = flipRotation;
        }

        myMonster.onTakeDamage += takeDamage;
        myMonster.onHealed += healHealth;
        myMonster.onAnimPlayed += playAnimation;
        myMonster.onProjectileShot += ShootProjectile;
        myMonster.onApplyStatus += applyStatus;
        myMonster.onProcStatus += procStatus;
        myMonster.onDamagePopup += damagePopup;
        myMonster.onMovePosition += movePosition;
        myMonster.onRemoveTaunt += RemoveTaunt;
        myMonster.onAttackBreaksShields += DestroyShields;
        myMonster.onRemoveConnections += RemoveConnections;

        nameText.text = myMonster.GetMonsterNickname();
        healthText.text = myMonster.GetCurrentHealth() + "";
        healthText.color = (myMonster.getHealthPercentage() >= 0.7f) ? new Vector4(0, 255, 0, 255) : new Vector4(255, 180, 180, 255);

        List<int> statuses = myMonster.GetStatusList();
        foreach(int index in statuses)
        {
            myMonster.TryRemoveStatus(index, false);
        }
    }

    private void SetImage(SpriteRenderer target, Sprite image, int stage)
    {
        target.sprite = image;
        if (myMonster.bFlipSprite[stage] == true)
            target.flipX = !target.flipX;
    }

    private void GrabDependecies(monsterDependecy dependecy)
    {
        monsterSprite = dependecy.GetMonsterSprite();
        monsterShadow = dependecy.GetMonsterShadow();
        health = dependecy.GetHealthBar();
        healthText = dependecy.GetHealthText();
        nameText = dependecy.GetNameText();
        HUD = dependecy.GetHUD();
        monsterAnimator = (myMonster.IsAI()) ? dependecy.GetEnemyAnimator(myMonster.GetMonsterID(), myMonster.GetSpriteIndexFromLevel()) : dependecy.GetAnimator(myMonster.GetMonsterID(), myMonster.GetSpriteIndexFromLevel());
        attackPoint = dependecy.GetAttackPoint();
        statusProcPrefabs = dependecy.GetStatusPrefabs();
        tempHealth = dependecy.GetTempHealth();
        effectSpawn = dependecy.GetEffectSpawn();

        myMonster.SetEffectsList(dependecy.GetEffectsList());
        myMonster.SetStatusEffectPrefab(dependecy.GetStatusEffectPrefab());
    }

    private void RemoveConnections()
    {
        myMonster.onTakeDamage -= takeDamage;
        myMonster.onHealed -= healHealth;
        myMonster.onAnimPlayed -= playAnimation;
        myMonster.onProjectileShot -= ShootProjectile;
        myMonster.onApplyStatus -= applyStatus;
        myMonster.onProcStatus -= procStatus;
        myMonster.onDamagePopup -= damagePopup;
        myMonster.onMovePosition -= movePosition;
        myMonster.onRemoveTaunt -= RemoveTaunt;
        myMonster.onAttackBreaksShields -= DestroyShields;
        myMonster.onRemoveConnections -= RemoveConnections;
    }

    void Update()
    {
        health.fillAmount = Mathf.Lerp(health.fillAmount, myMonster.getHealthPercentage(), 4 * Time.deltaTime);
        tempHealth.fillAmount = Mathf.Lerp(tempHealth.fillAmount, myMonster.GetBubblePercentage(), 4 * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, lerpLocation, 5 * Time.deltaTime);
    }

    public monster GetMonster()
    {
        return myMonster;
    }

    public void SetIsMouseOver(bool state)
    {
        bMouseOver = state;
    }

    private void movePosition(float x, float y)
    {
        lerpLocation.x = x;
        lerpLocation.y = y;
    }

    private void playAnimation(string animName)
    {
        monsterAnimator.SetTrigger(animName);
    }

    private void healHealth(monster recivingMon, monster usingMon, int heal)
    {
        SetHealthText();
    }

    private void takeDamage(monster recivingMon, monster usingMon, int change, bool died, bool crit, bool burnDamage)
    {
        SetHealthText();

        if (died)
        {
            StartCoroutine(destroyMyself());
            return;
        }

        monsterAnimator.SetTrigger("damaged");
    }

    public bool IsCrit(int addedChance)
    {
        int roll = Random.Range(0, 101);
        int threshold = (myMonster.GetCurrentSpeed() * 2) + addedChance;

        if(roll <= threshold)
        {
            return true;
        }
        return false;
    }

    private void SetHealthText()
    {
        healthText.text = myMonster.GetCurrentHealth() + "";
    }

    GameObject[] statusPrefabs = new GameObject[20];
    GameObject[] statusProcPrefabs = new GameObject[20];

    private void applyStatus(int whichStatus, GameObject statusPrefab)
    {
        statusPrefabs[whichStatus] = Instantiate(statusPrefab, effectSpawn);
        statusPrefabs[whichStatus].transform.localPosition = Vector3.one;

        if(whichStatus == 4)
            SetTaunt();
    }

    private void procStatus(bool shouldDestroy, int whichStatus, bool triggerProc)
    {
        if(shouldDestroy)
        {
            statusEffectUI status = myMonster.GetStatus(whichStatus);
            if(status != null)
            {
                Destroy(statusPrefabs[whichStatus]);

                status.GettingRemoved();
            }
        }

        if (triggerProc == false)
            return;

        GameObject proc = Instantiate(statusProcPrefabs[whichStatus], transform);
        proc.transform.localPosition = Vector3.one;
        Destroy(proc, 1f);
    }

    private void SetTaunt()
    {
        if (myMonster.GetOwnership())
            return;

        ApplyTargetable(gameMaster.GetMonstersTeam(myMonster));
    }

    private void RemoveTaunt()
    {
        if (myMonster.GetOwnership())
            return;

        monster[] enemyTeam = gameMaster.GetMonstersTeam(myMonster);
        for (int i = 0; i < enemyTeam.Length; i++)
        {
            statusEffectUI taunt = enemyTeam[i].GetStatus(4);
            if (taunt != null)
            {
                ApplyTargetable(enemyTeam);
                return;
            }

            enemyTeam[i].isTargetable = true;
        }
    }

    private void ApplyTargetable(monster[] enemyTeam)
    {
        for (int i = 0; i < enemyTeam.Length; i++)
        {
            statusEffectUI taunt = enemyTeam[i].GetStatus(4);
            if (taunt == null)
            {
                enemyTeam[i].isTargetable = false;
            }
            else
            {
                enemyTeam[i].isTargetable = true;
            }
        }
    }

    private void ShootProjectile(projectileScript projectilePrefab, Transform target, Transform whichSpawn)
    {
        projectileScript newProjectile = Instantiate(projectilePrefab, whichSpawn);
        newProjectile.Init(target, monsterSprite.transform);
    }

    private void damagePopup(int change, bool shieldedAttack, bool crit)
    {
        damagePopScript popUp = Instantiate(damagePop, transform.position, transform.rotation);
        popUp.Init(change, shieldedAttack, crit);
    }

    private IEnumerator destroyMyself()
    {
        monsterAnimator.SetTrigger("dead");
        
        if (gameMaster.activeMonsters[0] == myMonster && myMonster.GetOwnership())
        {
            gameMaster.NextTurn();
        }

        yield return new WaitForSeconds(0.3f);

        gameMaster.removeFromMonsterBase(this);

        health.color = new Color(0, 0, 0, 0);
        tempHealth.color = new Color(0, 0, 0, 0);
        nameText.text = "";
        healthText.text = "";

        List<int> listOfIndexes = GetMonster().GetStatusList();
        if (listOfIndexes.Count > 0)
        {
            foreach (int index in listOfIndexes)
            {
                myMonster.TryRemoveStatus(index, false);
            }
        }
        myMonster.statusEffects.Clear();

        yield return new WaitForSeconds(0.35f);
        float force = (float)Random.Range(-0.1f, 0.1f);
        explodeHealthBar(force);

        yield return new WaitForSeconds(1f);
        force = (float)Random.Range(10f, 15f);
        explodeHealthBar(force);

        myMonster.RemoveConnections();

        GameObject deathEffect = (gameMaster.bRegularDeath) ? gameMaster.regularDeath : gameMaster.grimmetalDeath ;
        GameObject deathSmoke = Instantiate(deathEffect);
        deathSmoke.transform.position = monsterSprite.transform.position;

        Destroy(this.gameObject, 1f);
    }

    public void explodeHealthBar(float force)
    {
        foreach (Transform child in gameObject.transform.Find("HUD").transform.Find("healthBar").transform)
        {
            Rigidbody2D childRB = child.GetComponent<Rigidbody2D>();

            Vector2 direction = new Vector2((float)Random.Range(-500, 500), (float)Random.Range(-500, 500));

            childRB.gravityScale = 0f;
            childRB.AddForce(direction * force);
        }
    }

    public int GetMoveDamage(int moveContentID, int moveScaleID)
    {
        int tier = myMonster.GetSpriteIndexFromLevel();
        return GetCurrentMove(moveContentID).GetScaleValues(moveScaleID)[tier];
    }

    public monster GetTargetedMonster()
    {
        return gameMaster.targetedMonster.myMonster;
    }

    public moveContent GetCurrentMove(int whichMove)
    {
        moveContent[] moves = myMonster.GetMoveContents();
        return moves[whichMove];
    }

    public void FinishMove(bool consumeTurn, bool isAttack)
    {
        StartCoroutine(FinishAllyMove(consumeTurn, isAttack));
    }

    private IEnumerator FinishAllyMove(bool consumeTurn, bool isAttack)
    {
        gameMaster.waitingForAction = false;

        gameMaster.UsedAction(myMonster, GetTargetedMonster(), isAttack);

        if (isAttack)
        {
            destroyShields = false;
        }

        yield return new WaitForSeconds(0.2f);

        while (gameMaster.holdItDeerCrossing)
        {
            yield return new WaitForEndOfFrame();
        }

        if (consumeTurn == true)
        {
            gameMaster.NextTurn();
        }
    }

    private void DestroyShields(bool state)
    {
        destroyShields = state;
    }

    public void OnMouseOver()
    {
        if(gameMaster.activeMonsters[0] == GetMonster() && GetMonster().GetOwnership() && loadingInfoScreen == false)
        {
            loadingInfoScreen = true;
            StartCoroutine(loadInfoScreen());
        }

        if(gameMaster.bRendering == false && loadingInfoScreen == false)
        {
            loadingInfoScreen = true;
            StartCoroutine(loadInfoScreen());
        }

        if (gameMaster.bRendering == false || bMouseOver == true || gameMaster.inInfoScreen == true) 
            return;

        if (myMonster.isTargetable == false)
            return;

        bMouseOver = true;
        gameMaster.targetedMonster = this;

        if (myMonster.GetOwnership())
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
        loadingInfoScreen = false;

        if (bMouseOver == false || gameMaster.inInfoScreen == true) 
            return;

        bMouseOver = false;

        if (myMonster.GetOwnership())
        {
            greenLine.focusTarget(false, gameObject.transform);
        }
        else
        {
            redLine.focusTarget(false, gameObject.transform);
        }
    }

    public IEnumerator loadInfoScreen()
    {
        yield return new WaitForSeconds(1.5f);
        if (loadingInfoScreen == true)
        {
            onOpenInfo?.Invoke();
            GameObject.FindObjectOfType<infoPageScript>().DisplayMonster(myMonster);
        }
    }
}

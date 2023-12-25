using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.ComponentModel;

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
    private Image health;
    private Image tempHealth;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI nameText;
    private Transform HUD;
    private Animator monsterAnimator;
    private Transform attackPoint;
    private Transform effectSpawn;
    private damagePopScript damagePop;

    private Transform spawnLocation;
    private Vector3 lerpLocation;

    public int attackMultiplier = 100;

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
        this.spawnLocation = spawnLocation;
        this.myMonster.spawnLocation = spawnLocation.transform.position;
        lerpLocation.x = spawnLocation.transform.position.x;
        lerpLocation.y = spawnLocation.transform.position.y;

        myMonster.myBase = this;

        monsterDependecy dependecies = GetComponent<monsterDependecy>();
        GrabDependecies(dependecies);

        this.myMonster.attackPoint = attackPoint.position;

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
        myMonster.onHealed += healHealth;
        myMonster.onAnimPlayed += playAnimation;
        myMonster.onProjectileShot += ShootProjectile;
        myMonster.onApplyStatus += applyStatus;
        myMonster.onProcStatus += procStatus;
        myMonster.onDamagePopup += damagePopup;
        myMonster.onMovePosition += movePosition;
        myMonster.onRemoveTaunt += RemoveTaunt;

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
        tempHealth = dependecy.GetTempHealth();
        effectSpawn = dependecy.GetEffectSpawn();

        myMonster.SetEffectsList(dependecy.GetEffectsList());
        myMonster.SetStatusEffectPrefab(dependecy.GetStatusEffectPrefab());
    }

    void Update()
    {
        health.fillAmount = Mathf.Lerp(health.fillAmount, myMonster.getHealthPercentage(), 4 * Time.deltaTime);
        tempHealth.fillAmount = Mathf.Lerp(tempHealth.fillAmount, myMonster.GetBubblePercentage(), 4 * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, lerpLocation, 5 * Time.deltaTime);
    }

    public monster GetMyMonster()
    {
        return myMonster;
    }

    public void SetIsMouseOver(bool state)
    {
        bMouseOver = state;
    }

    private void movePosition(bool goHome, float x, float y)
    {
        if(goHome)
        {
            lerpLocation.x = spawnLocation.transform.position.x;
            lerpLocation.y = spawnLocation.transform.position.y;
            return;
        }

        lerpLocation.x = x;
        lerpLocation.y = y;

    }

    private void playAnimation(string animName)
    {
        monsterAnimator.SetTrigger(animName);
    }

    private void healHealth(int change, bool bMine, int userIndex)
    {
        SetHealthText();
    }

    private void takeDamage(int change, bool died, bool bMine, int userIndex, monster recivingMon)
    {
        SetHealthText();

        if (died)
        {
            StartCoroutine(destroyMyself());
            return;
        }

        monsterAnimator.SetTrigger("damaged");
    }

    private void SetHealthText()
    {
        healthText.text = myMonster.GetCurrentHealth() + "";
        healthText.color = (myMonster.getHealthPercentage() >= 0.7f) ? new Vector4(0, 255, 0, 255) : new Vector4(255, 180, 180, 255);

        if (myMonster.getHealthPercentage() >= 0.2f)
            healthText.color = new Vector4(229, 66, 66, 255);
    }

    GameObject[] statusPrefabs = new GameObject[12];
    GameObject[] statusProcPrefabs = new GameObject[12];

    private void applyStatus(int whichStatus, GameObject statusPrefab)
    {
        statusPrefabs[whichStatus] = Instantiate(statusPrefab, effectSpawn);
        statusPrefabs[whichStatus].transform.localPosition = Vector3.one;

        if(whichStatus == 2)
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
        if (myMonster.bMine)
            return;

        monster[] myTeam = gameMaster.GetMonstersTeam(myMonster);
        for (int i = 0; i < myTeam.Length; i++)
        {
            if(myTeam[i].GetStatus(2) == null)
            {
                myTeam[i].isTargetable = false;
            }
            else
            {
                myTeam[i].isTargetable = true;
            }
        }
    }

    private void RemoveTaunt()
    {
        if (myMonster.bMine)
            return;

        monster[] myTeam = gameMaster.GetMonstersTeam(myMonster);
        bool someoneIsTaunting = false;
        for (int i = 0; i < myTeam.Length; i++)
        {
            if (myTeam[i].GetStatus(2) != null)
            {
                someoneIsTaunting = true;
            }
        }

        if (someoneIsTaunting)
        {
            myMonster.isTargetable = false;
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            myTeam[i].isTargetable = true;
        }
    }

    private void ShootProjectile(projectileScript projectilePrefab, Transform target, bool uniqueSpawn, Transform spawn)
    {
        Transform spawnPoint = attackPoint;
        if(uniqueSpawn)
        {
            spawnPoint = spawn;
        }

        projectileScript newProjectile = Instantiate(projectilePrefab, spawnPoint);
        newProjectile.Init(target);
    }

    private void damagePopup(int change, bool shieldedAttack)
    {
        damagePopScript popUp = Instantiate(damagePop, transform.position, transform.rotation);
        popUp.Init(change, shieldedAttack);
    }

    private IEnumerator destroyMyself()
    {
        monsterAnimator.SetTrigger("dead");
        
        if (gameMaster.activeMonsters[0] == GetMyMonster() && gameMaster.IsItMyTurn())
        {
            gameMaster.NextTurn();
        }

        yield return new WaitForSeconds(0.3f);

        gameMaster.removeFromMonsterBase(this);

        health.color = new Color(0, 0, 0, 0);
        tempHealth.color = new Color(0, 0, 0, 0);
        nameText.text = "";
        healthText.text = "";

        List<int> listOfIndexesToDelete = new List<int>();
        foreach (statusEffectUI status in GetMyMonster().statusEffects)
        {
            listOfIndexesToDelete.Add(status.GetIndex());
        }

        if (listOfIndexesToDelete.Count > 0)
        {
            foreach (int index in listOfIndexesToDelete)
            {
                if(index == 5)
                {
                    foreach(monster mon in gameMaster.GetMonstersTeam(GetMyMonster()))
                    {
                        if(mon.GetStatus(index) != null)
                        {
                            mon.GetStatus(index).GettingRemoved();
                            mon.DestroyStatus(index);
                        }
                        else if(mon.GetStatus(index + 1) != null)
                        {
                            mon.GetStatus(index + 1).GettingRemoved();
                            mon.DestroyStatus(index + 1);
                        }
                    }
                }
                else
                {
                    GetMyMonster().GetStatus(index).GettingRemoved();
                    GetMyMonster().DestroyStatus(index);
                }
            }
        }
        GetMyMonster().statusEffects.Clear();

        yield return new WaitForSeconds(0.35f);
        float force = (float)Random.Range(-0.1f, 0.1f);
        explodeHealthBar(force);

        yield return new WaitForSeconds(1f);
        force = (float)Random.Range(10f, 15f);
        explodeHealthBar(force);

        Destroy(this, 0.5f);
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
        if(gameMaster.bRendering == false)
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
        loadingInfoScreen = false;

        if (bMouseOver == false || gameMaster.inInfoScreen == true) 
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

    public IEnumerator loadInfoScreen()
    {
        yield return new WaitForSeconds(1);

        onOpenInfo?.Invoke();

        if (loadingInfoScreen == true)
        {
            GameObject.FindObjectOfType<infoPageScript>().DisplayMonster(myMonster);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster")]
public class monster : ScriptableObject
{
    [Header("ID")]
    [SerializeField] private int monsterID;
    [Space(35)]

    [Header("Info")]
    [SerializeField] private string monsterName;
    private string monsterNickname;
    private bool canAct = false;
    private bool isAI;
    private bool bPlayer1;
    private int teamIndex;
    private bool ownership;
    private bool dead;

    [Header("Public Fields")]
    public GameMaster gameMaster;
    public Transform spawnLocation;
    public Transform ownerTransform;
    public Transform attackPoint;
    public bool[] bFlipSprite = new bool[3];
    public bool isTargetable = true;
    public Sprite[] stages = new Sprite[3];
    public Sprite[] stagesIcons = new Sprite[3];
    public Sprite circleOutline;
    public Color matchingColor;

    [Header("Stats")]
    private int level;
    private int exp = 0;
    private List<expHolder> expHolders = new List<expHolder>();

    public float GetLevelPercentage()
    {
        return (exp * 1f) / 100f;
    }

    public bool TryLevelUp()
    {
        exp++;
        if(exp >= 100)
        {
            dead = false; // might want to do this somewhere else could be badge
            strawberries += 3;
            exp = 0;
            SetLevel(level + 1);
            return true;
        }
        return false;
    }

    private int hpDamageCap = int.MinValue;
    public void SetDamageCap(int cap) { hpDamageCap = cap; }

    [SerializeField] private int initial_HP;
    private int maxHP;
    private int currentHP;
    private int growthHP;

    [SerializeField] private int initial_Strength;
    private int baseStrength;
    private int currentStrength;
    private int growthStrength;

    [SerializeField] private int initial_Magic;
    private int baseMagic;
    private int currentMagic;
    private int growthMagic;

    [SerializeField] private int initial_Speed;
    private int baseSpeed;
    private int currentSpeed;
    private int growthSpeed;

    [Header("MoveSet")]
    private int AttackID;
    private int AbilityID;
    private int PassiveID;
    [SerializeField] moveContent[] moveContents;

    public string GetMonsterName() { return monsterName; }
    public string GetMonsterNickname() { return monsterNickname; }
    public bool isPlayer1() { return bPlayer1; }
    public void SetAsPlayer1() { bPlayer1 = true; }
    public bool IsAI() { return isAI; }
    public void SetAsAI() { isAI = true; }
    public int GetIndex() { return teamIndex; }
    public void SetTeamIndex(int which) { teamIndex = which; }
    public bool isDead() { return dead; }
    public void SetAct(bool state) { canAct = state; }
    public bool CanAct() { return canAct; }
    public void SetOwnership(bool state) { ownership = state; }
    public bool GetOwnership() { return ownership; }


    public int GetMonsterID() { return monsterID; }
    public int GetInitialHP() { return initial_HP; }
    public int GetInitialStrength() { return initial_Strength; }
    public int GetInitialMagic() { return initial_Magic; }
    public int GetInitialSpeed() { return initial_Speed; }

    public int GetAttackID() { return AttackID; }
    public int GetAbilityID() { return AbilityID; }
    public int GetPassiveID() { return PassiveID; }


    public int GetCurrentHealth() { return currentHP; }
    public int GetMaxHealth() { return maxHP; }
    public int GetCurrentStrength() { return currentStrength; }
    public int GetCurrentMagic() { return currentMagic; }
    public int GetCurrentSpeed() { return currentSpeed; }

    public void ChangeCurrentStrength(int change) { currentStrength += change; }
    public void ChangeCurrentMagic(int change) { currentMagic += change; }
    public void ChangeCurrentSpeed(int change) { currentSpeed += change; }

    public int GetBaseStrength() { return baseStrength; }
    public int GetBaseMagic() { return baseMagic; }
    public int GetBaseSpeed() { return baseSpeed; }

    public void SetEffectsList(Transform list) { effectsList = list; }
    public void SetStatusEffectPrefab(statusEffectUI prefab) { statusEffectPrefab = prefab; }

    public void SetExpHolder(expHolder holder) { expHolders.Add(holder); }
    public List<expHolder> GetExpHolders() { return expHolders; }
    public expHolder GetExpHold(int which) { return expHolders[which]; }


    private int strawberries = 5;
    public bool TryConsumeStrawberry()
    {
        if (strawberries > 0)
        {
            strawberries--;
            return true;
        }

        return false;
    }

    public void AddStrawberries()
    {
        strawberries += 3;
    }

    public int[] GetGrowths()
    {
        int[] growths = new int[4];
        growths[0] = growthHP;
        growths[1] = growthStrength;
        growths[2] = growthMagic;
        growths[3] = growthSpeed;

        return growths;
    }

    public int[] GetStatBlock()
    {
        int[] statBlock = new int[4];
        statBlock[0] = GetInitialHP();
        statBlock[1] = GetInitialStrength();
        statBlock[2] = GetInitialMagic();
        statBlock[3] = GetInitialSpeed();

        return statBlock;
    }

    public int[] GetCurrentStatBlock()
    {
        int[] statBlock = new int[4];
        statBlock[0] = GetCurrentHealth();
        statBlock[1] = GetCurrentStrength();
        statBlock[2] = GetCurrentMagic();
        statBlock[3] = GetCurrentSpeed();

        return statBlock;
    }

    public int GetLevel()
    {
        return level;
    }

    public moveContent[] GetMoveContents()
    {
        return moveContents;
    }

    public int GetSpriteIndexFromLevel()
    {
        switch (level)
        {
            default:
                return 0;

            case 3:
            case 4:
            case 5:
                return 1;

            case > 5:
                return 2;
        }
    }

    public void RemoveConnections()
    {
        onRemoveConnections?.Invoke();
    }

    public delegate void OnRemoveConnections();
    public event OnRemoveConnections onRemoveConnections;

    public delegate void OnTakeDamage(monster recivingMon, monster usingMon, int damage, bool died, bool crit, bool burnDamage);
    public event OnTakeDamage onTakeDamage;

    public delegate void OnDamagePopup(int change, bool shielededAttack, bool crit);
    public event OnDamagePopup onDamagePopup;

    public delegate void OnHealed(monster recivingMon, monster usingMon, int heal);
    public event OnHealed onHealed;

    public delegate void OnDeclaredDamage(monster recivingMon, monster usingMon, int damage, bool willDie, bool destroyShields, bool crit);
    public event OnDeclaredDamage onDeclaredDamage;

    public delegate void OnAnimPlayed(string animName);
    public event OnAnimPlayed onAnimPlayed;

    public delegate void OnUsedAction(monster targetOfAction, bool isAttack);
    public event OnUsedAction onUsedAction;

    public delegate void OnMovePosition(float x, float y);
    public event OnMovePosition onMovePosition;

    public delegate void OnAttackAgain(monster targetMon, int percentageMultiplier);
    public event OnAttackAgain onAttackAgain;

    public delegate void OnProjectileShot(projectileScript projectilePrefab, Transform target, Transform whichSpawn);
    public event OnProjectileShot onProjectileShot;

    public delegate void OnNextTurn();
    public event OnNextTurn onNextTurn;

    public delegate void OnAttackBreaksShields(bool state);
    public event OnAttackBreaksShields onAttackBreaksShields;

    public void ShootProjectile(projectileScript projectilePrefab, Transform target, Transform whichSpawn)
    {
        onProjectileShot?.Invoke(projectilePrefab, target, whichSpawn);
    }

    public void AttackAgain(monster targetMon, int extraDamage)
    {
        onAttackAgain?.Invoke(targetMon, extraDamage);
    }

    public void MovePosition(float x, float y)
    {
        onMovePosition?.Invoke(x, y);
    }

    public void UsedAction(monster targetOfAction, bool isAttack)
    {
        if (gameMaster.movingToNewGame == true || dead)
            return;

        onUsedAction?.Invoke(targetOfAction, isAttack);
    }

    public void PlayAnimation(string anim)
    {
        onAnimPlayed?.Invoke(anim);
    }

    public void ApplyShieldBreak(bool state)
    {
        onAttackBreaksShields?.Invoke(state);
    }

    public void AdjustDamageReduction(int change)
    {
        damagePercentage += change;
    }

    int damagePercentage = 100;

    public void NextTurn()
    {
        if (gameMaster.movingToNewGame == true || dead)
            return;

        onNextTurn?.Invoke();
    }

    public void DelcaringDamage(monster usingMon, int damage, bool destroyShields, bool crit)
    {
        bool willDie = false;

        if(destroyShields)
        {
            if(GetStatusList().Count > 0)
            {
                TryRemoveStatus(3, true);
                TryRemoveStatus(5, true);
            }
        }

        HandleDamageChanges(damage, crit, false, false);

        int bubbleHealth = 0;
        statusEffectUI bubbleStatus = GetStatus(3);
        if (bubbleStatus != null)
            bubbleHealth = bubbleStatus.GetCounter();

        int theoreticalHealth = currentHP + bubbleHealth;

        theoreticalHealth += damage;

        if (theoreticalHealth <= 0)
        {
            willDie = true;
        }

        gameMaster.estimatedDamage = damage;

        onDeclaredDamage?.Invoke(this, usingMon, theoreticalHealth, willDie, destroyShields, crit);
    }

    private bool BurnDamage;
    public void SetBurnDamage() { BurnDamage = true; }
    public void TakeDamage(monster usingMon, int change, bool crit)
    {
        if (change >= 0 || dead == true || gameMaster.movingToNewGame == true)
            return;

        change = HandleDamageChanges(change, crit, BurnDamage, true);

        change = HandleBubble(change, usingMon, crit);

        if (change >= 0)
            return;

        currentHP += change;

        if (currentHP <= 0)
        {
            currentHP = 0;
            dead = true;

            if(isPlayer1())
            {
                //gameMaster.GiveKillExp(usingMon);
            }
        }

        onTakeDamage?.Invoke(this, usingMon, change, dead, crit, BurnDamage);
        
        BurnDamage = false;
        
        onDamagePopup?.Invoke(change, false, crit);
    }

    public int HandleBubble(int change, monster attackingMon, bool crit)
    {
        statusEffectUI bubbleStatus = GetStatus(3);
        if (bubbleStatus != null && bubbleStatus.GetCounter() > 0)
        {
            int bubbleAmount = bubbleStatus.GetCounter();
            bubbleStatus.UpdateStatusCounter(bubbleAmount + change);

            while (bubbleStatus.GetCounter() < 0)
            {
                bubbleStatus.UpdateStatusCounter(bubbleStatus.GetCounter() + 1);
            }

            if (bubbleStatus.GetCounter() > 0)
            {
                onDamagePopup?.Invoke(change, true, false);
                onTakeDamage?.Invoke(this, attackingMon, change, false, crit, BurnDamage);
                BurnDamage = false;
                change = 0;
            }
            else
            {
                TryRemoveStatus(3, true);
                change += bubbleAmount;
                onDamagePopup?.Invoke(-bubbleAmount, true, false);
            }
        }

        return change;
    }

    private int HandleDamageChanges(int damage, bool crit, bool burnDamage, bool triggerConductive)
    {
        if (crit)
        {
            float critDamage = damage * 1.5f;
            damage = Mathf.RoundToInt(critDamage);
        }

        if (burnDamage == false && GetStatus(0) != null) // Conductive
        {
            float conductiveDamage = (damage * 0.5f) + damage;
            damage = Mathf.RoundToInt(conductiveDamage);

            if (triggerConductive)
            {
                TryRemoveStatus(0, true);
            }
        }

        statusEffectUI weakness = GetStatus(2);
        if(burnDamage == false && weakness != null)
        {
            damage -= weakness.GetPower();
        }

        float Multiplier = damage * (1f * damagePercentage / 100f); // damage reduction
        damage = Mathf.RoundToInt(Multiplier);

        if (damage <= hpDamageCap) // damage cap
            damage = hpDamageCap;

        return damage;
    }

    public void HealHealth(monster usingMon, int heal)
    {
        if (heal <= 0 || dead == true || gameMaster.movingToNewGame == true)
            return;

        currentHP += heal;

        if (currentHP >= maxHP)
            currentHP = maxHP;

        onDamagePopup?.Invoke(heal, false, false);
        onHealed?.Invoke(this, usingMon, heal);
    }

    public int GetHalfStrength()
    {
        int strength = GetCurrentStrength();

        float cutInHalf = strength / 2;
        strength = Mathf.RoundToInt(cutInHalf);

        return strength;
    }

    public delegate void OnApplyStatus(int statusIndex, GameObject statusPrefab);
    public event OnApplyStatus onApplyStatus;

    public delegate void OnProcStatus(bool shouldDestroy, int statusIndex, bool triggerProc);
    public event OnProcStatus onProcStatus;

    public delegate void OnRemoveTaunt();
    public event OnRemoveTaunt onRemoveTaunt;

    public List<statusEffectUI> statusEffects = new List<statusEffectUI>();
    private Transform effectsList;
    private statusEffectUI statusEffectPrefab;

    public statusEffectUI GetStatus(int which)
    {
        if (statusEffects.Count <= 0)
            return null;

        foreach(statusEffectUI status in statusEffects)
        {
            if(status.GetIndex() == which)
            {
                return status;
            }
        }

        return null;
    }

    public List<int> GetStatusList()
    {
        List<int> listOfIndexes = new List<int>();

        foreach (statusEffectUI status in statusEffects)
        {
            listOfIndexes.Add(status.GetIndex());
        }

        return listOfIndexes;
    }

    public void procStatus(bool shouldDestroy, int statusIndex, bool triggerProc)
    {
        onProcStatus?.Invoke(shouldDestroy, statusIndex, triggerProc);
    }

    public void ApplyStatus(monster usingMonster, int statusIndex, GameObject statusPrefab, int counter, int power)
    {
        statusEffectUI status = GetStatus(statusIndex);

        statusEffectUI spellShield = GetStatus(5);
        if(spellShield != null)
        {
            if(statusIndex == 0 || statusIndex == 1 || statusIndex == 2)
            {
                if(spellShield.GetCounter() >= 160)
                    TryRemoveStatus(5, true);

                return;
            }
        }

        if (status == null)
        {
            statusEffectUI newUI = Instantiate(statusEffectPrefab, effectsList);
            newUI.SetStatusIndex(this, usingMonster, statusIndex, counter, power, gameMaster);
            statusEffects.Add(newUI);

            onApplyStatus?.Invoke(statusIndex, statusPrefab);
        }
        else
        {
            status.StatusGotReapplied(counter, power);
        }
    }

    public void TryRemoveStatus(int statusIndex, bool shouldProcStatus)
    {
        if (GetStatus(statusIndex) == null)
            return;

        procStatus(true, statusIndex, shouldProcStatus);
        DestroyStatus(statusIndex);
    }

    public void DestroyStatus(int statusIndex)
    {
        Destroy(GetStatus(statusIndex).gameObject);
        statusEffects.Remove(GetStatus(statusIndex));

        if (statusIndex == 4)
        {
            onRemoveTaunt?.Invoke();
        }
    }

    public float getHealthPercentage()
    {
        return (currentHP * 1.0f / maxHP);
    }

    public float GetBubblePercentage()
    {
        statusEffectUI status = GetStatus(3);

        if (status == null)
            return 0;

        return (status.GetCounter() * 1.0f / maxHP);
    }


    public void SetFromPref(monsterPreferences build)
    {
        AttackID = build.monsterValues[1];
        AbilityID = build.monsterValues[2];
        PassiveID = build.monsterValues[3];
        growthHP = build.monsterValues[4];
        growthStrength = build.monsterValues[5];
        growthMagic = build.monsterValues[6];
        growthSpeed = build.monsterValues[7];

        monsterNickname = build.monsterNickname;
    }

    public void SetInitialStats()
    {
        statusEffects.Clear();

        level = 1;

        maxHP = initial_HP;
        currentHP = initial_HP;

        baseStrength = initial_Strength;
        currentStrength = initial_Strength;

        baseMagic = initial_Magic;
        currentMagic = initial_Magic;

        baseSpeed = initial_Speed;
        currentSpeed = initial_Speed;
    }

    public void SetLevel(int level)
    {
        SetInitialStats(); // Initialize Level 1
        this.level = level;
        level--;

        float hpToAdd = ((growthHP + 1f) * 3f * level);
        maxHP += Mathf.RoundToInt(hpToAdd);
        currentHP += Mathf.RoundToInt(hpToAdd);

        float strengthToAdd = ((growthStrength + 0.99f) / 2 * level);
        baseStrength += Mathf.RoundToInt(strengthToAdd);
        currentStrength += Mathf.RoundToInt(strengthToAdd);

        float MagicToAdd = ((growthMagic + 0.99f) / 2 * level);
        baseMagic += Mathf.RoundToInt(MagicToAdd);
        currentMagic += Mathf.RoundToInt(MagicToAdd);

        float speedToAdd = ((growthSpeed + 0.99f) / 2 * level);
        baseSpeed += Mathf.RoundToInt(speedToAdd);
        currentSpeed += Mathf.RoundToInt(speedToAdd);
    }
}

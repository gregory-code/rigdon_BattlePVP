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
    public bool[] bFlipSprite = new bool[3];
    public bool canAct = false;
    public bool bMine;
    public int teamIndex;
    public monsterBase myBase;
    public Vector3 spawnLocation;
    public Transform attackPoint;
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

    public bool isMine()
    {
        return bMine;
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

    public delegate void OnRemoveConnections();
    public event OnRemoveConnections onRemoveConnections;

    public void RemoveConnections()
    {
        onRemoveConnections?.Invoke();
    }

    public delegate void OnTakeDamage(int change, bool died, bool bMine, int userIndex, monster recivingMon, bool burnDamage);
    public event OnTakeDamage onTakeDamage;

    public delegate void OnDamagePopup(int change, bool shielededAttack);
    public event OnDamagePopup onDamagePopup;

    public delegate void OnHealed(int change, bool bMine, int userIndex);
    public event OnHealed onHealed;

    public delegate void OnDeclaredDamage(int finalCalculations, bool bMine2, int userIndex, bool willKill);
    public event OnDeclaredDamage onDeclaredDamage;

    public void DelcaringDamage(int theoreticalDamage, bool bMine2, int userIndex, bool destroyShields)
    {
        bool died = false;

        if(destroyShields)
        {
            if(statusEffects.Count > 0)
            {
                statusEffectUI bubble = GetStatus(1);
                statusEffectUI spellShield = GetStatus(8);
                if(bubble != null)
                {
                    onProcStatus?.Invoke(true, 1, true);
                    DestroyStatus(1);
                }
                if(spellShield != null)
                {
                    onProcStatus?.Invoke(true, 8, true);
                    DestroyStatus(8);
                }
            }
        }

        theoreticalDamage = CalculateConductive(theoreticalDamage, false);

        theoreticalDamage = DamageCap(theoreticalDamage);

        statusEffectUI bubbleStatus = GetStatus(1);
        int bubbleHealth = 0;
        if (bubbleStatus != null)
            bubbleHealth = bubbleStatus.GetCounter();

        int theoreticalHealth = currentHP + bubbleHealth;

        theoreticalHealth += theoreticalDamage;

        if (theoreticalHealth <= 0)
        {
            died = true;
        }

        myBase.gameMaster.estimatedDamage = theoreticalDamage;

        onDeclaredDamage?.Invoke(theoreticalHealth, bMine2, userIndex, died);
    }

    private void HealBurn()
    {
        statusEffectUI burnStatus = GetStatus(7);
        if (burnStatus != null)
        {
            if (burnStatus.TickCounter() == true)
            {
                onProcStatus?.Invoke(true, 7, true);
                DestroyStatus(7);
            }
        }
    }

    private bool BurnDamage;
    private bool destroyBubble = false;
    public void SetBurnDamage() { BurnDamage = true; }
    public void ChangeHealth(int change, bool bMine, int userIndex, bool isAttack) // for who did the attack
    {
        if (change == 0 || currentHP <= 0 || myBase.gameMaster.movingToNewGame == true)
            return;

        if(change > 0 && isAttack == false)
        {
            currentHP += change;

            if (currentHP >= maxHP)
                currentHP = maxHP;

            HealBurn();
            onDamagePopup?.Invoke(change, false);
            onHealed?.Invoke(change, bMine, userIndex);
            return; // it's a heal
        }

        bool died = false;

        if(BurnDamage == false)
            change = CalculateConductive(change, true);

        if (change >= 0 && isAttack == true)
            return;

        change = DamageCap(change);

        statusEffectUI bubbleStatus = GetStatus(1);
        if (bubbleStatus != null && bubbleStatus.GetCounter() > 0) // bubble
        {
            int bubbleAmount = bubbleStatus.GetCounter();
            bubbleStatus.UpdateStatusCounter(bubbleAmount + change);

            while (bubbleStatus.GetCounter() < 0)
            {
                bubbleStatus.UpdateStatusCounter(bubbleStatus.GetCounter() + 1);
            }

            if (bubbleStatus.GetCounter() > 0)
            {
                onDamagePopup?.Invoke(change, true);
                change = 0;
            }
            else
            {
                if(BurnDamage == false)
                {
                    onProcStatus?.Invoke(true, 1, true);
                    DestroyStatus(1);
                }
                else
                {
                    destroyBubble = true;
                }
                change += bubbleAmount;
                onDamagePopup?.Invoke(-bubbleAmount, true);
            }
        }


        currentHP += change;

        if (currentHP <= 0)
        {
            currentHP = 0;
            died = true;

            //myBase.gameMaster.GetSpecificMonster(bMine, userIndex).GetExpHold(1).GainExp();
        }

        onTakeDamage?.Invoke(change, died, bMine, userIndex, this, BurnDamage);
        
        BurnDamage = false;
        
        onDamagePopup?.Invoke(change, false);
    }

    private int DamageCap(int incomingDamage)
    {
        if (incomingDamage <= hpDamageCap)
            incomingDamage = hpDamageCap;

        return incomingDamage;
    }

    private int CalculateConductive(int damage, bool triggerProc)
    {
        if (GetStatus(0) != null) // conductive
        {
            float conductiveDamage = (damage * 0.5f) + damage;
            damage = Mathf.RoundToInt(conductiveDamage);

            if(triggerProc)
            {
                onProcStatus?.Invoke(true, 0, true);
                DestroyStatus(0);
            }
        }

        return damage;
    }

    public int GetHalfStrength()
    {
        int strength = GetCurrentStrength();

        float cutInHalf = strength / 2;
        strength = Mathf.RoundToInt(cutInHalf);

        return strength;
    }

    public delegate void OnAnimPlayed(string animName);
    public event OnAnimPlayed onAnimPlayed;

    public void PlayAnimation(string anim)
    {
        onAnimPlayed?.Invoke(anim);
    }

    public delegate void OnUsedAction(bool isAttack);
    public event OnUsedAction onUsedAction;

    public void UsedAction(bool isAttack)
    {
        onUsedAction?.Invoke(isAttack);
    }

    public delegate void OnMovePosition(bool goHome, float x, float y);
    public event OnMovePosition onMovePosition;

    public void MovePosition(bool goHome, float x, float y)
    {
        onMovePosition?.Invoke(goHome, x, y);
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

    public void procStatus(bool shouldDestroy, int statusIndex, bool triggerProc)
    {
        onProcStatus?.Invoke(shouldDestroy, statusIndex, triggerProc);
    }

    public void DestroyStatus(int statusIndex)
    {
        Destroy(GetStatus(statusIndex).gameObject);
        statusEffects.Remove(GetStatus(statusIndex));

        if(statusIndex == 2 || statusIndex == 10)
        {
            onRemoveTaunt?.Invoke();
        }
    }

    public void ApplyStatus(int statusIndex, GameObject statusPrefab, int counter, int power, bool bMine, int userIndex)
    {
        statusEffectUI status = GetStatus(statusIndex);

        statusEffectUI onHitShield = GetStatus(8);
        if(onHitShield != null)
        {
            if(statusIndex == 0 || statusIndex == 3 || statusIndex == 7)
            {
                onProcStatus?.Invoke(true, 8, true);
                DestroyStatus(8);
                return;
            }
        }

        if(status == null)
        {
            statusEffectUI newUI = Instantiate(statusEffectPrefab, effectsList);
            newUI.SetStatusIndex(statusIndex, counter, power, this, bMine, userIndex, myBase.gameMaster);
            statusEffects.Add(newUI);

            onApplyStatus?.Invoke(statusIndex, statusPrefab);
        }
        else
        {
            status.StatusGotReapplied(counter, power);
        }
    }

    public void RemoveStatus(int statusIndex, bool bMine2, int TargetIndex)
    {
        procStatus(true, statusIndex, true);
        DestroyStatus(statusIndex);
    }

    public delegate void OnAttackAgain(int percentageMultiplier, bool bMine2, int TargetOfTargetIndex);
    public event OnAttackAgain onAttackAgain;

    public void AttackAgain(int percentageMultiplier, bool bMine2, int TargetOfTargetIndex)
    {
        onAttackAgain?.Invoke(percentageMultiplier, bMine2, TargetOfTargetIndex);
    }

    public delegate void OnProjectileShot(projectileScript projectilePrefab, Transform target, bool uniqueSpawn, Transform whichSpawn);
    public event OnProjectileShot onProjectileShot;

    public void ShootProjectile(projectileScript projectilePrefab, Transform target, bool uniqueSpawn, Transform whichSpawn)
    {
        onProjectileShot?.Invoke(projectilePrefab, target, uniqueSpawn, whichSpawn);
    }

    public delegate void OnNextTurn();
    public event OnNextTurn onNextTurn;

    public void NextTurn()
    {
        if (myBase.gameMaster.movingToNewGame == true)
            return;

        if(statusEffects.Count <= 0)
        {
            onNextTurn?.Invoke();
            return;
        }

        List<int> listOfIndexesToDelete = new List<int>();
        foreach( statusEffectUI status in statusEffects)
        {
            status.NextTurn();

            if (myBase.gameMaster.movingToNewGame == true)
                return;

            if (currentHP <= 0)
                return;

            if (status.GetCounter() <= 0)
            {
                listOfIndexesToDelete.Add(status.GetIndex());
            }
        }

        if (destroyBubble)
        {
            statusEffectUI bubble = GetStatus(1);
            if(bubble != null)
            {
                procStatus(true, 1, true);
                DestroyStatus(1);
            }
        }

        if (listOfIndexesToDelete.Count > 0)
        {
            foreach (int index in listOfIndexesToDelete)
            {
                bool shouldProcStatus = true;

                if (index == 0 || index == 1)
                    shouldProcStatus = false;

                onProcStatus?.Invoke(true, index, shouldProcStatus);
                DestroyStatus(index);
            }
        }

        destroyBubble = false;

        onNextTurn?.Invoke();
    }

    public float getHealthPercentage()
    {
        return (currentHP * 1.0f / maxHP);
    }

    public float GetBubblePercentage()
    {
        statusEffectUI status = GetStatus(1);

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

        float hpToAdd = ((growthHP + 1f) * 2f * level);
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

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
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
    public Vector3 attackPoint;
    public bool isTargetable = true;
    public Sprite[] stages = new Sprite[3];
    public Sprite[] stagesIcons = new Sprite[3];
    public Sprite circleOutline;
    public Color matchingColor;

    [Header("Stats")]
    [SerializeField] private int level;
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

    public delegate void OnTakeDamage(int change, bool died, bool bMine, int userIndex);
    public event OnTakeDamage onTakeDamage;

    public delegate void OnDamagePopup(int change, bool shielededAttack);
    public event OnDamagePopup onDamagePopup;

    public delegate void OnHealed(int change, bool bMine, int userIndex);
    public event OnHealed onHealed;

    public delegate void OnDeclaredDamage(int finalCalculations, bool bMine2, int userIndex, bool willKill);
    public event OnDeclaredDamage onDeclaredDamage;

    public void DelcaringDamage(int theoreticalDamage, bool bMine2, int userIndex)
    {
        bool died = false;

        theoreticalDamage = CalculateConductive(theoreticalDamage, false);

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

        onDeclaredDamage?.Invoke(theoreticalHealth, bMine2, userIndex, died);
    }

    public void ChangeHealth(int change, bool bMine, int userIndex) // for who did the attack
    {
        if (change == 0)
            return;

        bool died = false;

        change = CalculateConductive(change, true);

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
                onProcStatus?.Invoke(true, 1, true);
                DestroyStatus(1);
                change += bubbleAmount;
                onDamagePopup?.Invoke(-bubbleAmount, true);
            }
        }

        currentHP += change;

        if (currentHP >= maxHP) 
            currentHP = maxHP;

        if (currentHP <= 0)
        {
            currentHP = 0;
            died = true;
        }

        if(change < 0)
        {
            onTakeDamage?.Invoke(change, died, bMine, userIndex);
            onDamagePopup?.Invoke(change, false);
        }
        else if(change > 0)
        {
            onHealed?.Invoke(change, bMine, userIndex);
        }
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
    }

    public void ApplyStatus(int statusIndex, GameObject statusPrefab, int counter, int power, bool bMine, int userIndex)
    {
        statusEffectUI status = GetStatus(statusIndex);

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

    public delegate void OnProjectileShot(projectileScript projectilePrefab, Transform target);
    public event OnProjectileShot onProjectileShot;

    public void ShootProjectile(projectileScript projectilePrefab, Transform target)
    {
        onProjectileShot?.Invoke(projectilePrefab, target);
    }

    public delegate void OnNextTurn();
    public event OnNextTurn onNextTurn;

    public void NextTurn()
    {
        if(statusEffects.Count <= 0)
        {
            onNextTurn?.Invoke();
            return;
        }

        List<int> listOfIndexesToDelete = new List<int>();
        foreach( statusEffectUI status in statusEffects)
        {
            status.NextTurn();
            if(status.GetCounter() <= 0)
            {
                bool shouldProcStatus = true;

                if (status.GetIndex() == 0 || status.GetIndex() == 1)
                    shouldProcStatus = false;

                Debug.Log("Proc a status from turn end: ID is " + status.GetIndex());
                onProcStatus?.Invoke(true, status.GetIndex(), shouldProcStatus);

                listOfIndexesToDelete.Add(status.GetIndex());
            }
        }

        if(listOfIndexesToDelete.Count > 0)
        {
            foreach (int index in listOfIndexesToDelete)
            {
                DestroyStatus(index);
                if(index == 2)
                {
                    onRemoveTaunt?.Invoke();
                }
            }
        }

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

    public void LevelUp()
    {
        level++;

        maxHP += (growthHP + 3);
        currentHP += (growthHP + 3);

        baseStrength += (growthStrength + 1) / 2;
        currentStrength += (growthStrength + 1) / 2;

        baseMagic += (growthMagic + 1) / 2;
        currentMagic += (growthMagic + 1) / 2;

        baseSpeed += (growthSpeed + 1) / 2;
        currentSpeed += (growthSpeed + 1) / 2;
    }

    public void SetLevel(int level)
    {
        SetInitialStats(); // Initialize Level 1
        for (int i = 0; i < level; i++)
        {
            LevelUp();
        }
    }
}

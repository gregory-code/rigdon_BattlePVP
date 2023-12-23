using System.Collections;
using System.Collections.Generic;
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
    public bool[] hasStatus = new bool[3];
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

    public int GetBaseStrength() { return baseStrength; }
    public int GetBaseMagic() { return baseMagic; }
    public int GetBaseSpeed() { return baseSpeed; }

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

    public void ChangeHealth(int change, bool bMine, int userIndex) // for who did the attack
    {
        if (change == 0)
            return;

        bool died = false;

        if (hasStatus[0]) // conductive
        {
            float conductiveDamage = (change * 0.5f) + change;
            change = Mathf.RoundToInt(conductiveDamage);
            hasStatus[0] = false;
            onProcStatus?.Invoke(true, 0, true);
        }

        if (hasStatus[1] && statusCounter[1] > 0) // bubble
        {
            int bubbleAmount = statusCounter[1];
            statusCounter[1] += change;

            while (statusCounter[1] < 0)
            {
                statusCounter[1]++;
            }

            if (statusCounter[1] > 0)
            {
                onDamagePopup?.Invoke(change, true);
                onUpdateStatusUI?.Invoke(false, statusCounter[1], 1);
                change = 0;
            }
            else
            {
                hasStatus[1] = false;
                onProcStatus?.Invoke(true, 1, true);
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

    public delegate void OnAnimPlayed(string animName);
    public event OnAnimPlayed onAnimPlayed;

    public void PlayAnimation(string anim)
    {
        onAnimPlayed?.Invoke(anim);
    }

    public delegate void OnApplyStatus(int statusIndex, GameObject statusPrefab, int statusCounter);
    public event OnApplyStatus onApplyStatus;

    public delegate void OnProcStatus(bool shouldDestroy, int whichStatus, bool triggerProc);
    public event OnProcStatus onProcStatus;

    public delegate void OnUpdateStatusUI(bool createNew, int statusCounter, int index);
    public event OnUpdateStatusUI onUpdateStatusUI;

    private int[] statusCounter = new int[3];
    public int GetStatusCounter(int which)
    {
        return statusCounter[which];
    }

    public void ApplyStatus(int statusIndex, GameObject statusPrefab, int statusCounter)
    {
        if (hasStatus[statusIndex])
        {
            this.statusCounter[statusIndex] += statusCounter;
            onUpdateStatusUI?.Invoke(false, statusCounter, statusIndex);
            return;
        }

        this.statusCounter[statusIndex] = statusCounter;
        hasStatus[statusIndex] = true;
        onUpdateStatusUI?.Invoke(true, statusCounter, statusIndex);
        onApplyStatus?.Invoke(statusIndex, statusPrefab, statusCounter);
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
        for(int i = 0; i < statusCounter.Length; i++)
        {
            if (hasStatus[i] == false)
                continue;

            if(i == 1) // 1 is bubble
            {
                float newBubbleValue = (statusCounter[1] * 0.9f) - 1;
                statusCounter[1] = Mathf.RoundToInt(newBubbleValue);
            }
            else
            {
                statusCounter[i]--;
            }


            if (statusCounter[i] == 0)
            {
                onProcStatus?.Invoke(true, i, true);
            }
            else
            {
                onUpdateStatusUI?.Invoke(false, statusCounter[i], i);
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
        return (statusCounter[1] * 1.0f / maxHP);
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

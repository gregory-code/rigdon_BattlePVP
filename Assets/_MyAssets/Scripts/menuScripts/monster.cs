using System.Collections;
using System.Collections.Generic;
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
    public moveBase[] moves = new moveBase[4];
    private int AttackID;
    private int AbilityID;
    private int PassiveID;
    [SerializeField] moveContent[] moveContents;

    public string GetMonsterName() { return monsterName; }
    public int GetMonsterID() { return monsterID; }
    public int GetInitialHP() { return initial_HP; }
    public int GetInitialStrength() { return initial_Strength; }
    public int GetInitialMagic() { return initial_Magic; }
    public int GetInitialSpeed() { return initial_Speed; }


    public int GetCurrentHealth() { return currentHP; }
    public int GetCurrentStrength() { return currentStrength; }
    public int GetCurrentMagic() { return currentMagic; }
    public int GetCurrentSpeed() { return currentSpeed; }

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

    public void changeHealth(int change)
    {
        currentHP += change;

        if (currentHP >= maxHP) currentHP = maxHP;
        if (currentHP <= 0)
        {
            currentHP = 0;
            //die
        }
    }

    public float getHealthPercentage()
    {
        return (currentHP * 1.0f / maxHP);
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

    public int[] GetStatBlock()
    {
        int[] statBlock = new int[4];
        statBlock[0] = GetInitialHP();
        statBlock[1] = GetInitialStrength();
        statBlock[2] = GetInitialMagic();
        statBlock[3] = GetInitialSpeed();

        return statBlock;
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

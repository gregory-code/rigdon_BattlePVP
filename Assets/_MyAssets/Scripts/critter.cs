using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Critter")]
public class critter : ScriptableObject
{
    [Header("ID")]
    [SerializeField] private int critterID;
    public int GetCritterID() { return critterID; }
    [Space(35)]

    [Header("Info")]

    [SerializeField] private string critterName;
    public string GetCritterName() { return critterName; }

    public Sprite circleOutline;

    [SerializeField] bool[] bFlipSprite = new bool[3];
    public Sprite[] stages = new Sprite[3];


    [Header("Stats")]

    [SerializeField] private int initial_HP;
    public int GetInitialHP() { return initial_HP; }
    private int maxHP;
    private int currentHP;
    private int growthHP;

    [SerializeField] private int initial_Strength;
    public int GetInitialStrength() { return initial_Strength; }
    private int baseStrength;
    private int currentStrength;
    private int growthStrength;

    [SerializeField] private int initial_Magic;
    public int GetInitialMagic() { return initial_Magic; }
    private int baseMagic;
    private int currentMagic;
    private int growthMagic;

    [SerializeField] private int initial_Speed;
    public int GetInitialSpeed() { return initial_Speed; }
    private int baseSpeed;
    private int currentSpeed;
    private int growthSpeed;

    [Header("MoveSet")]
    public int AttackID;
    public int AbilityID;
    public int PassiveID;


    private void Set_Initial_Stats()
    {
        maxHP = initial_HP;
        currentHP = initial_HP;

        baseStrength = initial_Strength;
        currentStrength = initial_Strength;

        baseMagic = initial_Magic;
        currentMagic = initial_Magic;

        baseSpeed = initial_Speed;
        currentSpeed = initial_Speed;
    }

    public float[] getStatBlock()
    {
        float[] statBlock = new float[4];
        statBlock[0] = GetInitialHP();
        statBlock[1] = GetInitialStrength();
        statBlock[2] = GetInitialMagic();
        statBlock[3] = GetInitialSpeed();

        return statBlock;
    }

    public void levelUp()
    {
        maxHP += growthHP;
        currentHP += growthHP;

        baseStrength += growthStrength;
        currentStrength += growthStrength;

        baseMagic += growthMagic;
        currentMagic += growthMagic;

        baseSpeed += growthSpeed;
        currentSpeed += growthSpeed;
    }

    public void setLevel(int level)
    {
        Set_Initial_Stats(); // Initialize Level 1
        for(int i = 0; i < level; i++)
        {
            levelUp();
        }
    }
}

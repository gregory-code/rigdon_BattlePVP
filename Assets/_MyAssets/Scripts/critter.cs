using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Critter")]
public class critter : ScriptableObject
{

    [Header("ID")]
    [SerializeField] private int critterID;
    [Space(35)]

    [Header("Components")]
    critterController critterClickController;
    Animator critterAnimator;

    [Header("Info")]
    [SerializeField] bool[] bFlipSprite = new bool[3];
    [SerializeField] private string critterName;
    private string critterNickname;
    public Sprite[] stages = new Sprite[3];
    public Sprite circleOutline;
    public Color matchingColor;

    [Header("Stats")]
    [SerializeField] private int initial_HP;
    private int maxHP;
    private int currentHP;
    [SerializeField] private int growthHP; // growths are serialized for testing

    [SerializeField] private int initial_Strength;
    private int baseStrength;
    private int currentStrength;
    [SerializeField] private int growthStrength;

    [SerializeField] private int initial_Magic;
    private int baseMagic;
    private int currentMagic;
    [SerializeField] private int growthMagic;

    [SerializeField] private int initial_Speed;
    private int baseSpeed;
    private int currentSpeed;
    [SerializeField] private int growthSpeed;

    [Header("MoveSet")]
    private int AttackID;
    private int AbilityID;
    private int PassiveID;
    
    public string GetCritterName() { return critterName; }
    public int GetCritterID() { return critterID; }
    public int GetInitialHP() { return initial_HP; }
    public int GetInitialStrength() { return initial_Strength; }
    public int GetInitialMagic() { return initial_Magic; }
    public int GetInitialSpeed() { return initial_Speed; }

    public void SetFromCritterBuild(critterBuild build)
    {
        AttackID = build.critterValue[1];
        AbilityID = build.critterValue[2];
        PassiveID = build.critterValue[3];
        growthHP = build.critterValue[4];
        growthStrength = build.critterValue[5];
        growthMagic = build.critterValue[6];
        growthSpeed = build.critterValue[7];

        critterNickname = build.critterNickname;
    }

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

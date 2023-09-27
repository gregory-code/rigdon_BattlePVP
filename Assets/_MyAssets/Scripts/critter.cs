using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Critter")]
public class critter : ScriptableObject
{
    [Header("ID")]
    [SerializeField] private int critterID;
    [Space(35)]

    [Header("Info")]

    [SerializeField] private string critterName;
    [SerializeField] private string nickName;

    [SerializeField] bool[] bFlipSprite = new bool[3];
    [SerializeField] Sprite[] stages = new Sprite[3];


    [Header("Stats")]

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

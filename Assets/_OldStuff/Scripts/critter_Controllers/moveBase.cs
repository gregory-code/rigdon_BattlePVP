using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Move")]
public class moveBase : ScriptableObject
{
    public string moveName;
    public bool bMeleeAttack;
    public string animType;

    public float firstWait;
    public float secondWait;
    public float thirdWait;

    public int[] leveledPower;

    public int statID;


}

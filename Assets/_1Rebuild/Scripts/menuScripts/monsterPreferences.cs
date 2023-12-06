using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "monsterPref")]
public class monsterPreferences : ScriptableObject
{
    private int[] monsterValues = new int[8];

    public int GetMonsterID()
    {
        return monsterValues[0];
    }    

    public void SetMonsterID(int value)
    {
        monsterValues[0] = value;
    }

    // 0  is ID
    // 1 is attack ID
    // 2 is ability ID
    // 3 is passive ID
    // 4 is HP growth
    // 5 is Strength growth
    // 6 is Magic growth
    // 7 is Speed Growth

    public string critterNickname;
}

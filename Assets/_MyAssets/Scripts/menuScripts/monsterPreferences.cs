using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "monsterPref")]
public class monsterPreferences : ScriptableObject
{
    public int[] monsterValues = new int[8];

    private List<int> upgradeList = new List<int>();
    private int lostHealth = 0;

    public void AddUpgrade(int index)
    {
        upgradeList.Add(index);
    }

    public void SetUpradeList(List<int> list)
    {
        upgradeList = list;
    }

    public List<int> GetUpgradeList()
    {
        return upgradeList;
    }

    public void SetLostHealth(int health)
    {
        lostHealth = health;
    }

    public int GetLostHealth()
    {
        return lostHealth;
    }

    // 0  is ID
    // 1 is attack ID
    // 2 is ability ID
    // 3 is passive ID
    // 4 is HP growth
    // 5 is Strength growth
    // 6 is Magic growth
    // 7 is Speed Growth

    public void DeseralizePref(string seralizedContent)
    {
        List<string> content = seralizedContent.Split("_").ToList<string>();

        for(int i = 0; i < monsterValues.Length; i++)
        {
            monsterValues[i] = int.Parse(content[i]);
        }
    }

    public string SeralizedPref()
    {
        string preferences = "";

        for(int i = 0; i < monsterValues.Length; i++)
        {
            preferences += monsterValues[i];

            if (i == monsterValues.Length - 1) //Skips the last one
                break;

            preferences += "_";
        }

        return preferences;
    }

    public string monsterNickname = "";
}

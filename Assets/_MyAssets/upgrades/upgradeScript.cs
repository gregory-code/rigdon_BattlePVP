using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade")]
public class upgradeScript : ScriptableObject
{
    [Header("Rariety 0 is common, 1 is uncommon, 2 is ability, 3 is legendary")]
    [SerializeField] int rariety;

    [SerializeField] int upgradeID;

    [SerializeField] Sprite upgradeIcon;
    [SerializeField] string upgradeTitle;
    [SerializeField] string upgradeDescription;

    public Sprite GetImage()
    {
        return upgradeIcon;
    }

    public string GetTitle()
    {
        return upgradeTitle;
    }

    public string GetDescription()
    {
        return upgradeDescription;
    }

    public int GetID()
    {
        return upgradeID;
    }
}

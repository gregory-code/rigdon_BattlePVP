using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ExpHolder")]
public class expHolder : ScriptableObject
{
    [SerializeField] int expModifier;
    [SerializeField] string titleText;

    private int timesDone;

    public void GainExp()
    {
        timesDone++;
    }
    
    public string GetTitle()
    {
        return titleText;
    }

    public float GetExp()
    {
        return timesDone * expModifier;
    }

    public float ExplenishExp()
    {
        float returnExp = GetExp();
        timesDone = 0;
        return returnExp;
    }

    public string GetExpText()
    {
        string expText = $"+{GetExp()} (x{timesDone}) |";
        return expText;
    }

}

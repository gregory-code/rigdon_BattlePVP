using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class moveContent : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI moveName;

    [SerializeField] TextMeshProUGUI[] numText = new TextMeshProUGUI[3];
    [SerializeField] bool[] exists = new bool[3];
    
    [SerializeField] int[] statIndex;
    [SerializeField] bool[] usesStatModifier = new bool[3];
    
    [SerializeField] bool[] isPerecentage = new bool[3];

    [SerializeField] int percentageStatMultiplier;

    [SerializeField] int[] scaleNums1 = new int[3];
    [SerializeField] int[] scaleNums2 = new int[3];
    [SerializeField] int[] scaleNums3 = new int[3];

    [SerializeField] bool givesStatGrowth;
    [SerializeField] int statGrowthIndex; // from 0 - 3 so you have to add +4

    public bool DoesPassiveGiveGrowth()
    {
        return givesStatGrowth;
    }

    public int GetPassiveGrowth()
    {
        return statGrowthIndex;
    }

    public void SetUpConnection(teamBuilder builder)
    {
        onClickAction = () => builder.CheckPassiveGrowth(this, true);

        transform.parent.GetComponent<Button>().onClick.AddListener(onClickAction);
    }

    public int[] GetScaleValues(int whichNumGroup)
    {
        switch(whichNumGroup)
        {
            case 0:
                return scaleNums1;

            case 1:
                return scaleNums2;

            case 2:
                return scaleNums3;
        }

        return scaleNums1;
    }

    private UnityAction onClickAction;

    public void RemoveConnection(teamBuilder builder)
    {
        transform.parent.GetComponent<Button>().onClick.RemoveListener(onClickAction);
    }

    public void SetTier(int tier, int[] statBlock)
    {
        if (exists[0])
            HandleTier(0, scaleNums1, statBlock, tier);

        if (exists[1])
            HandleTier(1, scaleNums2, statBlock, tier);

        if (exists[2])
            HandleTier(2, scaleNums3, statBlock, tier);
    }

    private void HandleTier(int numIndex, int[] scaleNumGroup, int[] leveledstatBlock, int tier)
    {
        int extraModifier = 0;

        if (isPerecentage[numIndex] == true)
        {
            if (usesStatModifier[numIndex])
                extraModifier = leveledstatBlock[statIndex[numIndex]] * percentageStatMultiplier;

            numText[numIndex].text = $"{scaleNumGroup[tier] + extraModifier}%";
        }
        else
        {
            if (usesStatModifier[numIndex])
                extraModifier = leveledstatBlock[statIndex[numIndex]];

            numText[numIndex].text = $"{scaleNumGroup[tier] + extraModifier}";
        }
    }
}

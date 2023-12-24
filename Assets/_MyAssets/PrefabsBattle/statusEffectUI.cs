using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static monster;

public class statusEffectUI : MonoBehaviour
{
    [SerializeField] Image statusImage;
    [SerializeField] Sprite[] statusSprites;
    
    [SerializeField] TextMeshProUGUI activeCounterText;

    private monster myMonster;

    private int statusIndex;
    private int counter;
    private int power;

    public void SetStatusIndex(int statusIndex, int counter, int power, monster myMonster)
    {
        statusImage.sprite = statusSprites[statusIndex];
        this.statusIndex = statusIndex;
        this.power = power;
        this.myMonster = myMonster;
        myMonster.onUsedAction += UsedAction;
        UpdateStatusCounter(counter);

        if(statusIndex == 3)
        {
            StatChange(0, power);
        }
    }

    public int GetIndex()
    {
        return statusIndex;
    }

    public int GetPower()
    {
        return power;
    }

    public int GetCounter()
    {
        return counter;
    }

    public Sprite GetStatusSprite()
    {
        return statusSprites[statusIndex];
    }

    public bool NextTurnVanish()
    {
        if (statusIndex == 1) // 1 is bubble
        {
            float newBubbleValue = (counter * 0.9f) - 1;
            counter = Mathf.RoundToInt(newBubbleValue);
        }

        if(statusIndex == 0 || statusIndex == 2)
        {
            counter--;
        }

        if (counter == 0)
        {
            return true;
        }

        UpdateStatusCounter(counter);
        return false;
    }

    public void StatusGotReapplied(int newCounter, int power)
    {
        if(statusIndex == 0 || statusIndex == 1 || statusIndex == 2)
        {
            UpdateStatusCounter(counter + newCounter);
        }

        if(statusIndex == 3)
        {

        }
    }

    private void UsedAction(bool isAttack)
    {
        if(statusIndex == 3)
        {
            counter--;
        }

        if(counter == 0)
        {
            myMonster.procStatus(true, statusIndex, true);
        }
    }

    public void GettingRemoved()
    {
        if (statusIndex == 3)
        {
            StatChange(0, -power);
        }
    }

    private void StatChange(int which, int change)
    {
        switch(which)
        {
            case 0: // strength
                myMonster.ChangeCurrentStrength(change);
                break;

            case 1: // magic
                myMonster.ChangeCurrentMagic(change);
                break;

            case 2: // speed
                myMonster.ChangeCurrentSpeed(change);
                break;
        }
    }

    public void UpdateStatusCounter(int newCounter)
    {
        this.counter = newCounter;
        activeCounterText.text = $"{newCounter}";
    }
}

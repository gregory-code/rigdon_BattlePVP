using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class statusEffectUI : MonoBehaviour
{
    [SerializeField] Image statusImage;
    [SerializeField] Sprite[] statusSprites;
    
    [SerializeField] TextMeshProUGUI activeCounterText;

    private monster myMonster;
    private monster usingMonster;

    private GameMaster gameMaster;

    private int statusIndex;
    private int counter;
    private int power;

    private int secondaryPower;

    public void SetStatusIndex(int statusIndex, int counter, int power, monster myMonster, bool bmine, int userIndex, GameMaster gameMaster)
    {
        usingMonster = gameMaster.GetMonstersTeam(myMonster)[userIndex]; // this will always grab mine not the enemeis

        statusImage.sprite = statusSprites[statusIndex];
        if(statusIndex == 5 || statusIndex == 6)
        {
            usingMonster.onDeclaredDamage += DelcaringDamage;
            statusImage.sprite = usingMonster.stagesIcons[usingMonster.GetSpriteIndexFromLevel()];
        }

        this.gameMaster = gameMaster;
        this.statusIndex = statusIndex;
        this.power = power;
        this.myMonster = myMonster;
        myMonster.onUsedAction += UsedAction;
        UpdateStatusCounter(counter);

        if(statusIndex == 3)
        {
            StatChange(0, power);
        }

        if (statusIndex == 4)
        {
            StatChange(0, power);
            StatChange(1, power);
        }

        if(statusIndex == 5)
        {
            this.power = usingMonster.GetCurrentStrength();
            secondaryPower = usingMonster.GetCurrentMagic();

            if (usingMonster.GetSpriteIndexFromLevel() == 0)
            {
                float cutInHalf = this.power / 2f;
                this.power = Mathf.RoundToInt(cutInHalf);

                float cutInHalf2 = secondaryPower / 2f;
                secondaryPower = Mathf.RoundToInt(cutInHalf2);
            }

            StatChange(0, this.power);
            StatChange(1, secondaryPower);
        }
    }

    public monster GetUsingMonster()
    {
        return usingMonster;
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

    public void NextTurn()
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

        UpdateStatusCounter(counter);
    }

    public void DelcaringDamage(int finalCalculations, bool bMine2, int userIndex, bool willKill)
    {
        if(willKill)
        {
            gameMaster.redirectedIndex = myMonster.teamIndex;
            gameMaster.MoveMonster(false, myMonster.teamIndex, false, false, usingMonster.teamIndex);
            StartCoroutine(goBack());
        }
    }

    public IEnumerator goBack()
    {
        yield return new WaitForSeconds(0.7f);
        gameMaster.MoveMonster(false, myMonster.teamIndex, true, true, myMonster.teamIndex);
    }

    public void StatusGotReapplied(int newCounter, int power)
    {
        if(statusIndex == 0 || statusIndex == 1 || statusIndex == 2)
        {
            UpdateStatusCounter(counter + newCounter);
        }

        if(statusIndex == 3 || statusIndex == 4)
        {
            // do a thing here
        }
    }

    private void UsedAction(bool isAttack)
    {
        if(statusIndex == 3 || statusIndex == 4)
        {
            counter--;

            if (counter == 0)
            {
                myMonster.procStatus(true, statusIndex, true);
            }
        }
    }

    public void GettingRemoved()
    {
        if (statusIndex == 3)
        {
            StatChange(0, -power);
        }

        if (statusIndex == 4)
        {
            StatChange(0, -power);
            StatChange(1, -power);
        }

        if(statusIndex == 5)
        {
            usingMonster.onDeclaredDamage -= DelcaringDamage;
            StatChange(0, -power);
            StatChange(1, -power);
        }

        if(statusIndex == 6)
        {
            usingMonster.onDeclaredDamage -= DelcaringDamage;
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
        activeCounterText.text = (newCounter == 99) ? "" : $"{newCounter}";
    }
}

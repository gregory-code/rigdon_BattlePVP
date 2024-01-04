using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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

    private enum status { Conductive, Burn, Weakness, Bubble, Taunt, SpellShield, BrambleCrown, GoldenHorn  }
    private status myStatus;

    private int statusIndex;
    private int counter;
    private int power;

    bool alreadyRemoved = false;

    public void SetStatusIndex(monster myMonster, monster usingMonster, int statusIndex, int counter, int power, GameMaster gameMaster)
    {
        statusImage.sprite = statusSprites[statusIndex];

        this.myMonster = myMonster;
        this.usingMonster = usingMonster;
        this.statusIndex = statusIndex;
        UpdateStatusCounter(counter);
        this.power = power;
        this.gameMaster = gameMaster;
        myMonster.onUsedAction += UsedAction;

        if(Enum.IsDefined(typeof(status), statusIndex))
        {
            myStatus = (status)statusIndex;
        }
        else
        {
            Debug.Log("Index outside of defined status condintions");
        }

        GettingApplied();
    }

    public int GetLevelIndex()
    {
        return myMonster.GetSpriteIndexFromLevel();
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

    public int GetBurnDamage()
    {
        float burnDamage = (myMonster.GetMaxHealth() * 0.4f) * -1f;
        burnDamage += myMonster.GetCurrentStrength();
        int damage = Mathf.RoundToInt(burnDamage);
        if(power == 1)
        {
            damage -= usingMonster.GetHalfStrength();
        }
        return damage;
    }

    public void StatusGotReapplied(int newCounter, int power)
    {
        switch (myStatus)
        {
            case status.Conductive:
                UpdateStatusCounter(counter + newCounter);
                break;

            case status.Burn:
                UpdateStatusCounter(counter + newCounter);
                break;

            case status.Weakness:
                ReapplyPower(power);
                UpdateStatusCounter(counter + newCounter);
                break;

            case status.Bubble:
                UpdateStatusCounter(counter + newCounter);
                break;

            case status.Taunt:
                UpdateStatusCounter(counter + newCounter);
                break;

            case status.SpellShield:
                break;

            case status.BrambleCrown:
                ReapplyPower(power);
                UpdateStatusCounter(counter + newCounter);
                break;

            case status.GoldenHorn:
                myMonster.procStatus(false, statusIndex, true);
                UpdateStatusCounter(counter + newCounter);
                break;
        }
    }

    private void UsedAction(bool isAttack)
    {
        bool shouldProcStatus = true;

        switch (myStatus)
        {
            case status.Conductive:
                shouldProcStatus = false;
                counter--;
                break;

            case status.Burn:
                counter--;
                myMonster.SetBurnDamage();
                myMonster.TakeDamage(usingMonster, GetBurnDamage(), false);
                break;

            case status.Weakness:
                counter--;
                break;

            case status.Bubble:
                shouldProcStatus = false;
                float newBubbleValue = (counter * 0.9f) - 1;
                counter = Mathf.RoundToInt(newBubbleValue);
                break;

            case status.Taunt:
                counter--;
                break;

            case status.SpellShield:
                shouldProcStatus = false;
                counter--;
                break;

            case status.BrambleCrown:
                counter--;
                break;

            case status.GoldenHorn:
                break;
        }

        UpdateStatusCounter(counter);

        if (counter <= 0)
        {
            myMonster.TryRemoveStatus(statusIndex, shouldProcStatus);
        }
    }

    private void GettingApplied()
    {
        switch (myStatus)
        {
            case status.Conductive:
                break;
            case status.Burn:
                break;
            case status.Weakness:
                break;
            case status.Bubble:
                break;
            case status.Taunt:
                break;
            case status.SpellShield:
                break;
            case status.BrambleCrown:
                int tier = myMonster.GetSpriteIndexFromLevel();
                int reduction = 40 + (tier * 10);
                myMonster.AdjustDamageReduction(-reduction);

                StatChange(3, power);
                break;

            case status.GoldenHorn:
                myMonster.procStatus(false, statusIndex, true);
                break;
        }
    }

    public void GettingRemoved()
    {
        if (alreadyRemoved)
            return;

        alreadyRemoved = true;

        myMonster.onUsedAction -= UsedAction;

        switch (myStatus)
        {
            case status.Conductive:
                break;
            case status.Burn:
                break;
            case status.Weakness:
                break;
            case status.Bubble:
                break;
            case status.Taunt:
                break;
            case status.SpellShield:
                break;
            case status.BrambleCrown:
                int tier = myMonster.GetSpriteIndexFromLevel();
                int reduction = 40 + (tier * 10);
                myMonster.AdjustDamageReduction(reduction);

                StatChange(3, -power);
                break;
            case status.GoldenHorn:
                break;
        }
    }

    private void ReapplyPower(int power)
    {
        GettingRemoved();
        float roundedPower = (power + this.power) / 2;
        this.power = Mathf.RoundToInt(roundedPower);
        GettingApplied();
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

            case 3: // all stats
                myMonster.ChangeCurrentStrength(change);
                myMonster.ChangeCurrentMagic(change);
                myMonster.ChangeCurrentSpeed(change);
                break;

            case 4: // highest stat
                int strength = myMonster.GetCurrentStrength();
                int magic = myMonster.GetCurrentMagic();
                int speed = myMonster.GetCurrentSpeed();

                if(strength >= magic && strength >= magic)
                {
                    myMonster.ChangeCurrentStrength(change);
                }
                else if (magic >= strength && magic >= speed)
                {
                    myMonster.ChangeCurrentMagic(change);
                }
                else
                {
                    myMonster.ChangeCurrentSpeed(change);
                }
                break;
        }
    }

    public void UpdateStatusCounter(int newCounter)
    {
        this.counter = newCounter;
        activeCounterText.text = (newCounter >= 160) ? "" : $"{newCounter}";
    }
}

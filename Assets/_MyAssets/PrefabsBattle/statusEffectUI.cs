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

    private enum status { Conductive, Burn, Weakness, Bubble, Taunt, SpellShield, BrambleCrown, GoldenHorn, 
        LeadTheCharge, StrengthBuff, Stun, WasteLandHunter, SpikeyCarapace, Vow  }
    private status myStatus;

    private int statusIndex;
    private int counter;
    private int power;
    private int secondaryPower;

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

    public int GetSecondaryPower()
    {
        return secondaryPower;
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

            case status.LeadTheCharge:
                this.power = power;
                break;

            case status.StrengthBuff:
                ReapplyPower(power);
                UpdateStatusCounter(counter + newCounter);
                break;

            case status.Stun:
                break;

            case status.WasteLandHunter:
                UpdateStatusCounter(counter + newCounter);
                break;

            case status.SpikeyCarapace:
                UpdateStatusCounter(counter + newCounter);
                break;

            case status.Vow:
                UpdateStatusCounter(counter + newCounter);
                break;
        }
    }

    private void UsedAction(monster targetOfAction, bool isAttack)
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

            case status.LeadTheCharge:
                if(isAttack)
                {
                    if(myMonster.GetOwnership() && targetOfAction.isDead() == false)
                    {
                        gameMaster.ApplyStatus(myMonster, targetOfAction, 2, secondaryPower, power);
                        gameMaster.ApplyStatus(myMonster, targetOfAction, 4, secondaryPower, 0);
                    }
                    myMonster.TryRemoveStatus(8, true);
                }
                break;

            case status.StrengthBuff:
                counter--;
                break;

            case status.Stun:
                break;

            case status.WasteLandHunter:
                shouldProcStatus = false;
                counter--;
                break;

            case status.SpikeyCarapace:
                counter--;
                break;

            case status.Vow:
                counter--;
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
            case status.LeadTheCharge:
                secondaryPower = myMonster.GetSpriteIndexFromLevel() + 2;
                break;

            case status.StrengthBuff:
                StatChange(0, power);
                break;

            case status.Stun:
                break;

            case status.WasteLandHunter:
                break;

            case status.SpikeyCarapace:
                myMonster.onTakeDamage += SpikeDamage;
                break;

            case status.Vow:
                usingMonster.AdjustDamageReduction(-power);
                myMonster.onDeclaredDamage += allyGettingDeclaredDamage;
                usingMonster.onRemoveConnections += RemoveConnection;
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
            case status.LeadTheCharge:
                break;
            case status.StrengthBuff:
                StatChange(0, -power);
                break;

            case status.Stun:
                break;

            case status.WasteLandHunter:
                break;

            case status.SpikeyCarapace:
                myMonster.onTakeDamage -= SpikeDamage;
                break;

            case status.Vow:
                usingMonster.AdjustDamageReduction(power);
                myMonster.onDeclaredDamage -= allyGettingDeclaredDamage;
                usingMonster.onRemoveConnections -= RemoveConnection;
                break;
        }
    }

    private void RemoveConnection()
    {
        if(myStatus == status.Vow && usingMonster.GetOwnership())
        {
            gameMaster.TryRemoveStatus(myMonster, 13);
        }
    }

    private void allyGettingDeclaredDamage(monster recivingMon, monster usingMon, int damage, bool willDie, bool destroyShields, bool crit)
    {
        if(recivingMon == myMonster && myStatus == status.Vow)
        {
            gameMaster.redirectedMon = usingMonster;
            StartCoroutine(MoveToProtect());
        }
    }

    private IEnumerator MoveToProtect()
    {
        if(usingMonster.GetOwnership())
        {
            gameMaster.MoveMonster(usingMonster, myMonster, 0);
            yield return new WaitForSeconds(1f);
            gameMaster.MoveMonster(usingMonster, myMonster, 1);
        }
    }

    private void ReapplyPower(int power)
    {
        GettingRemoved();
        float roundedPower = (power + this.power) / 2;
        this.power = Mathf.RoundToInt(roundedPower);
        GettingApplied();
    }

    private void SpikeDamage(monster recivingMon, monster usingMon, int damage, bool died, bool crit, bool burnDamage)
    {
        if(burnDamage == false)
        {
            float spikeMultiplier = damage * (1f * power / 100f); // damage reduction
            int spikeDamage = Mathf.RoundToInt(spikeMultiplier);

            usingMon.SetBurnDamage();
            usingMon.TakeDamage(myMonster, spikeDamage, false);
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

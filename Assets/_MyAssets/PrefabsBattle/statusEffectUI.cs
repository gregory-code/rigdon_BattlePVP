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

    private int statusIndex;
    private int counter;
    private int power;

    bool alreadyRemoved = false;

    private int secondaryPower;

    public void SetStatusIndex(monster myMonster, monster usingMonster, int statusIndex, int counter, int power, GameMaster gameMaster)
    {
        statusImage.sprite = statusSprites[statusIndex];
        if(statusIndex == 5 || statusIndex == 6)
        {
            usingMonster.onDeclaredDamage += DelcaringDamage;
            statusImage.sprite = usingMonster.stagesIcons[usingMonster.GetSpriteIndexFromLevel()];
        }

        this.usingMonster = usingMonster;
        this.gameMaster = gameMaster;
        this.statusIndex = statusIndex;
        this.power = power;
        this.myMonster = myMonster;
        myMonster.onUsedAction += UsedAction;
        UpdateStatusCounter(counter);

        switch(statusIndex)
        {
            case 3:
                StatChange(0, power);
                break;

            case 4:
                StatChange(0, power);
                StatChange(1, power);
                break;

            case 5:
                EstablishBestFriends();
                break;

            case 9:
                //power -= 100;
                //myMonster.myBase.attackMultiplier += power;
                //myMonster.myBase.destroyShields = true;
                break;
        }
    }

    private void EstablishBestFriends()
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

        if(statusIndex == 0 || statusIndex == 2 || statusIndex == 10)
        {
            counter--;
        }

        if (statusIndex == 7 && gameMaster.activeMonsters[0] == myMonster)
        {
            counter--;
            myMonster.SetBurnDamage();
            myMonster.TakeDamage(usingMonster, GetBurnDamage());
        }

        UpdateStatusCounter(counter);
    }

    public bool TickCounter()
    {
        counter--;
        if(counter == 0)
        {
            return true;
        }

        return false;
    }

    public int GetBurnDamage()
    {
        float burnDamage = (myMonster.GetMaxHealth() * 0.35f) * -1f;
        burnDamage += myMonster.GetCurrentStrength();
        int damage = Mathf.RoundToInt(burnDamage);
        if(power == 1)
        {
            damage -= usingMonster.GetHalfStrength();
        }
        return damage;
    }

    public void DelcaringDamage(monster recivingMon, monster usingMon, int damage, bool willDie, bool destroyShields)
    {
        if(willDie)
        {
            gameMaster.redirectedMon = myMonster;
            myMonster.MovePosition(usingMonster.attackPoint.transform.position.x, usingMonster.attackPoint.position.y);
            StartCoroutine(goBack());
        }
    }

    public IEnumerator goBack()
    {
        yield return new WaitForSeconds(0.7f);
        myMonster.MovePosition(myMonster.spawnLocation.transform.position.x, myMonster.spawnLocation.position.y);
    }

    public void StatusGotReapplied(int newCounter, int power)
    {
        if(statusIndex == 0 || statusIndex == 1 || statusIndex == 2 || statusIndex == 7)
        {
            UpdateStatusCounter(counter + newCounter);
        }

        if(statusIndex == 3 || statusIndex == 4) // stats for cuddle and best friends
        {
            UpdateStatusCounter(counter + newCounter);
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
        if (alreadyRemoved)
            return;

        alreadyRemoved = true;

        switch(statusIndex)
        {
            case 3:
                StatChange(0, -power);
                break;

            case 4:
                StatChange(0, -power);
                StatChange(1, -power);
                break;

            case 5:
                usingMonster.onDeclaredDamage -= DelcaringDamage;
                StatChange(0, -power);
                StatChange(1, -power);
                break;

            case 6:
                usingMonster.onDeclaredDamage -= DelcaringDamage;
                break;

            case 9: // add extra logic if another multipler attack gets added
                //power -= 100;
                //myMonster.myBase.attackMultiplier -= power;
                //myMonster.myBase.destroyShields = false;
                break;

            case 10:
                monster[] myTeam = gameMaster.GetMonstersTeam(myMonster);

                for (int i = 0; i < 3; i++)
                {
                    if (myTeam[i].GetCurrentHealth() > 0)
                    {
                        myTeam[i].MovePosition(myTeam[i].spawnLocation.position.x, myTeam[i].spawnLocation.position.y);
                    }
                }
                break;
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
        activeCounterText.text = (newCounter >= 99) ? "" : $"{newCounter}";
    }
}

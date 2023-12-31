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
                this.power -= 100;
                myMonster.ChangeAttackMultiplier(this.power);
                myMonster.ApplyShieldBreak(true);
                break;

            case 12:
                StatChange(0, power);
                break;

            case 13:
                StatChange(0, power);
                break;

            case 15:
                switch(myMonster.GetSpriteIndexFromLevel())
                {
                    case 0:
                        myMonster.AdjustDamageReduction(-40);
                        break;

                    case 1:
                        myMonster.AdjustDamageReduction(-50);
                        break;

                    case 2:
                        myMonster.AdjustDamageReduction(-60);
                        break;
                }
                StatChange(3, power);
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

    public bool NextTurn()
    {
        bool shouldDelete = false;

        if (statusIndex == 1) // 1 is bubble
        {
            float newBubbleValue = (counter * 0.9f) - 1;
            counter = Mathf.RoundToInt(newBubbleValue);
            shouldDelete = (counter <= 0);
        }

        if(statusIndex == 0 || statusIndex == 2 || statusIndex == 10 || statusIndex == 15)
        {
            shouldDelete = TickCounter();
        }

        if (statusIndex == 7 && gameMaster.activeMonsters[0] == myMonster)
        {
            shouldDelete = TickCounter();
            myMonster.SetBurnDamage();
            myMonster.TakeDamage(usingMonster, GetBurnDamage());
        }

        UpdateStatusCounter(counter);
        return shouldDelete;
    }

    public bool TickCounter()
    {
        counter--;
        if(counter <= 0)
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
        if(statusIndex == 0 || statusIndex == 1 || statusIndex == 2 || statusIndex == 3 || statusIndex == 4 || statusIndex == 7 || statusIndex == 12 || statusIndex == 13 || statusIndex == 14)
        {
            UpdateStatusCounter(counter + newCounter);
        }
    }

    private void UsedAction(bool isAttack)
    {
        if (statusIndex == 3 || statusIndex == 4 || statusIndex == 12 || statusIndex == 13)
        {

            if(isAttack && statusIndex == 12 && myMonster.GetOwnership() && counter >= 1)
            {
                monster[] allies = gameMaster.GetMonstersTeam(myMonster);
                foreach(monster ally  in allies)
                {
                    gameMaster.ApplyStatus(myMonster, ally, 13, 1, (power + 2));
                    gameMaster.AnimateMonster(ally, "idle");
                }
            }
            
            counter--;

            if (counter <= 0)
            {
                myMonster.TryRemoveStatus(statusIndex, true);

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

            case 9:
                myMonster.ChangeAttackMultiplier(-power);
                myMonster.ApplyShieldBreak(false);
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

            case 12:
                StatChange(0, -power);
                break;

            case 13:
                StatChange(0, -power);
                break;

            case 15:
                switch (myMonster.GetSpriteIndexFromLevel())
                {
                    case 0:
                        myMonster.AdjustDamageReduction(40);
                        break;

                    case 1:
                        myMonster.AdjustDamageReduction(50);
                        break;

                    case 2:
                        myMonster.AdjustDamageReduction(60);
                        break;
                }
                StatChange(3, -power);
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

            case 3: // highest stat
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
        activeCounterText.text = (newCounter >= 99) ? "" : $"{newCounter}";
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static monster;

public class minfurAlly : monsterAlly
{
    private void Start()
    {
        onAttack += UseAttack;
        onAbility += UseAbility;
        GetMyMonster().onAttackAgain += AttackAgain;
        GetMyMonster().onRemoveConnections += RemoveConnections;

        monster[] monsters = gameMaster.GetMonstersTeam(GetMyMonster());
        foreach(monster mon in monsters)
        {
            mon.onTakeDamage += TookDamage;
        }
    }

    private void RemoveConnections()
    {
        onAttack -= UseAttack;
        onAbility -= UseAbility;
        GetMyMonster().onAttackAgain -= AttackAgain;
        GetMyMonster().onRemoveConnections -= RemoveConnections;

        monster[] monsters = gameMaster.GetMonstersTeam(GetMyMonster());
        foreach (monster mon in monsters)
        {
            mon.onTakeDamage -= TookDamage;
        }
    }

    private void AttackAgain(int percentageMultiplier, bool bMine2, int TargetOfTargetIndex)
    {
        if (GetMyMonster().bMine == false)
            return;

        while (attackMultiplier > 100)
        {
            attackMultiplier--;
            percentageMultiplier++;
        }
        attackMultiplier = percentageMultiplier;
        UseAttack(GetMyMonster().GetAttackID(), TargetOfTargetIndex, false);
    }

    private void TookDamage(int change, bool died, bool bMine, int userIndex, monster recivingMon, bool burnDamage)
    {
        if (GetMyMonster().GetPassiveID() == 2 && !isMine()) // index for acron
        {
            if (recivingMon.GetCurrentHealth() <= 0)
                return;

            if(GetMyMonster().TryConsumeStrawberry() == true)
            {
                Debug.Log("You get a strawberry Mr. " + recivingMon.teamIndex);
                StartCoroutine(GiveStrawberryWait(recivingMon));
            }
        }
    }

    private IEnumerator GiveStrawberryWait(monster recivingMon)
    {
        gameMaster.AnimateMonster(isMine(), GetMyMonster().teamIndex, "ability1");
        yield return new WaitForSeconds(0.1f);
        gameMaster.ShootProjectile(isMine(), GetMyMonster().teamIndex, 6, isMine(), recivingMon.teamIndex, false, 0);
        yield return new WaitForSeconds(0.2f);
        int heal = GetMyMonster().GetBaseMagic() + GetMoveDamage(5, 0);
        gameMaster.ChangeMonsterHealth(isMine(), GetMyMonster().teamIndex, isMine(), recivingMon.teamIndex, heal, false);

    }

    private void UseAttack(int attackID, int targetIndex, bool consumeTurn)
    {
        switch (attackID)
        {
            default:
                break;

            case 1:
                StartCoroutine(Cuddle(targetIndex, consumeTurn));
                break;

            case 2:
                StartCoroutine(FluffyRoll(targetIndex, consumeTurn));
                break;
        }
    }

    private void UseAbility(int abilityID)
    {
        switch (abilityID)
        {
            default:
                break;

            case 1:
                StartCoroutine(Yippers());
                break;

            case 2:
                if(GetTargetedMonster().teamIndex == GetMyMonster().teamIndex)
                {
                    GetMyMonster().canAct = true;
                    return;
                }
                StartCoroutine(BestFriends());
                break;
        }
    }

    private IEnumerator Cuddle(int targetIndex, bool consumeTurn)
    {
        gameMaster.AnimateMonster(isMine(), GetMyMonster().teamIndex, "attack1");
        gameMaster.MoveMonster(isMine(), GetMyMonster().teamIndex, false, !isMine(), targetIndex, false, 0);

        yield return new WaitForSeconds(0.83f);

        int attack = GetMyMonster().GetCurrentStrength() + GetMoveDamage(0, 0);
        attack = GetMultiplierDamage(attack);

        gameMaster.ShootProjectile(isMine(), GetMyMonster().teamIndex, 3, !isMine(), targetIndex, false, 0);
        gameMaster.ApplyStatus(isMine(), GetMyMonster().teamIndex, 3, !isMine(), targetIndex, GetMoveDamage(0, 1), -attack);

        yield return new WaitForSeconds(0.5f);

        if (GetMyMonster().GetPassiveID() == 1)
        {
            YoinkStatuses(false, 1, 2, 4, 8, 9);
        }

        yield return new WaitForSeconds(0.5f);

        gameMaster.MoveMonster(isMine(), GetMyMonster().teamIndex, true, isMine(), GetMyMonster().teamIndex, false, 0);

        yield return new WaitForSeconds(0.5f);

        FinishMove(consumeTurn, true);
    }

    private void YoinkStatuses(bool isAlly, int status, int status2, int status3, int status4, int status5)
    {
        if (GetTargetedMonster().statusEffects.Count <= 0)
            return;

        List<int> listOfIndexesToSteal = new List<int>();

        foreach (statusEffectUI effect in GetTargetedMonster().statusEffects)
        {
            if (effect.GetIndex() == status || effect.GetIndex() == status2 || effect.GetIndex() == status3 || effect.GetIndex() == status4 || effect.GetIndex() == status5) // add more of these as they get added
            {
                listOfIndexesToSteal.Add(effect.GetIndex());
            }
        }

        if (listOfIndexesToSteal.Count > 0)
        {
            foreach (int index in listOfIndexesToSteal)
            {
                statusEffectUI stolenStatus = GetTargetedMonster().GetStatus(index);
                gameMaster.RemoveStatus(isMine(), GetMyMonster().teamIndex, index, isAlly, GetTargetedMonster().teamIndex);
                gameMaster.ApplyStatus(isMine(), GetMyMonster().teamIndex, index, true, GetMyMonster().teamIndex, stolenStatus.GetCounter(), stolenStatus.GetPower());

            }
        }
    }

    private IEnumerator FluffyRoll(int targetIndex, bool consumeTurn)
    {
        gameMaster.AnimateMonster(isMine(), GetMyMonster().teamIndex, "attack2");
        gameMaster.ShootProjectile(isMine(), GetMyMonster().teamIndex, 5, !isMine(), targetIndex, false, 0);
        gameMaster.MoveMonster(isMine(), GetMyMonster().teamIndex, false, !isMine(), targetIndex, false, 0);

        int attack = GetMyMonster().GetCurrentStrength() + GetMoveDamage(1, 0);
        attack = GetMultiplierDamage(attack);

        gameMaster.DeclaringDamage(isMine(), GetMyMonster().teamIndex, !isMine(), targetIndex, -attack, destroyShields);
        yield return new WaitForSeconds(0.1f);
        targetIndex = gameMaster.GetRedirectedIndex(targetIndex);

        yield return new WaitForSeconds(0.2f);
        gameMaster.ChangeMonsterHealth(isMine(), GetMyMonster().teamIndex, !isMine(), targetIndex, -attack, true);

        if (GetMyMonster().GetPassiveID() == 1)
        {
            YoinkStatuses(false, 1, 2, 4, 8, 9);
        }

        gameMaster.AdjustTurnOrder(!isMine(), targetIndex, false, true);

        yield return new WaitForSeconds(1.15f);

        gameMaster.MoveMonster(isMine(), GetMyMonster().teamIndex, true, isMine(), GetMyMonster().teamIndex, false, 0);

        yield return new WaitForSeconds(0.3f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator Yippers()
    {
        gameMaster.AnimateMonster(isMine(), GetMyMonster().teamIndex, "ability1");

        yield return new WaitForSeconds(0.3f);

        int attack = GetMyMonster().GetCurrentMagic() + GetMoveDamage(2, 0);
        attack = GetMultiplierDamage(attack);

        if (GetMyMonster().GetPassiveID() == 1)
        {
            YoinkStatuses(true, 0, 3, 7, -1, -1);
        }

        monster[] myteam = gameMaster.GetMonstersTeam(GetMyMonster());
        for (int i = 0; i < 3; i++)
        {
            if (myteam[i].GetCurrentHealth() > 0)
            {
                if (myteam[i].teamIndex == GetMyMonster().teamIndex)
                {
                    gameMaster.ApplyStatus(isMine(), GetMyMonster().teamIndex, 4, isMine(), i, (GetMoveDamage(2, 1) + 1), attack);
                }
                else
                {
                    gameMaster.ApplyStatus(isMine(), GetMyMonster().teamIndex, 4, isMine(), i, GetMoveDamage(2, 1), attack);
                    gameMaster.AnimateMonster(isMine(), i, "idle");
                }
            }
        }

        yield return new WaitForSeconds(0.8f);

        FinishMove(true, false);
    }

    private IEnumerator BestFriends()
    {
        gameMaster.AnimateMonster(isMine(), GetMyMonster().teamIndex, "ability2");
        gameMaster.MoveMonster(isMine(), GetMyMonster().teamIndex, false, isMine(), GetTargetedMonster().teamIndex, false, 0);

        statusEffectUI minfurfriendStatus = GetMyMonster().GetStatus(6);
        if (minfurfriendStatus != null)
        {
            gameMaster.RemoveStatus(isMine(), GetMyMonster().teamIndex, 6, isMine(), GetMyMonster().teamIndex);
            
            foreach(monster ally in gameMaster.GetMonstersTeam(GetMyMonster()))
            {
                statusEffectUI bestFriend = ally.GetStatus(5);
                if(bestFriend != null)
                {
                    gameMaster.RemoveStatus(isMine(), GetMyMonster().teamIndex, 5, isMine(), ally.teamIndex);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);

        gameMaster.ShootProjectile(isMine(), GetMyMonster().teamIndex, 4, isMine(), GetTargetedMonster().teamIndex, false, 0);

        if (GetMyMonster().GetPassiveID() == 1)
        {
            YoinkStatuses(true, 0, 3, 7, -1, -1);
        }

        yield return new WaitForSeconds(0.5f);

        gameMaster.ApplyStatus(isMine(), GetMyMonster().teamIndex, 5, isMine(), GetTargetedMonster().teamIndex, 99, 0);
        gameMaster.ApplyStatus(isMine(), GetTargetedMonster().teamIndex, 6, isMine(), GetMyMonster().teamIndex, 99, 0);

        yield return new WaitForSeconds(0.5f);

        gameMaster.MoveMonster(isMine(), GetMyMonster().teamIndex, true, isMine(), GetMyMonster().teamIndex, false, 0);

        FinishMove(true, false);
    }
}

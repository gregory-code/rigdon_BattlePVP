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

    private void TookDamage(int change, bool died, bool bMine, int userIndex, monster recivingMon)
    {
        if (GetMyMonster().GetPassiveID() == 2 && !gameMaster.IsItMyTurn()) // index for acron
        {
            if(GetMyMonster().TryConsumeStrawberry() == true)
            {
                Debug.Log("You get a strawberry Mr. " + recivingMon.teamIndex);
                StartCoroutine(GiveStrawberryWait(recivingMon));
            }
        }
    }

    private IEnumerator GiveStrawberryWait(monster recivingMon)
    {
        gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, "ability1");
        yield return new WaitForSeconds(0.1f);
        gameMaster.ShootProjectile(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 6, gameMaster.IsItMyTurn(), recivingMon.teamIndex, false, 0);
        yield return new WaitForSeconds(0.2f);
        int heal = GetMyMonster().GetBaseMagic() + GetMoveDamage(5, 0);
        gameMaster.ChangeMonsterHealth(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, gameMaster.IsItMyTurn(), recivingMon.teamIndex, heal, false);

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
        gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, "attack1");
        gameMaster.MoveMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, false, !gameMaster.IsItMyTurn(), targetIndex);

        yield return new WaitForSeconds(0.83f);

        int attack = GetMyMonster().GetCurrentStrength() + GetMoveDamage(0, 0);
        attack = GetMultiplierDamage(attack);

        gameMaster.ShootProjectile(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 3, !gameMaster.IsItMyTurn(), targetIndex, false, 0);
        gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 3, !gameMaster.IsItMyTurn(), targetIndex, GetMoveDamage(0, 1), -attack);

        yield return new WaitForSeconds(0.5f);

        if (GetMyMonster().GetPassiveID() == 1)
        {
            YoinkStatuses(false, 1, 2, 4, 8, 9);
        }

        yield return new WaitForSeconds(0.5f);

        gameMaster.MoveMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, true, gameMaster.IsItMyTurn(), GetMyMonster().teamIndex);

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
            if (effect.GetIndex() == status || effect.GetIndex() == status2 || effect.GetIndex() == status3 || effect.GetIndex() == status4) // add more of these as they get added
            {
                listOfIndexesToSteal.Add(effect.GetIndex());
            }
        }

        if (listOfIndexesToSteal.Count > 0)
        {
            foreach (int index in listOfIndexesToSteal)
            {
                statusEffectUI stolenStatus = GetTargetedMonster().GetStatus(index);
                gameMaster.RemoveStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, index, isAlly, GetTargetedMonster().teamIndex);
                gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, index, true, GetMyMonster().teamIndex, stolenStatus.GetCounter(), stolenStatus.GetPower());

            }
        }
    }

    private IEnumerator FluffyRoll(int targetIndex, bool consumeTurn)
    {
        gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, "attack2");
        gameMaster.ShootProjectile(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 5, !gameMaster.IsItMyTurn(), targetIndex, false, 0);
        gameMaster.MoveMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, false, !gameMaster.IsItMyTurn(), targetIndex);

        int attack = GetMyMonster().GetCurrentStrength() + GetMoveDamage(1, 0);
        attack = GetMultiplierDamage(attack);

        gameMaster.DeclaringDamage(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), targetIndex, -attack, destroyShields);
        yield return new WaitForSeconds(0.1f);
        targetIndex = gameMaster.GetRedirectedIndex(targetIndex);

        yield return new WaitForSeconds(0.2f);
        gameMaster.ChangeMonsterHealth(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), targetIndex, -attack, true);

        if (GetMyMonster().GetPassiveID() == 1)
        {
            YoinkStatuses(false, 1, 2, 4, 8, 9);
        }

        gameMaster.AdjustTurnOrder(!gameMaster.IsItMyTurn(), targetIndex, false, true);

        yield return new WaitForSeconds(1.15f);

        gameMaster.MoveMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, true, gameMaster.IsItMyTurn(), GetMyMonster().teamIndex);

        yield return new WaitForSeconds(0.3f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator Yippers()
    {
        gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, "ability1");

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
                    gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 4, gameMaster.IsItMyTurn(), i, (GetMoveDamage(2, 1) + 1), attack);
                }
                else
                {
                    gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 4, gameMaster.IsItMyTurn(), i, GetMoveDamage(2, 1), attack);
                    gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), i, "idle");
                }
            }
        }

        yield return new WaitForSeconds(0.8f);

        FinishMove(true, false);
    }

    private IEnumerator BestFriends()
    {
        gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, "ability2");
        gameMaster.MoveMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, false, gameMaster.IsItMyTurn(), GetTargetedMonster().teamIndex);

        statusEffectUI minfurfriendStatus = GetMyMonster().GetStatus(6);
        if (minfurfriendStatus != null)
        {
            gameMaster.RemoveStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 6, gameMaster.IsItMyTurn(), GetMyMonster().teamIndex);
            
            foreach(monster ally in gameMaster.GetMonstersTeam(GetMyMonster()))
            {
                statusEffectUI bestFriend = ally.GetStatus(5);
                if(bestFriend != null)
                {
                    gameMaster.RemoveStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 5, gameMaster.IsItMyTurn(), ally.teamIndex);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);

        gameMaster.ShootProjectile(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 4, gameMaster.IsItMyTurn(), GetTargetedMonster().teamIndex, false, 0);

        if (GetMyMonster().GetPassiveID() == 1)
        {
            YoinkStatuses(true, 0, 3, 7, -1, -1);
        }

        yield return new WaitForSeconds(0.5f);

        gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 5, gameMaster.IsItMyTurn(), GetTargetedMonster().teamIndex, 99, 0);
        gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetTargetedMonster().teamIndex, 6, gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 99, 0);

        yield return new WaitForSeconds(0.5f);

        gameMaster.MoveMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, true, gameMaster.IsItMyTurn(), GetMyMonster().teamIndex);

        FinishMove(true, false);
    }
}

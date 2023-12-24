using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lusseliaAlly : monsterAlly
{
    private void Start()
    {
        onAttack += UseAttack;
        onAbility += UseAbility;
        GetMyMonster().onAttackAgain += AttackAgain;
        GetMyMonster().onTakeDamage += TookDamage;
    }

    private void AttackAgain(int percentageMultiplier, bool bMine2, int TargetOfTargetIndex)
    {
        if (GetMyMonster().bMine == false)
            return;

        attackMultiplier = percentageMultiplier;
        UseAttack(GetMyMonster().GetAttackID(), TargetOfTargetIndex, false);
    }

    private void TookDamage(int change, bool died, bool bMine, int userIndex)
    {
        if (GetMyMonster().GetPassiveID() == 2) // index for cloud legend
        {

        }
    }

    private void UseAttack(int attackID, int targetIndex, bool consumeTurn)
    {
        switch (attackID)
        {
            default:
                break;

            case 1:
                StartCoroutine(Starfall(targetIndex, consumeTurn));
                break;

            case 2:
                StartCoroutine(SolarWave(targetIndex, consumeTurn));
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
                StartCoroutine(LunarBlanket());
                break;

            case 2:
                StartCoroutine(Moonlight());
                break;
        }
    }

    private void FinishMove(bool consumeTurn, bool isAttack)
    {
        attackMultiplier = 100;
        gameMaster.waitingForAction = false;

        gameMaster.UsedAction(true, GetMyMonster().teamIndex, isAttack);
        
        if (consumeTurn == true)
            gameMaster.NextTurn();
        
    }

    private IEnumerator Starfall(int targetIndex, bool consumeTurn)
    {
        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "attack1");

        yield return new WaitForSeconds(0.5f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator SolarWave(int targetIndex, bool consumeTurn)
    {
        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "attack2");

        yield return new WaitForSeconds(1f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator LunarBlanket()
    {
        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "ability1");

        yield return new WaitForSeconds(0.3f);

        FinishMove(true, false);
    }

    private IEnumerator Moonlight()
    {
        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "ability2");

        yield return new WaitForSeconds(0.3f);

        FinishMove(true, false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class minfurAlly : monsterAlly
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
            gameMaster.ApplyStatus(0, bMine, userIndex, 4); // index 0  for conductive status
        }
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
                StartCoroutine(BestFriends());
                break;
        }
    }

    private void FinishMove(bool consumeTurn)
    {
        attackMultiplier = 100;
        gameMaster.waitForAction = false;

        if (consumeTurn == true)
            gameMaster.NextTurn();
    }

    private IEnumerator Cuddle(int targetIndex, bool consumeTurn)
    {
        yield return new WaitForSeconds(0.6f);

        FinishMove(consumeTurn);
    }

    private IEnumerator FluffyRoll(int targetIndex, bool consumeTurn)
    {
        yield return new WaitForSeconds(0.5f);

        FinishMove(consumeTurn);
    }

    private IEnumerator Yippers()
    {
        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "ability1");

        yield return new WaitForSeconds(0.3f);

        FinishMove(true);
    }

    private IEnumerator BestFriends()
    {
        yield return new WaitForSeconds(0.3f);

        FinishMove(true);
    }
}

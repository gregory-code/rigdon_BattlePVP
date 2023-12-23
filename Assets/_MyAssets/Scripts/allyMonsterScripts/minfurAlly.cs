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
        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "attack1");
        gameMaster.MoveMonster(true, GetMyMonster().teamIndex, false, GetTargetedMonster().spawnLocation);

        yield return new WaitForSeconds(0.83f);

        int attack = GetMyMonster().GetCurrentStrength() + GetMoveDamage(0, 0);
        attack = GetMultiplierDamage(attack);

        gameMaster.ShootProjectile(true, GetMyMonster().teamIndex, 3, false, targetIndex);
        gameMaster.ApplyActionBasedStatus(0, false, targetIndex, GetMoveDamage(0, 1), attack);

        yield return new WaitForSeconds(1f);

        gameMaster.MoveMonster(true, GetMyMonster().teamIndex, true, GetMyMonster().spawnLocation);

        yield return new WaitForSeconds(0.5f);

        FinishMove(consumeTurn);
    }

    private IEnumerator FluffyRoll(int targetIndex, bool consumeTurn)
    {
        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "attack2");
        gameMaster.MoveMonster(true, GetMyMonster().teamIndex, false, GetTargetedMonster().spawnLocation);

        yield return new WaitForSeconds(0.5f);

        int attack = GetMyMonster().GetCurrentStrength() + GetMoveDamage(1, 0);
        attack = GetMultiplierDamage(attack);

        gameMaster.ChangeMonsterHealth(true, GetMyMonster().teamIndex, false, targetIndex, -attack);

        //Apply slow

        yield return new WaitForSeconds(1f);

        gameMaster.MoveMonster(true, GetMyMonster().teamIndex, true, GetMyMonster().spawnLocation);



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
        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "ability2");

        yield return new WaitForSeconds(0.3f);

        FinishMove(true);
    }
}

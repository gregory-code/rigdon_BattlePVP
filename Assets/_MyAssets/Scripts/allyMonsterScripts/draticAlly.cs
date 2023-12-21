using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class draticAlly : monsterAlly
{
    private void Start()
    {
        onAttack += UseAttack;
        onAbility += UseAbility;
    }

    private void UseAttack(int attackID)
    {
        switch (attackID)
        {
            default:
                break;

            case 1:
                StartCoroutine(RingingThunder());
                break;

            case 2:
                StartCoroutine(BoomSpear());
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
                break;

            case 2:
                break;
        }
    }

    private IEnumerator RingingThunder() // handle passives and stuff here too
    {
        int tier = GetMyMonster().GetSpriteIndexFromLevel();

        int attack1 = GetMyMonster().GetCurrentStrength() + GetCurrentMove(0).GetScaleValues(0)[tier]; // consider reducing by a % that would be hype
        int attack2 = GetMyMonster().GetCurrentStrength() + GetCurrentMove(0).GetScaleValues(1)[tier];
        int attack3 = GetMyMonster().GetCurrentStrength() + GetCurrentMove(0).GetScaleValues(2)[tier];

        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "attack1");

        int[] enemyIndexes = new int[3] { 0, 1, 2 };


        yield return new WaitForSeconds(0.4f);
        gameMaster.ShootProjectile(true, GetMyMonster().teamIndex, 2, false, GetTargetedMonster().teamIndex);
        enemyIndexes[GetTargetedMonster().teamIndex] = -1;

        yield return new WaitForSeconds(0.65f);
        gameMaster.ChangeMonsterHealth(false, GetTargetedMonster().teamIndex, -attack1);

        int nextTarget = -1;
        while(nextTarget == -1)
        {
            nextTarget = enemyIndexes[Random.Range(0, 3)];
        }

        gameMaster.ShootProjectile(false, GetTargetedMonster().teamIndex, 2, false, nextTarget);
        enemyIndexes[nextTarget] = -1;

        yield return new WaitForSeconds(0.1f);
        gameMaster.ChangeMonsterHealth(false, nextTarget, -attack2);

        int finalTarget = -1;
        while (finalTarget == -1)
        {
            finalTarget = enemyIndexes[Random.Range(0, 3)];
        }

        gameMaster.ShootProjectile(false, nextTarget, 2, false, finalTarget);

        yield return new WaitForSeconds(0.1f);
        gameMaster.ChangeMonsterHealth(false, finalTarget, -attack3);

        gameMaster.NextTurn();
    }

    private IEnumerator BoomSpear()
    {
        int tier = GetMyMonster().GetSpriteIndexFromLevel();

        int attack1 = GetMyMonster().GetCurrentMagic() + GetCurrentMove(1).GetScaleValues(0)[tier];
        int attack2 = GetMyMonster().GetCurrentStrength() + GetCurrentMove(1).GetScaleValues(1)[tier];

        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "attack2");

        yield return new WaitForSeconds(0.16f); // when the shot should come out
        gameMaster.ShootProjectile(true, GetMyMonster().teamIndex, 0, false, GetTargetedMonster().teamIndex);

        yield return new WaitForSeconds(0.5f);
        gameMaster.ChangeMonsterHealth(false, GetTargetedMonster().teamIndex, -attack1); // - for damage

        yield return new WaitForSeconds(0.6f);
        gameMaster.ShootProjectile(true, GetMyMonster().teamIndex, 1, false, GetTargetedMonster().teamIndex);

        yield return new WaitForSeconds(0.5f);
        gameMaster.ChangeMonsterHealth(false, GetTargetedMonster().teamIndex, -attack2); // - for damage

        gameMaster.NextTurn();
    }
}

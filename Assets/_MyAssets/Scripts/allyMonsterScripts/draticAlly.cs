using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class draticAlly : monsterAlly
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
            gameMaster.ApplyStatus(0, bMine, userIndex, 4, 0); // index 0  for conductive status
        }
    }

    private void UseAttack(int attackID, int targetIndex, bool consumeTurn)
    {
        switch (attackID)
        {
            default:
                break;

            case 1:
                StartCoroutine(RingingThunder(targetIndex, consumeTurn));
                break;

            case 2:
                StartCoroutine(BoomSpear(targetIndex, consumeTurn));
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
                StartCoroutine(LightningRound());
                break;

            case 2:
                StartCoroutine(EyeOfTheStorm());
                break;
        }
    }

    private void FinishMove(bool consumeTurn, bool isAttack)
    {
        attackMultiplier = 100;
        gameMaster.waitForAction = false;

        gameMaster.UsedAction(true, GetMyMonster().teamIndex, isAttack);

        if (consumeTurn == true)
            gameMaster.NextTurn();
    }

    private IEnumerator RingingThunder(int targetIndex, bool consumeTurn) // handle passives and stuff here too
    {
        int attack1 = GetMyMonster().GetCurrentStrength() + GetMoveDamage(0,0); // consider reducing by a % that would be hype
        attack1 = GetMultiplierDamage(attack1);

        int attack2 = GetMyMonster().GetCurrentStrength() + GetMoveDamage(0, 1);
        attack2 = GetMultiplierDamage(attack2);

        int attack3 = GetMyMonster().GetCurrentStrength() + GetMoveDamage(0, 2);
        attack3 = GetMultiplierDamage(attack3);

        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "attack1");

        yield return new WaitForSeconds(0.6f);
        gameMaster.ShootProjectile(true, GetMyMonster().teamIndex, 2, false, targetIndex);

        yield return new WaitForSeconds(0.65f);
        gameMaster.ChangeMonsterHealth(true, GetMyMonster().teamIndex, false, targetIndex, -attack1);

        yield return new WaitForSeconds(0.1f);

        if (GetMyMonster().GetPassiveID() == 1)
            gameMaster.ApplyStatus(0, false, targetIndex, 4, 0);

        int nextTarget = gameMaster.GetRandomEnemyIndex(targetIndex, -1);
        if(nextTarget != 5)
        {
            gameMaster.ShootProjectile(false, targetIndex, 2, false, nextTarget);

            yield return new WaitForSeconds(0.1f);
            gameMaster.ChangeMonsterHealth(true, GetMyMonster().teamIndex, false, nextTarget, -attack2);

            yield return new WaitForSeconds(0.1f);

            if (GetMyMonster().GetPassiveID() == 1)
                gameMaster.ApplyStatus(0, false, nextTarget, 4, 0);

            int finalTarget = gameMaster.GetRandomEnemyIndex(targetIndex, nextTarget);
            if(finalTarget != 5)
            {
                gameMaster.ShootProjectile(false, nextTarget, 2, false, finalTarget);

                yield return new WaitForSeconds(0.1f);
                gameMaster.ChangeMonsterHealth(true, GetMyMonster().teamIndex, false, finalTarget, -attack3);

                yield return new WaitForSeconds(0.1f);

                if (GetMyMonster().GetPassiveID() == 1)
                    gameMaster.ApplyStatus(0, false, finalTarget, 4, 0);
            }
        }

        FinishMove(consumeTurn, true);
    }

    private IEnumerator BoomSpear(int targetIndex, bool consumeTurn)
    {
        int attack1 = GetMyMonster().GetCurrentMagic() + GetMoveDamage(1,0);
        attack1 = GetMultiplierDamage(attack1);

        int attack2 = GetMyMonster().GetCurrentStrength() + GetMoveDamage(1,1);
        attack2 = GetMultiplierDamage(attack2);

        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "attack2");

        yield return new WaitForSeconds(0.3f);
        gameMaster.ShootProjectile(true, GetMyMonster().teamIndex, 0, false, targetIndex);

        yield return new WaitForSeconds(0.5f);
        gameMaster.ChangeMonsterHealth(true, GetMyMonster().teamIndex, false, targetIndex, -attack1); // - for damage

        yield return new WaitForSeconds(0.1f);

        if (GetMyMonster().GetPassiveID() == 1)
            gameMaster.ApplyStatus(0, false, targetIndex, 4, 0);

        yield return new WaitForSeconds(0.4f);
        gameMaster.ShootProjectile(true, GetMyMonster().teamIndex, 1, false, targetIndex);

        yield return new WaitForSeconds(0.45f);
        gameMaster.ChangeMonsterHealth(true, GetMyMonster().teamIndex, false, targetIndex, -attack2); // - for damage

        yield return new WaitForSeconds(0.3f);

        if (GetMyMonster().GetPassiveID() == 1)
            gameMaster.ApplyStatus(0, false, targetIndex, 4, 0);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator LightningRound()
    {
        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "ability1");
        
        yield return new WaitForSeconds(0.3f);

        int randomTarget = gameMaster.GetRandomConductiveEnemyIndex();
        if(randomTarget == 5)
            randomTarget = gameMaster.GetRandomEnemyIndex(-1, -1);

        int abilityMultiplier = (GetMyMonster().GetCurrentMagic() * GetCurrentMove(2).GetPercentageMultiplier()) + GetMoveDamage(2, 0);

        gameMaster.AttackAgain(true, GetTargetedMonster().teamIndex, abilityMultiplier, false, randomTarget);

        gameMaster.waitForAction = true;
        while(gameMaster.waitForAction)
        {
            yield return new WaitForEndOfFrame();
        }

        FinishMove(true, false);
    }

    private IEnumerator EyeOfTheStorm()
    {
        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "ability2");
        yield return new WaitForSeconds(0.3f);
        int shieldMultiplier = (GetMyMonster().GetCurrentMagic() * GetCurrentMove(3).GetPercentageMultiplier()) + GetMoveDamage(3, 1);
        float shieldStrength = GetTargetedMonster().GetMaxHealth() * (1f * shieldMultiplier / 100f);
        int shield = Mathf.RoundToInt(shieldStrength);

        float newBubbleBuffer = (shield * 1.1f) + 1;
        shield = Mathf.RoundToInt(newBubbleBuffer);

        gameMaster.ApplyStatus(1, true, GetTargetedMonster().teamIndex, shield, 0);
        gameMaster.ApplyStatus(2, true, GetTargetedMonster().teamIndex, (GetMoveDamage(3, 0) + 1), 0); // I think however many turns +1 since NextTurn();

        yield return new WaitForSeconds(0.3f);

        FinishMove(true, false);
    }
}

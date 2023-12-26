using System;
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
        GetMyMonster().onRemoveConnections += RemoveConnections;
    }

    private void RemoveConnections()
    {
        onAttack -= UseAttack;
        onAbility -= UseAbility;
        GetMyMonster().onAttackAgain -= AttackAgain;
        GetMyMonster().onTakeDamage -= TookDamage;
        GetMyMonster().onRemoveConnections -= RemoveConnections;
    }

    private void AttackAgain(int percentageMultiplier, bool bMine2, int TargetOfTargetIndex)
    {
        if (GetMyMonster().bMine == false)
            return;

        attackMultiplier = percentageMultiplier;
        UseAttack(GetMyMonster().GetAttackID(), TargetOfTargetIndex, false);
    }

    private void TookDamage(int change, bool died, bool bMine, int userIndex, monster recivingMon)
    {
        if (GetMyMonster().GetPassiveID() == 2 && gameMaster.IsItMyTurn()) // index for cloud legend
        {
            gameMaster.ApplyStatus(!gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 0, gameMaster.IsItMyTurn(), userIndex, 4, 0); // index 0  for conductive status
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
        gameMaster.waitingForAction = false;

        gameMaster.UsedAction(true, GetMyMonster().teamIndex, isAttack);
        
        if (consumeTurn == true)
        {
            gameMaster.NextTurn();
        }
        
    }

    private IEnumerator RingingThunder(int targetIndex, bool consumeTurn)
    {
        int attack1 = GetMyMonster().GetCurrentStrength() + GetMoveDamage(0,0); // consider reducing by a % that would be hype
        attack1 = GetMultiplierDamage(attack1);

        int attack2 = GetMyMonster().GetCurrentStrength() + GetMoveDamage(0, 1);
        attack2 = GetMultiplierDamage(attack2);

        int attack3 = GetMyMonster().GetCurrentStrength() + GetMoveDamage(0, 2);
        attack3 = GetMultiplierDamage(attack3);

        gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, "attack1");

        yield return new WaitForSeconds(0.55f);
        gameMaster.ShootProjectile(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 2, !gameMaster.IsItMyTurn(), targetIndex, false, 0);

        gameMaster.DeclaringDamage(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), targetIndex, -attack1);
        yield return new WaitForSeconds(0.2f);
        targetIndex = gameMaster.GetRedirectedIndex(targetIndex);

        yield return new WaitForSeconds(0.40f);
        gameMaster.ChangeMonsterHealth(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), targetIndex, -attack1, true);

        if (GetMyMonster().GetPassiveID() == 1 && gameMaster.estimatedDamage < 0)
            gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 0, !gameMaster.IsItMyTurn(), targetIndex, 4, 0);

        yield return new WaitForSeconds(0.1f);

        int nextTarget = gameMaster.GetRandomEnemyIndex(targetIndex, -1);
        if(nextTarget != 5)
        {
            gameMaster.DeclaringDamage(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), nextTarget, -attack2);
            yield return new WaitForSeconds(0.2f);
            nextTarget = gameMaster.GetRedirectedIndex(nextTarget);

            gameMaster.ShootProjectile(!gameMaster.IsItMyTurn(), targetIndex, 2, !gameMaster.IsItMyTurn(), nextTarget, false, 0);

            yield return new WaitForSeconds(0.1f);

            gameMaster.ChangeMonsterHealth(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), nextTarget, -attack2, true);

            if (GetMyMonster().GetPassiveID() == 1 && gameMaster.estimatedDamage < 0)
                gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 0, !gameMaster.IsItMyTurn(), nextTarget, 4, 0);

            int finalTarget = gameMaster.GetRandomEnemyIndex(targetIndex, nextTarget);
            if (finalTarget != 5)
            {
                gameMaster.DeclaringDamage(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), finalTarget, -attack3);
                yield return new WaitForSeconds(0.2f);
                finalTarget = gameMaster.GetRedirectedIndex(finalTarget);

                gameMaster.ShootProjectile(!gameMaster.IsItMyTurn(), nextTarget, 2, !gameMaster.IsItMyTurn(), finalTarget, false, 0);

                yield return new WaitForSeconds(0.1f);

                gameMaster.ChangeMonsterHealth(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), finalTarget, -attack3, true);

                if (GetMyMonster().GetPassiveID() == 1 && gameMaster.estimatedDamage < 0)
                    gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 0, !gameMaster.IsItMyTurn(), finalTarget, 4, 0);
            }
        }

        yield return new WaitForSeconds(0.8f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator BoomSpear(int targetIndex, bool consumeTurn)
    {
        int attack1 = GetMyMonster().GetCurrentMagic() + GetMoveDamage(1,0);
        attack1 = GetMultiplierDamage(attack1);

        int attack2 = GetMyMonster().GetCurrentStrength() + GetMoveDamage(1,1);
        attack2 = GetMultiplierDamage(attack2);

        gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, "attack2");

        yield return new WaitForSeconds(0.3f);
        gameMaster.ShootProjectile(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 0, !gameMaster.IsItMyTurn(), targetIndex, false, 0);

        gameMaster.DeclaringDamage(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), targetIndex, -attack1);
        yield return new WaitForSeconds(0.2f);
        targetIndex = gameMaster.GetRedirectedIndex(targetIndex);
        yield return new WaitForSeconds(0.3f);
        gameMaster.ChangeMonsterHealth(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), targetIndex, -attack1, true); // - for damage

        yield return new WaitForSeconds(0.1f);

        if (GetMyMonster().GetPassiveID() == 1 && gameMaster.estimatedDamage < 0)
            gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 0, !gameMaster.IsItMyTurn(), targetIndex, 4, 0);

        gameMaster.ShootProjectile(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 1, !gameMaster.IsItMyTurn(), targetIndex, false, 0);

        gameMaster.DeclaringDamage(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), targetIndex, -attack2);
        yield return new WaitForSeconds(0.2f);
        targetIndex = gameMaster.GetRedirectedIndex(targetIndex);

        yield return new WaitForSeconds(0.25f);
        gameMaster.ChangeMonsterHealth(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), targetIndex, -attack2, true); // - for damage


        if (GetMyMonster().GetPassiveID() == 1 && gameMaster.estimatedDamage < 0)
            gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 0, !gameMaster.IsItMyTurn(), targetIndex, 4, 0);

        yield return new WaitForSeconds(0.8f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator LightningRound()
    {
        gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, "ability1");
        
        yield return new WaitForSeconds(0.3f);
        
        int randomTarget = gameMaster.GetRandomConductiveEnemyIndex();

        if (randomTarget == 5)
            randomTarget = gameMaster.GetRandomEnemyIndex(-1, -1);

        int abilityMultiplier = (GetMyMonster().GetCurrentMagic() * GetCurrentMove(2).GetPercentageMultiplier()) + GetMoveDamage(2, 0);

        gameMaster.AttackAgain(gameMaster.IsItMyTurn(), GetTargetedMonster().teamIndex, abilityMultiplier, !gameMaster.IsItMyTurn(), randomTarget);

        gameMaster.waitingForAction = true;
        while(gameMaster.waitingForAction == true)
        {
            yield return new WaitForEndOfFrame();
        }

        FinishMove(true, false);
    }

    private IEnumerator EyeOfTheStorm()
    {
        gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, "ability2");
        yield return new WaitForSeconds(0.3f);
        int shieldMultiplier = (GetMyMonster().GetCurrentMagic() * GetCurrentMove(3).GetPercentageMultiplier()) + GetMoveDamage(3, 1);
        float shieldStrength = GetTargetedMonster().GetMaxHealth() * (1f * shieldMultiplier / 100f);
        int shield = Mathf.RoundToInt(shieldStrength);
        shield = GetMultiplierDamage(shield);

        float newBubbleBuffer = (shield * 1.1f) + 1.05f;
        shield = Mathf.RoundToInt(newBubbleBuffer);

        gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 1, gameMaster.IsItMyTurn(), GetTargetedMonster().teamIndex, shield, 0);
        gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 2, gameMaster.IsItMyTurn(), GetTargetedMonster().teamIndex, (GetMoveDamage(3, 0) + 1), 0); // I think however many turns +1 since NextTurn();

        yield return new WaitForSeconds(0.3f);

        FinishMove(true, false);
    }
}

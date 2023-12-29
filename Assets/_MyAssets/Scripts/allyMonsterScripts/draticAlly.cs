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
        GetMonster().onAttackAgain += AttackAgain;
        GetMonster().onTakeDamage += TookDamage;
        GetMonster().onRemoveConnections += RemoveConnections;
    }

    private void RemoveConnections()
    {
        onAttack -= UseAttack;
        onAbility -= UseAbility;
        GetMonster().onAttackAgain -= AttackAgain;
        GetMonster().onTakeDamage -= TookDamage;
        GetMonster().onRemoveConnections -= RemoveConnections;
    }

    private void AttackAgain(monster targetMon, int percentageMultiplier)
    {
        if (GetMonster().GetOwnership() == false)
            return;

        while(attackMultiplier > 100)
        {
            attackMultiplier--;
            percentageMultiplier++;
        }
        attackMultiplier = percentageMultiplier;
        UseAttack(GetMonster().GetAttackID(), targetMon, false);
    }

    private void TookDamage(monster recivingMon, monster usingMon, int damage, bool died, bool burnDamage)
    {
        if (burnDamage)
            return;

        if (GetMonster().GetPassiveID() == 2 && GetMonster().GetOwnership()) // index for cloud legend
        {
            gameMaster.ApplyStatus(GetMonster(), usingMon, 0, 4, 0); // index 0  for conductive status
        }
    }

    private void UseAttack(int attackID, monster target, bool consumeTurn)
    {
        switch (attackID)
        {
            default:
                break;

            case 1:
                StartCoroutine(RingingThunder(target, consumeTurn));
                break;

            case 2:
                StartCoroutine(BoomSpear(target, consumeTurn));
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

    private IEnumerator RingingThunder(monster target, bool consumeTurn)
    {
        int attack1 = GetMonster().GetCurrentStrength() + GetMoveDamage(0,0); // consider reducing by a % that would be hype
        attack1 = GetMultiplierDamage(attack1);

        int attack2 = GetMonster().GetCurrentStrength() + GetMoveDamage(0, 1);
        attack2 = GetMultiplierDamage(attack2);

        int attack3 = GetMonster().GetCurrentStrength() + GetMoveDamage(0, 2);
        attack3 = GetMultiplierDamage(attack3);

        gameMaster.AnimateMonster(GetMonster(), "attack1");

        yield return new WaitForSeconds(0.55f);
        gameMaster.ShootProjectile(GetMonster(), target, 2, 0);

        gameMaster.DeclaringDamage(GetMonster(), target, -attack1, destroyShields);
        yield return new WaitForSeconds(0.2f);
        target = gameMaster.GetRedirectedMonster(target);

        yield return new WaitForSeconds(0.40f);
        gameMaster.DamageMonster(GetMonster(), target, -attack1);

        if (GetMonster().GetPassiveID() == 1 && gameMaster.estimatedDamage < 0)
            gameMaster.ApplyStatus(GetMonster(), target, 0, 4, 0);

        yield return new WaitForSeconds(0.1f);

        monster nextTarget = gameMaster.GetRandomEnemy(target.GetIndex(), -1, false);
        if(nextTarget != null)
        {
            gameMaster.DeclaringDamage(GetMonster(), nextTarget, -attack2, destroyShields);
            yield return new WaitForSeconds(0.2f);
            nextTarget = gameMaster.GetRedirectedMonster(nextTarget);

            gameMaster.ShootProjectile(target, nextTarget, 2, 0);

            yield return new WaitForSeconds(0.1f);

            gameMaster.DamageMonster(GetMonster(), nextTarget, -attack2);

            if (GetMonster().GetPassiveID() == 1 && gameMaster.estimatedDamage < 0)
                gameMaster.ApplyStatus(GetMonster(), nextTarget, 0, 4, 0);

            monster finalTarget = gameMaster.GetRandomEnemy(target.GetIndex(), nextTarget.GetIndex(), false);
            if (finalTarget != null)
            {
                gameMaster.DeclaringDamage(GetMonster(), finalTarget, -attack3, destroyShields);
                yield return new WaitForSeconds(0.2f);
                finalTarget = gameMaster.GetRedirectedMonster(finalTarget);

                gameMaster.ShootProjectile(nextTarget, finalTarget, 2, 0);

                yield return new WaitForSeconds(0.1f);

                gameMaster.DamageMonster(GetMonster(), finalTarget, -attack3);

                if (GetMonster().GetPassiveID() == 1 && gameMaster.estimatedDamage < 0)
                    gameMaster.ApplyStatus(GetMonster(), finalTarget, 0, 4, 0);
            }
        }

        yield return new WaitForSeconds(0.8f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator BoomSpear(monster target, bool consumeTurn)
    {
        int attack1 = GetMonster().GetCurrentMagic() + GetMoveDamage(1,0);
        attack1 = GetMultiplierDamage(attack1);

        int attack2 = GetMonster().GetCurrentStrength() + GetMoveDamage(1,1);
        attack2 = GetMultiplierDamage(attack2);

        gameMaster.AnimateMonster(GetMonster(), "attack2");

        yield return new WaitForSeconds(0.3f);
        gameMaster.ShootProjectile(GetMonster(), target, 0, 0);

        gameMaster.DeclaringDamage(GetMonster(), target, -attack1, destroyShields);
        yield return new WaitForSeconds(0.2f);
        target = gameMaster.GetRedirectedMonster(target);
        yield return new WaitForSeconds(0.3f);
        gameMaster.DamageMonster(GetMonster(), target, -attack1);

        yield return new WaitForSeconds(0.1f);

        if (GetMonster().GetPassiveID() == 1 && gameMaster.estimatedDamage < 0)
            gameMaster.ApplyStatus(GetMonster(), target, 0, 4, 0);

        gameMaster.ShootProjectile(GetMonster(), target, 1, 0);

        gameMaster.DeclaringDamage(GetMonster(), target, -attack2, destroyShields);
        yield return new WaitForSeconds(0.2f);
        target = gameMaster.GetRedirectedMonster(target);

        yield return new WaitForSeconds(0.25f);
        gameMaster.DamageMonster(GetMonster(), target, -attack2);


        if (GetMonster().GetPassiveID() == 1 && gameMaster.estimatedDamage < 0)
            gameMaster.ApplyStatus(GetMonster(), target, 0, 4, 0);

        yield return new WaitForSeconds(0.8f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator LightningRound()
    {
        gameMaster.AnimateMonster(GetMonster(), "ability1");
        
        yield return new WaitForSeconds(0.3f);
        
        monster randomTarget = gameMaster.GetRandomConductiveEnemy();

        if (randomTarget == null)
            randomTarget = gameMaster.GetRandomEnemy(-1, -1, false);

        int abilityMultiplier = (GetMonster().GetCurrentMagic() * GetCurrentMove(2).GetPercentageMultiplier()) + GetMoveDamage(2, 0);

        gameMaster.AttackAgain(GetTargetedMonster(), randomTarget, abilityMultiplier);

        gameMaster.waitingForAction = true;
        while(gameMaster.waitingForAction == true)
        {
            yield return new WaitForEndOfFrame();
        }

        FinishMove(true, false);
    }

    private IEnumerator EyeOfTheStorm()
    {
        gameMaster.AnimateMonster(GetMonster(), "ability2");
        yield return new WaitForSeconds(0.3f);
        int shieldMultiplier = (GetMonster().GetCurrentMagic() * GetCurrentMove(3).GetPercentageMultiplier()) + GetMoveDamage(3, 1);
        float shieldStrength = GetTargetedMonster().GetMaxHealth() * (1f * shieldMultiplier / 100f);
        int shield = Mathf.RoundToInt(shieldStrength);
        shield = GetMultiplierDamage(shield);

        float newBubbleBuffer = (shield * 1.1f) + 1.05f;
        shield = Mathf.RoundToInt(newBubbleBuffer);

        gameMaster.ApplyStatus(GetMonster(), GetTargetedMonster(), 1, shield, 0);
        gameMaster.ApplyStatus(GetMonster(), GetTargetedMonster(), 2, (GetMoveDamage(3, 0) + 1), 0); // I think however many turns +1 since NextTurn();

        yield return new WaitForSeconds(0.3f);

        FinishMove(true, false);
    }
}

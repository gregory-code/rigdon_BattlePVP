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

    private void AttackAgain(monster targetMon, int extraDamage)
    {
        if (GetMonster().GetOwnership() == false)
            return;

        UseAttack(GetMonster().GetAttackID(), GetMonster(), targetMon, false, extraDamage);
    }

    private void TookDamage(monster recivingMon, monster usingMon, int damage, bool died, bool crit, bool burnDamage)
    {
        if (burnDamage)
            return;

        if (GetMonster().GetPassiveID() == 2 && GetMonster().GetOwnership()) // index for cloud legend
        {
            gameMaster.ApplyStatus(GetMonster(), usingMon, 0, 2, 0); // index 0  for conductive status
        }
    }

    private void UseAttack(int attackID, monster user, monster target, bool consumeTurn, int extraDamage)
    {
        switch (attackID)
        {
            default:
                break;

            case 1:
                StartCoroutine(RingingThunder(user, target, consumeTurn, extraDamage));
                break;

            case 2:
                StartCoroutine(BoomSpear(user, target, consumeTurn, extraDamage));
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

    private IEnumerator RingingThunder(monster user, monster target, bool consumeTurn, int extraDamage)
    {
        int attack1 = user.GetCurrentStrength() + GetMoveDamage(0,0); // consider reducing by a % that would be hype
        attack1 += extraDamage;

        float inHalf = (attack1 / 2f);
        int attack2 = Mathf.RoundToInt(inHalf);
        int attack3 = Mathf.RoundToInt(inHalf);

        gameMaster.AnimateMonster(user, "attack1");

        yield return new WaitForSeconds(0.55f);
        gameMaster.ShootProjectile(user, target, 2, 0);

        bool didCrit = IsCrit(0);
        gameMaster.DeclaringDamage(user, target, -attack1, destroyShields, didCrit);
        yield return new WaitForSeconds(0.2f);
        target = gameMaster.GetRedirectedMonster(target);

        yield return new WaitForSeconds(0.40f);
        gameMaster.DamageMonster(user, target, -attack1, didCrit);

        if (GetMonster().GetPassiveID() == 1 && gameMaster.estimatedDamage < 0)
            gameMaster.ApplyStatus(user, target, 0, 1, 0);

        yield return new WaitForSeconds(0.1f);

        monster nextTarget = gameMaster.GetRandomEnemy(target.GetIndex(), -1, false);
        if(nextTarget != null)
        {
            didCrit = IsCrit(0);

            gameMaster.DeclaringDamage(user, nextTarget, -attack2, destroyShields, didCrit);
            yield return new WaitForSeconds(0.2f);
            nextTarget = gameMaster.GetRedirectedMonster(nextTarget);

            gameMaster.ShootProjectile(target, nextTarget, 2, 0);

            yield return new WaitForSeconds(0.1f);

            gameMaster.DamageMonster(user, nextTarget, -attack2, didCrit);

            if (GetMonster().GetPassiveID() == 1 && gameMaster.estimatedDamage < 0)
                gameMaster.ApplyStatus(user, nextTarget, 0, 1, 0);

            monster finalTarget = gameMaster.GetRandomEnemy(target.GetIndex(), nextTarget.GetIndex(), false);
            if (finalTarget != null)
            {
                didCrit = IsCrit(0);

                gameMaster.DeclaringDamage(user, finalTarget, -attack3, destroyShields, didCrit);
                yield return new WaitForSeconds(0.2f);
                finalTarget = gameMaster.GetRedirectedMonster(finalTarget);

                gameMaster.ShootProjectile(nextTarget, finalTarget, 2, 0);

                yield return new WaitForSeconds(0.1f);

                gameMaster.DamageMonster(user, finalTarget, -attack3, didCrit);

                if (GetMonster().GetPassiveID() == 1 && gameMaster.estimatedDamage < 0)
                    gameMaster.ApplyStatus(user, finalTarget, 0, 1, 0);
            }
        }

        yield return new WaitForSeconds(0.8f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator BoomSpear(monster user, monster target, bool consumeTurn, int extraDamage)
    {
        int attack1 = user.GetCurrentMagic() + GetMoveDamage(1,0);
        attack1 += extraDamage;

        int attack2 = user.GetCurrentStrength() + GetMoveDamage(1,1);
        attack2 += extraDamage;

        gameMaster.AnimateMonster(user, "attack2");

        yield return new WaitForSeconds(0.3f);
        gameMaster.ShootProjectile(user, target, 0, 0);

        bool didCrit = IsCrit(0);
        gameMaster.DeclaringDamage(user, target, -attack1, destroyShields, didCrit);
        yield return new WaitForSeconds(0.2f);
        target = gameMaster.GetRedirectedMonster(target);
        yield return new WaitForSeconds(0.3f);
        gameMaster.DamageMonster(user, target, -attack1, didCrit);

        yield return new WaitForSeconds(0.1f);

        if (GetMonster().GetPassiveID() == 1 && gameMaster.estimatedDamage < 0)
            gameMaster.ApplyStatus(user, target, 0, 1, 0);

        gameMaster.ShootProjectile(user, target, 1, 0);

        didCrit = IsCrit(0);
        gameMaster.DeclaringDamage(user, target, -attack2, destroyShields, didCrit);
        yield return new WaitForSeconds(0.2f);
        target = gameMaster.GetRedirectedMonster(target);

        yield return new WaitForSeconds(0.25f);
        gameMaster.DamageMonster(user, target, -attack2, didCrit);


        if (GetMonster().GetPassiveID() == 1 && gameMaster.estimatedDamage < 0)
            gameMaster.ApplyStatus(user, target, 0, 1, 0);

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

        int extraDamage = (GetMonster().GetCurrentMagic() + GetMoveDamage(2, 0));

        gameMaster.AttackAgain(GetTargetedMonster(), randomTarget, extraDamage);

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

        int shieldMultiplier = (GetMonster().GetCurrentMagic() * GetCurrentMove(3).GetPercentageMultiplier()) + GetMoveDamage(3, 0);
        float shieldStrength = GetTargetedMonster().GetMaxHealth() * (1f * shieldMultiplier / 100f);
        int shield = Mathf.RoundToInt(shieldStrength);
        
        float newBubbleBuffer = (shield * 1.1f) + 1.05f;
        if(GetMonster() == GetTargetedMonster()) 
            shield = Mathf.RoundToInt(newBubbleBuffer);

        int turnDuration = (GetMoveDamage(3, 1));
        if (GetMonster() == GetTargetedMonster())
            turnDuration++;

        gameMaster.ApplyStatus(GetMonster(), GetTargetedMonster(), 3, shield, 0);
        gameMaster.ApplyStatus(GetMonster(), GetTargetedMonster(), 4, turnDuration, 0);

        yield return new WaitForSeconds(0.3f);

        FinishMove(true, false);
    }
}

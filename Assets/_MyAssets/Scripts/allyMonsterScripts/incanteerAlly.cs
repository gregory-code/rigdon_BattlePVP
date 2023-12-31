using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class incanteerAlly : monsterAlly
{
    private void Start()
    {
        onAttack += UseAttack;
        onAbility += UseAbility;

        if(GetMonster().GetPassiveID() == 2)
        {
            foreach(monster ally in gameMaster.GetMonstersTeam(GetMonster()))
            {
                if(ally != GetMonster())
                {
                    ally.onAboutToAttack += AllyAttacked;
                }
            }
        }

        GetMonster().onAttackAgain += AttackAgain;
        GetMonster().onRemoveConnections += RemoveConnections;
    }

    private void AllyAttacked(monster allyAttacking, monster enemyGettingAttacked)
    {
        if (GetMonster().GetOwnership() == false)
            return;

        allyAttacking.HoldAttack();

        int attackMulti = GetMoveDamage(5, 0);

        while (attackMultiplier > 100)
        {
            attackMultiplier--;
            attackMulti++;
        }
        attackMultiplier = attackMulti;

        UseAttack(GetMonster().GetAttackID(), enemyGettingAttacked, false);
    }

    private void RemoveConnections()
    {
        onAttack -= UseAttack;
        onAbility -= UseAbility;

        if (GetMonster().GetPassiveID() == 2)
        {
            foreach (monster ally in gameMaster.GetMonstersTeam(GetMonster()))
            {
                if (ally != GetMonster())
                {
                    ally.onAboutToAttack -= AllyAttacked;
                }
            }
        }

        GetMonster().onAttackAgain -= AttackAgain;
        GetMonster().onRemoveConnections -= RemoveConnections;
    }

    private void AttackAgain(monster targetMon, int percentageMultiplier)
    {
        if (GetMonster().GetOwnership() == false)
            return;

        while (attackMultiplier > 100)
        {
            attackMultiplier--;
            percentageMultiplier++;
        }
        attackMultiplier = percentageMultiplier;
        UseAttack(GetMonster().GetAttackID(), targetMon, false);
    }

    private void UseAttack(int attackID, monster target, bool consumeTurn)
    {
        switch (attackID)
        {
            default:
                break;

            case 1:
                StartCoroutine(GoldenHorn(target, consumeTurn));
                break;

            case 2:
                StartCoroutine(FireRush(target, consumeTurn));
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
                if (GetTargetedMonster().GetIndex() == GetMonster().GetIndex())
                {
                    gameMaster.SetFilter(false);
                    GetMonster().SetAct(true);
                    return;
                }
                StartCoroutine(LeadTheCharge());
                break;

            case 2:
                if (GetTargetedMonster().GetIndex() == GetMonster().GetIndex())
                {
                    gameMaster.SetFilter(false);
                    GetMonster().SetAct(true);
                    return;
                }
                StartCoroutine(BrambleCrown());
                break;
        }
    }

    private int goldenHornDamage = 0;

    private IEnumerator GoldenHorn(monster target, bool consumeTurn)
    {
        if (holdAttack)
        {
            holdAttack = false;
            yield return new WaitForSeconds(1f);
        }

        gameMaster.AnimateMonster(GetMonster(), "attack1");
        gameMaster.MoveMonster(GetMonster(), target, 0);

        int attack1 = GetMonster().GetCurrentStrength() + GetMoveDamage(0, 0);
        attack1 += goldenHornDamage;
        attack1 = GetMultiplierDamage(attack1);

        gameMaster.ShootProjectile(GetMonster(), target, 11, 0);
        yield return new WaitForSeconds(0.6f);
        yield return new WaitForSeconds(0.4f);
        gameMaster.DeclaringDamage(GetMonster(), target, -attack1, destroyShields);
        yield return new WaitForSeconds(0.3f);
        target = gameMaster.GetRedirectedMonster(target);
        gameMaster.DamageMonster(GetMonster(), target, -attack1);


        yield return new WaitForSeconds(0.55f);
        gameMaster.MoveMonster(GetMonster(), target, 1);

        yield return new WaitForSeconds(2.2f);
        int extraDamage = GetMonster().GetCurrentSpeed() + GetMoveDamage(0, 1);
        gameMaster.ApplyStatus(GetMonster(), GetMonster(), 14, extraDamage, 0);
        goldenHornDamage += extraDamage;

        FinishMove(consumeTurn, true);
    }

    private IEnumerator FireRush(monster target, bool consumeTurn)
    {
        if (holdAttack)
        {
            holdAttack = false;
            yield return new WaitForSeconds(1f);
        }

        gameMaster.AnimateMonster(GetMonster(), "attack1");
        gameMaster.MoveMonster(GetMonster(), target, 0);

        int attack1 = (GetMonster().GetCurrentStrength() * GetCurrentMove(1).GetPercentageMultiplier()) + GetMoveDamage(1, 0);
        int enemyCurrentHealth = target.GetCurrentHealth();

        float percentageEnemyHealth = enemyCurrentHealth * (1f * attack1 / 100f);
        attack1 = Mathf.RoundToInt(percentageEnemyHealth);

        attack1 = GetMultiplierDamage(attack1);

        gameMaster.ShootProjectile(GetMonster(), target, 12, 0);
        yield return new WaitForSeconds(0.6f);
        yield return new WaitForSeconds(0.4f);
        gameMaster.DeclaringDamage(GetMonster(), target, -attack1, destroyShields);
        yield return new WaitForSeconds(0.3f);
        target = gameMaster.GetRedirectedMonster(target);
        gameMaster.DamageMonster(GetMonster(), target, -attack1);

        gameMaster.ApplyStatus(GetMonster(), target, 7, 1, 0);

        yield return new WaitForSeconds(0.55f);
        gameMaster.MoveMonster(GetMonster(), target, 1);

        yield return new WaitForSeconds(1.6f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator LeadTheCharge()
    {
        gameMaster.AnimateMonster(GetMonster(), "ability1");

        gameMaster.MoveMonster(GetMonster(), GetMonster(), 5);

        yield return new WaitForSeconds(0.5f);

        int buff = GetMonster().GetCurrentMagic() + GetMoveDamage(2, 0);

        gameMaster.ApplyStatus(GetMonster(), GetTargetedMonster(), 12, 1, buff);

        if (GetMonster().GetPassiveID() == 1)
            gameMaster.AdjustTurnOrder(GetTargetedMonster(), true, false);

        yield return new WaitForSeconds(1.6f);

        gameMaster.MoveMonster(GetMonster(), GetMonster(), 1);

        FinishMove(true, false);
    }

    private IEnumerator BrambleCrown()
    {
        gameMaster.AnimateMonster(GetMonster(), "ability2");

        yield return new WaitForSeconds(0.4f);

        if (GetMonster().GetPassiveID() == 1)
            gameMaster.AdjustTurnOrder(GetTargetedMonster(), true, false);

        FinishMove(true, false);
    }
}

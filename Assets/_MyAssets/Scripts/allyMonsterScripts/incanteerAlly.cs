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

        float attackMulti = GetMoveDamage(5, 0);
        int attackID = GetMonster().GetAttackID();
        int attackDamage = GetMonster().GetCurrentStrength() + GetMoveDamage((attackID - 1), 0);
        float damageMultiplied = (attackDamage * 1f) * (attackMulti / 100);
        int extraDamage = Mathf.RoundToInt(damageMultiplied);

        UseAttack(GetMonster().GetAttackID(), GetMonster(), enemyGettingAttacked, false, extraDamage);
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

    private void AttackAgain(monster targetMon, int extraDamage)
    {
        if (GetMonster().GetOwnership() == false)
            return;

        UseAttack(GetMonster().GetAttackID(), GetMonster(), targetMon, false, extraDamage);
    }

    private void UseAttack(int attackID, monster user, monster target, bool consumeTurn, int extraDamage)
    {
        switch (attackID)
        {
            default:
                break;

            case 1:
                StartCoroutine(GoldenHorn(user, target, consumeTurn, extraDamage));
                break;

            case 2:
                StartCoroutine(FireRush(user, target, consumeTurn, extraDamage));
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
                StartCoroutine(LeadTheCharge());
                break;

            case 2:
                StartCoroutine(BrambleCrown());
                break;
        }
    }

    private int goldenHornDamage = 0;

    private IEnumerator GoldenHorn(monster user, monster target, bool consumeTurn, int extraDamage)
    {
        if (holdAttack)
        {
            holdAttack = false;
            yield return new WaitForSeconds(1f);
        }

        gameMaster.AnimateMonster(user, "attack1");
        gameMaster.MoveMonster(user, target, 0);

        int attack1 = user.GetCurrentStrength() + GetMoveDamage(0, 0);
        attack1 += goldenHornDamage;
        attack1 += extraDamage;

        gameMaster.ShootProjectile(user, target, 6, 0);
        yield return new WaitForSeconds(0.6f);
        yield return new WaitForSeconds(0.4f);
        bool didCrit = IsCrit(0);
        gameMaster.DeclaringDamage(user, target, -attack1, destroyShields, didCrit);
        yield return new WaitForSeconds(0.3f);
        target = gameMaster.GetRedirectedMonster(target);
        gameMaster.DamageMonster(user, target, -attack1, didCrit);


        yield return new WaitForSeconds(0.55f);
        gameMaster.MoveMonster(user, target, 1);

        yield return new WaitForSeconds(2.2f);
        int bonusDamage = user.GetCurrentSpeed() + GetMoveDamage(0, 1);
        gameMaster.ApplyStatus(user, user, 7, bonusDamage, 0);
        goldenHornDamage += bonusDamage;

        FinishMove(consumeTurn, true);
    }

    private IEnumerator FireRush(monster user, monster target, bool consumeTurn, int extraDamage)
    {
        if (holdAttack)
        {
            holdAttack = false;
            yield return new WaitForSeconds(1f);
        }

        gameMaster.AnimateMonster(user, "attack1");
        gameMaster.MoveMonster(user, target, 0);

        int attack1 = user.GetCurrentStrength() + GetMoveDamage(1, 0);
        attack1 += extraDamage;

        gameMaster.ShootProjectile(user, target, 7, 0);
        yield return new WaitForSeconds(0.6f);
        yield return new WaitForSeconds(0.4f);
        bool didCrit = IsCrit(GetMonster().GetCurrentSpeed() * 2);
        gameMaster.DeclaringDamage(user, target, -attack1, destroyShields, didCrit);
        yield return new WaitForSeconds(0.3f);
        target = gameMaster.GetRedirectedMonster(target);
        gameMaster.DamageMonster(user, target, -attack1, didCrit);

        gameMaster.ApplyStatus(user, target, 1, 1, 0);

        yield return new WaitForSeconds(0.55f);
        gameMaster.MoveMonster(user, target, 1);

        yield return new WaitForSeconds(1.6f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator LeadTheCharge()
    {
        gameMaster.AnimateMonster(GetMonster(), "ability1");

        gameMaster.MoveMonster(GetMonster(), GetMonster(), 5);

        yield return new WaitForSeconds(0.5f);

        int weakness = GetMonster().GetCurrentMagic() + GetMoveDamage(2, 0);

        gameMaster.ApplyStatus(GetMonster(), GetTargetedMonster(), 8, 200, weakness);

        if (GetMonster().GetPassiveID() == 1 && GetTargetedMonster().GetIndex() != GetMonster().GetIndex())
            gameMaster.AdjustTurnOrder(GetTargetedMonster(), true, false);

        yield return new WaitForSeconds(1.6f);

        gameMaster.MoveMonster(GetMonster(), GetMonster(), 1);

        FinishMove(true, false);
    }

    private IEnumerator BrambleCrown()
    {
        gameMaster.AnimateMonster(GetMonster(), "ability2");
        gameMaster.MoveMonster(GetMonster(), GetTargetedMonster(), 0);

        yield return new WaitForSeconds(.8f);

        gameMaster.ShootProjectile(GetMonster(), GetTargetedMonster(), 8, 0);

        yield return new WaitForSeconds(0.2f);

        int counter = GetMoveDamage(3, 2);
        int power = GetMonster().GetCurrentMagic() + GetMoveDamage(3, 1);
        if (GetMonster() == GetTargetedMonster())
            counter++;

        gameMaster.ApplyStatus(GetMonster(), GetTargetedMonster(), 6, counter, power);

        if (GetMonster().GetPassiveID() == 1 && GetTargetedMonster().GetIndex() != GetMonster().GetIndex())
            gameMaster.AdjustTurnOrder(GetTargetedMonster(), true, false);

        yield return new WaitForSeconds(0.5f);

        gameMaster.MoveMonster(GetMonster(), GetTargetedMonster(), 1);

        FinishMove(true, false);
    }
}

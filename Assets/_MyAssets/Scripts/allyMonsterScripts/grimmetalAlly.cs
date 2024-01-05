using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class grimmetalAlly : monsterAlly
{
    private void Start()
    {
        onAttack += UseAttack;
        onAbility += UseAbility;
        gameMaster.onMonsterDied += MonsterDied;

        if (GetMonster().GetPassiveID() == 2)
            gameMaster.bRegularDeath = false;

        GetMonster().onAttackAgain += AttackAgain;
        GetMonster().onRemoveConnections += RemoveConnections;

        if (GetMonster().GetPassiveID() == 1)
            GetMonster().SetDamageCap(GetMoveDamage(4, 0) + GetMonster().GetCurrentMagic());
    }

    private void RemoveConnections()
    {
        onAttack -= UseAttack;
        onAbility -= UseAbility;
        gameMaster.onMonsterDied -= MonsterDied;
        GetMonster().onAttackAgain -= AttackAgain;
        GetMonster().onRemoveConnections -= RemoveConnections;
    }

    private void AttackAgain(monster targetMon, int extraDamage)
    {
        if (GetMonster().GetOwnership() == false)
            return;

        UseAttack(GetMonster().GetAttackID(), GetMonster(), targetMon, false, extraDamage);
    }

    private void MonsterDied(monster whoDied)
    {
        if(GetMonster().GetPassiveID() == 2)
        {
            if(GetMonster().GetCurrentHealth() <= 0)
            {
                return;
            }

            GetMonster().PlayAnimation("idle");
            GetMonster().GetExpHold(2).GainExp();
        }
    }

    private void UseAttack(int attackID, monster user, monster target, bool consumeTurn, int extraDamage)
    {
        switch (attackID)
        {
            default:
                break;

            case 1:
                StartCoroutine(Cleave(user, target, consumeTurn, extraDamage));
                break;

            case 2:
                StartCoroutine(Duel(user, target, consumeTurn, extraDamage));
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
                StartCoroutine(SteelYourself());
                break;

            case 2:
                StartCoroutine(MirrorSword());
                break;
        }
    }

    private IEnumerator Cleave(monster user, monster target, bool consumeTurn, int extraDamage)
    {
        gameMaster.AnimateMonster(user, "attack1");
        gameMaster.MoveMonster(user, target, 0);

        int attack1 = user.GetCurrentStrength() + GetMoveDamage(0, 0);
        int bonusDamage = user.GetCurrentStrength() + GetMoveDamage(0, 1);
        float attackEffect = bonusDamage * (1f * user.GetCurrentHealth() / user.GetMaxHealth() * 1f);
        attack1 += Mathf.RoundToInt(attackEffect);
        attack1 += extraDamage;

        yield return new WaitForSeconds(1f);
        bool didCrit = IsCrit(0);
        gameMaster.DeclaringDamage(user, target, -attack1, destroyShields, didCrit);
        yield return new WaitForSeconds(0.2f);
        target = gameMaster.GetRedirectedMonster(target);

        gameMaster.ShootProjectile(user, target, 10, 0);
        yield return new WaitForSeconds(0.1f);
        gameMaster.DamageMonster(user, target, -attack1, didCrit);

        yield return new WaitForSeconds(0.4f);
        gameMaster.MoveMonster(user, target, 1);
        yield return new WaitForSeconds(0.4f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator Duel(monster user, monster target, bool consumeTurn, int extraDamage)
    {
        gameMaster.AnimateMonster(user, "attack2");

        yield return new WaitForSeconds(0.5f);

        monster[] enemyTeam = gameMaster.GetMonstersTeam(target);
        monster[] myTeam = gameMaster.GetMonstersTeam(user);

        gameMaster.MoveMonster(user, myTeam[1], 0);
        gameMaster.MoveMonster(target, enemyTeam[1], 0);

        gameMaster.ApplyStatus(user, user, 10, 2, 0);
        gameMaster.ApplyStatus(user, target, 10, 2, 0);

        gameMaster.AdjustTurnOrder(target, true, false);

        for (int i = 0; i < 3; i++)
        {
            if (enemyTeam[i].GetCurrentHealth() > 0)
            {
                if (enemyTeam[i] != target)
                {
                    gameMaster.MoveMonster(enemyTeam[i], enemyTeam[i], 3);
                }
            }

            if (myTeam[i].GetCurrentHealth() > 0)
            {
                if (myTeam[i] != user)
                {
                    gameMaster.MoveMonster(myTeam[i], myTeam[i], 3);
                }
            }
        }

        int attack1 = user.GetCurrentStrength() + GetMoveDamage(1, 0);
        attack1 += extraDamage;

        yield return new WaitForSeconds(0.3f);

        bool didCrit = IsCrit(0);
        gameMaster.DeclaringDamage(user, target, -attack1, destroyShields, didCrit);
        yield return new WaitForSeconds(0.2f);
        target = gameMaster.GetRedirectedMonster(target);

        gameMaster.ShootProjectile(user, target, 10, 0);
        gameMaster.DamageMonster(user, target, -attack1, didCrit);

        yield return new WaitForSeconds(0.6f);
        if(GetTargetedMonster().isDead())
        {
            gameMaster.TryRemoveStatus(user, 10);
        }
        yield return new WaitForSeconds(0.4f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator SteelYourself()
    {
        gameMaster.AnimateMonster(GetMonster(), "ability1");

        yield return new WaitForSeconds(0.5f);

        int attackMultiplier = (GetMonster().GetCurrentMagic() * GetCurrentMove(2).GetPercentageMultiplier()) + GetMoveDamage(2, 0);
        gameMaster.ApplyStatus(GetMonster(), GetTargetedMonster(), 9, 200, attackMultiplier);

        yield return new WaitForSeconds(0.5f);

        FinishMove(true, false);
    }

    private IEnumerator MirrorSword()
    {
        gameMaster.AnimateMonster(GetMonster(), "ability2");
        yield return new WaitForSeconds(0.4f);


        int turnDuration = GetMoveDamage(3, 1);
        if (GetMonster() == GetTargetedMonster())
            turnDuration++;

        int strengthPower = GetMoveDamage(3, 0) + GetMonster().GetCurrentMagic();
        gameMaster.ApplyStatus(GetMonster(), GetTargetedMonster(), 9, turnDuration, strengthPower); // apply strength

        gameMaster.ApplyStatus(GetMonster(), GetTargetedMonster(), 5, turnDuration, 0);


        yield return new WaitForSeconds(0.4f);

        FinishMove(true, false);
    }
}

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
        GetMonster().onTakeDamage += TookDamage;
        GetMonster().onRemoveConnections += RemoveConnections;

        if (GetMonster().GetPassiveID() == 1)
            GetMonster().SetDamageCap(GetMoveDamage(4, 0));
    }

    private void RemoveConnections()
    {
        onAttack -= UseAttack;
        onAbility -= UseAbility;
        gameMaster.onMonsterDied -= MonsterDied;
        GetMonster().onAttackAgain -= AttackAgain;
        GetMonster().onTakeDamage -= TookDamage;
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

    private void TookDamage(monster recivingMon, monster usingMon, int damage, bool died, bool burnDamage)
    {

    }

    private void UseAttack(int attackID, monster target, bool consumeTurn)
    {
        switch (attackID)
        {
            default:
                break;

            case 1:
                StartCoroutine(Cleave(target, consumeTurn));
                break;

            case 2:
                StartCoroutine(Duel(target, consumeTurn));
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
                StartCoroutine(MirrorArmor());
                break;
        }
    }

    private IEnumerator Cleave(monster target, bool consumeTurn)
    {
        if (holdAttack)
        {
            holdAttack = false;
            yield return new WaitForSeconds(1f);
        }

        gameMaster.AnimateMonster(GetMonster(), "attack1");
        gameMaster.MoveMonster(GetMonster(), target, 0);

        int attack1 = GetMonster().GetCurrentStrength() + GetMoveDamage(0, 0);
        int extraDamage = GetMonster().GetCurrentStrength() + GetMoveDamage(0, 1);
        float attackEffect = extraDamage * (1f * GetMonster().GetCurrentHealth() / GetMonster().GetMaxHealth() * 1f);
        attack1 += Mathf.RoundToInt(attackEffect);
        attack1 = GetMultiplierDamage(attack1);


        yield return new WaitForSeconds(1f);
        gameMaster.DeclaringDamage(GetMonster(), target, -attack1, destroyShields);
        yield return new WaitForSeconds(0.2f);
        target = gameMaster.GetRedirectedMonster(target);

        gameMaster.ShootProjectile(GetMonster(), target, 10, 0);
        yield return new WaitForSeconds(0.1f);
        gameMaster.DamageMonster(GetMonster(), target, -attack1);

        yield return new WaitForSeconds(0.4f);
        gameMaster.MoveMonster(GetMonster(), target, 1);
        yield return new WaitForSeconds(0.4f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator Duel(monster target, bool consumeTurn)
    {
        if (holdAttack)
        {
            holdAttack = false;
            yield return new WaitForSeconds(1f);
        }

        gameMaster.AnimateMonster(GetMonster(), "attack2");

        yield return new WaitForSeconds(0.5f);

        monster[] enemyTeam = gameMaster.GetMonstersTeam(GetTargetedMonster());
        monster[] myTeam = gameMaster.GetMonstersTeam(GetMonster());

        gameMaster.MoveMonster(GetMonster(), myTeam[1], 0);
        gameMaster.MoveMonster(target, enemyTeam[1], 0);

        gameMaster.ApplyStatus(GetMonster(), GetMonster(), 10, 2, 0);
        gameMaster.ApplyStatus(GetMonster(), target, 10, 2, 0);

        gameMaster.AdjustTurnOrder(target, true, false);

        for (int i = 0; i < 3; i++)
        {
            if (enemyTeam[i].GetCurrentHealth() > 0)
            {
                if (enemyTeam[i] != GetTargetedMonster())
                {
                    gameMaster.MoveMonster(enemyTeam[i], enemyTeam[i], 3);
                }
            }

            if (myTeam[i].GetCurrentHealth() > 0)
            {
                if (myTeam[i] != GetMonster())
                {
                    gameMaster.MoveMonster(myTeam[i], myTeam[i], 3);
                }
            }
        }

        int attack1 = GetMonster().GetCurrentStrength() + GetMoveDamage(1, 0);
        attack1 = GetMultiplierDamage(attack1);

        yield return new WaitForSeconds(0.3f);

        gameMaster.DeclaringDamage(GetMonster(), target, -attack1, destroyShields);
        yield return new WaitForSeconds(0.2f);
        target = gameMaster.GetRedirectedMonster(target);

        gameMaster.ShootProjectile(GetMonster(), target, 10, 0);
        gameMaster.DamageMonster(GetMonster(), target, -attack1);

        yield return new WaitForSeconds(0.6f);
        if(GetTargetedMonster().isDead())
        {
            gameMaster.TryRemoveStatus(GetMonster(), 10);
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

    private IEnumerator MirrorArmor()
    {
        gameMaster.AnimateMonster(GetMonster(), "ability2");
        yield return new WaitForSeconds(0.4f);

        int allyAmount = GetMoveDamage(3, 0);
        gameMaster.ApplyStatus(GetMonster(), GetTargetedMonster(), 11, 200, 0);

        if(allyAmount >= 2)
        {
            monster targetAlly = gameMaster.GetRandomEnemy(-1, GetTargetedMonster().GetIndex(), true);
            if(targetAlly != null)
                gameMaster.ApplyStatus(GetMonster(), targetAlly, 11, 200, 0);

            if (allyAmount >= 3)
            {
                monster finalAlly = gameMaster.GetRandomEnemy(targetAlly.GetIndex(), GetTargetedMonster().GetIndex(), true);
                if (finalAlly != null)
                    gameMaster.ApplyStatus(GetMonster(), finalAlly, 11, 200, 0);
            }
        }

        yield return new WaitForSeconds(0.4f);

        FinishMove(true, false);
    }
}

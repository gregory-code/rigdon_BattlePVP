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

        if (GetMyMonster().GetPassiveID() == 2)
            gameMaster.bRegularDeath = false;

        GetMyMonster().onAttackAgain += AttackAgain;
        GetMyMonster().onTakeDamage += TookDamage;
        GetMyMonster().onRemoveConnections += RemoveConnections;

        if (GetMyMonster().GetPassiveID() == 1)
            GetMyMonster().SetDamageCap(GetMoveDamage(4, 0));
    }

    private void RemoveConnections()
    {
        onAttack -= UseAttack;
        onAbility -= UseAbility;
        gameMaster.onMonsterDied -= MonsterDied;
        GetMyMonster().onAttackAgain -= AttackAgain;
        GetMyMonster().onTakeDamage -= TookDamage;
        GetMyMonster().onRemoveConnections -= RemoveConnections;
    }

    private void AttackAgain(int percentageMultiplier, bool bMine2, int TargetOfTargetIndex)
    {
        if (GetMyMonster().bMine == false)
            return;

        while (attackMultiplier > 100)
        {
            attackMultiplier--;
            percentageMultiplier++;
        }
        attackMultiplier = percentageMultiplier;
        UseAttack(GetMyMonster().GetAttackID(), TargetOfTargetIndex, false);
    }

    private void MonsterDied(monster whoDied)
    {
        if(GetMyMonster().GetPassiveID() == 2)
        {
            if(GetMyMonster().GetCurrentHealth() <= 0)
            {
                return;
            }

            GetMyMonster().PlayAnimation("idle");
            GetMyMonster().GetExpHold(2).GainExp();
        }
    }

    private void TookDamage(int change, bool died, bool bMine, int userIndex, monster recivingMon, bool burnDamage)
    {

    }

    private void UseAttack(int attackID, int targetIndex, bool consumeTurn)
    {
        switch (attackID)
        {
            default:
                break;

            case 1:
                StartCoroutine(Cleave(targetIndex, consumeTurn));
                break;

            case 2:
                StartCoroutine(Duel(targetIndex, consumeTurn));
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
                StartCoroutine(AlmostThere());
                break;
        }
    }

    private IEnumerator Cleave(int targetIndex, bool consumeTurn)
    {
        gameMaster.AnimateMonster(isMine(), GetMyMonster().teamIndex, "attack1");
        gameMaster.MoveMonster(isMine(), GetMyMonster().teamIndex, false, !isMine(), targetIndex, false, 0);

        int attack1 = GetMyMonster().GetCurrentStrength() + GetMoveDamage(0, 0);
        int extraDamage = GetMyMonster().GetCurrentStrength() + GetMoveDamage(0, 1);
        float attackEffect = extraDamage * (1f * GetMyMonster().GetCurrentHealth() / GetMyMonster().GetMaxHealth() * 1f);
        attack1 += Mathf.RoundToInt(attackEffect);
        attack1 = GetMultiplierDamage(attack1);


        yield return new WaitForSeconds(1f);
        gameMaster.DeclaringDamage(isMine(), GetMyMonster().teamIndex, !isMine(), targetIndex, -attack1, destroyShields);
        yield return new WaitForSeconds(0.2f);
        targetIndex = gameMaster.GetRedirectedIndex(targetIndex);

        gameMaster.ShootProjectile(isMine(), GetMyMonster().teamIndex, 10, !isMine(), targetIndex, false, 0);
        yield return new WaitForSeconds(0.1f);
        gameMaster.ChangeMonsterHealth(isMine(), GetMyMonster().teamIndex, !isMine(), targetIndex, -attack1, true);

        yield return new WaitForSeconds(0.4f);
        gameMaster.MoveMonster(isMine(), GetMyMonster().teamIndex, true, isMine(), GetMyMonster().teamIndex, false, 0);
        yield return new WaitForSeconds(0.4f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator Duel(int targetIndex, bool consumeTurn)
    {
        gameMaster.AnimateMonster(isMine(), GetMyMonster().teamIndex, "attack2");

        yield return new WaitForSeconds(0.5f);

        monster[] enemyTeam = gameMaster.GetMonstersTeam(GetTargetedMonster());
        monster[] myTeam = gameMaster.GetMonstersTeam(GetMyMonster());

        gameMaster.MoveMonster(isMine(), GetMyMonster().teamIndex, false, isMine(), myTeam[1].teamIndex, false, 0);
        gameMaster.MoveMonster(!isMine(), targetIndex, false, !isMine(), enemyTeam[1].teamIndex, false, 0);

        gameMaster.ApplyStatus(isMine(), GetMyMonster().teamIndex, 10, !isMine(), targetIndex, 2, 0);
        gameMaster.ApplyStatus(isMine(), GetMyMonster().teamIndex, 10, isMine(), GetMyMonster().teamIndex, 2, 0);

        gameMaster.AdjustTurnOrder(!isMine(), targetIndex, true, false);

        for (int i = 0; i < 3; i++)
        {
            if (enemyTeam[i].GetCurrentHealth() > 0)
            {
                if (enemyTeam[i] != GetTargetedMonster())
                {
                    gameMaster.MoveMonster(!isMine(), enemyTeam[i].teamIndex, false, !isMine(), enemyTeam[i].teamIndex, true, 1);
                }
            }

            if (myTeam[i].GetCurrentHealth() > 0)
            {
                if (myTeam[i] != GetMyMonster())
                {
                    gameMaster.MoveMonster(isMine(), myTeam[i].teamIndex, false, isMine(), myTeam[i].teamIndex, true, 1);
                }
            }
        }

        int attack1 = GetMyMonster().GetCurrentStrength() + GetMoveDamage(1, 0);
        attack1 = GetMultiplierDamage(attack1);

        yield return new WaitForSeconds(0.3f);

        gameMaster.DeclaringDamage(isMine(), GetMyMonster().teamIndex, !isMine(), targetIndex, -attack1, destroyShields);
        yield return new WaitForSeconds(0.2f);
        targetIndex = gameMaster.GetRedirectedIndex(targetIndex);

        gameMaster.ShootProjectile(isMine(), GetMyMonster().teamIndex, 10, !isMine(), targetIndex, false, 0);
        gameMaster.ChangeMonsterHealth(isMine(), GetMyMonster().teamIndex, !isMine(), targetIndex, -attack1, true);

        yield return new WaitForSeconds(0.6f);
        if(GetTargetedMonster().GetCurrentHealth() <= 0)
        {
            gameMaster.RemoveStatus(isMine(), GetMyMonster().teamIndex, 10, isMine(), GetMyMonster().teamIndex);
        }
        yield return new WaitForSeconds(0.4f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator SteelYourself()
    {
        gameMaster.AnimateMonster(isMine(), GetMyMonster().teamIndex, "ability1");

        yield return new WaitForSeconds(0.5f);

        int attackMultiplier = (GetMyMonster().GetCurrentMagic() * GetCurrentMove(2).GetPercentageMultiplier()) + GetMoveDamage(2, 0);
        gameMaster.ApplyStatus(isMine(), GetMyMonster().teamIndex, 9, isMine(), GetTargetedMonster().teamIndex, 200, attackMultiplier);

        yield return new WaitForSeconds(0.5f);

        FinishMove(true, false);
    }

    private IEnumerator AlmostThere()
    {
        gameMaster.AnimateMonster(isMine(), GetMyMonster().teamIndex, "ability2");
        yield return new WaitForSeconds(0.3f);

        yield return new WaitForSeconds(0.3f);

        FinishMove(true, false);
    }
}

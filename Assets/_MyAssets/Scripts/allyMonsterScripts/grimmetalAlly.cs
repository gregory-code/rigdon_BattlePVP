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
            if (whoDied = GetMyMonster())
                return;

            GetMyMonster().GetExpHold(2).GainExp();
        }
    }

    private void TookDamage(int change, bool died, bool bMine, int userIndex, monster recivingMon)
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
                StartCoroutine(NotYet(targetIndex, consumeTurn));
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
        gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, "attack1");
        gameMaster.MoveMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, false, !gameMaster.IsItMyTurn(), targetIndex);

        int attack1 = GetMyMonster().GetCurrentStrength() + GetMoveDamage(0, 0);
        int extraDamage = GetMyMonster().GetCurrentStrength() + GetMoveDamage(0, 1);
        float attackEffect = extraDamage * (1f * GetMyMonster().GetCurrentHealth() / GetMyMonster().GetMaxHealth() * 1f);
        attack1 += Mathf.RoundToInt(attackEffect);
        attack1 = GetMultiplierDamage(attack1);


        yield return new WaitForSeconds(1f);
        gameMaster.DeclaringDamage(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), targetIndex, -attack1, destroyShields);
        yield return new WaitForSeconds(0.2f);
        targetIndex = gameMaster.GetRedirectedIndex(targetIndex);

        gameMaster.ShootProjectile(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 10, !gameMaster.IsItMyTurn(), targetIndex, false, 0);
        yield return new WaitForSeconds(0.1f);
        gameMaster.ChangeMonsterHealth(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), targetIndex, -attack1, true);

        yield return new WaitForSeconds(0.4f);
        gameMaster.MoveMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, true, gameMaster.IsItMyTurn(), GetMyMonster().teamIndex);
        yield return new WaitForSeconds(0.4f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator NotYet(int targetIndex, bool consumeTurn)
    {
        gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, "attack2");

        yield return new WaitForSeconds(0.8f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator SteelYourself()
    {
        gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, "ability1");

        yield return new WaitForSeconds(0.5f);

        int attackMultiplier = (GetMyMonster().GetCurrentMagic() * GetCurrentMove(2).GetPercentageMultiplier()) + GetMoveDamage(2, 0);
        gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 9, gameMaster.IsItMyTurn(), GetTargetedMonster().teamIndex, 200, attackMultiplier);

        yield return new WaitForSeconds(0.5f);

        FinishMove(true, false);
    }

    private IEnumerator AlmostThere()
    {
        gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, "ability2");
        yield return new WaitForSeconds(0.3f);

        yield return new WaitForSeconds(0.3f);

        FinishMove(true, false);
    }
}

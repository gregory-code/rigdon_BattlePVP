using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static monsterAlly;

public class lusseliaAlly : monsterAlly
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
        if (GetMyMonster().GetPassiveID() == 2) // index for cloud legend
        {

        }
    }

    private void UseAttack(int attackID, int targetIndex, bool consumeTurn)
    {
        switch (attackID)
        {
            default:
                break;

            case 1:
                StartCoroutine(Starfall(targetIndex, consumeTurn));
                break;

            case 2:
                StartCoroutine(SolarWave(targetIndex, consumeTurn));
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
                StartCoroutine(LunarBlanket());
                break;

            case 2:
                StartCoroutine(Moonlight());
                break;
        }
    }

    private void FinishMove(bool consumeTurn, bool isAttack)
    {
        attackMultiplier = 100;
        gameMaster.waitingForAction = false;

        gameMaster.UsedAction(true, GetMyMonster().teamIndex, isAttack);
        
        if (consumeTurn == true)
            gameMaster.NextTurn();
        
    }

    private IEnumerator Starfall(int targetIndex, bool consumeTurn)
    {
        gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, "attack1");

        yield return new WaitForSeconds(0.2f);

        int attack1 = GetMyMonster().GetCurrentStrength() + GetMoveDamage(0, 0);
        attack1 = GetMultiplierDamage(attack1);

        gameMaster.ShootProjectile(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 7, !gameMaster.IsItMyTurn(), targetIndex, true, 0);

        gameMaster.DeclaringDamage(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), targetIndex, -attack1);
        yield return new WaitForSeconds(0.2f);
        targetIndex = gameMaster.GetRedirectedIndex(targetIndex);
        yield return new WaitForSeconds(0.25f);
        gameMaster.ChangeMonsterHealth(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), targetIndex, -attack1, true); // - for damage

        int shouldAddBurnDamage = 0;
        if (GetMyMonster().GetPassiveID() == 2)
            shouldAddBurnDamage = 1;

        if (gameMaster.estimatedDamage < 0)
            gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 7, !gameMaster.IsItMyTurn(), targetIndex, GetMoveDamage(0, 1), shouldAddBurnDamage);

        yield return new WaitForSeconds(0.8f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator SolarWave(int targetIndex, bool consumeTurn)
    {
        gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, "attack2");

        yield return new WaitForSeconds(0.3f);

        int attack1 = GetMyMonster().GetCurrentStrength() + GetMoveDamage(1, 0);
        attack1 = GetMultiplierDamage(attack1);

        monster[] enemyTeam = gameMaster.GetMonstersTeam(GetTargetedMonster());
        for (int i = 0; i < 3; i++)
        {
            if (enemyTeam[i].GetCurrentHealth() > 0)
            {
                targetIndex = i;
                gameMaster.ShootProjectile(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 9, !gameMaster.IsItMyTurn(), targetIndex, true, 1);
                gameMaster.DeclaringDamage(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), targetIndex, -attack1);
                yield return new WaitForSeconds(1.2f);
                gameMaster.ChangeMonsterHealth(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, !gameMaster.IsItMyTurn(), targetIndex, -attack1, true); // - for damage

                int shouldAddBurnDamage = 0;
                if (GetMyMonster().GetPassiveID() == 2)
                    shouldAddBurnDamage = 1;

                if (gameMaster.estimatedDamage < 0)
                    gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 7, !gameMaster.IsItMyTurn(), targetIndex, 1, shouldAddBurnDamage);
            }
        }

        yield return new WaitForSeconds(0.8f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator LunarBlanket()
    {
        gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, "ability1");

        yield return new WaitForSeconds(0.3f);
        int shieldStrength = (GetMyMonster().GetCurrentMagic() + GetMoveDamage(2, 0));
        shieldStrength = GetMultiplierDamage(shieldStrength);

        float newBubbleBuffer = (shieldStrength * 1.1f) + 1.05f;
        shieldStrength = Mathf.RoundToInt(newBubbleBuffer);

        monster[] myteam = gameMaster.GetMonstersTeam(GetMyMonster());
        for (int i = 0; i < 3; i++)
        {
            if (myteam[i].GetCurrentHealth() > 0)
            {
                gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 1, gameMaster.IsItMyTurn(), i, shieldStrength, 0);

                if (GetMyMonster().GetPassiveID() == 1)
                    gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 8, gameMaster.IsItMyTurn(), i, 200, 0);
            }
        }

        yield return new WaitForSeconds(0.3f);

        FinishMove(true, false);
    }

    private IEnumerator Moonlight()
    {
        gameMaster.AnimateMonster(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, "ability2");

        yield return new WaitForSeconds(0.3f);

        int healMultiplier = (GetMyMonster().GetCurrentMagic() * GetCurrentMove(3).GetPercentageMultiplier()) + GetMoveDamage(3, 0);
        float heal = GetTargetedMonster().GetMaxHealth() * (1f * healMultiplier / 100f);
        int finalHeal = Mathf.RoundToInt(heal);
        finalHeal = GetMultiplierDamage(finalHeal);

        gameMaster.ShootProjectile(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 8, gameMaster.IsItMyTurn(), GetTargetedMonster().teamIndex, false, 0);

        yield return new WaitForSeconds(0.4f);

        gameMaster.ChangeMonsterHealth(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, gameMaster.IsItMyTurn(), GetTargetedMonster().teamIndex, finalHeal, false);

        if (GetMyMonster().GetPassiveID() == 1)
            gameMaster.ApplyStatus(gameMaster.IsItMyTurn(), GetMyMonster().teamIndex, 8, gameMaster.IsItMyTurn(), GetTargetedMonster().teamIndex, 200, 0);

        yield return new WaitForSeconds(0.8f);

        FinishMove(true, false);
    }
}

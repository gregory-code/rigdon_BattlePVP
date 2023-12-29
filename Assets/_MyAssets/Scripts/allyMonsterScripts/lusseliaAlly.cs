using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static monsterAlly;
using static UnityEngine.GraphicsBuffer;

public class lusseliaAlly : monsterAlly
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

        while (attackMultiplier > 100)
        {
            attackMultiplier--;
            percentageMultiplier++;
        }
        attackMultiplier = percentageMultiplier;
        UseAttack(GetMonster().GetAttackID(), targetMon, false);
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
                StartCoroutine(Starfall(target, consumeTurn));
                break;

            case 2:
                StartCoroutine(SolarWave(target, consumeTurn));
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

    private IEnumerator Starfall(monster target, bool consumeTurn)
    {
        gameMaster.AnimateMonster(GetMonster(), "attack1");

        yield return new WaitForSeconds(0.2f);

        int attack1 = GetMonster().GetCurrentStrength() + GetMoveDamage(0, 0);
        attack1 = GetMultiplierDamage(attack1);

        gameMaster.ShootProjectile(GetMonster(), target, 7, 2);

        gameMaster.DeclaringDamage(GetMonster(), target, -attack1, destroyShields);
        yield return new WaitForSeconds(0.2f);
        target = gameMaster.GetRedirectedMonster(target);
        yield return new WaitForSeconds(0.25f);
        gameMaster.DamageMonster(GetMonster(), target, -attack1);

        int shouldAddBurnDamage = 0;
        if (GetMonster().GetPassiveID() == 2)
            shouldAddBurnDamage = 1;

        if (gameMaster.estimatedDamage < 0)
            gameMaster.ApplyStatus(GetMonster(), target, 7, GetMoveDamage(0, 1), shouldAddBurnDamage);

        yield return new WaitForSeconds(0.8f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator SolarWave(monster target, bool consumeTurn)
    {
        gameMaster.AnimateMonster(GetMonster(), "attack2");

        yield return new WaitForSeconds(0.3f);

        int attack1 = GetMonster().GetCurrentStrength() + GetMoveDamage(1, 0);
        attack1 = GetMultiplierDamage(attack1);

        monster[] enemyTeam = gameMaster.GetMonstersTeam(GetTargetedMonster());
        for (int i = 0; i < 3; i++)
        {
            if (enemyTeam[i].isDead())
                continue;

            target = enemyTeam[i];
            gameMaster.ShootProjectile(GetMonster(), target, 9, 3);
            yield return new WaitForSeconds(0.4f);
            gameMaster.DeclaringDamage(GetMonster(), target, -attack1, destroyShields);
            yield return new WaitForSeconds(0.2f);
            target = gameMaster.GetRedirectedMonster(target);
            gameMaster.DamageMonster(GetMonster(), target, -attack1);

            int shouldAddBurnDamage = 0;
            if (GetMonster().GetPassiveID() == 2)
                shouldAddBurnDamage = 1;

            if (gameMaster.estimatedDamage < 0)
                gameMaster.ApplyStatus(GetMonster(), target, 7, 1, shouldAddBurnDamage);
        }

        yield return new WaitForSeconds(0.8f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator LunarBlanket()
    {
        gameMaster.AnimateMonster(GetMonster(), "ability1");

        yield return new WaitForSeconds(0.3f);
        int shieldStrength = (GetMonster().GetCurrentMagic() + GetMoveDamage(2, 0));
        shieldStrength = GetMultiplierDamage(shieldStrength);

        float newBubbleBuffer = (shieldStrength * 1.1f) + 1.05f;
        shieldStrength = Mathf.RoundToInt(newBubbleBuffer);

        monster[] myteam = gameMaster.GetMonstersTeam(GetMonster());
        for (int i = 0; i < 3; i++)
        {
            if (myteam[i].isDead())
                continue;

            gameMaster.ApplyStatus(GetMonster(), myteam[i], 1, shieldStrength, 0);

            if (GetMonster().GetPassiveID() == 1)
                gameMaster.ApplyStatus(GetMonster(), myteam[i], 8, 200, 0);
        }

        yield return new WaitForSeconds(0.3f);

        FinishMove(true, false);
    }

    private IEnumerator Moonlight()
    {
        gameMaster.AnimateMonster(GetMonster(), "ability2");

        yield return new WaitForSeconds(0.3f);

        int healMultiplier = (GetMonster().GetCurrentMagic() * GetCurrentMove(3).GetPercentageMultiplier()) + GetMoveDamage(3, 0);
        float heal = GetTargetedMonster().GetMaxHealth() * (1f * healMultiplier / 100f);
        int finalHeal = Mathf.RoundToInt(heal);
        finalHeal = GetMultiplierDamage(finalHeal);

        gameMaster.ShootProjectile(GetMonster(), GetTargetedMonster(), 8, 0);

        yield return new WaitForSeconds(0.4f);

        gameMaster.HealMonster(GetMonster(), GetTargetedMonster(), finalHeal);

        if (GetMonster().GetPassiveID() == 1)
            gameMaster.ApplyStatus(GetMonster(), GetTargetedMonster(), 8, 200, 0);

        yield return new WaitForSeconds(0.8f);

        FinishMove(true, false);
    }
}

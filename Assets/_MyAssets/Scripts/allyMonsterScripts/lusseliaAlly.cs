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

    private void AttackAgain(monster targetMon, int extraDamage)
    {
        if (GetMonster().GetOwnership() == false)
            return;

        UseAttack(GetMonster().GetAttackID(), GetMonster(), targetMon, false, extraDamage);
    }

    private void TookDamage(monster recivingMon, monster usingMon, int damage, bool died, bool burnDamage)
    {

    }

    private void UseAttack(int attackID, monster user, monster target, bool consumeTurn, int extraDamage)
    {
        switch (attackID)
        {
            default:
                break;

            case 1:
                StartCoroutine(Starfall(user, target, consumeTurn, extraDamage));
                break;

            case 2:
                StartCoroutine(SolarWave(user, target, consumeTurn, extraDamage));
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

    private IEnumerator Starfall(monster user, monster target, bool consumeTurn, int extraDamage)
    {
        if (holdAttack)
        {
            holdAttack = false;
            yield return new WaitForSeconds(1f);
        }

        gameMaster.AnimateMonster(user, "attack1");

        yield return new WaitForSeconds(0.2f);

        int attack1 = user.GetCurrentStrength() + GetMoveDamage(0, 0);
        attack1 += extraDamage;

        gameMaster.ShootProjectile(user, target, 3, 2);

        gameMaster.DeclaringDamage(user, target, -attack1, destroyShields);
        yield return new WaitForSeconds(0.2f);
        target = gameMaster.GetRedirectedMonster(target);
        yield return new WaitForSeconds(0.25f);
        gameMaster.DamageMonster(user, target, -attack1);

        int shouldAddBurnDamage = 0;
        if (GetMonster().GetPassiveID() == 2)
            shouldAddBurnDamage = 1;

        if (gameMaster.estimatedDamage < 0)
            gameMaster.ApplyStatus(user, target, 1, GetMoveDamage(0, 1), shouldAddBurnDamage);

        yield return new WaitForSeconds(0.8f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator SolarWave(monster user, monster target, bool consumeTurn, int extraDamage)
    {
        if (holdAttack)
        {
            holdAttack = false;
            yield return new WaitForSeconds(1f);
        }

        gameMaster.AnimateMonster(user, "attack2");

        yield return new WaitForSeconds(0.3f);

        int attack1 = user.GetCurrentStrength() + GetMoveDamage(1, 0);
        attack1 += extraDamage;

        monster[] enemyTeam = gameMaster.GetMonstersTeam(GetTargetedMonster());
        for (int i = 0; i < 3; i++)
        {
            if (enemyTeam[i].isDead())
                continue;

            target = enemyTeam[i];
            gameMaster.ShootProjectile(user, target, 4, 3);
            yield return new WaitForSeconds(0.4f);
            gameMaster.DeclaringDamage(user, target, -attack1, destroyShields);
            yield return new WaitForSeconds(0.2f);
            target = gameMaster.GetRedirectedMonster(target);
            gameMaster.DamageMonster(user, target, -attack1);

            int shouldAddBurnDamage = 0;
            if (GetMonster().GetPassiveID() == 2)
                shouldAddBurnDamage = 1;

            if (gameMaster.estimatedDamage < 0)
                gameMaster.ApplyStatus(user, target, 1, 1, shouldAddBurnDamage);
        }

        yield return new WaitForSeconds(0.8f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator LunarBlanket()
    {
        gameMaster.AnimateMonster(GetMonster(), "ability1");

        yield return new WaitForSeconds(0.3f);
        int shieldStrength = (GetMonster().GetCurrentMagic() + GetMoveDamage(2, 0));
        float newBubbleBuffer = (shieldStrength * 1.1f) + 1.05f;

        monster[] myteam = gameMaster.GetMonstersTeam(GetMonster());
        for (int i = 0; i < 3; i++)
        {
            if (myteam[i].isDead())
                continue;

            int value = shieldStrength;
            if (GetMonster() == myteam[i])
                value = Mathf.RoundToInt(newBubbleBuffer);

            gameMaster.ApplyStatus(GetMonster(), myteam[i], 3, value, 0);

            if (GetMonster().GetPassiveID() == 1)
                gameMaster.ApplyStatus(GetMonster(), myteam[i], 5, 200, 0);
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

        gameMaster.ShootProjectile(GetMonster(), GetTargetedMonster(), 5, 0);

        yield return new WaitForSeconds(0.4f);

        gameMaster.HealMonster(GetMonster(), GetTargetedMonster(), finalHeal);

        if (GetMonster().GetPassiveID() == 1)
            gameMaster.ApplyStatus(GetMonster(), GetTargetedMonster(), 5, 200, 0);

        yield return new WaitForSeconds(0.8f);

        FinishMove(true, false);
    }
}

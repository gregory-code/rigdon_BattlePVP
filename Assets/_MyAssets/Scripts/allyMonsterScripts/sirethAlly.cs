using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class sirethAlly : monsterAlly
{
    private void Start()
    {
        onAttack += UseAttack;
        onAbility += UseAbility;
        GetMonster().onAttackAgain += AttackAgain;
        GetMonster().onRemoveConnections += RemoveConnections;

        if (GetMonster().GetPassiveID() == 1)
        {
            monster[] monsters = gameMaster.GetMonstersTeam(GetMonster());
            foreach (monster mon in monsters)
            {
                mon.onTakeDamage += AllyTookDamage;
            }
        }
    }

    private void AllyTookDamage(monster recivingMon, monster usingMon, int damage, bool died, bool crit, bool burnDamage)
    {
        float threshold = GetMoveDamage(4, 0) * -1f;
        if(damage <= threshold && GetMonster().GetOwnership() && burnDamage == false)
        {
            gameMaster.ApplyStatus(GetMonster(), usingMon, 11, (GetMoveDamage(4, 1) + 1), 0);
        }
    }

    private void RemoveConnections()
    {
        onAttack -= UseAttack;
        onAbility -= UseAbility;
        GetMonster().onAttackAgain -= AttackAgain;
        GetMonster().onRemoveConnections -= RemoveConnections;

        if (GetMonster().GetPassiveID() == 1)
        {
            monster[] monsters = gameMaster.GetMonstersTeam(GetMonster());
            foreach (monster mon in monsters)
            {
                mon.onTakeDamage -= AllyTookDamage;
            }
        }
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
                StartCoroutine(Bite(user, target, consumeTurn, extraDamage));
                break;

            case 2:
                StartCoroutine(NotYet(user, target, consumeTurn, extraDamage));
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
                StartCoroutine(SpikeyCarapace());
                break;

            case 2:
                StartCoroutine(Nuhuh());
                break;
        }
    }

    private IEnumerator Bite(monster user, monster target, bool consumeTurn, int extraDamage)
    {
        gameMaster.AnimateMonster(user, "attack1");
        gameMaster.MoveMonster(user, target, 0);

        int attack1 = user.GetCurrentStrength() + GetMoveDamage(0, 0);
        attack1 += extraDamage;

        yield return new WaitForSeconds(1f);
        bool didCrit = IsCrit(0);
        gameMaster.DeclaringDamage(user, target, -attack1, destroyShields, didCrit);
        yield return new WaitForSeconds(0.2f);
        target = gameMaster.GetRedirectedMonster(target);

        yield return new WaitForSeconds(0.3f);
        gameMaster.ShootProjectile(user, target, 11, 0);
        gameMaster.DamageMonster(user, target, -attack1, didCrit);

        if(target.GetStatus(11) != null)
        {
            gameMaster.ApplyStatus(user, target, 10, 200, 0);
            gameMaster.TryRemoveStatus(target, 11);
        }

        int weakness = user.GetCurrentMagic() + GetMoveDamage(0, 1);
        gameMaster.ApplyStatus(user, target, 2, GetMoveDamage(0, 2), weakness);

        yield return new WaitForSeconds(0.4f);
        gameMaster.MoveMonster(user, target, 1);
        yield return new WaitForSeconds(0.4f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator NotYet(monster user, monster target, bool consumeTurn, int extraDamage)
    {
        gameMaster.AnimateMonster(user, "attack2");

        yield return new WaitForSeconds(0.5f);

        yield return new WaitForSeconds(0.4f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator SpikeyCarapace()
    {
        gameMaster.AnimateMonster(GetMonster(), "ability1");

        yield return new WaitForSeconds(0.5f);

        int turnDuration = (GetMoveDamage(3, 0));
        if (GetMonster() == GetTargetedMonster())
            turnDuration++;

        int power = GetMoveDamage(3, 1) + (GetMonster().GetCurrentMagic() * GetCurrentMove(3).GetPercentageMultiplier());

        gameMaster.ApplyStatus(GetMonster(), GetTargetedMonster(), 4, turnDuration, 0);
        gameMaster.ApplyStatus(GetMonster(), GetTargetedMonster(), 12, turnDuration, power);

        yield return new WaitForSeconds(0.5f);

        FinishMove(true, false);
    }

    private IEnumerator Nuhuh()
    {
        gameMaster.AnimateMonster(GetMonster(), "ability2");
        yield return new WaitForSeconds(0.4f);

        yield return new WaitForSeconds(0.4f);

        FinishMove(true, false);
    }
}

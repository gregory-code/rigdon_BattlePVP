using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using static monster;
using static UnityEngine.GraphicsBuffer;

public class minfurAlly : monsterAlly
{
    private void Start()
    {
        onAttack += UseAttack;
        onAbility += UseAbility;
        GetMonster().onAttackAgain += AttackAgain;
        GetMonster().onRemoveConnections += RemoveConnections;

        monster[] monsters = gameMaster.GetMonstersTeam(GetMonster());
        foreach(monster mon in monsters)
        {
            mon.onTakeDamage += TookDamage;
        }
    }

    private void RemoveConnections()
    {
        onAttack -= UseAttack;
        onAbility -= UseAbility;
        GetMonster().onAttackAgain -= AttackAgain;
        GetMonster().onRemoveConnections -= RemoveConnections;

        monster[] monsters = gameMaster.GetMonstersTeam(GetMonster());
        foreach (monster mon in monsters)
        {
            mon.onTakeDamage -= TookDamage;
        }
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
        UseAttack(GetMonster().GetAttackID(), GetMonster(), targetMon, false);
    }

    private void TookDamage(monster recivingMon, monster usingMon, int damage, bool died, bool burnDamage)
    {
        if (GetMonster().GetPassiveID() == 2 && GetMonster().GetOwnership()) // index for acron
        {
            if (recivingMon.isDead() || recivingMon.GetCurrentHealth() >= recivingMon.GetMaxHealth())
                return;

            if(GetMonster().TryConsumeStrawberry() == true)
            {
                StartCoroutine(GiveStrawberryWait(recivingMon));
            }
        }
    }

    private IEnumerator GiveStrawberryWait(monster recivingMon)
    {
        gameMaster.AnimateMonster(GetMonster(), "ability1");
        yield return new WaitForSeconds(0.1f);
        gameMaster.ShootProjectile(GetMonster(), recivingMon, 6, 0);
        yield return new WaitForSeconds(0.2f);
        int heal = GetMonster().GetBaseMagic() + GetMoveDamage(5, 0);
        gameMaster.HealMonster(GetMonster(), recivingMon, heal);

    }

    private void UseAttack(int attackID, monster user, monster target, bool consumeTurn)
    {
        switch (attackID)
        {
            default:
                break;

            case 1:
                StartCoroutine(Cuddle(user, target, consumeTurn));
                break;

            case 2:
                StartCoroutine(FluffyRoll(user, target, consumeTurn));
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
                StartCoroutine(Yippers());
                break;

            case 2:
                if(GetTargetedMonster().GetIndex() == GetMonster().GetIndex())
                {
                    gameMaster.SetFilter(false);
                    GetMonster().SetAct(true);
                    return;
                }
                StartCoroutine(BestFriends());
                break;
        }
    }

    private IEnumerator Cuddle(monster user, monster target, bool consumeTurn)
    {
        if (holdAttack)
        {
            holdAttack = false;
            yield return new WaitForSeconds(1f);
        }

        gameMaster.AnimateMonster(user, "attack1");
        gameMaster.MoveMonster(user, target, 0);

        yield return new WaitForSeconds(0.83f);

        int attack = user.GetCurrentStrength() + GetMoveDamage(0, 0);
        attack = GetMultiplierDamage(attack);

        gameMaster.ShootProjectile(user, target, 3, 0);
        gameMaster.ApplyStatus(user, target, 3, GetMoveDamage(0, 1), -attack);

        yield return new WaitForSeconds(0.5f);

        if (GetMonster().GetPassiveID() == 1)
        {
            YoinkStatuses(target, 1, 2, 4, 8, 9, 11, 12, 13, 15);
        }

        yield return new WaitForSeconds(0.5f);

        gameMaster.MoveMonster(user, target, 1);

        yield return new WaitForSeconds(0.5f);

        FinishMove(consumeTurn, true);
    }

    private void YoinkStatuses(monster yoinkie, int status, int status2, int status3, int status4, int status5, int status6, int status7, int status8, int status9)
    {
        if (GetTargetedMonster().statusEffects.Count <= 0)
            return;

        List<int> listOfIndexesToSteal = new List<int>();

        foreach (statusEffectUI effect in GetTargetedMonster().statusEffects)
        {
            if (effect.GetIndex() == status || effect.GetIndex() == status2 || effect.GetIndex() == status3 || effect.GetIndex() == status4 
       || effect.GetIndex() == status5 || effect.GetIndex() == status6 || effect.GetIndex() == status7 || effect.GetIndex() == status8 || effect.GetIndex() == status9) // add more of these as they get added
            {
                listOfIndexesToSteal.Add(effect.GetIndex());
            }
        }

        if (listOfIndexesToSteal.Count > 0)
        {
            foreach (int index in listOfIndexesToSteal)
            {
                statusEffectUI stolenStatus = GetTargetedMonster().GetStatus(index);
                gameMaster.TryRemoveStatus(yoinkie, index);
                gameMaster.ApplyStatus(GetMonster(), GetMonster(), stolenStatus.GetIndex(), stolenStatus.GetCounter(), stolenStatus.GetPower());

            }
        }
    }

    private IEnumerator FluffyRoll(monster user, monster target, bool consumeTurn)
    {
        if (holdAttack)
        {
            holdAttack = false;
            yield return new WaitForSeconds(1f);
        }

        gameMaster.AnimateMonster(user, "attack2");
        gameMaster.ShootProjectile(user, target, 5, 0);
        gameMaster.MoveMonster(user, target, 0);

        int attack = user.GetCurrentStrength() + GetMoveDamage(1, 0);
        attack = GetMultiplierDamage(attack);

        gameMaster.DeclaringDamage(user, target, -attack, destroyShields);
        yield return new WaitForSeconds(0.1f);
        target = gameMaster.GetRedirectedMonster(target);

        yield return new WaitForSeconds(0.2f);
        gameMaster.DamageMonster(user, target, -attack);

        if (GetMonster().GetPassiveID() == 1)
        {
            YoinkStatuses(target, 1, 2, 4, 8, 9, 11, 12, 13, 15);
        }

        gameMaster.AdjustTurnOrder(target, false, true);

        yield return new WaitForSeconds(1.15f);

        gameMaster.MoveMonster(user, target, 1);

        yield return new WaitForSeconds(0.3f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator Yippers()
    {
        gameMaster.AnimateMonster(GetMonster(), "ability1");

        yield return new WaitForSeconds(0.3f);

        int attack = GetMonster().GetCurrentMagic() + GetMoveDamage(2, 0);
        attack = GetMultiplierDamage(attack);

        if (GetMonster().GetPassiveID() == 1)
        {
            YoinkStatuses(GetTargetedMonster(), 0, 3, 7, -1, -1, -1, -1, -1, -1);
        }

        monster[] myteam = gameMaster.GetMonstersTeam(GetMonster());
        for (int i = 0; i < 3; i++)
        {
            if (myteam[i].isDead())
                continue;

            if (myteam[i] == GetMonster())
            {
                gameMaster.ApplyStatus(GetMonster(), myteam[i], 4, (GetMoveDamage(2, 1) + 1), attack);
            }
            else
            {
                gameMaster.ApplyStatus(GetMonster(), myteam[i], 4, GetMoveDamage(2, 1), attack);
                gameMaster.AnimateMonster(myteam[i], "idle");
            }
        }

        yield return new WaitForSeconds(0.8f);

        FinishMove(true, false);
    }

    private IEnumerator BestFriends()
    {
        gameMaster.AnimateMonster(GetMonster(), "ability2");
        gameMaster.MoveMonster(GetMonster(), GetTargetedMonster(), 0);

        statusEffectUI minfurfriendStatus = GetMonster().GetStatus(6);
        if (minfurfriendStatus != null)
        {
            gameMaster.TryRemoveStatus(GetMonster(), 6);
            
            foreach(monster ally in gameMaster.GetMonstersTeam(GetMonster()))
            {
                statusEffectUI bestFriend = ally.GetStatus(5);
                if(bestFriend != null)
                {
                    gameMaster.TryRemoveStatus(ally, 5);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);

        gameMaster.ShootProjectile(GetMonster(), GetTargetedMonster(), 4, 0);

        if (GetMonster().GetPassiveID() == 1)
        {
            YoinkStatuses(GetTargetedMonster(), 0, 3, 7, -1, -1, -1, -1, -1, -1);
        }

        yield return new WaitForSeconds(0.5f);

        gameMaster.ApplyStatus(GetTargetedMonster(), GetMonster(), 6, 99, 0);
        gameMaster.ApplyStatus(GetMonster(), GetTargetedMonster(), 5, 99, 0);

        yield return new WaitForSeconds(0.5f);

        gameMaster.MoveMonster(GetMonster(), GetTargetedMonster(), 1);

        FinishMove(true, false);
    }
}

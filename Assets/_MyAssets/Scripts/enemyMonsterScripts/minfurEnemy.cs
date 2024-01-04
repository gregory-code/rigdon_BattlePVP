using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class minfurEnemy : monsterEnemyAI
{
    private void Start()
    {
        onAttack += UseAttack;
        onAbility += UseAbility;
        GetMonster().onRemoveConnections += RemoveConnections;
    }

    private void RemoveConnections()
    {
        onAttack -= UseAttack;
        onAbility -= UseAbility;
        GetMonster().onRemoveConnections -= RemoveConnections;
    }
    private void UseAttack(int attackID, monster user, monster target, bool consumeTurn, int extraDamage)
    {
        switch (attackID)
        {
            default:
                break;

            case 1:
                StartCoroutine(Roll(user, target, consumeTurn, extraDamage));
                break;

            case 2:
                //StartCoroutine(FluffyRoll(user, target, consumeTurn, extraDamage));
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
                //StartCoroutine(Yippers());
                break;

            case 2:
                //StartCoroutine(BestFriends());
                break;
        }
    }

    private IEnumerator Roll(monster user, monster target, bool consumeTurn, int extraDamage)
    {
        gameMaster.AnimateMonster(user, "attack2");
        //gameMaster.ShootProjectile(user, target, 5, 0);
        gameMaster.MoveMonster(user, target, 0);

        int attack = user.GetCurrentStrength() + GetMoveDamage(0, 0);
        attack += extraDamage;

        bool didCrit = IsCrit(0);
        gameMaster.DeclaringDamage(user, target, -attack, destroyShields, didCrit);
        yield return new WaitForSeconds(0.1f);
        target = gameMaster.GetRedirectedMonster(target);

        yield return new WaitForSeconds(0.2f);
        gameMaster.DamageMonster(user, target, -attack, didCrit);

        yield return new WaitForSeconds(1.15f);

        gameMaster.MoveMonster(user, target, 1);

        yield return new WaitForSeconds(0.3f);

        FinishMove(consumeTurn, true);
    }
}

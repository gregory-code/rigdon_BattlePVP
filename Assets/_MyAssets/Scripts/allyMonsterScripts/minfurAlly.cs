using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static monsterAlly;

public class minfurAlly : monsterAlly
{
    private void Start()
    {
        onAttack += UseAttack;
        onAbility += UseAbility;
        GetMyMonster().onAttackAgain += AttackAgain;
        GetMyMonster().onTakeDamage += TookDamage;
    }

    private void AttackAgain(int percentageMultiplier, bool bMine2, int TargetOfTargetIndex)
    {
        if (GetMyMonster().bMine == false)
            return;

        attackMultiplier = percentageMultiplier;
        UseAttack(GetMyMonster().GetAttackID(), TargetOfTargetIndex, false);
    }

    private void TookDamage(int change, bool died, bool bMine, int userIndex)
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
                StartCoroutine(Cuddle(targetIndex, consumeTurn));
                break;

            case 2:
                StartCoroutine(FluffyRoll(targetIndex, consumeTurn));
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
                StartCoroutine(BestFriends());
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

    private IEnumerator Cuddle(int targetIndex, bool consumeTurn)
    {
        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "attack1");
        gameMaster.MoveMonster(true, GetMyMonster().teamIndex, false, false, targetIndex);

        yield return new WaitForSeconds(0.83f);

        int attack = GetMyMonster().GetCurrentStrength() + GetMoveDamage(0, 0);
        attack = GetMultiplierDamage(attack);

        gameMaster.ShootProjectile(true, GetMyMonster().teamIndex, 3, false, targetIndex);
        gameMaster.ApplyStatus(3, false, targetIndex, GetMoveDamage(0, 1), -attack);

        yield return new WaitForSeconds(1f);

        gameMaster.MoveMonster(true, GetMyMonster().teamIndex, true, true, GetMyMonster().teamIndex);

        yield return new WaitForSeconds(0.5f);

        FinishMove(consumeTurn, true);
    }

    private IEnumerator FluffyRoll(int targetIndex, bool consumeTurn)
    {
        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "attack2");
        gameMaster.MoveMonster(true, GetMyMonster().teamIndex, false, false, targetIndex);

        yield return new WaitForSeconds(0.5f);

        int attack = GetMyMonster().GetCurrentStrength() + GetMoveDamage(1, 0);
        attack = GetMultiplierDamage(attack);

        gameMaster.ChangeMonsterHealth(true, GetMyMonster().teamIndex, false, targetIndex, -attack);

        //Apply slow

        yield return new WaitForSeconds(1f);

        gameMaster.MoveMonster(true, GetMyMonster().teamIndex, true, true, GetMyMonster().teamIndex);



        FinishMove(consumeTurn, true);
    }

    private IEnumerator Yippers()
    {
        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "ability1");

        yield return new WaitForSeconds(0.3f);

        int attack = GetMyMonster().GetCurrentMagic() + GetMoveDamage(2, 0);
        attack = GetMultiplierDamage(attack);

        monster[] myteam = gameMaster.GetMonstersTeam(GetMyMonster());
        for (int i = 0; i < 3; i++)
        {
            if (myteam[i].GetCurrentHealth() > 0)
            {
                if (myteam[i].teamIndex == GetMyMonster().teamIndex)
                {
                    gameMaster.ApplyStatus(4, true, i, (GetMoveDamage(2, 1) + 1), attack);
                }
                else
                {
                    gameMaster.ApplyStatus(4, true, i, GetMoveDamage(2, 1), attack);
                    gameMaster.AnimateMonster(true, i, "idle");
                }
            }
        }

        yield return new WaitForSeconds(0.8f);

        FinishMove(true, false);
    }

    private IEnumerator BestFriends()
    {
        gameMaster.AnimateMonster(true, GetMyMonster().teamIndex, "ability2");
        gameMaster.MoveMonster(true, GetMyMonster().teamIndex, false, true, GetTargetedMonster().teamIndex);

        yield return new WaitForSeconds(0.6f);

        gameMaster.MoveMonster(true, GetMyMonster().teamIndex, true, true, GetMyMonster().teamIndex);

        FinishMove(true, false);
    }
}

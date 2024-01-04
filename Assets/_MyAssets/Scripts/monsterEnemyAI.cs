using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class monsterEnemyAI : monsterBase
{
    public delegate void OnAttack(int attackID, monster user, monster target, bool consumeTurn, int extraDamage);
    public event OnAttack onAttack;

    public delegate void OnAbility(int abilityID);
    public event OnAbility onAbility;

    public void AITurn()
    {
        if (GetMonster() == null || GetMonster().isDead())
            return;

        ChooseMove();
    }

    private void ChooseMove()
    {
        if (GetMonster().CanAct() == false)
            return;

        GetMonster().SetAct(false);
        onAttack?.Invoke(GetMonster().GetAttackID(), GetMonster(), GetRandomEnemy(), true, 0);
    }

    private monster GetRandomEnemy()
    {
        gameMaster.targetedMonster = gameMaster.GetRandomEnemy(-1, -1, true).ownerTransform.GetComponent<monsterBase>();
        return GetTargetedMonster(); // I think this is true for enemy allies?
    }
}

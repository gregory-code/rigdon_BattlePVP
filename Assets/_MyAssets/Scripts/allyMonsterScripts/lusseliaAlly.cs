using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lusseliaAlly : monsterAlly
{
    private void Start()
    {
        onAttack += UseAttack;
        onAbility += UseAbility;
    }

    private void UseAttack(int attackID, int targetIndex, bool consumeTurn)
    {
        switch(attackID)
        {
            default:
                break;

            case 1:
                break;

            case 2:
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
                break;

            case 2:
                break;
        }
    }
}

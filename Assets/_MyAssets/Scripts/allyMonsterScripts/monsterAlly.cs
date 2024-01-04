using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class monsterAlly : monsterBase
{
    public delegate void OnAttack(int attackID, monster user, monster target, bool consumeTurn, int extraDamage);
    public event OnAttack onAttack;

    public delegate void OnAbility(int abilityID);
    public event OnAbility onAbility;

    private void Awake()
    {
        onOpenInfo += openInfo;
    }

    private void openInfo()
    {
        if(gameMaster.inInfoScreen)
        {
            lineRenderState(false);
        }
    }

    public void OnMouseDown()
    {
        if (gameMaster.inInfoScreen || gameMaster.movingToNewGame)
            return;

        if (gameMaster.activeMonsters[0] == GetMonster() && GetMonster().GetOwnership())
        {
            lineRenderState(true);
            gameMaster.selectedMonster = this;
            greenLine.resetReticle(renderCamera.ScreenToWorldPoint(gameMaster.touchedPos));
        }
    }

    public void OnMouseUp()
    {
        if (gameMaster.inInfoScreen)
        {
            lineRenderState(false);
            return;
        }

        if (redLine.IsHoveringOverTarget() && gameMaster.activeMonsters[0] == GetMonster() && GetMonster().CanAct() == true && gameMaster.movingToNewGame == false)
        {
            MadeChoice();
            onAttack?.Invoke(GetMonster().GetAttackID(), GetMonster(), GetTargetedMonster(), true, 0);
        }
        if (greenLine.IsHoveringOverTarget() && gameMaster.activeMonsters[0] == GetMonster() && GetMonster().CanAct() == true && gameMaster.movingToNewGame == false)
        {
            MadeChoice();
            onAbility?.Invoke(GetMonster().GetAbilityID());
        }
        lineRenderState(false);
    }

    private void MadeChoice()
    {
        GetMonster().SetAct(false);
        gameMaster.SetFilter(true);
    }

    private void lineRenderState(bool state)
    {
        gameMaster.bRendering = state;

        redLine.enabled = state;
        greenLine.enabled = state;

        if (state)
        {
            gameMaster.touchedPos = Input.mousePosition;
            greenLine.enable(true, renderCamera.ScreenToWorldPoint(gameMaster.touchedPos));
        }
        else
        {
            SetIsMouseOver(false);
            redLine.enable(false, Vector2.zero);
            greenLine.enable(false, Vector2.zero);
            greenLine.focusTarget(false, gameObject.transform);
            redLine.focusTarget(false, gameObject.transform);
        }
    }
}

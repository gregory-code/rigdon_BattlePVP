using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class monsterAlly : monsterBase
{
    public delegate void OnAttack(int attackID, int targetIndex, bool consumeTurn);
    public event OnAttack onAttack;

    public delegate void OnAbility(int abilityID);
    public event OnAbility onAbility;

    private void LateUpdate()
    {
        if (gameMaster.bRendering == false || redLine.IsHoveringOverTarget() || greenLine.IsHoveringOverTarget()) return;

        Vector3 touchPos = Input.mousePosition;
        redLine.updateReticleLocation(renderCamera.ScreenToWorldPoint(touchPos));
        greenLine.updateReticleLocation(renderCamera.ScreenToWorldPoint(touchPos));
    }

    public void OnMouseDown()
    {
        if (gameMaster.activeMonsters[0] == GetMyMonster() && GetMyMonster().bMine)
        {
            lineRenderState(true);
            gameMaster.selectedMonster = this;
            greenLine.resetReticle(renderCamera.ScreenToWorldPoint(gameMaster.touchedPos));
        }
    }

    public void OnMouseUp()
    {
        if (redLine.IsHoveringOverTarget() && gameMaster.activeMonsters[0] == GetMyMonster() && GetMyMonster().canAct == true)
        {
            GetMyMonster().canAct = false;
            onAttack?.Invoke(GetMyMonster().GetAttackID(), GetTargetedMonster().teamIndex, true);
        }
        if (greenLine.IsHoveringOverTarget() && gameMaster.activeMonsters[0] == GetMyMonster() && GetMyMonster().canAct == true)
        {
            GetMyMonster().canAct = false;
            onAbility?.Invoke(GetMyMonster().GetAbilityID());
        }
        lineRenderState(false);
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

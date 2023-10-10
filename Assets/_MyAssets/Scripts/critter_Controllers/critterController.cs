using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class critterController : critterBase
{
    private void LateUpdate()
    {
        if (BattleMaster.bRendering == false || redLine.IsHoveringOverTarget() || greenLine.IsHoveringOverTarget()) return;

        Vector3 touchPos = Input.mousePosition;
        redLine.updateReticleLocation(renderCamera.ScreenToWorldPoint(touchPos));
        greenLine.updateReticleLocation(renderCamera.ScreenToWorldPoint(touchPos));
    }

    public void OnMouseDown()
    {
        if (BattleMaster.allCritters[0] == myCritter)
        {
            lineRenderState(true);
            BattleMaster.selectedCritter = myCritter;
            greenLine.resetReticle(renderCamera.ScreenToWorldPoint(BattleMaster.touchedPos));
        }
    }

    public void OnMouseUp()
    {
        if (redLine.IsHoveringOverTarget() && BattleMaster.allCritters[0] == myCritter)
        {
            BattleMaster.NextTurn();
            //Put in the attack order
        }
        lineRenderState(false);
    }

    private void lineRenderState(bool state)
    {
        BattleMaster.bRendering = state;

        redLine.enabled = state;
        greenLine.enabled = state;

        if (state)
        {
            BattleMaster.touchedPos = Input.mousePosition;
            greenLine.enable(true, renderCamera.ScreenToWorldPoint(BattleMaster.touchedPos));
        }
        else
        {
            bMouseOver = false;
            redLine.enable(false, Vector2.zero);
            greenLine.enable(false, Vector2.zero);
            greenLine.focusTarget(false, gameObject.transform);
            redLine.focusTarget(false, gameObject.transform);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.iOS;
using UnityEngine.UI;

public class critterController : MonoBehaviour
{
    battleMaster BattleMaster;
    lineScript redLine;
    lineScript greenLine;

    Camera renderCamera;
    bool bMouseOver;


    private void Start()
    {
        BattleMaster = GameObject.FindGameObjectWithTag("BattleField").GetComponent<battleMaster>();
        redLine = GameObject.Find("redLineRender").GetComponent<lineScript>();
        greenLine = GameObject.Find("greenLineRender").GetComponent<lineScript>();
        renderCamera = GameObject.Find("lineRenderCamera").GetComponent<Camera>();
    }

    private void Update()
    {
        if (BattleMaster.bRendering == false || redLine.IsHoveringOverTarget() || greenLine.IsHoveringOverTarget()) return;

        Vector3 touchPos = Input.mousePosition;
        redLine.updateReticleLocation(renderCamera.ScreenToWorldPoint(touchPos));
        greenLine.updateReticleLocation(renderCamera.ScreenToWorldPoint(touchPos));
    }

    public void OnMouseDown()
    {
        lineRenderState(true);
        greenLine.resetReticle(renderCamera.ScreenToWorldPoint(BattleMaster.touchedPos));
    }

    public void OnMouseUp()
    {
        lineRenderState(false);
    }

    public void OnMouseOver()
    {
        if (BattleMaster.bRendering == false || bMouseOver == true) return;

        bMouseOver = true;
        redLine.enable(false, Vector2.zero);
        greenLine.enable(true, renderCamera.ScreenToWorldPoint(BattleMaster.touchedPos));
        greenLine.focusTarget(true, gameObject.transform);
    }

    public void OnMouseExit()
    {
        if (bMouseOver == false) return;

        bMouseOver = false;
        greenLine.focusTarget(false, gameObject.transform);
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

using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.iOS;
using UnityEngine.UI;
public class enemyCritter : MonoBehaviour
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

    public void OnMouseDown()
    {
        
    }

    public void OnMouseUp()
    {
        
    }

    public void OnMouseOver()
    {
        if (BattleMaster.bRendering == false || bMouseOver == true) return;

        bMouseOver = true;
        greenLine.enable(false, Vector2.zero);
        redLine.enable(true, renderCamera.ScreenToWorldPoint(BattleMaster.touchedPos));
        redLine.focusTarget(true, gameObject.transform);
    }

    public void OnMouseExit()
    {
        if (bMouseOver == false) return;

        bMouseOver = false;
        redLine.focusTarget(false, gameObject.transform);
    }
}

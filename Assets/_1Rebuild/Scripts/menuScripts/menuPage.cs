using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class menuPage : canvasGroupRenderer
{
    [SerializeField] menuTab myTab;

    private void Awake()
    {
        myTab.onTabSelected += SetCanvasStatus;
    }
}

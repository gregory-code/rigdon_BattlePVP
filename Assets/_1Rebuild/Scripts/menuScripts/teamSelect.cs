using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class teamSelect : canvasGroupRenderer
{
    private teamSelect[] teamSelects;

    [SerializeField] menuTab builderTab;
    [SerializeField] Image[] monsterImages;

    [SerializeField] Transform nameScreen;
    
    [SerializeField] Transform teamPos;
    [SerializeField] Transform nameScreenPos;

    private Vector3 originalLocation;
    private Vector3 originalScale;

    private Vector3 nameScreenOriginalLocation;

    private bool selected;

    private void Awake()
    {
        teamSelects = GameObject.FindObjectsOfType<teamSelect>();
        builderTab.onTabSelected += OpenBuilderTab;

        originalLocation = transform.localPosition;
        originalScale = transform.localScale;

        nameScreenOriginalLocation = nameScreen.localPosition;
    }

    private void LateUpdate()
    {
        Vector3 pos = originalLocation;
        Vector3 scale = originalScale;
        Vector3 namePos = nameScreenOriginalLocation;
        if(selected)
        {
            pos = teamPos.localPosition;
            scale = teamPos.localScale;
            namePos = nameScreenPos.localPosition;
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, pos, 6 * Time.deltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, scale, 6 * Time.deltaTime);
        nameScreen.localPosition = Vector3.Lerp(nameScreen.localPosition, namePos, 6 * Time.deltaTime);
    }

    public void Unselect()
    {
        selected = false;
    }

    private void OpenBuilderTab(bool state)
    {
        if(state)
        {
            selected = false;
            SetCanvasStatus(true);
        }
    }

    public void SelectThisTeam()
    {
        foreach(teamSelect team in teamSelects)
        {
            team.Unselect();
            team.SetCanvasStatus(false);
        }

        selected = true;
        SetCanvasStatus(true);
    }
}

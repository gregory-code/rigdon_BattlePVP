using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class teamBuilder : canvasGroupRenderer
{
    [SerializeField] Sprite[] monsterImageLibrary;
    [SerializeField] menuTab builderTab;

    private teamSelect currentTeam;

    private void Awake()
    {
        builderTab.onTabSelected += OpenBuilderTab;
    }

    void Start()
    {
        teamSelect[] teams = GameObject.FindObjectsOfType<teamSelect>();
        foreach(teamSelect team in teams)
        {
            team.onTeamSelected += SetCurrentTeam;
        }
    }

    private void OpenBuilderTab(bool state)
    {
        if (state)
        {
            SetCanvasStatus(false);
        }
    }

    private void SetCurrentTeam(teamSelect team)
    {
        SetCanvasStatus(true);
        currentTeam = team;
    }

    private void SetMonsterImage(Image target, Sprite newImage)
    {
        target.sprite = newImage;
        target.SetNativeSize();
    }
}

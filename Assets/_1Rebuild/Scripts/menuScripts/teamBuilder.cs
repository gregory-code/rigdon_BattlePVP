using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class teamBuilder : canvasGroupRenderer
{
    [SerializeField] Sprite emptyMonster;
    [SerializeField] menuTab builderTab;
    [SerializeField] monster[] monsters;

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

    public Sprite GetMonsterImageFromID(int id, int stage)
    {
        return monsters[id].stages[stage];
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

    public void SetMonsterImage(Image target, Sprite newImage)
    {
        target.sprite = newImage;
        target.SetNativeSize();
    }
}

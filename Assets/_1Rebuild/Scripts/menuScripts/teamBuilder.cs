using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class teamBuilder : canvasGroupRenderer
{
    [SerializeField] FirebaseScript fireBaseScript;

    [SerializeField] Sprite emptyMonster;
    [SerializeField] Sprite transparent;
    [SerializeField] menuTab builderTab;
    [SerializeField] monster[] monsters;
    [SerializeField] Image[] monsterImages;
    [SerializeField] Image[] monsterOutlines;
    [SerializeField] bool[] monsterExists;
    private teamSelect currentTeam;
    private int currentMonster;
    private monsterTab[] monsterTabs;


    [Header("Monster List")]
    [SerializeField] canvasGroupRenderer monsterListCanvasGroup;
    [SerializeField] monsterSelectButton monsterSelectPrefab;
    [SerializeField] Transform monsterSelectList;

    List<monsterSelectButton> monsterSelectButtons = new List<monsterSelectButton>();


    private void Awake()
    {
        monsterTabs = GameObject.FindObjectsOfType<monsterTab>();
        for(int i = 0; i < monsterTabs.Length; i++)
        {
            monsterTabs[i].onMonsterSelected += SelectMonster;
        }

        teamSelect[] teams = GameObject.FindObjectsOfType<teamSelect>();
        foreach (teamSelect team in teams)
        {
            team.onTeamSelected += SetCurrentTeam;
        }

        builderTab.onTabSelected += OpenBuilderTab;
    }

    private void OpenBuilderTab(bool state)
    {
        monsterListCanvasGroup.SetCanvasStatus(false);
        SetOutline(0, false);
        SetCanvasStatus(false);
        foreach(monsterTab tab in monsterTabs)
        {
            tab.SetTab(false);
        }
    }

    private void SelectMonster(int whichMon)
    {
        currentMonster = whichMon;
        SetOutline(whichMon, true);
        
        if (monsterExists[whichMon])
        {

        }
        else
        {
            monsterListCanvasGroup.SetCanvasStatus(true);
            OpenMonsterList();
        }
    }

    private void SetOutline(int which, bool bSelected)
    {
        for (int i = 0; i < monsterOutlines.Length; i++)
        {
            monsterOutlines[i].fillAmount = 0;
        }

        if(bSelected) 
            monsterOutlines[which].fillAmount = 1;
    }

    public void OpenMonsterList()
    {
        foreach (monsterSelectButton monsterSelect in monsterSelectButtons)
        {
            Destroy(monsterSelect.gameObject);
        }
        monsterSelectButtons.Clear();

        foreach (monster mon in monsters)
        {
            if (mon.GetMonsterID() == 0)
                continue;

            monsterSelectButton newMon = Instantiate(monsterSelectPrefab, monsterSelectList);
            newMon.transform.localScale = new Vector3(1, 1, 1);
            newMon.Init(monsters[mon.GetMonsterID()], this);

            monsterSelectButtons.Add(newMon);
        }
    }

    public void CreateNewMonster(int ID)
    {
        monsterListCanvasGroup.SetCanvasStatus(false);
        monsterExists[currentMonster] = true;

        UpdateMonsterPref(0, ID);

        currentTeam.SetChooseText(false);
        SetMonsterImage(monsterImages[currentMonster], monsters[ID].stages[0]);
        SetMonsterImage(monsterOutlines[currentMonster], monsters[ID].circleOutline);
        SetMonsterImage(currentTeam.GetMonsterImage(currentMonster), monsters[ID].stages[0]);
    }

    public Sprite GetMonsterImageFromID(int id, int stage)
    {
        return monsters[id].stages[stage];
    }

    public Sprite GetMonsterOutlineFromID(int id)
    {
        return monsters[id].circleOutline;
    }

    private void UpdateMonsterPref(int key, int value)
    {
        currentTeam.GetMonsterPref(currentMonster).monsterValues[key] = value;
        CloudUpdateMonsterPref();
    }

    private void CloudUpdateMonsterPref()
    {
        List<string> teamPrefs = new List<string>();

        for(int i = 0; i < currentTeam.AmountOfPrefs(); i++)
        {
            teamPrefs.Add(currentTeam.GetMonsterPref(i).SeralizedPref());
        }

        StartCoroutine(fireBaseScript.UpdateObject("team" + currentTeam.GetTeamIndex(), teamPrefs));
    }

    private void SetCurrentTeam(teamSelect team)
    {
        SetCanvasStatus(true);
        currentTeam = team;

        for(int i = 0; i < monsterImages.Length; i++)
        {
            if (team.GetMonsterPref(i).monsterValues[0] == 0)
            {
                monsterExists[i] = false;
                monsterImages[i].sprite = emptyMonster;
                monsterOutlines[i].sprite = transparent;
            }
            else
            {
                monsterExists[i] = true;
                monsterImages[i].sprite = team.GetMonsterImage(i).sprite;
                monsterOutlines[i].sprite = monsters[team.GetMonsterPref(i).monsterValues[0]].circleOutline;
            }
        }
    }

    public void SetMonsterImage(Image target, Sprite newImage)
    {
        target.sprite = newImage;
        target.SetNativeSize();
    }
}

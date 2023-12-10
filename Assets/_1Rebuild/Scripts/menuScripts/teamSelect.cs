using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UI;

public class teamSelect : canvasGroupRenderer, IDataPersistence
{
    private teamSelect[] teamSelects;

    [SerializeField] menuTab builderTab;
    [SerializeField] teamBuilder builder;
    [SerializeField] TMP_InputField nameScreen;
    [SerializeField] TextMeshProUGUI chooseText;
    [SerializeField] Image[] monsterImages;
    [SerializeField] monsterPreferences[] monsterPrefs;
    [SerializeField] int teamSelectIndex;

    private string teamName = "";


    public delegate void OnTabSelected(teamSelect team);
    public event OnTabSelected onTeamSelected;


    private void Awake()
    {
        teamSelects = GameObject.FindObjectsOfType<teamSelect>();
        builderTab.onTabSelected += OpenBuilderTab;

        for(int i = 0; i < monsterPrefs.Length; i++)
        {
            monsterPrefs[i] = ScriptableObject.CreateInstance<monsterPreferences>();
        }
    }

    private void InitalizeTeamPref()
    {
        for(int i = 0; i < monsterPrefs.Length; i++)
        {
            if (monsterPrefs[i].monsterValues[0] == 0)
                continue;

            builder.SetMonsterImage(monsterImages[i], builder.GetMonsterImageFromID(monsterPrefs[i].monsterValues[0], 0)); // the 0 means ID and baby form
            SetChooseText(false);
        }

        if (teamName != "")
            nameScreen.text = teamName;
    }

    public void SetChooseText(bool state)
    {
        string text = (state) ? "Create New" : "" ;
        chooseText.text = text;
    }

    private void OpenBuilderTab(bool state)
    {
        if(state)
        {
            SetCanvasStatus(true);
        }
    }

    public monsterPreferences GetMonsterPref(int which)
    {
        return monsterPrefs[which];
    }

    public int AmountOfPrefs()
    {
        return monsterPrefs.Length;
    }

    public Image GetMonsterImage(int which)
    {
        return monsterImages[which];
    }

    public void SetTeamName(string teamName)
    {
        nameScreen.GetComponent<TMP_InputField>().text = teamName;
    }

    public void SelectThisTeam()
    {
        foreach(teamSelect team in teamSelects)
        {
            team.SetCanvasStatus(false);
        }

        onTeamSelected?.Invoke(this);
    }

    public int GetTeamIndex()
    {
        return teamSelectIndex;
    }

    public void LoadData(DataSnapshot data)
    {

        for (int i = 0; i < data.Child("team" + teamSelectIndex).ChildrenCount; ++i)
        {
            GetMonsterPref(i).DeseralizePref(data.Child("team" + teamSelectIndex).Child("" + i).Value.ToString());
        }

        if(data.Child("teamName" + teamSelectIndex).Exists)
        {
            teamName = data.Child("teamName" + teamSelectIndex).Value.ToString();
        }

        InitalizeTeamPref();
    }

    public void LoadOtherPlayersData(string key, object data)
    {
        
    }
}
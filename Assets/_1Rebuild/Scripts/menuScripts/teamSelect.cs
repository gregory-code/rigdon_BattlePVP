using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class teamSelect : canvasGroupRenderer
{
    private teamSelect[] teamSelects;

    [SerializeField] menuTab builderTab;
    [SerializeField] Image[] monsterImages;

    [SerializeField] TMP_InputField nameScreen;

    public delegate void OnTabSelected(teamSelect team);
    public event OnTabSelected onTeamSelected;

    [SerializeField] monsterPreferences[] monsterPrefs;

    private void Awake()
    {
        teamSelects = GameObject.FindObjectsOfType<teamSelect>();
        builderTab.onTabSelected += OpenBuilderTab;

        for(int i = 0; i < monsterPrefs.Length; i++)
        {
            monsterPrefs[i] = ScriptableObject.CreateInstance<monsterPreferences>();
        }
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

    public Image[] GetMonsterImages()
    {
        return monsterImages;
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
}

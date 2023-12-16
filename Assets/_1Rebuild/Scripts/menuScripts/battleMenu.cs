using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class battleMenu : MonoBehaviour, IDataPersistence
{
    [SerializeField] menuTab battleTab;
    public TMP_Dropdown teamDropdown;
    string[] teamNames = new string[3];

    private void Start()
    {
        battleTab.onTabSelected += OpenBattleTab;
        teamDropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(teamDropdown);});
    }

    private void OpenBattleTab(bool state)
    {
        teamDropdown.ClearOptions();
        teamDropdown.AddOptions(new List<string>(teamNames));
    }

    public void DropdownValueChanged(TMP_Dropdown dropdown)
    {
        int selectedOption = dropdown.value;

        SelectTeam(selectedOption);
    }

    private void SelectTeam(int which)
    {
        Debug.Log("Selected team: " + which);

    }

    public void UpdateTeamName(int which, string newName)
    {
        teamNames[which - 1] = newName;
    }

    public IEnumerator LoadData(DataSnapshot data)
    {
        for(int i = 1; i < 4; i++)
        {
            if (data.Child("teamName" + i).Exists)
            {
                teamNames[i - 1] = data.Child("teamName" + i).Value.ToString();
            }
        }

        yield return new WaitForEndOfFrame();
    }

    public void LoadOtherPlayersData(string key, object data)
    {

    }
}

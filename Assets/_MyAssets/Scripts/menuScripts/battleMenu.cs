using Firebase.Database;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class battleMenu : MonoBehaviour, IDataPersistence
{
    notifScript NotificationScript;

    [SerializeField] menuTab battleTab;
    [SerializeField] teamBuilder teamBuilderScript;

    [SerializeField] TextMeshProUGUI[] teamNames = new TextMeshProUGUI[3];
    [SerializeField] TextMeshProUGUI selectedTeamName;
    [SerializeField] Transform teamOptions;
    [SerializeField] Image[] monsterImages;

    [SerializeField] TextMeshProUGUI selectedFormatName;
    [SerializeField] Transform formatOptions;
    [SerializeField] Image formatIamge;
    [SerializeField] Sprite[] formatImages;

    [SerializeField] Transform findMatchTransform;
    [SerializeField] TextMeshProUGUI findMatchText;
    [SerializeField] Image findMatchImage;

    [SerializeField] Image topBar;
    [SerializeField] Image bottomBar;
    [SerializeField] TextMeshProUGUI barText;

    private bool openTeamOptions = false;
    private bool openFormatOptions = false;
    private bool searchingForMatch = false;

    private int selectedTeam;

    [SerializeField] teamSelect[] teamSelects;

    [SerializeField] format currentFormat = format.standard;

    public delegate void OnSearchForMatch(format chosenFormat, bool searching);
    public event OnSearchForMatch onSearchForMatch;

    public enum format
    {
        standard,
        random,
        draft
    }

    private void Start()
    {
        NotificationScript = GameObject.FindObjectOfType<notifScript>();
        battleTab.onTabSelected += OpenBattleTab;
    }

    private void Update()
    {
        Vector3 teamOptionsLerp = (openTeamOptions) ? new Vector3(-315, 17) : new Vector3(-315, 140);
        Vector3 teamOptionsScale = (openTeamOptions) ? Vector3.one : new Vector3(1, 0, 0);

        teamOptions.transform.localPosition = Vector3.Lerp(teamOptions.transform.localPosition, teamOptionsLerp, 6 * Time.deltaTime);
        teamOptions.transform.localScale = Vector3.Lerp(teamOptions.transform.localScale, teamOptionsScale, 6 * Time.deltaTime);

        Vector3 formatOptionsLerp = (openFormatOptions) ? new Vector3(102, 17) : new Vector3(102, 126);
        Vector3 formatOptionsScale = (openFormatOptions) ? Vector3.one : new Vector3(1, 0, 0);

        formatOptions.transform.localPosition = Vector3.Lerp(formatOptions.transform.localPosition, formatOptionsLerp, 6 * Time.deltaTime);
        formatOptions.transform.localScale = Vector3.Lerp(formatOptions.transform.localScale, formatOptionsScale, 6 * Time.deltaTime);

        Vector3 findMatchLerp = (openTeamOptions == true || openFormatOptions == true) ? new Vector3(-90, -150) : new Vector3(-90, 20);
        findMatchTransform.localPosition = Vector3.Lerp(findMatchTransform.localPosition, findMatchLerp, 6 * Time.deltaTime);

        int lerpAmount = (searchingForMatch) ? 1 : 0 ;
        findMatchImage.fillAmount = Mathf.Lerp(findMatchImage.fillAmount, lerpAmount, 6 * Time.deltaTime);

        topBar.fillAmount = Mathf.Lerp(topBar.fillAmount, lerpAmount, 6 * Time.deltaTime);
        bottomBar.fillAmount = Mathf.Lerp(bottomBar.fillAmount, lerpAmount, 6 * Time.deltaTime);
    }

    public void OpenTeamDropdown()
    {
        openTeamOptions = !openTeamOptions;
    }

    public void OpenFormatDropdown()
    {
        if(searchingForMatch)
        {
            NotificationScript.createNotif($"Cannot change formats while looking for a match", Color.yellow);
            return;
        }

        openFormatOptions = !openFormatOptions;
    }

    private void OpenBattleTab(bool state)
    {

    }

    public void TeamSelected(int selectedOption)
    {
        openTeamOptions = false;
        teamBuilderScript.AssignMonsterImagesFromBattleMenu(teamSelects[selectedOption], monsterImages);
        selectedTeam = selectedOption;
        selectedTeamName.text = teamNames[selectedOption].text;
    }

    public void FormatSelected(int selectedOption)
    {
        openFormatOptions = false;
        formatIamge.sprite = formatImages[selectedOption];

        switch (selectedOption)
        {
            case 0:
                selectedFormatName.text = "Standard";
                currentFormat = format.standard;
                break;

            case 1:
                selectedFormatName.text = "Random Teams";
                currentFormat = format.random;
                break;

            case 2:
                selectedFormatName.text = "Draft League";
                currentFormat = format.draft;
                break;
        }
    }

    public void UpdateTeamName(int which, string newName)
    {
        teamNames[which - 1].text = newName;
    }

    public monsterPreferences[] GetMonsterPrefsFromSelectedTeam()
    {
        monsterPreferences[] monsters = new monsterPreferences[3];
        for(int i = 0; i < 3; i++)
        {
            monsters[i] = teamSelects[selectedTeam].GetMonsterPref(i);
        }
        return monsters;
    }

    public string GetSelectedTeamName()
    {
        return teamSelects[selectedTeam].GetTeamName();
    }

    public void FindRandomMatch()
    {
        openTeamOptions = false;
        openFormatOptions = false;

        searchingForMatch = !searchingForMatch;

        if (PhotonNetwork.InRoom == false || PhotonNetwork.CurrentRoom.Name != "LobbyRoom")
        {
            NotificationScript.createNotif($"Not connected", Color.red);
            searchingForMatch = false;
        }

        onSearchForMatch?.Invoke(currentFormat, searchingForMatch);
        findMatchText.text = (searchingForMatch) ? "Cancel search" : "Find a random Opponent ";
        barText.text = (searchingForMatch) ? "Searching for an opponent..." : "";
    }

    public IEnumerator LoadData(DataSnapshot data)
    {
        for(int i = 1; i < 4; i++)
        {
            if (data.Child("teamName" + i).Exists)
            {
                teamNames[i - 1].text = data.Child("teamName" + i).Value.ToString();
            }
        }

        selectedTeamName.text = teamNames[0].text;

        yield return new WaitForEndOfFrame();

        teamBuilderScript.AssignMonsterImagesFromBattleMenu(teamSelects[0], monsterImages);
    }

    public void LoadOtherPlayersData(string key, object data)
    {

    }
}

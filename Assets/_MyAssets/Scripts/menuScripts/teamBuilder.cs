using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class teamBuilder : canvasGroupRenderer
{
    [SerializeField] FirebaseScript fireBaseScript;
    [SerializeField] battleMenu battleMenuScript;

    [SerializeField] Sprite emptyMonster;
    [SerializeField] Sprite transparent;
    [SerializeField] menuTab builderTab;
    [SerializeField] monster[] monsters;
    [SerializeField] Image[] monsterImages;
    [SerializeField] Image[] monsterOutlines;
    [SerializeField] bool[] monsterExists;
    [SerializeField] TMP_InputField teamNameInput;
    private teamSelect currentTeam;
    private int currentMonster;
    private monsterTab[] monsterTabs;


    private void Awake()
    {
        monsterTabs = GameObject.FindObjectsOfType<monsterTab>();
        for(int i = 0; i < monsterTabs.Length; i++)
        {
            monsterTabs[i].onMonsterDelete += SpawnDeleteButton;
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
        monsterEditCanvasGroup.SetCanvasStatus(false);
        statGrowthPage.SetActive(false);
        SetOutline(0, false);
        SetCanvasStatus(false);
        foreach(monsterTab tab in monsterTabs)
        {
            tab.SetTab(false);
        }
    }

    private void SelectMonster(int whichMon)
    {
        dontPopup = true;

        currentMonster = whichMon;
        SetOutline(whichMon, true);
        statGrowthPage.SetActive(false);
        bStatGrowthMenu = false;

        if (monsterExists[whichMon])
        {
            DeleteContent();
            SetMoveContent();
            SetGrowthBars();

            for (int i = 1; i < 4; i++)
            {
                moveType = i;
                SetMoveTransparency(currentTeam.GetMonsterPref(currentMonster).monsterValues[i]);
            }

            SetEditorStats(level);
            monsterEditCanvasGroup.SetCanvasStatus(true);
            monsterListCanvasGroup.SetCanvasStatus(false);
        }
        else
        {
            monsterEditCanvasGroup.SetCanvasStatus(false);
            monsterListCanvasGroup.SetCanvasStatus(true);
            OpenMonsterList();
        }

        dontPopup = false;
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
        teamNameInput.text = currentTeam.GetTeamName();

        for (int i = 0; i < monsterImages.Length; i++)
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

    private int[] GetCurrentStatBlock()
    {
        monster mon = monsters[currentTeam.GetMonsterPref(currentMonster).monsterValues[0]];
        int[] stats = mon.GetStatBlock();
        return stats;
    }

    public void OnEndEditTeamName(string teamName)
    {
        StartCoroutine(fireBaseScript.UpdateObject("teamName" + currentTeam.GetTeamIndex(), teamName));
        currentTeam.UpdateTeamName(teamName);

        battleMenuScript.UpdateTeamName(currentTeam.GetTeamIndex(), teamName);
    }

    public void AssignMonsterImagesFromBattleMenu(teamSelect selectedTeam, Image[] monsterImages)
    {
        for(int i = 0; i < monsterImages.Length; i++)
        {
            if (selectedTeam.GetMonsterPref(i).monsterValues[0] == 0)
            {
                monsterImages[i].sprite = transparent;
            }
            else
            {
                monsterImages[i].sprite = monsters[selectedTeam.GetMonsterPref(i).monsterValues[0]].stages[0];
            }

        }
    }

    [Header("Monster List")]
    [SerializeField] canvasGroupRenderer monsterListCanvasGroup;
    [SerializeField] monsterSelectButton monsterSelectPrefab;
    [SerializeField] Transform monsterSelectList;
    [SerializeField] GameObject deletePrefab;

    List<monsterSelectButton> monsterSelectButtons = new List<monsterSelectButton>();

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
        if(ID != 2 && ID != 4  && ID != 6 && ID != 8 && ID != 14)
        {
            GameObject.FindObjectOfType<notifScript>().createNotif("Not yet implimented :(", Color.red);
            return;
        }

        monsterListCanvasGroup.SetCanvasStatus(false);
        monsterExists[currentMonster] = true;

        UpdateMonsterPref(0, ID);

        currentTeam.SetChooseText(false);
        SetMonsterImage(monsterImages[currentMonster], monsters[ID].stages[0]);
        SetMonsterImage(monsterOutlines[currentMonster], monsters[ID].circleOutline);
        SetMonsterImage(currentTeam.GetMonsterImage(currentMonster), monsters[ID].stages[0]);
    }

    private void SpawnDeleteButton(GameObject parent, int which)
    {
        currentMonster = which;
        if (monsterExists[which])
        {
            GameObject deleteButton = Instantiate(deletePrefab, parent.transform);
            deleteButton.transform.localPosition = Vector3.zero;
            deleteButton.GetComponent<Button>().onClick.AddListener(() => DeleteMonster(which));
            Destroy(deleteButton, 3f);
        }
    }

    private void DeleteMonster(int whichMonster)
    {
        monsterExists[currentMonster] = false;
        statGrowthPage.SetActive(false);
        monsterEditCanvasGroup.SetCanvasStatus(false);
        monsterListCanvasGroup.SetCanvasStatus(false);

        for(int i = 0; i < 8; i++)
        {
            UpdateMonsterPref(i, 0);
        }

        SetMonsterImage(monsterImages[currentMonster], emptyMonster);
        SetMonsterImage(monsterOutlines[currentMonster], transparent);
        SetMonsterImage(currentTeam.GetMonsterImage(currentMonster), transparent);
    }

    [Header("Monster Stats")]
    [SerializeField] canvasGroupRenderer monsterEditCanvasGroup;
    [SerializeField] GameObject statGrowthPage;
    [SerializeField] Slider growthPointsSlider;
    [SerializeField] TextMeshProUGUI pointsText1;
    [SerializeField] TextMeshProUGUI pointsText2;
    [SerializeField] Slider[] statSliders;
    [SerializeField] Image[] growthIcons;
    [SerializeField] Sprite[] growthSprites;
    [SerializeField] Color[] growthColors;
    [SerializeField] TextMeshProUGUI[] statTexts;
    [SerializeField] statNotif statNotification;
    [SerializeField] TextMeshProUGUI levelText;
    bool dontPopup;
    int growthPoints = 4;
    int level = 1;
    int statIndex;
    bool bStatGrowthMenu;

    public void StatGrowthState()
    {
        bStatGrowthMenu = !bStatGrowthMenu;
        statGrowthPage.SetActive(bStatGrowthMenu);

        if (bStatGrowthMenu == false)
            return;

        SetGrowthBars();
    }

    private void SetGrowthBars()
    {
        growthPoints = 4;
        for (int i = 0; i < statSliders.Length; i++)
        {
            int growthValue = currentTeam.GetMonsterPref(currentMonster).monsterValues[i + 4];

            if (passiveIsGivingGrowth && i == passiveGrowthID)
                growthValue--;

            statSliders[i].value = growthValue;
            growthPoints -= growthValue;

            if (passiveIsGivingGrowth && i == passiveGrowthID)
                growthValue++;

            SetUIFromGrowth(i, growthValue);
        }

        UpdateGrowthPoints(0);
    }

    private void SetUIFromGrowth(int which, int growth)
    {
        growthIcons[which].sprite = growthSprites[growth];
        growthIcons[which].color = growthColors[growth];
    }

    public void SetStatIndex(int index)
    {
        statIndex = index;
    }

    public void ChangeStatGrowth(float value)
    {
        if (isStatGrowthIncrease(value))
        {
            if (growthPoints >= 1)
            {
                if (passiveIsGivingGrowth && passiveGrowthID + 4 == statIndex)
                    value++;
                UpdateStatGrowth(-1, int.MinValue, value, statIndex);
            }
            else
            {
                statSliders[statIndex - 4].value = currentTeam.GetMonsterPref(currentMonster).monsterValues[statIndex];
                return;
            }
        }
        else
        {
            if (passiveIsGivingGrowth && passiveGrowthID + 4 == statIndex)
                value++;
            UpdateStatGrowth(1, int.MaxValue, value, statIndex);
        }
    }

    private void UpdateStatGrowth(int growthPoint, int minOrMax, float value, int statIndex)
    {
        UpdateGrowthPoints(growthPoint);
        UpdateMonsterPref(statIndex, (int)value);
        SetUIFromGrowth(statIndex - 4, (int)value);
        SetStatsFromGrowthSlider(minOrMax, statIndex);
    }

    private void SetStatsFromGrowthSlider(int previousLevel, int statIndex)
    {
        SetEditorStat(statIndex - 4, previousLevel, GetCurrentStatBlock(), true);
        UpdateContent();
    }

    private void UpdateGrowthPoints(int change)
    {
        growthPoints += change;
        growthPointsSlider.value = growthPoints;
        pointsText1.text = growthPoints + "";
        pointsText2.text = growthPoints + "";
    }

    private bool isStatGrowthIncrease(float newValue)
    {
        return newValue >= currentTeam.GetMonsterPref(currentMonster).monsterValues[statIndex];
    }

    public void SetEditorStats(float level)
    {
        int previousLevel = this.level;
        this.level = (int)level;

        levelText.text = "Level " + this.level;

        for(int i = 0; i < GetCurrentStatBlock().Length; i++)
        {
            SetEditorStat(i, previousLevel, GetCurrentStatBlock(), false);
        }

        UpdateContent();
    }

    private void SetEditorStat(int statIndex, int previousLevel, int[] stats, bool isSliderUpdate)
    {
        int level = this.level - 1;

        float statGrowth = currentTeam.GetMonsterPref(currentMonster).monsterValues[statIndex + 4];
        float statToAdd = (statIndex == 0) ? ((statGrowth + 1f) * 3f * level) + 2 : ((statGrowth + 1) / 2 * level);
        stats[statIndex] += (int)Mathf.Abs(statToAdd);
        statTexts[statIndex].text = stats[statIndex] + "";

        if (dontPopup)
            return;

        float statChange = 0;

        if(isSliderUpdate)
        {
            statChange = (statIndex == 0) ? level * 3 : 0.5f * level;
        }
        else
        {
            statChange = (statIndex == 0) ? (statGrowth + 1f) * 3f : (1 + statGrowth) / 2;
        }

        if (level < previousLevel) statChange *= -1f;
        statNotification.spawnStatPopup(statTexts[statIndex].transform, statChange);
    }

    private int GetTier()
    {
        switch(level)
        {
            case < 3:
                return 0;

            case 3:
            case 4:
            case 5:
                return 1;

            case > 5:
                return 2;
        }
    }

    [Header("Monster Moves")]
    [SerializeField] Transform[] moveParents;
    [SerializeField] List<moveContent> moveContentList = new List<moveContent>();
    [SerializeField] Image[] attackButtons;
    [SerializeField] Image[] supportButtons;
    [SerializeField] Image[] passiveButtons;
    bool passiveIsGivingGrowth;
    int passiveGrowthID;
    int moveType = 1;

    public void SetMoveIndex(int index)
    {
        moveType = index;
    }

    public void SetMovePref(int moveIndex)
    {
        SetMoveTransparency(moveIndex);
        UpdateMonsterPref(moveType, moveIndex);
    }

    public void CheckPassiveGrowth(moveContent content, bool updateImmedately)
    {
        if (moveType != 3) //only works for passives
            return;

        if (passiveIsGivingGrowth && content.DoesPassiveGiveGrowth() == true && content.GetPassiveGrowth() == passiveGrowthID)
            return;

        int growthValue = 0;

        if(passiveIsGivingGrowth && content.DoesPassiveGiveGrowth() == true && content.GetPassiveGrowth() != passiveGrowthID)
        {
            growthValue = currentTeam.GetMonsterPref(currentMonster).monsterValues[passiveGrowthID + 4];
            if(growthValue != 0)
                UpdatePassiveGrowth(true, growthValue, updateImmedately);
            passiveIsGivingGrowth = false;
        }

        passiveGrowthID = content.GetPassiveGrowth();

        growthValue = currentTeam.GetMonsterPref(currentMonster).monsterValues[passiveGrowthID + 4];

        if (passiveIsGivingGrowth && content.DoesPassiveGiveGrowth() == false)
        {
            UpdatePassiveGrowth(true, growthValue, updateImmedately);
        }
        else if(passiveIsGivingGrowth == false && content.DoesPassiveGiveGrowth() == true)
        {
            UpdatePassiveGrowth(false, growthValue, updateImmedately);
        }

        passiveIsGivingGrowth = content.DoesPassiveGiveGrowth();
    }

    private void UpdatePassiveGrowth(bool removeGrowth, int growthValue, bool updateImmedately)
    {
        if (removeGrowth)
        {
            growthValue--;

            if (updateImmedately)
                UpdateStatGrowth(0, int.MinValue, growthValue, passiveGrowthID + 4);
        }
        else
        {
            growthValue++;

            if (updateImmedately)
                UpdateStatGrowth(0, int.MaxValue, growthValue, passiveGrowthID + 4);
        }
    }

    private void SetMoveTransparency(int moveIndex)
    {
        switch (moveType)
        {
            case 1:
                attackButtons[0].color = new Color(1, 1, 1, 0.1f);
                attackButtons[1].color = new Color(1, 1, 1, 0.1f);
                if (moveIndex == 0) return;
                attackButtons[moveIndex - 1].color = new Color(1, 1, 1, 1);
                break; //attack

            case 2: // support
                supportButtons[0].color = new Color(1, 1, 1, 0.1f);
                supportButtons[1].color = new Color(1, 1, 1, 0.1f);
                if (moveIndex == 0) return;
                supportButtons[moveIndex - 1].color = new Color(1, 1, 1, 1);
                break;

            case 3: // passive
                passiveButtons[0].color = new Color(1, 1, 1, 0.1f);
                passiveButtons[1].color = new Color(1, 1, 1, 0.1f);
                if (moveIndex == 0) return;
                passiveButtons[moveIndex - 1].color = new Color(1, 1, 1, 1);
                break;
        }
    }

    private void SetMoveContent()
    {
        moveContent[] contents = monsters[currentTeam.GetMonsterPref(currentMonster).monsterValues[0]].GetMoveContents();
        if(contents.Length > 0)
        {
            moveType = 3;

            for (int i = 0; i < contents.Length; i++)
            {
                moveContent newContent = Instantiate(contents[i], moveParents[i]);
                newContent.transform.localPosition = Vector3.zero;
                newContent.SetUpConnection(this);

                if(newContent.DoesPassiveGiveGrowth() && currentTeam.GetMonsterPref(currentMonster).monsterValues[3] == i - 3)
                {
                    passiveIsGivingGrowth = true;
                    passiveGrowthID = newContent.GetPassiveGrowth();
                }

                moveContentList.Add(newContent);
            }
        }
    }

    private void DeleteContent()
    {
        passiveIsGivingGrowth = false;
        foreach (moveContent content in moveContentList)
        {
            content.RemoveConnection(this);
            Destroy(content.gameObject);
        }
        moveContentList.Clear();
    }

    private void UpdateContent()
    {
        int[] stats = GetCurrentStatBlock();
        int level = this.level - 1;

        for (int i = 0; i < stats.Length; i++)
        {
            float growth = currentTeam.GetMonsterPref(currentMonster).monsterValues[i + 4];
            float statToAdd = (i == 0) ? ((growth + 3) * level) : (((growth + 1) / 2) * level);
            stats[i] += (int)Mathf.Abs(statToAdd);
        }

        foreach(moveContent content in moveContentList)
        {
            content.SetTier(GetTier(), stats);
        }
    }
}

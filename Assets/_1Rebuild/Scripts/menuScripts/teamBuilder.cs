using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
        currentMonster = whichMon;
        SetOutline(whichMon, true);
        statGrowthPage.SetActive(false);
        bStatGrowthMenu = false;

        if (monsterExists[whichMon])
        {
            SetGrowthBars();
            dontShowPopup = 4;
            SetEditorStats(1);
            monsterEditCanvasGroup.SetCanvasStatus(true);
            monsterListCanvasGroup.SetCanvasStatus(false);
        }
        else
        {
            monsterEditCanvasGroup.SetCanvasStatus(false);
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

    [Header("Monster List")]
    [SerializeField] canvasGroupRenderer monsterListCanvasGroup;
    [SerializeField] monsterSelectButton monsterSelectPrefab;
    [SerializeField] Transform monsterSelectList;

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
        monsterListCanvasGroup.SetCanvasStatus(false);
        monsterExists[currentMonster] = true;

        UpdateMonsterPref(0, ID);

        currentTeam.SetChooseText(false);
        SetMonsterImage(monsterImages[currentMonster], monsters[ID].stages[0]);
        SetMonsterImage(monsterOutlines[currentMonster], monsters[ID].circleOutline);
        SetMonsterImage(currentTeam.GetMonsterImage(currentMonster), monsters[ID].stages[0]);
    }

    [Header("Monster Edit")]
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
    int dontShowPopup;
    int growthPoints = 5;
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
        growthPoints = 5;
        for (int i = 0; i < statSliders.Length; i++)
        {
            int growthValue = currentTeam.GetMonsterPref(currentMonster).monsterValues[i + 4];
            statSliders[i].value = growthValue;
            growthPoints -= growthValue;
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
                UpdateGrowthPoints(-1);
                UpdateMonsterPref(statIndex, (int)value);
                SetUIFromGrowth(statIndex - 4, (int)value);
                SetStatsFromGrowthSlider(int.MinValue, statIndex);
            }
            else
            {
                statSliders[statIndex - 4].value = currentTeam.GetMonsterPref(currentMonster).monsterValues[statIndex];
                return;
            }
        }
        else
        {
            UpdateGrowthPoints(1);
            UpdateMonsterPref(statIndex, (int)value);
            SetUIFromGrowth(statIndex - 4, (int)value);
            SetStatsFromGrowthSlider(int.MaxValue, statIndex);
        }
    }

    private void SetStatsFromGrowthSlider(int previousLevel, int statIndex)
    {
        monster mon = monsters[currentTeam.GetMonsterPref(currentMonster).monsterValues[0]];
        float[] stats = mon.GetStatBlock();

        SetEditorStat(statIndex - 4, previousLevel, stats, true);
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
        monster mon = monsters[currentTeam.GetMonsterPref(currentMonster).monsterValues[0]];
        float[] stats = mon.GetStatBlock();
        int previousLevel = this.level;
        this.level = (int)level;

        for(int i = 0; i < stats.Length; i++)
        {
            SetEditorStat(i, previousLevel, stats, false);
        }
    }

    private void SetEditorStat(int statIndex, int previousLevel, float[] stats, bool isSliderUpdate)
    {
        int level = this.level - 1;

        float statGrowth = currentTeam.GetMonsterPref(currentMonster).monsterValues[statIndex + 4];
        stats[statIndex] += (statIndex == 0) ? ((statGrowth + 3) * level) : ((statGrowth + 1) / 2 * level);
        statTexts[statIndex].text = stats[statIndex] + "";

        if(dontShowPopup > 0)
        {
            dontShowPopup--;
            return;
        }

        float statChange = 0;

        if(isSliderUpdate)
        {
            statChange = (statIndex == 0) ? level * 1 : 0.5f * level;
        }
        else
        {
            statChange = (statIndex == 0) ? 3 + statGrowth : (1 + statGrowth) / 2;
        }

        if (level < previousLevel) statChange *= -1f;
        statNotification.spawnStatPopup(statTexts[statIndex].transform, statChange);
    }
}

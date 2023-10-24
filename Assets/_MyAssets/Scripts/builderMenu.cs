using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class builderMenu : MonoBehaviour, IDataPersistence
{
    notifScript NotificationScript;

    //battleMenuManager
    BattleMenuManager battleMenuManager;

    //Firebase Data
    FirebaseScript fireBaseScript;

    //Online
    onlineScript OnlineScript;

    //statPopup
    statNotif statNotification;

    //Aniamtor
    private Animator menuAnimations;

    //Varibles for menu
    public int selectedCritter;
    public bool bDeleteCritter;
    public int selectedTeam { get; private set; }
    public bool bSelectedTeam { get; private set; }

    private bool bCritterList;

    [Header("Sprites")]
    [SerializeField] private Sprite transparentSprite;
    [SerializeField] Sprite addCritterSprite;
    [SerializeField] Sprite[] critterSprites;
    [SerializeField] Sprite[] growthSprites;
    
    [Header("Prefabs")]
    [SerializeField] GameObject deletePrefab;
    [SerializeField] GameObject critterPrefab;
    [SerializeField] Transform critterParent;


    [Header("Teams")]
    [SerializeField] critter[] critterCollection;
    private Transform critterList;
    private List<GameObject> critterGameObjectList = new List<GameObject>();
    private TMP_InputField[] teamName = new TMP_InputField[3];
    private TMP_Text[] createNewText = new TMP_Text[3];
    bool bShaderDissolve;

    [SerializeField] SpriteRenderer[] team1CritterImages; //each team
    [SerializeField] SpriteRenderer[] team2CritterImages;
    [SerializeField] SpriteRenderer[] team3CritterImages;

    [SerializeField] critterBuild[] team1CritterBuilds = new critterBuild[3];
    [SerializeField] critterBuild[] team2CritterBuilds = new critterBuild[3];
    [SerializeField] critterBuild[] team3CritterBuilds = new critterBuild[3];

    public critterBuild[] activeCritterTeam = new critterBuild[3];
    
    private Transform[] team1CritterTransforms = new Transform[3];
    private Transform[] team2CritterTransforms = new Transform[3];
    private Transform[] team3CritterTransforms = new Transform[3];

    [SerializeField] Material[] critterShaders;

    [Header("Stats Growth Page")]
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text[] statValueTexts;
    [SerializeField] TMP_Text[] growthTexts;
    [SerializeField] Image[] statGrowths;

    private Transform editCritterTransform;
    private Transform growthPageTransform;
    private Transform growthPointsTransform;
    private TMP_Text growthPointsText;
    private bool bGrowthEditor;
    float level;
    
    public bool bEditCritter;

    private Image[] hpGrowthButtons = new Image[4];
    private Image[] strGrowthButtons = new Image[4];
    private Image[] magicGrowthButtons = new Image[4];
    private Image[] speedGrowthButtons = new Image[4];

    private void InitializeVariables()
    {
        battleMenuManager = GameObject.FindGameObjectWithTag("BattleMenuManager").GetComponent<BattleMenuManager>();
        statNotification = GetComponent<statNotif>();
        fireBaseScript = GameObject.FindGameObjectWithTag("Data").GetComponent<FirebaseScript>();
        OnlineScript = GameObject.FindGameObjectWithTag("Online").GetComponent<onlineScript>();
        NotificationScript = GameObject.FindGameObjectWithTag("Canvas").GetComponent<notifScript>();
        menuAnimations = GameObject.Find("menu").GetComponent<Animator>();

        critterList = transform.Find("critterList").GetComponent<Transform>();
        editCritterTransform = transform.Find("editCritter").GetComponent<Transform>();
        growthPageTransform = editCritterTransform.Find("growthPage").GetComponent<Transform>();
        growthPointsTransform = editCritterTransform.Find("growthPoints").GetComponent<Transform>();
        growthPointsText = growthPointsTransform.Find("points1").GetComponent<TMP_Text>();

        for(int i = 0; i < hpGrowthButtons.Length; ++i)
        {
            int find = i - 1;
            hpGrowthButtons[i] = growthPageTransform.Find(find + "HP").GetComponent<Image>();
            strGrowthButtons[i] = growthPageTransform.Find(find + "Str").GetComponent<Image>();
            magicGrowthButtons[i] = growthPageTransform.Find(find + "Magic").GetComponent<Image>();
            speedGrowthButtons[i] = growthPageTransform.Find(find + "Speed").GetComponent<Image>();
        }

        for (int i = 0; i < team1CritterBuilds.Length; ++i)
        {
            int team = i + 1;
            teamName[i] = transform.Find("team" + team).transform.Find("nameScreen").GetComponent<TMP_InputField>();
            createNewText[i] = transform.Find("team" + team).transform.Find("chooseText").GetComponent<TMP_Text>();

            team1CritterTransforms[i] = team1CritterImages[i].GetComponent<Transform>();
            team2CritterTransforms[i] = team2CritterImages[i].GetComponent<Transform>();
            team3CritterTransforms[i] = team3CritterImages[i].GetComponent<Transform>();
        }
    }

    private void Awake()
    {
        InitializeVariables();

        bShaderDissolve = true;

        selectedCritter = -1;
        level = 1;
        critterShaders[0].SetFloat("Vector1_E974001A", 2);
        critterShaders[1].SetFloat("Vector1_E974001A", 2);
        critterShaders[2].SetFloat("Vector1_E974001A", 2);
    }

    void Update()
    {
        if (battleMenuManager.state != BattleMenuManager.battleMenuState.Menu && battleMenuManager.state != BattleMenuManager.battleMenuState.Searching) return;

        float critterLerp = (bCritterList) ? -64 : -569;
        critterList.localPosition = Vector2.Lerp(critterList.localPosition, new Vector2(-26, critterLerp), 6 * Time.deltaTime);

        if (bSelectedTeam == false) return;

        float editCritterLerp = (bEditCritter) ? 0 : -500;
        editCritterTransform.localPosition = Vector2.Lerp(editCritterTransform.localPosition, new Vector2(0, editCritterLerp), 7.5f * Time.deltaTime);

        float editGrowthLerp = (bGrowthEditor) ? -76.4f : 43;
        growthPageTransform.localPosition = Vector2.Lerp(growthPageTransform.localPosition, new Vector2(-17, editGrowthLerp), 7.5f * Time.deltaTime);

        float editGrowthPoints = (bGrowthEditor) ? 420f : 800;
        growthPointsTransform.localPosition = Vector2.Lerp(growthPointsTransform.localPosition, new Vector2(editGrowthPoints, -60), 7.5f * Time.deltaTime);

        float editGrowthSize = (bGrowthEditor) ? 1 : 0.6f;
        growthPageTransform.localScale = Vector2.Lerp(growthPageTransform.localScale, new Vector2(1, editGrowthSize), 7.5f * Time.deltaTime);

        this.critterLerp();
    }

    private void critterLerp()
    {
        Transform[] critterTransforms = new Transform[team1CritterTransforms.Length];
        switch (selectedTeam)
        {
            case 0: critterTransforms = team1CritterTransforms; break;
            case 1: critterTransforms = team2CritterTransforms; break;
            case 2: critterTransforms = team3CritterTransforms; break;
        }

        for (int i = 0; i < team1CritterTransforms.Length; ++i)
        {
            float critterIconLerp = (selectedCritter == i) ? 30 : 0;
            int critterOutlineLerp = (selectedCritter == i) ? 1 : 0;
            
            Image critterOutline = critterTransforms[i].transform.Find("outline").GetComponent<Image>();
            critterOutline.fillAmount = Mathf.Lerp(critterOutline.fillAmount, critterOutlineLerp, 30 * Time.deltaTime);
            
            critterTransforms[i].localPosition = Vector2.Lerp(critterTransforms[i].localPosition, new Vector2(critterTransforms[i].localPosition.x, critterIconLerp), 10 * Time.deltaTime);

            int dissolve = (bShaderDissolve) ? 2 : 0;
            critterShaders[i].SetFloat("Vector1_E974001A", Mathf.Lerp(critterShaders[i].GetFloat("Vector1_E974001A"), dissolve, 6 * Time.deltaTime));
        }
    }

    private void deserializeCritterValue(string toDeserialize, int critterValue)
    {
        List<string> teams = toDeserialize.Split('*').ToList<string>();

        for (int i = 0; i < teams.Count; ++i)
        {
            critterBuild[] critterGroupBuilds = getCritterBuildGroup(i);
            SpriteRenderer[] critterGroup = getCritterGroup(i);

            List<string> ids = teams[i].Split('_').ToList<string>();


            for (int x = 0; x < teams.Count; ++x)
            {
                critterGroupBuilds[x].critterValue[critterValue] = int.Parse(ids[x]);

                if(critterValue == 0)
                {
                    if (critterGroupBuilds[x].critterValue[critterValue] != -1)
                    {
                        createNewText[i].text = "";
                        critterGroup[x].sprite = critterCollection[critterGroupBuilds[x].critterValue[critterValue]].stages[0];
                        critterGroup[x].transform.Find("outline").GetComponent<Image>().sprite = critterCollection[critterGroupBuilds[x].critterValue[critterValue]].circleOutline;
                    }
                }
            }
        }
    }

    public void openGrowthEditor()
    {
        bGrowthEditor = !bGrowthEditor;
    }

    public void inBuilderSprites(int team)
    {
        SpriteRenderer[] critterGroup = getCritterGroup(team);

        for(int i = 0; i < critterGroup.Length; ++i)
        {
            if (critterGroup[i].sprite == transparentSprite)
            {
                critterGroup[i].sprite = addCritterSprite;
            }
        }
    }

    public void inBuilderMenuSprites(int team)
    {
        bSelectedTeam = false;
        bCritterList = false;
        selectedCritter = -1;
        selectedTeam = -1;

        critterBuild[] critterBuild = getCritterBuildGroup(team);
        SpriteRenderer[] critterGroup = getCritterGroup(team);

        for(int i = 0; i < critterGroup.Length; ++i)
        {
            if (critterBuild[i].critterValue[0] == -1)
            {
                critterGroup[i].sprite = transparentSprite;
            }
        }

        if (critterGroup[0].sprite == transparentSprite && critterGroup[1].sprite == transparentSprite && critterGroup[2].sprite == transparentSprite)
        {
            createNewText[team].text = "Create New";
            teamName[team].text = "";
        }
    }

    private void processStatGrowth(int change, int statID, Image[] growthButtons)
    {
        if (validPointReduction(0, change) == false) { return; }
        critterBuild[] critterIDGroup = getCritterBuildGroup(selectedTeam);

        float previousStat = float.Parse(statValueTexts[statID].text);

        changeGrowth(change, growthButtons, statID);

        float currerntStat = float.Parse(statValueTexts[statID].text);

        statNotification.spawnStatPopup(statValueTexts[statID].transform, currerntStat - previousStat);
    }

    public void changeHPGrowth(int change)
    {
        processStatGrowth(change, 0, hpGrowthButtons);
    }

    public void changeStrGrowth(int change)
    {
        processStatGrowth(change, 1, strGrowthButtons);
    }

    public void changeMagicGrowth(int change)
    {
        processStatGrowth(change, 2, magicGrowthButtons);
    }

    public void changeSpeedGrowth(int change)
    {
        processStatGrowth(change, 3, speedGrowthButtons);
    }

    private bool validPointReduction(int statToExclude, int newGrowth)
    {
        critterBuild[] critterBuild = getCritterBuildGroup(selectedTeam);

        int totalPoints = 1;
        if (statToExclude + 4 != 4) totalPoints -= critterBuild[selectedCritter].critterValue[4]; //hp
        if (statToExclude + 4 != 5) totalPoints -= critterBuild[selectedCritter].critterValue[5]; //str
        if (statToExclude + 4 != 6) totalPoints -= critterBuild[selectedCritter].critterValue[6]; //magic
        if (statToExclude + 4 != 7) totalPoints -= critterBuild[selectedCritter].critterValue[7]; //speed

        totalPoints -= newGrowth;

        if (totalPoints <= -1) return false;

        return true;
    }

    private void changeGrowth(int newGrowth, Image[] growthButtons, int statID)
    {
        critterBuild[] critterBuild = getCritterBuildGroup(selectedTeam);

        critterBuild[selectedCritter].critterValue[statID + 4] = newGrowth;

        foreach (Image img in growthButtons)
        {
            img.color = new Color(1, 1, 1, 0.1f);
        }

        Color growthColor = getGrowthColor(newGrowth);

        growthButtons[newGrowth + 1].color = new Color(1, 1, 1, 1);
        growthTexts[statID].text = newGrowth + "";
        Vector3 newTextPos = growthTexts[statID].transform.localPosition;
        newTextPos.y = growthButtons[newGrowth + 1].transform.localPosition.y;

        growthTexts[statID].color = growthColor;
        growthTexts[statID].transform.localPosition = newTextPos;

        statGrowths[statID].color = growthColor;
        statGrowths[statID].sprite = growthSprites[newGrowth + 1];

        int newPoints = 1;
        newPoints -= critterBuild[selectedCritter].critterValue[4]; //hp
        newPoints -= critterBuild[selectedCritter].critterValue[5]; //str
        newPoints -= critterBuild[selectedCritter].critterValue[6]; //magic
        newPoints -= critterBuild[selectedCritter].critterValue[7]; //speed
        growthPointsText.text = newPoints + "";
        growthPointsText.transform.GetChild(0).GetComponent<TMP_Text>().text = newPoints + "";

        int statToUpdate = statID + 4;
        setStats(level);
        CloudUpdateCritterBuild(statToUpdate);
    }

    private Color getGrowthColor(int value)
    {
        Color growthColor = new Color();
        switch (value)
        {
            case -1: growthColor = Color.red; break;
            case 0: growthColor = new Color(0, 194, 255, 255); break;
            case 1: growthColor = Color.green; break;
            case 2: growthColor = Color.green; break;
        }
        return growthColor;
    }

    private SpriteRenderer[] getCritterGroup(int which)
    {
        SpriteRenderer[] critterGroup = new SpriteRenderer[team1CritterImages.Length];
        switch (which)
        {
            case 0: critterGroup = team1CritterImages; break;
            case 1: critterGroup = team2CritterImages; break;
            case 2: critterGroup = team3CritterImages; break;
        }

        return critterGroup;
    }

    private critterBuild[] getCritterBuildGroup(int which)
    {
        critterBuild[] critterBuild = new critterBuild[team1CritterBuilds.Length];
        switch (which)
        {
            case 0: critterBuild = team1CritterBuilds; break;
            case 1: critterBuild = team2CritterBuilds; break;
            case 2: critterBuild = team3CritterBuilds; break;
        }

        return critterBuild;
    }

    public void selectCritter(int team)
    {
        if (bDeleteCritter == true) bDeleteCritter = false;

        if (selectedTeam == -1 && bSelectedTeam == false)
        {
            selectedTeam = team;
            bEditCritter = false;
            bSelectedTeam = true;
            menuAnimations.SetTrigger("openTeam" + (team + 1));
            menuAnimations.ResetTrigger("closeTeam" + 1);
            menuAnimations.ResetTrigger("closeTeam" + 2);
            menuAnimations.ResetTrigger("closeTeam" + 3);
        }
    }

    public void getSelectedCritter(int which)
    {
        if (bSelectedTeam == true)
        {
            selectedCritter = which;
            critterBuild[] critterGroupBuilds = getCritterBuildGroup(selectedTeam);
            SpriteRenderer[] critterImages = getCritterGroup(selectedTeam);


            if (bDeleteCritter == true && critterGroupBuilds[which].critterValue[0] != -1)
            {
                bDeleteCritter = false;
                StartCoroutine(spawnDeleteButton(critterImages[selectedCritter].transform));
                return;
            }


            bEditCritter = (critterGroupBuilds[which].critterValue[0] == -1) ? false : true;
            bCritterList = (critterGroupBuilds[which].critterValue[0] == -1) ? true : false;

            setGrowthandStats();

            createCritterSelect();
        }
    }

    private void deleteCritter(int selectTeam, int selectCritter)
    {
        bEditCritter = false;
        bCritterList = false;
        selectedCritter = -1;

        critterBuild[] critterGroupBuilds = getCritterBuildGroup(selectTeam);
        SpriteRenderer[] critterImages = getCritterGroup(selectTeam);

        critterShaders[selectCritter].SetColor("Color_2ED9CBC6", Color.white);

        for (int i = 0; i < 7; ++i)
        {
            critterGroupBuilds[selectCritter].critterValue[i] = -1;
            CloudUpdateCritterBuild(i);

            critterImages[selectCritter].transform.Find("outline").GetComponent<Image>().sprite = transparentSprite;
            critterImages[selectCritter].sprite = addCritterSprite;
        }
    }

    private IEnumerator spawnDeleteButton(Transform spawnParent)
    {
        GameObject newPopup = Instantiate(deletePrefab);

        newPopup.transform.SetParent(spawnParent);
        newPopup.transform.localScale = Vector3.one;
        newPopup.transform.localPosition = Vector3.zero;

        int selectTeam = selectedTeam;
        int selectCritter = selectedCritter;
        newPopup.GetComponent<Button>().onClick.AddListener(() => deleteCritter(selectTeam, selectCritter));

        yield return new WaitForSeconds(6);

        Destroy(newPopup);
    }

    private void createCritterSelect()
    {
        foreach (GameObject critter in critterGameObjectList)
        {
            Destroy(critter);
        }
        critterGameObjectList.Clear();

        foreach (critter Critter in critterCollection)
        {
            GameObject newCritter = Instantiate(critterPrefab);

            newCritter.transform.SetParent(critterParent.transform);
            newCritter.transform.localScale = new Vector3(1, 1, 1);

            newCritter.transform.Find("critterName").GetComponent<TMP_Text>().text = Critter.GetCritterName();
            newCritter.transform.Find("graphic").GetComponent<Image>().sprite = Critter.stages[0];
            newCritter.transform.Find("border").GetComponent<Image>().sprite = Critter.circleOutline;
            newCritter.transform.Find("hpText").GetComponent<TMP_Text>().text = Critter.GetInitialHP() + "";
            newCritter.transform.Find("strText").GetComponent<TMP_Text>().text = Critter.GetInitialStrength() + "";
            newCritter.transform.Find("magicText").GetComponent<TMP_Text>().text = Critter.GetInitialMagic() + "";
            newCritter.transform.Find("speedText").GetComponent<TMP_Text>().text = Critter.GetInitialSpeed() + "";

            newCritter.GetComponent<Button>().onClick.AddListener(() => chooseCritter(Critter.GetCritterID()));

            critterGameObjectList.Add(newCritter);
        }

    }

    private void chooseCritter(int ID)
    {
        SpriteRenderer[] critterGroup = getCritterGroup(selectedTeam);
        critterBuild[] critterIDGroup = getCritterBuildGroup(selectedTeam);

        critterIDGroup[selectedCritter].critterValue[0] = ID;

        createNewText[selectedTeam].text = "";

        critterGroup[selectedCritter].sprite = critterCollection[ID].stages[0];
        critterGroup[selectedCritter].transform.Find("outline").GetComponent<Image>().sprite = critterCollection[ID].circleOutline;

        bCritterList = false;
        bEditCritter = true;

        setGrowthandStats();

        checkStageSprite(selectedCritter);

        CloudUpdateCritterBuild(0);
    }

    private void setGrowthandStats()
    {
        critterBuild[] critterIDGroup = getCritterBuildGroup(selectedTeam);

        if (critterIDGroup[selectedCritter].critterValue[0] != -1)
        {
            changeGrowth(critterIDGroup[selectedCritter].critterValue[4], hpGrowthButtons, 0);
            changeGrowth(critterIDGroup[selectedCritter].critterValue[5], strGrowthButtons, 1);
            changeGrowth(critterIDGroup[selectedCritter].critterValue[6], magicGrowthButtons, 2);
            changeGrowth(critterIDGroup[selectedCritter].critterValue[7], speedGrowthButtons, 3);

            setStats(level);
        }
    }

    private void setStats(float level)
    {
        critterBuild[] critterIDGroup = getCritterBuildGroup(selectedTeam);
        float[] stats = critterCollection[critterIDGroup[selectedCritter].critterValue[0]].getStatBlock();
        float lvl = level - 1f;

        for(int i = 0; i < 4; ++i)
        {
            float addedValue = (i == 0) ? lvl * (4 + critterIDGroup[selectedCritter].critterValue[4]) : lvl * (1f + (critterIDGroup[selectedCritter].critterValue[i + 4] / 2f));
            stats[i] += addedValue;
            stats[i] = Mathf.FloorToInt(stats[i]);
            statValueTexts[i].text = stats[i] + "";
        }
    }

    public void setStatsFromLevel(float sliderValue) // From Slider
    {
        float previousLevel = level;

        this.level = sliderValue;
        levelText.text = "Level " + sliderValue;

        setStats(level);
        checkStageSprite(0);
        checkStageSprite(1);
        checkStageSprite(2);

        critterBuild[] critterIDGroup = getCritterBuildGroup(selectedTeam);

        for(int i = 0; i < 4; ++i)
        {
            float statChange = (i == 0) ? (4 + critterIDGroup[selectedCritter].critterValue[4]) : (1f + (critterIDGroup[selectedCritter].critterValue[i + 4] / 2f));
            if (level < previousLevel) statChange *= -1f;
            statNotification.spawnStatPopup(statValueTexts[i].transform, statChange);
        }
    }

    private void checkStageSprite(int which)
    {
        critterBuild[] critterIDGroup = getCritterBuildGroup(selectedTeam);

        if (critterIDGroup[which].critterValue[0] == -1) return;
        if (critterIDGroup[which].critterValue[0] == 12) return;

        SpriteRenderer[] critterImages = getCritterGroup(selectedTeam);

        critterShaders[which].SetColor("Color_2ED9CBC6", critterCollection[which].matchingColor);

        if (level <= 2 && critterImages[which].sprite != critterCollection[critterIDGroup[which].critterValue[0]].stages[0])
        {
            StartCoroutine(handleDissolve(which, 0));
        }
        else if (level >= 3 && level <= 5 && critterImages[which].sprite != critterCollection[critterIDGroup[which].critterValue[0]].stages[1])
        {
            StartCoroutine(handleDissolve(which, 1));
        }
        else if (level >= 6 && critterImages[which].sprite != critterCollection[critterIDGroup[which].critterValue[0]].stages[2])
        {
            StartCoroutine(handleDissolve(which, 2));
        }
    }

    private IEnumerator handleDissolve(int i, int stage)
    {
        SpriteRenderer[] critterImages = getCritterGroup(selectedTeam);
        critterBuild[] critterIDGroup = getCritterBuildGroup(selectedTeam);

        bShaderDissolve = false;
        critterShaders[i].SetFloat("Vector1_E974001A", 2);

        yield return new WaitForSeconds(0.6f);

        bShaderDissolve = true;
        critterImages[i].sprite = critterCollection[critterIDGroup[i].critterValue[0]].stages[stage];
    }

    public void setActiveTeam(int which)
    {
        activeCritterTeam = getCritterBuildGroup(which);
    }

    public void updateTeamName()
    {
        List<string> teamList = new List<string>();
        teamList.Add(teamName[0].text);
        teamList.Add(teamName[1].text);
        teamList.Add(teamName[2].text);
        StartCoroutine(fireBaseScript.UpdateObject("teamNames", teamList));
    }

    private void CloudUpdateCritterBuild(int critterValue)
    {
        string newValues = "";

        for (int i = 0; i < team1CritterBuilds.Length; ++i)
        {
            critterBuild[] critterGroupBuilds = getCritterBuildGroup(i);

            newValues += critterGroupBuilds[0].critterValue[critterValue];
            newValues += "_";
            newValues += critterGroupBuilds[1].critterValue[critterValue];
            newValues += "_";
            newValues += critterGroupBuilds[2].critterValue[critterValue];
            if (i != team1CritterBuilds.Length - 1) newValues += "*";

        }

        string nameToSave = "";
        switch (critterValue)
        {
            case 0: nameToSave = "critterIDs"; break;
            case 1: nameToSave = "attackIDs"; break;
            case 2: nameToSave = "abilityIDs"; break;
            case 3: nameToSave = "passiveIDs"; break;
            case 4: nameToSave = "HPGrowth"; break;
            case 5: nameToSave = "StrengthGrowth"; break;
            case 6: nameToSave = "MagicGrowth"; break;
            case 7: nameToSave = "SpeedGrowth"; break;
        }

        StartCoroutine(fireBaseScript.UpdateObject(nameToSave, newValues));
    }

    private void InitalizeBuilder()
    {

        for (int i = 0; i < team1CritterBuilds.Length; ++i)
        {
            selectedTeam = i;
            inBuilderMenuSprites(i);
        }

        selectedTeam = -1;
    }

    public void LoadData(Dictionary<string, object> dataDictionary)
    {
        deserializeCritterValue(dataDictionary["critterIDs"].ToString(), 0);
        deserializeCritterValue(dataDictionary["attackIDs"].ToString(), 1);
        deserializeCritterValue(dataDictionary["abilityIDs"].ToString(), 2);
        deserializeCritterValue(dataDictionary["passiveIDs"].ToString(), 3);
        deserializeCritterValue(dataDictionary["HPGrowth"].ToString(), 4);
        deserializeCritterValue(dataDictionary["StrengthGrowth"].ToString(), 5);
        deserializeCritterValue(dataDictionary["MagicGrowth"].ToString(), 6);
        deserializeCritterValue(dataDictionary["SpeedGrowth"].ToString(), 7);


        // 0  is ID
        // 1 is attack ID
        // 2 is ability ID
        // 3 is passive ID
        // 4 is HP growth
        // 5 is Strength growth
        // 6 is Magic growth
        // 7 is Speed Growth
        for(int i = 0; i < 3; ++i)
        {
            string name = dataDictionary["teamNames" + i].ToString();
            teamName[i].text = name;
        }

        InitalizeBuilder();
    }

    public void LoadOtherPlayersData(string key, object data)
    {
        
    }
}

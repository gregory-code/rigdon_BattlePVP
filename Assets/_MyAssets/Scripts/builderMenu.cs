using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class builderMenu : MonoBehaviour, IDataPersistence
{
    notifScript NotificationScript;

    //Firebase Data
    FirebaseScript fireBaseScript;

    //Online
    onlineScript OnlineScript;

    //statPopup
    statNotif statNotification;

    public bool bDeleteCritter;
    [SerializeField] GameObject deletePrefab;

    //Aniamtor
    private Animator menuAnimations;

    [SerializeField] private Sprite transparentSprite;

    [Header("Builder")]
    private bool bCritterList;
    [SerializeField] Transform critterList;
    [SerializeField] critter[] critterCollection;
    private List<GameObject> critterGameObjectList = new List<GameObject>();
    [SerializeField] Transform critterParent;
    [SerializeField] GameObject critterPrefab;

    [SerializeField] Sprite[] critterSprites;
    [SerializeField] Sprite addCritterSprite;

    public int selectedCritter;
    public int selectedTeam;
    public bool bSelectedTeam;

    [SerializeField] TMP_InputField[] teamName; // for all 3 teams
    [SerializeField] TMP_Text[] createNewText;

    [SerializeField] critterBuild[] team1CritterBuilds = new critterBuild[3];
    [SerializeField] critterBuild[] team2CritterBuilds = new critterBuild[3];
    [SerializeField] critterBuild[] team3CritterBuilds = new critterBuild[3];

    [SerializeField] Image[] team1CritterImages; //each team
    [SerializeField] Image[] team2CritterImages;
    [SerializeField] Image[] team3CritterImages;

    [SerializeField] Transform[] team1CritterTransforms; //each team
    [SerializeField] Transform[] team2CritterTransforms;
    [SerializeField] Transform[] team3CritterTransforms;

    [Header("Stats Growth Page")]
    float level;
    [SerializeField] Transform editCritterTransform;
    bool bGrowthEditor;
    [SerializeField] Transform growthPageTransform;
    [SerializeField] Transform growthPointsTransform;
    [SerializeField] TMP_Text growthPointsText;
    public bool bEditCritter;

    [SerializeField] TMP_Text levelText;

    [SerializeField] TMP_Text[] statValueTexts;
    [SerializeField] Image[] statGrowths;

    [SerializeField] Sprite[] growthSprites;

    [SerializeField] TMP_Text[] growthTexts;
    [SerializeField] Image[] hpGrowthButtons;
    [SerializeField] Image[] strGrowthButtons;
    [SerializeField] Image[] magicGrowthButtons;
    [SerializeField] Image[] speedGrowthButtons;

    private void Awake()
    {
        selectedCritter = -1;
        level = 1;

        statNotification = GetComponent<statNotif>();

        fireBaseScript = GameObject.FindGameObjectWithTag("Data").GetComponent<FirebaseScript>();
        OnlineScript = GameObject.FindGameObjectWithTag("Online").GetComponent<onlineScript>();

        NotificationScript = GameObject.FindGameObjectWithTag("Canvas").GetComponent<notifScript>();

        menuAnimations = GameObject.Find("menu").GetComponent<Animator>();
    }

    void Update()
    {
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

        float critterIconLerp = 0;
        Transform[] critterTransforms = new Transform[team1CritterTransforms.Length];
        switch (selectedTeam)
        {
            case 0: critterTransforms = team1CritterTransforms; break;
            case 1: critterTransforms = team2CritterTransforms; break;
            case 2: critterTransforms = team3CritterTransforms; break;
        }
        for (int i = 0; i < team1CritterTransforms.Length; ++i)
        {
            critterIconLerp = (selectedCritter == i) ? 30 : 0;
            Image critterOutline = critterTransforms[i].transform.Find("outline").GetComponent<Image>();
            int critterOutlineLerp = (selectedCritter == i) ? 1 : 0;
            critterOutline.fillAmount = Mathf.Lerp(critterOutline.fillAmount, critterOutlineLerp, 30 * Time.deltaTime);
            critterTransforms[i].localPosition = Vector2.Lerp(critterTransforms[i].localPosition, new Vector2(critterTransforms[i].localPosition.x, critterIconLerp), 10 * Time.deltaTime);
        }
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

    private void deserializeCritterValue(string toDeserialize, int critterValue)
    {
        List<string> teams = toDeserialize.Split('*').ToList<string>();

        for (int i = 0; i < teams.Count; ++i)
        {
            critterBuild[] critterGroupBuilds = getCritterBuildGroup(i);
            Image[] critterGroup = getCritterGroup(i);

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

    public void openGrowthEditor()
    {
        bGrowthEditor = !bGrowthEditor;
    }

    public void inBuilderSprites(int team)
    {
        Image[] critterGroup = getCritterGroup(team);

        if (critterGroup[0].sprite == transparentSprite) critterGroup[0].sprite = addCritterSprite;
        if (critterGroup[1].sprite == transparentSprite) critterGroup[1].sprite = addCritterSprite;
        if (critterGroup[2].sprite == transparentSprite) critterGroup[2].sprite = addCritterSprite;
    }

    public void inBuilderMenuSprites(int team)
    {
        bSelectedTeam = false;
        bCritterList = false;
        selectedCritter = -1;
        selectedTeam = -1;

        critterBuild[] critterBuild = getCritterBuildGroup(team);

        Image[] critterGroup = getCritterGroup(team);

        if (critterBuild[0].critterValue[0] == -1) critterGroup[0].sprite = transparentSprite;
        if (critterBuild[1].critterValue[0] == -1) critterGroup[1].sprite = transparentSprite;
        if (critterBuild[2].critterValue[0] == -1) critterGroup[2].sprite = transparentSprite;

        if (critterGroup[0].sprite == transparentSprite && critterGroup[1].sprite == transparentSprite && critterGroup[2].sprite == transparentSprite)
        {
            createNewText[team].text = "Create New";
            teamName[team].text = "";
        }
    }

    public void changeHPGrowth(int change)
    {

        if (validPointReduction(0, change) == false){
            return;
        }
        critterBuild[] critterIDGroup = getCritterBuildGroup(selectedTeam);

        float previousStat = float.Parse(statValueTexts[0].text);

        changeGrowth(change, hpGrowthButtons, 0);

        float currerntStat = float.Parse(statValueTexts[0].text);

        statNotification.spawnStatPopup(statValueTexts[0].transform, currerntStat - previousStat);
    }

    public void changeStrGrowth(int change)
    {

        if (validPointReduction(1, change) == false)
        {
            return;
        }
        critterBuild[] critterIDGroup = getCritterBuildGroup(selectedTeam);

        float previousStat = float.Parse(statValueTexts[1].text);

        changeGrowth(change, strGrowthButtons, 1);

        float currerntStat = float.Parse(statValueTexts[1].text);

        statNotification.spawnStatPopup(statValueTexts[1].transform, currerntStat - previousStat);
    }

    public void changeMagicGrowth(int change)
    {

        if (validPointReduction(2, change) == false)
        {
            return;
        }
        critterBuild[] critterIDGroup = getCritterBuildGroup(selectedTeam);

        float previousStat = float.Parse(statValueTexts[2].text);

        changeGrowth(change, magicGrowthButtons, 2);

        float currerntStat = float.Parse(statValueTexts[2].text);

        statNotification.spawnStatPopup(statValueTexts[2].transform, currerntStat - previousStat);
    }

    public void changeSpeedGrowth(int change)
    {

        if (validPointReduction(3, change) == false)
        {
            return;
        }
        critterBuild[] critterIDGroup = getCritterBuildGroup(selectedTeam);

        float previousStat = float.Parse(statValueTexts[3].text);

        changeGrowth(change, speedGrowthButtons, 3);

        float currerntStat = float.Parse(statValueTexts[3].text);

        statNotification.spawnStatPopup(statValueTexts[3].transform, currerntStat - previousStat);
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
        setStats();
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

    private Image[] getCritterGroup(int which)
    {
        Image[] critterGroup = new Image[team1CritterImages.Length];
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
            Image[] critterImages = getCritterGroup(selectedTeam);


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
        Image[] critterImages = getCritterGroup(selectTeam);

        for(int i = 0; i < 7; ++i)
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
        Image[] critterGroup = getCritterGroup(selectedTeam);
        critterBuild[] critterIDGroup = getCritterBuildGroup(selectedTeam);

        critterIDGroup[selectedCritter].critterValue[0] = ID;

        createNewText[selectedTeam].text = "";

        critterGroup[selectedCritter].sprite = critterCollection[ID].stages[0];
        critterGroup[selectedCritter].transform.Find("outline").GetComponent<Image>().sprite = critterCollection[ID].circleOutline;

        bCritterList = false;
        bEditCritter = true;

        setGrowthandStats();

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

            setStats();
        }
    }

    private void setStats()
    {
        critterBuild[] critterIDGroup = getCritterBuildGroup(selectedTeam);

        float hp = critterCollection[critterIDGroup[selectedCritter].critterValue[0]].GetInitialHP();
        float str = critterCollection[critterIDGroup[selectedCritter].critterValue[0]].GetInitialStrength();
        float magic = critterCollection[critterIDGroup[selectedCritter].critterValue[0]].GetInitialMagic();
        float speed = critterCollection[critterIDGroup[selectedCritter].critterValue[0]].GetInitialSpeed();

        float addedHpValue = (level - 1) * (4 + critterIDGroup[selectedCritter].critterValue[4]);
        hp += addedHpValue;
        hp = Mathf.FloorToInt(hp);

        float addedStrValue = (level - 1) * (1f + (critterIDGroup[selectedCritter].critterValue[5] / 2f));
        str += addedStrValue;
        str = Mathf.FloorToInt(str);

        float addedMagicValue = (level - 1) * (1f + (critterIDGroup[selectedCritter].critterValue[6] / 2f));
        magic += addedMagicValue;
        magic = Mathf.FloorToInt(magic);

        float addedSpeedValue = (level - 1) * (1f + (critterIDGroup[selectedCritter].critterValue[7] / 2f));
        speed += addedSpeedValue;
        speed = Mathf.FloorToInt(speed);

        statValueTexts[0].text = hp + "";
        statValueTexts[1].text = str + "";
        statValueTexts[2].text = magic + "";
        statValueTexts[3].text = speed + "";
    }

    public void setStatsFromLevel(float sliderValue)
    {
        float previousLevel = level;

        this.level = sliderValue;
        levelText.text = "Level " + sliderValue;
        setStats();

        critterBuild[] critterIDGroup = getCritterBuildGroup(selectedTeam);

        float hpChange = (4 + critterIDGroup[selectedCritter].critterValue[4]);
        if (level < previousLevel) hpChange *= -1f;
        statNotification.spawnStatPopup(statValueTexts[0].transform, hpChange);

        float strChange = (1f + (critterIDGroup[selectedCritter].critterValue[5] / 2f));
        if (level < previousLevel) strChange *= -1f;
        statNotification.spawnStatPopup(statValueTexts[1].transform, strChange);

        float magicChange = (1f + (critterIDGroup[selectedCritter].critterValue[6] / 2f));
        if (level < previousLevel) magicChange *= -1f;
        statNotification.spawnStatPopup(statValueTexts[2].transform, magicChange);

        float speedChange = (1f + (critterIDGroup[selectedCritter].critterValue[7] / 2f));
        if (level < previousLevel) speedChange *= -1f;
        statNotification.spawnStatPopup(statValueTexts[3].transform, speedChange);
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

        InitalizeBuilder();
    }

    public void LoadOtherPlayersData(string key, object data)
    {
        
    }
}

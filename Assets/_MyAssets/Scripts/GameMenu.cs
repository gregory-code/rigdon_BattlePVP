using Firebase.Database;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMenu : MonoBehaviourPunCallbacks, IDataPersistence
{
    [SerializeField] FirebaseScript firebaseScript;
    [SerializeField] battleMenu battleMenuScript;

    [Header("Game Menu")]
    [SerializeField] CanvasGroup myGroup;
    [SerializeField] Image rightCurtain;
    [SerializeField] Image leftCurtain;
    [SerializeField] TextMeshProUGUI myTitle;
    [SerializeField] TextMeshProUGUI myTeamTitle;
    [SerializeField] TextMeshProUGUI enemyTitle;
    [SerializeField] TextMeshProUGUI enemyTeamTitle;
    [SerializeField] GameObject allMenus;
    [SerializeField] GameObject menuChecks;
    bool showCurtain;

    [SerializeField] private expHolder[] expHolders;

    [SerializeField] Image[] myImages;
    [SerializeField] TextMeshProUGUI[] myLevels;
    [SerializeField] Image[] myLevelBars;
    [SerializeField] Transform[] myExpLists;

    [SerializeField] Image[] enemyImages;
    [SerializeField] Image[] enemyLevelBars;
    [SerializeField] TextMeshProUGUI[] enemyLevels;
    [SerializeField] Transform[] enemyExpLists;

    [SerializeField] Transform expTitleList;
    [SerializeField] TextMeshProUGUI expEntry;

    List<TextMeshProUGUI> allText = new List<TextMeshProUGUI>();

    string myTeamName;
    string enemyTeamName;


    [Header("Player Data")]
    [SerializeField] private string player1_ID; // just for testing of course
    [SerializeField] private string player2_ID;
    [SerializeField] private bool bIsPlayer1;

    string myUsername;
    string enemyUsername;
    string foundUsername;

    monsterPreferences[] myTeamPrefs = new monsterPreferences[3];
    monsterPreferences[] enemyTeamPrefs = new monsterPreferences[3];

    bool gotEnemyTeam;

    [Header("Monster References")]
    [SerializeField] GameMaster gameMaster;
    [SerializeField] monster[] monsters;
    [SerializeField] private monster[] player1Team = new monster[3];
    [SerializeField] private monster[] player2Team = new monster[3];

    public monster[] GetEnemyTeam()
    {
        return (bIsPlayer1) ? player2Team : player1Team;
    }

    public monster[] GetMyTeam()
    {
        return (bIsPlayer1) ? player1Team : player2Team ;
    }

    void Start()
    {
        SetCanvasGroup(false);

        for(int i = 0; i < 3; i++)
        {
            myTeamPrefs[i] = ScriptableObject.CreateInstance<monsterPreferences>();
            enemyTeamPrefs[i] = ScriptableObject.CreateInstance<monsterPreferences>();
        }
    }

    void Update()
    {
        int curtainLerp = (showCurtain) ? 1 : 0 ;
        rightCurtain.fillAmount = Mathf.Lerp(rightCurtain.fillAmount, curtainLerp, 6 * Time.deltaTime);
        leftCurtain.fillAmount = Mathf.Lerp(leftCurtain.fillAmount, curtainLerp, 6 * Time.deltaTime);
    }

    private void SetCanvasGroup(bool state)
    {
        myGroup.interactable = state;
        myGroup.blocksRaycasts = state;
    }

    public void SetPlayerIDs(string ID_1, string ID_2)
    {
        player1_ID = ID_1;
        player2_ID = ID_2;
        bIsPlayer1 = (player1_ID == firebaseScript.GetUserID()) ? true : false;
    }

    public IEnumerator SetUpGame()
    {
        SetCanvasGroup(true);
        showCurtain = true;

        gotEnemyTeam = false;

        myTeamPrefs = battleMenuScript.GetMonsterPrefsFromSelectedTeam();
        string myTeamname = battleMenuScript.GetSelectedTeamName();
        myTeamName = myTeamname;

        yield return StartCoroutine(GetUsernames());
        
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(SendMyTeamPrefs(myTeamname));

        yield return StartCoroutine(CreateTeams());

        yield return new WaitForSeconds(0.5f);

        SetGameMenu();

        yield return new WaitForSeconds(1f);

        allMenus.SetActive(false);
        showCurtain = false;
        menuChecks.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        gameMaster.StartFight();

        SetCanvasGroup(false);
    }

    private void SetGameMenu()
    {
        myTeamTitle.text = myTeamName;
        enemyTeamTitle.text = enemyTeamName;

        for (int i = 0; i < 3; i++)
        {
            monster myNewMon = GetMyTeam()[i];
            myImages[i].sprite = myNewMon.stages[myNewMon.GetSpriteIndexFromLevel()];
            myLevels[i].text = "Lvl " + myNewMon.GetLevel();

            monster enemyNewMon = GetEnemyTeam()[i];
            enemyImages[i].sprite = enemyNewMon.stages[enemyNewMon.GetSpriteIndexFromLevel()];
            enemyLevels[i].text = "Lvl " + enemyNewMon.GetLevel();
        }
    }

    private IEnumerator EndOfMatchResults()
    {
        monster[] myTeam = GetMyTeam();
        monster[] enemyTeam = GetEnemyTeam();

        int expLength = myTeam[0].GetExpHolders().Count;

        for (int i = 0; i < expLength; i++)
        {
            if(ShouldPostIt(i))
            {
                AddExpText(expTitleList, myTeam[0].GetExpHold(i).GetTitle());

                for(int x = 0; x < 3; x++)
                {
                    yield return AddExp(myExpLists[x], myTeam[x], i, myLevels[x], myLevelBars[x], myImages[x]);
                }

                for (int x = 0; x < 3; x++)
                {
                    yield return AddExp(enemyExpLists[x], enemyTeam[x], i, enemyLevels[x], enemyLevelBars[x], enemyImages[x]);
                }
            }
        }
    }

    private IEnumerator AddExp(Transform list, monster mon, int expIndex, TextMeshProUGUI levelText, Image levelBar, Image monsterImage)
    {
        AddExpText(list, mon.GetExpHold(expIndex).GetExpText());
        float exp = mon.GetExpHold(expIndex).ExplenishExp();
        while (exp > 0)
        {
            yield return new WaitForEndOfFrame();
            bool levelUp = mon.TryLevelUp();
            levelBar.fillAmount = mon.GetLevelPercentage();
            exp--;

            if (levelUp)
            {
                levelText.text = "Lvl " + mon.GetLevel();
                monsterImage.sprite = mon.stages[mon.GetSpriteIndexFromLevel()];
                levelBar.fillAmount = 0;

            }
        }
    }

    private void AddExpText(Transform list, string text)
    {
        TextMeshProUGUI newTitle = Instantiate(expEntry, list);
        newTitle.text = text;
        allText.Add(newTitle);
    }

    private bool ShouldPostIt(int whichHold)
    {
        for (int i = 0; i < 3; i++)
        {
            if (GetMyTeam()[i].GetExpHold(whichHold).GetExp() > 0)
            {
                return true;
            }

            if (GetEnemyTeam()[i].GetExpHold(whichHold).GetExp() > 0)
            {
                return true;
            }
        }

        return false;
    }

    public void StartIntermission()
    {
        StartCoroutine(Intermission());
    }


    bool waitingForEnemy;
    private IEnumerator Intermission()
    {
        SetCanvasGroup(true);
        showCurtain = true;
        waitingForEnemy = true;

        CleanUp();

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(EndOfMatchResults());

        yield return new WaitForSeconds(5f);

        this.photonView.RPC("ReadyToContinueRPC", RpcTarget.OthersBuffered); // First person to search is player 2

        while (waitingForEnemy == true)
        {
            yield return new WaitForEndOfFrame();
        }

        gameMaster.StartFight();

        SetCanvasGroup(false);
        showCurtain = false;
    }

    private void CleanUp()
    {
        foreach (TextMeshProUGUI text in allText)
        {
            Destroy(text.gameObject);
        }
        allText.Clear();

        GameObject[] leftoverEffects = GameObject.FindGameObjectsWithTag("effect");
        foreach(GameObject effect in leftoverEffects)
        {
            Destroy(effect);
        }
    }

    private IEnumerator GetUsernames()
    {
        yield return StartCoroutine(firebaseScript.LoadOtherPlayersData(player1_ID, "username"));

        if (bIsPlayer1)
            myUsername = foundUsername;
        else
            enemyUsername = foundUsername;

        yield return StartCoroutine(firebaseScript.LoadOtherPlayersData(player2_ID, "username"));

        if (bIsPlayer1)
            enemyUsername = foundUsername;
        else
            myUsername = foundUsername;

        myTitle.text = myUsername;
        enemyTitle.text = enemyUsername;
    }

    private IEnumerator SendMyTeamPrefs(string teamNickName)
    {
        string[] myPref = new string[3];
        string[] nickname = new string[3];
        for (int i = 0; i < 3; i++)
        {
            myPref[i] = myTeamPrefs[i].SeralizedPref();
            nickname[i] = myTeamPrefs[i].monsterNickname;
        }

        this.photonView.RPC("GetEnemyMonsterRPC", RpcTarget.OthersBuffered, myPref, nickname, teamNickName); // First person to search is player 2

        while(gotEnemyTeam == false)
        {
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator CreateTeams()
    {
        for(int i = 0; i < 3; i++)
        {
            monster newAlly = Instantiate(monsters[myTeamPrefs[i].monsterValues[0]]);
            newAlly.SetInitialStats();
            newAlly.SetFromPref(myTeamPrefs[i]);
            newAlly.bMine = true;
            newAlly.teamIndex = i;

            monster newEnemy = Instantiate(monsters[enemyTeamPrefs[i].monsterValues[0]]);
            newEnemy.SetInitialStats();
            newEnemy.SetFromPref(enemyTeamPrefs[i]);
            newEnemy.teamIndex = i;

            for (int x = 0; x < expHolders.Length; x++)
            {
                expHolder newHolder1 = Instantiate(expHolders[x]);
                expHolder newHolder2 = Instantiate(expHolders[x]);
                newAlly.SetExpHolder(newHolder1);
                newEnemy.SetExpHolder(newHolder2);
            }

            if (bIsPlayer1)
            {
                player1Team[i] = newAlly;
                player2Team[i] = newEnemy;
            }
            else
            {
                player2Team[i] = newAlly;
                player1Team[i] = newEnemy;
            }
        }

        yield return new WaitForEndOfFrame();
    }

    [PunRPC]
    void GetEnemyMonsterRPC(string[] enemys, string[] nicknames, string teamName)
    {
        enemyTeamName = teamName;

        for(int i = 0; i < enemys.Length; i++)
        {
            enemyTeamPrefs[i].DeseralizePref(enemys[i]);
            enemyTeamPrefs[i].monsterNickname = nicknames[i];
        }

        gotEnemyTeam = true;
    }

    [PunRPC]
    void ReadyToContinueRPC()
    {
        waitingForEnemy = false;
    }

    public IEnumerator LoadData(DataSnapshot data)
    {
        yield return new WaitForEndOfFrame();
    }

    public void LoadOtherPlayersData(string key, object data)
    {
        if (key == "username")
        {
            foundUsername = data.ToString();
        }
    }
}

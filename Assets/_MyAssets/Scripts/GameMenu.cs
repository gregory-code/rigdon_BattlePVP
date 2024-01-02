using Firebase.Database;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
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
    [SerializeField] TextMeshProUGUI myGamesWonText;
    int myGameWon = 0;

    [SerializeField] TextMeshProUGUI enemyTitle;
    [SerializeField] TextMeshProUGUI enemyTeamTitle;
    [SerializeField] TextMeshProUGUI enemyGamesWonText;
    int enemyGamesWon = 0;

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

    private List<string> battlePlayList = new List<string>();
    [SerializeField] AudioMixerSnapshot RegularSnapshot;
    [SerializeField] AudioMixerSnapshot DecidingSnapshot;

    [Header("Player Data")]
    [SerializeField] private string player1_ID; // just for testing of course
    [SerializeField] private string player2_ID;
    [SerializeField] private bool bIsPlayer1;
    bool gameIsActive;

    string myUsername;
    string enemyUsername;
    string foundUsername;

    monsterPreferences[] myTeamPrefs = new monsterPreferences[3];
    monsterPreferences[] enemyTeamPrefs = new monsterPreferences[3];

    [SerializeField] randomTeam[] randoTeams;
    [SerializeField] string[] randoNames;

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

    public void PlayerWon(bool player1Won)
    {
        if(player1Won && bIsPlayer1)
        {
            myGameWon++;
        }
        else if (player1Won == false && bIsPlayer1 == false)
        {
            myGameWon++;
        }
        else
        {
            enemyGamesWon++;
        }
    }

    public bool OwnerShipCheck(bool isPlayer1)
    {
        return (isPlayer1 == this.bIsPlayer1) ? true : false ;
    }

    public monster GetMonsterFromReference(bool isPlayer1, int index)
    {
        if (bIsPlayer1 && isPlayer1)
            return player1Team[index];

        if (bIsPlayer1 == false && isPlayer1 == false)
            return player2Team[index];
        
        if(bIsPlayer1 && isPlayer1 == false)
            return player2Team[index];

        if (bIsPlayer1 == false && isPlayer1)
            return player1Team[index];

        return player1Team[index];
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

    public IEnumerator SetUpGame(int format)
    {
        SetCanvasGroup(true);
        showCurtain = true;

        myGameWon = 0;
        enemyGamesWon = 0;
        SetGamesWonText();

        SetFilter(false);

        gotEnemyTeam = false;

        switch(format)
        {
            case 0:
            case 2:
                myTeamPrefs = battleMenuScript.GetMonsterPrefsFromSelectedTeam();
                myTeamName = battleMenuScript.GetSelectedTeamName();
                break;

            case 1:
                int random = Random.Range(0, randoTeams.Length);
                myTeamPrefs = randoTeams[random].GetTeam();
                myTeamName = randoNames[random];
                break;
        }

        yield return StartCoroutine(GetUsernames());
        
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(SendMyTeamPrefs(myTeamName));

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

    private IEnumerator CreateTeams()
    {
        for (int i = 0; i < 3; i++)
        {
            monster newAlly = Instantiate(monsters[myTeamPrefs[i].monsterValues[0]]);
            newAlly.SetInitialStats();
            newAlly.SetFromPref(myTeamPrefs[i]);
            newAlly.SetTeamIndex(i);
            newAlly.SetOwnership(true);

            monster newEnemy = Instantiate(monsters[enemyTeamPrefs[i].monsterValues[0]]);
            newEnemy.SetInitialStats();
            newEnemy.SetFromPref(enemyTeamPrefs[i]);
            newEnemy.SetTeamIndex(i);

            for (int x = 0; x < expHolders.Length; x++)
            {
                expHolder newHolder1 = Instantiate(expHolders[x]);
                expHolder newHolder2 = Instantiate(expHolders[x]);
                newAlly.SetExpHolder(newHolder1);
                newEnemy.SetExpHolder(newHolder2);
            }

            if (bIsPlayer1)
            {
                newAlly.SetAsPlayer1();
                player1Team[i] = newAlly;
                player2Team[i] = newEnemy;
            }
            else
            {
                newEnemy.SetAsPlayer1();
                player2Team[i] = newAlly;
                player1Team[i] = newEnemy;
            }
        }

        yield return new WaitForEndOfFrame();
    }

    private void SetGamesWonText()
    {
        myGamesWonText.text = (myGameWon == 0) ? "" : "Games Won: " + myGameWon;
        enemyGamesWonText.text = (enemyGamesWon == 0) ? "" : "Games Won: " + enemyGamesWon;
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

        SetGamesWonText();

        SetFilter(false);

        CleanUp();

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(EndOfMatchResults());

        this.photonView.RPC("ReadyToContinueRPC", RpcTarget.OthersBuffered); // First person to search is player 2

        while (waitingForEnemy == true)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(8f);

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

    public void GetBattlePlayList(List<string> battlPlayList)
    {
        battlePlayList = battlPlayList;
        StartCoroutine(PlayTheMusic());
    }

    public void SetFilter(bool regular)
    {
        if(regular)
        {
            RegularSnapshot.TransitionTo(0.5f);
        }
        else
        {
            DecidingSnapshot.TransitionTo(0.5f);
        }
    }

    private IEnumerator PlayTheMusic()
    {
        gameIsActive = true;

        if (battlePlayList.Count > 0)
        {
            int randomIndex = Random.Range(0, battlePlayList.Count);
            jukeBox.PlaySong(battlePlayList[randomIndex]);

            while (gameIsActive)
            {
                if (jukeBox.IsSongPlaying() == false)
                {
                    randomIndex = Random.Range(0, battlePlayList.Count);
                    jukeBox.PlaySong(battlePlayList[randomIndex]);
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
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

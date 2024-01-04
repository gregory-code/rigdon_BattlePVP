using Firebase.Database;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.Arm;

public class GameMenu : MonoBehaviourPunCallbacks, IDataPersistence
{
    [SerializeField] FirebaseScript firebaseScript;
    [SerializeField] battleMenu battleMenuScript;

    [Header("Game Menu")]
    [SerializeField] CanvasGroup myGroup;

    [SerializeField] GameObject intermission;
    [SerializeField] GameObject decisionMonster;

    [SerializeField] TextMeshProUGUI myTitle;
    [SerializeField] TextMeshProUGUI myTeamTitle;
    [SerializeField] TextMeshProUGUI myGamesWonText;
    int myGameWon = 0;

    [SerializeField] TextMeshProUGUI enemyTitle;
    [SerializeField] TextMeshProUGUI enemyTeamTitle;
    [SerializeField] TextMeshProUGUI enemyGamesWonText;
    int enemyGamesWon = 0;

    [Header("Choose New Mon")]
    [SerializeField] Image[] monsterImages;
    [SerializeField] Transform[] attackTransforms;
    [SerializeField] Transform[] abilityTransforms;

    [SerializeField] Button[] chooseMonButtons;

    private bool choosing;

    private List<moveContent> moveContentList = new List<moveContent>();
    [SerializeField] monster[] monsterOptions = new monster[3];

    [Header("Choose New Upgrade")]
    [SerializeField] GameObject choosingUpgrade;
    [SerializeField] Image[] upgradeImage;
    [SerializeField] TextMeshProUGUI[] upgradeTitle;
    [SerializeField] TextMeshProUGUI[] upgradeDescription;

    [SerializeField] Button[] chooseUpgardeButtons;

    [Header("Enemy Mons")]
    [SerializeField] monster[] enemyMonsters;
    [SerializeField] monsterPreferences[] enemyPrefs;

    [Header("HouseKeeping")]

    [SerializeField] GameObject allMenus;
    [SerializeField] GameObject menuChecks;

    [SerializeField] private expHolder[] expHolders;

    [Header("Intermission")]
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
    [SerializeField] List<monsterPreferences> draftMonsters = new List<monsterPreferences>();
    [SerializeField] string[] randoNames;

    bool gotEnemyTeam;

    [Header("Monster References")]
    [SerializeField] GameMaster gameMaster;
    [SerializeField] monster[] monsters;
    [SerializeField] private monster[] player1Team = new monster[3];
    [SerializeField] private monster[] player2Team = new monster[3];

    private void Update()
    {
        int lerp = (myGroup.interactable) ? 1 : 0;
        myGroup.alpha = Mathf.Lerp(myGroup.alpha, lerp, 4f * Time.deltaTime);
    }

    public monster[] GetEnemyTeam()
    {
        return (bIsPlayer1) ? player2Team : player1Team;
    }

    public monster[] GetMyTeam()
    {
        return (bIsPlayer1) ? player1Team : player2Team ;
    }

    public List<monster> GetBothTeams()
    {
        List<monster> teams = new List<monster>();
        for(int i = 0; i < player1Team.Length; i++)
        {
            if (player1Team[i] == null)
                continue;
            teams.Add(player1Team[i]);
        }
        for (int i = 0; i < player2Team.Length; i++)
        {
            if (player2Team[i] == null)
                continue;
            teams.Add(player2Team[i]);
        }
        return teams;
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

    private void SetIntermission(bool state)
    {
        intermission.SetActive(state);
        decisionMonster.SetActive(!state);
        choosingUpgrade.SetActive(!state);
    }

    private IEnumerator ChooseNewIDs(int whichAllyIndex)
    {
        if(bIsPlayer1 == false)
        {
            while(waitingForEnemy == true)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        int[] usedIDs = new int[2] { -1, -1};
        for (int i = 0; i < 3; i++)
        {
            bool foundOne = false;
            while(foundOne == false)
            {
                int randomIndex = Random.Range(0, draftMonsters.Count);
                int ID = draftMonsters[randomIndex].monsterValues[0];
                if (ID != usedIDs[0] && ID != usedIDs[1])
                {
                    if (usedIDs[0] == -1)
                        usedIDs[0] = ID;
                    else if (usedIDs[1] == -1)
                        usedIDs[1] = ID;

                    monsterOptions[i] = CreateMonster(draftMonsters[randomIndex], whichAllyIndex, true, false, false);
                    this.photonView.RPC("RemoveMonsterFromPoolRPC", RpcTarget.OthersBuffered, randomIndex);
                    draftMonsters.RemoveAt(randomIndex);
                    foundOne = true;

                }
            }

        }

        this.photonView.RPC("ReadyToContinueRPC", RpcTarget.OthersBuffered); // First person to search is player 2
    }

    private void NewAllyChoice()
    {
        intermission.SetActive(false);
        decisionMonster.SetActive(true);
        choosingUpgrade.SetActive(false);

        allMenus.SetActive(false);
        menuChecks.SetActive(false);

        foreach (moveContent content in moveContentList)
        {
            Destroy(content.gameObject);
        }
        moveContentList.Clear();

        for (int i = 0; i < monsterOptions.Length; i++)
        {
            moveContent[] contents = monsterOptions[i].GetMoveContents();

            monsterImages[i].sprite = monsterOptions[i].stages[monsterOptions[i].GetSpriteIndexFromLevel()];

            moveContent newAttack = Instantiate(contents[monsterOptions[i].GetAttackID() - 1], attackTransforms[i]);
            newAttack.transform.localPosition = Vector3.zero;
            moveContentList.Add(newAttack);

            moveContent newAbility = Instantiate(contents[monsterOptions[i].GetAbilityID() + 1], abilityTransforms[i]);
            newAbility.transform.localPosition = Vector3.zero;
            moveContentList.Add(newAbility);

            foreach (moveContent content in moveContentList)
            {
                content.SetTier(monsterOptions[i].GetSpriteIndexFromLevel(), monsterOptions[i].GetCurrentStatBlock());
            }

            chooseMonButtons[i].onClick.RemoveAllListeners();
            int index = i;
            chooseMonButtons[i].onClick.AddListener(() => SelectAlly(monsterOptions[index]));
        }
    }

    private void SelectAlly(monster chosenAlly)
    {
        if (bIsPlayer1)
        {
            player1Team[chosenAlly.GetIndex()] = chosenAlly;
        }
        else
        {
            player2Team[chosenAlly.GetIndex()] = chosenAlly;
        }

        choosing = false;
        SetCanvasGroup(false);
    }

    public IEnumerator SetUpGame(int format)
    {
        SetCanvasGroup(true);

        myGameWon = 0;
        enemyGamesWon = 0;
        SetGamesWonText();

        SetFilter(false);

        gotEnemyTeam = false;

        switch(format)
        {
            case 0:
                SetIntermission(true);
                myTeamPrefs = battleMenuScript.GetMonsterPrefsFromSelectedTeam();
                myTeamName = battleMenuScript.GetSelectedTeamName();
                StartCoroutine(StartRegularPVP());
                break;

            case 1:
                SetIntermission(true);
                int random = Random.Range(0, randoTeams.Length);
                myTeamPrefs = randoTeams[random].GetTeam();
                myTeamName = randoNames[random];
                StartCoroutine(StartRegularPVP());
                break;

            case 2:
                waitingForEnemy = true;
                choosing = true;
                yield return ChooseNewIDs(1);
                NewAllyChoice();

                while(choosing)
                {
                    yield return new WaitForEndOfFrame();
                }

                CreateEnemyAITeam();

                gameMaster.StartFight(true);

                SetCanvasGroup(false);

                break;
        }
    }

    private IEnumerator StartRegularPVP()
    {
        yield return StartCoroutine(GetUsernames());

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(SendMyTeamPrefs(myTeamName));

        yield return StartCoroutine(CreateTeams());

        yield return new WaitForSeconds(0.5f);

        SetGameMenu();

        yield return new WaitForSeconds(1f);

        allMenus.SetActive(false);
        menuChecks.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        gameMaster.StartFight(false);

        SetCanvasGroup(false);
    }

    private IEnumerator CreateTeams()
    {
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < 3; i++)
        {
            CreateMonster(myTeamPrefs[i], i, true, true, false);
            CreateMonster(enemyTeamPrefs[i], i, false, true, false);
        }
    }

    private monster CreateMonster(monsterPreferences pref, int teamIndex, bool isMine, bool AddToTeam, bool isAI)
    {
        monster newMon = (isAI) ? Instantiate(enemyMonsters[pref.monsterValues[0]]) : Instantiate(monsters[pref.monsterValues[0]]);
        newMon.SetInitialStats();
        newMon.SetFromPref(pref);
        newMon.SetTeamIndex(teamIndex);

        if (isAI)
            newMon.SetAsAI();

        newMon.SetOwnership(isMine);

        for (int x = 0; x < expHolders.Length; x++)
        {
            expHolder newHolder1 = Instantiate(expHolders[x]);
            expHolder newHolder2 = Instantiate(expHolders[x]);
            newMon.SetExpHolder(newHolder1);
            newMon.SetExpHolder(newHolder2);
        }

        if (bIsPlayer1)
        {
            if(isMine)
                newMon.SetAsPlayer1();

            if (AddToTeam)
            {
                if(isMine)
                {
                    player1Team[teamIndex] = newMon;
                }
                else
                {
                    player2Team[teamIndex] = newMon;
                }
            }
        }
        else
        {
            if (!isMine)
                newMon.SetAsPlayer1();

            if (AddToTeam)
            {
                if(isMine)
                {
                    player2Team[teamIndex] = newMon;
                }
                else
                {
                    newMon.SetAsPlayer1();
                    player1Team[teamIndex] = newMon;
                }
            }
        }

        return newMon;
    }

    private void CreateEnemyAITeam()
    {
        CreateMonster(enemyPrefs[0], 1, false, true, true);
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

        yield return new WaitForEndOfFrame();

        while (exp > 0)
        {
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
        SetIntermission(true);
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

        gameMaster.StartFight(false);

        SetCanvasGroup(false);
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
        for (int i = 0; i < myTeamPrefs.Length; i++)
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

    [PunRPC]
    void RemoveMonsterFromPoolRPC(int which)
    {
        draftMonsters.RemoveAt(which);
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

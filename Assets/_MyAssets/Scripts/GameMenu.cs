using Firebase.Database;
using Photon.Pun;
using System;
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
    [SerializeField] TextMeshProUGUI enemyTitle;
    [SerializeField] GameObject allMenus;
    [SerializeField] GameObject menuChecks;
    bool showCurtain;


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

        yield return StartCoroutine(GetUsernames());
        
        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(SendMyTeamPrefs());

        yield return StartCoroutine(CreateTeams());

        yield return new WaitForSeconds(0.5f);

        allMenus.SetActive(false);
        showCurtain = false;
        menuChecks.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        gameMaster.StartFight();

        SetCanvasGroup(false);
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

    private IEnumerator SendMyTeamPrefs()
    {
        string[] myPref = new string[3];
        string[] nickname = new string[3];
        for (int i = 0; i < 3; i++)
        {
            myPref[i] = myTeamPrefs[i].SeralizedPref();
            nickname[i] = myTeamPrefs[i].monsterNickname;
        }

        this.photonView.RPC("GetEnemyMonsterRPC", RpcTarget.OthersBuffered, myPref, nickname); // First person to search is player 2

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

            monster newEnemy = Instantiate(monsters[enemyTeamPrefs[i].monsterValues[0]]);
            newEnemy.SetInitialStats();
            newEnemy.SetFromPref(enemyTeamPrefs[i]);

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
    void GetEnemyMonsterRPC(string[] enemys, string[] nicknames)
    {
        for(int i = 0; i < enemys.Length; i++)
        {
            enemyTeamPrefs[i].DeseralizePref(enemys[i]);
            enemyTeamPrefs[i].monsterNickname = nicknames[i];
        }

        gotEnemyTeam = true;
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

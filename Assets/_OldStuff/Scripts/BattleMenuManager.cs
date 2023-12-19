using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;

public class BattleMenuManager : MonoBehaviourPunCallbacks
{
    //Online
    onlineScript OnlineScript;

    battleMusicManager battleJukeBox;

    loadingScript loading;

    [SerializeField] GameObject menuGameObject;
    [SerializeField] GameObject battleFieldGameObject;

    public enum battleMenuState { Menu, Searching, Initializing, Scouting, StartBattle, Battling, BattleEnd };
    public battleMenuState state { get; private set; }

    private void Awake()
    {
        OnlineScript = GameObject.FindGameObjectWithTag("Online").GetComponent<onlineScript>();

        battleJukeBox = GetComponent<battleMusicManager>();

        loading = GameObject.FindObjectOfType<loadingScript>();
    }

    public void setState(battleMenuState newState)
    {
        Debug.Log(newState);
        state = newState;
    }

    public IEnumerator setUpFight()
    {
        loading.show();

        yield return new WaitForSeconds(0.5f);

        //battleJukeBox.setMusicPlaylists(musicBox.startSongs, musicBox.middleSongs, musicBox.finalSongs, musicBox.battleSongs, !musicBox.bBattlePlayListPreference);
        battleJukeBox.setGameState("start");

        menuGameObject.SetActive(false);
        battleFieldGameObject.SetActive(true);

        loading.hide();
    }

    public void wrapUpFight()
    {
        StartCoroutine(backToMenu());
    }

    public IEnumerator backToMenu()
    {
        loading.show();

        yield return new WaitForSeconds(0.5f);

        battleJukeBox.stopSong();
        menuGameObject.SetActive(true);
        OnlineScript.rejoinLobby();

        yield return new WaitForSeconds(0.5f);

        loading.hide();
    }

}

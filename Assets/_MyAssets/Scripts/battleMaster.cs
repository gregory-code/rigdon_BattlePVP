using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using Photon.Pun.Demo.PunBasics;

public class battleMaster : MonoBehaviourPunCallbacks
{
    builderMenu BuilderMenu;

    notifScript NotificationScript;

    [Header("Critters")]
    [SerializeField] critter[] critterCollection;

    [SerializeField] private critter[] player1Team = new critter[3];
    [SerializeField] private critter[] player2Team = new critter[3];

    [SerializeField] private critterBuild proxyBuild; // for storing info to put into a function

    private bool bIsPlayer1;
    [SerializeField] private string player1;
    [SerializeField] private string player2;

    private void Awake()
    {
        BuilderMenu = GameObject.Find("builder").GetComponent<builderMenu>();

        NotificationScript = GameObject.FindGameObjectWithTag("Canvas").GetComponent<notifScript>();
    }

    public void setPlayerData(string player_1, string player_2)
    {
        player1 = player_1;
        player2 = player_2;
        bIsPlayer1 = (PhotonNetwork.NickName == player_1) ? true : false ;
        int playerNum = (bIsPlayer1) ? 1 : 2 ;
        NotificationScript.createNotif($"Loaded as player {playerNum}", Color.green);

        BuilderMenu.setActiveTeam(0); // you can change this when selecting teams, it's set to 0
        sendMyTeam();

        for (int i = 0; i < 3; ++i) // gets my team
        {
            player1Team[i] = critterCollection[BuilderMenu.activeCritterTeam[i].critterValue[0]];
            player1Team[i].SetFromCritterBuild(BuilderMenu.activeCritterTeam[i]);
        }
    }

    private void sendMyTeam()
    {
        for (int i = 0; i < 3; ++i)
        {
            int[] critterValues = BuilderMenu.activeCritterTeam[i].critterValue;
            string critterNickname = BuilderMenu.activeCritterTeam[i].critterNickname;
            this.photonView.RPC("recieveEnemyCritterRPC", RpcTarget.OthersBuffered, i, critterValues, critterNickname); // First person to search is player 2
        }
    }

    [PunRPC]
    void recieveEnemyCritterRPC(int whichMember, int[] enemyValues, string enemyNickname)
    {
        player2Team[whichMember] = critterCollection[enemyValues[0]]; // careful if you both have the same critter it might reference the same scriptable object

        proxyBuild.critterNickname = enemyNickname;
        for (int i = 0; i < proxyBuild.critterValue.Length; ++i)
        {
            proxyBuild.critterValue[i] = enemyValues[i];
        }

        player2Team[whichMember].SetFromCritterBuild(proxyBuild);
    }
}

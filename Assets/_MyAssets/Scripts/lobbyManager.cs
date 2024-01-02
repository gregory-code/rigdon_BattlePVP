using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using Firebase.Database;
using System;

public class lobbyManager : MonoBehaviourPunCallbacks
{
    notifScript NotificationScript;

    [SerializeField] FirebaseScript firebaseScript;
    [SerializeField] battleMenu battleMenuScript;
    [SerializeField] editPlayLists playLists;
    [SerializeField] GameMenu gameMenu;
    
    string gameRoomID = "";
    bool isLobbyRoom = true;

    int format = 0;

    [SerializeField] TextMeshProUGUI playersOnlineText;

    [SerializeField] GameObject connectedWifi;
    [SerializeField] GameObject disconnectedWifi;

    public delegate void OnUserJoinedRoom(bool isLobbyRoom);
    public event OnUserJoinedRoom onUserJoinedRoom;

    public delegate void OnUserLeftRoom();
    public event OnUserLeftRoom onUserLeftRoom;

    public delegate void OnOtherUserJoinedRoom(Player other);
    public event OnOtherUserJoinedRoom onOtherUserJoinedRoom;

    public delegate void OnOtherUserLeftRoom(Player other);
    public event OnOtherUserLeftRoom onOtherUserLeftRoom;

    [Header("Battle Searches")]
    [SerializeField] private List<string> standardRoomIDs = new List<string>();
    [SerializeField] private List<string> randomRoomIDs = new List<string>();
    [SerializeField] private List<string> draftRoomIDs = new List<string>();

    void Start()
    {
        NotificationScript = GameObject.FindObjectOfType<notifScript>();

        battleMenuScript.onSearchForMatch += SearchingForMatch;

        PhotonNetwork.ConnectUsingSettings();
    }

    private void SearchingForMatch(battleMenu.format chosenFormat, bool searching)
    {
        List<string> RoomIDs = new List<string>();
        int formatID = 0;

        switch(chosenFormat)
        {
            case battleMenu.format.standard:
                RoomIDs = standardRoomIDs;
                formatID = 0;
                format = 0;
                break;

            case battleMenu.format.random:
                RoomIDs = randomRoomIDs;
                formatID = 1;
                format = 1;
                break;

            case battleMenu.format.draft:
                RoomIDs = draftRoomIDs;
                formatID = 2;
                format = 2;
                break;
        }

        string myID = firebaseScript.GetUserID();

        foreach (string roomID in RoomIDs)
        {
            if (roomID == myID && searching == false)
            {
                this.photonView.RPC("UpdateRoomIDsRPC", RpcTarget.AllBufferedViaServer, myID, false, formatID);
                return;
            }
            else if (roomID != myID) // do some MMR matchmaking here
            {
                this.photonView.RPC("UpdateRoomIDsRPC", RpcTarget.AllBufferedViaServer, roomID, false, formatID);
                this.photonView.RPC("MatchFoundRPC", RpcTarget.All, myID, roomID);
                return;
            }
        }

        this.photonView.RPC("UpdateRoomIDsRPC", RpcTarget.AllBufferedViaServer, myID, true, formatID);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;

        if (isLobbyRoom == true)
        {
            roomOptions.MaxPlayers = 20;
            PhotonNetwork.JoinOrCreateRoom("LobbyRoom", roomOptions, TypedLobby.Default);
        }
        else
        {
            roomOptions.MaxPlayers = 4;
            PhotonNetwork.JoinOrCreateRoom(gameRoomID, roomOptions, TypedLobby.Default);
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        UpdatePlayersOnline();

        onUserJoinedRoom?.Invoke(isLobbyRoom);

        changeWifiState(true);

        if (isLobbyRoom == false)
        {
            isLobbyRoom = true;

            List<string> playerIDs = gameRoomID.ToString().Split('õ').ToList();
            gameMenu.GetBattlePlayList(playLists.battleSongs);
            gameMenu.SetPlayerIDs(playerIDs[0], playerIDs[1]);
            StartCoroutine(gameMenu.SetUpGame(format));

        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);

        changeWifiState(false);

        NotificationScript.createNotif($"{message}", Color.red);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        changeWifiState(false);

        onUserLeftRoom?.Invoke();

        PhotonNetwork.JoinLobby();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        UpdatePlayersOnline();

        onOtherUserJoinedRoom?.Invoke(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        UpdatePlayersOnline();

        onOtherUserLeftRoom?.Invoke(otherPlayer);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);

        changeWifiState(false);

        NotificationScript.createNotif($"{cause}", Color.red);
    }

    private void UpdatePlayersOnline()
    {
        playersOnlineText.text = "Players Online: " + (PhotonNetwork.CurrentRoom.PlayerCount - 1);
    }

    private void changeWifiState(bool state)
    {
        connectedWifi.SetActive(state);
        disconnectedWifi.SetActive(!state);
    }

    [PunRPC]
    void MatchFoundRPC(string player1ID, string player2ID)
    {
        if (firebaseScript.GetUserID() != player1ID && firebaseScript.GetUserID() != player2ID) return;

        isLobbyRoom = false;
        gameRoomID = player1ID + "õ" + player2ID;
        PhotonNetwork.LeaveRoom();
    }

    [PunRPC]
    void UpdateRoomIDsRPC(string roomID, bool bAddToList, int whichFormat)
    {
        switch (whichFormat)
        {
            case 0:
                if (bAddToList)
                    standardRoomIDs.Add(roomID);
                else
                    standardRoomIDs.Remove(roomID);
                break;

            case 1:
                if (bAddToList)
                    randomRoomIDs.Add(roomID);
                else
                    randomRoomIDs.Remove(roomID);
                break;

            case 2:
                if (bAddToList)
                    draftRoomIDs.Add(roomID);
                else
                    draftRoomIDs.Remove(roomID);
                break;
        }
    }
}

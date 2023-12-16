using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Database;
using System;

public class lobbyManager : MonoBehaviourPunCallbacks
{
    notifScript NotificationScript;
    
    string gameRoomID;
    bool isLobbyRoom = true;

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

    void Start()
    {
        NotificationScript = GameObject.FindObjectOfType<notifScript>();

        PhotonNetwork.ConnectUsingSettings();
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
        //changeWifiState();

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
}

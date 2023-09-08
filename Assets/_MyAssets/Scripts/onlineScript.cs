using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;

public class onlineScript : MonoBehaviourPunCallbacks
{
    private PhotonView _view;
    private menuScript _MS;

    private int _selectedLobby;


    void Start()
    {
        _view = GetComponent<PhotonView>();
        _MS = GetComponent<menuScript>();

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() { PhotonNetwork.JoinLobby(); }

    public override void OnJoinedLobby() { switchWifi(); }

    public void AccessRoom(int lobbyNum)
    {
        if (PhotonNetwork.InRoom) return;

        _selectedLobby = lobbyNum;
        PhotonNetwork.JoinRoom("Lobby_" + _selectedLobby);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined the lobby");
        Debug.Log("Player Count: " + PhotonNetwork.CurrentRoom.PlayerCount);

        this.photonView.RPC("updateLobbyText", RpcTarget.AllBuffered, _selectedLobby, _MS.PlayerName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("message: " + message);

        if (message == "Game does not exist")
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 2;
            PhotonNetwork.CreateRoom("Lobby_" + _selectedLobby, roomOptions);
        }
    }

    private void switchWifi()
    {
        Image wifiImage = GameObject.Find("wifi").GetComponent<Image>();
        TextMeshProUGUI wifiText = GameObject.Find("wifi_Text").GetComponent<TextMeshProUGUI>();

        if (wifiText.text == "Online")
        {
            wifiImage.color = new Color(255, 0, 31, 255);
            wifiText.text = "Trying";

        }
        else
        {
            wifiImage.color = new Color(0, 255, 0, 255);
            wifiText.text = "Online";
        }
    }

    [PunRPC]
    void updateLobbyText(int newLobby, string name)
    {
        TextMeshProUGUI lobbyText = transform.Find("lobby_" + newLobby).Find("lobbyName").GetComponent<TextMeshProUGUI>();

        lobbyText.text = "-Waiting to Battle-         " + name;
    }
}

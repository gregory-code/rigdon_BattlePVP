using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using System.Linq;
using Photon.Pun.Demo.PunBasics;

public class onlineScript : MonoBehaviourPunCallbacks, IDataPersistence
{
    notifScript NotificationScript;

    //Firebase Data
    FirebaseScript fireBaseScript;

    loadingScript loading;

    //battleMenuManager
    BattleMenuManager battleMenuManager;

    menuScript MenuScript;

    bool bJoiningFightRoom;
    string fightRoomID = "";

    [SerializeField] TMP_Text playersInLobby;

    [SerializeField] Image wifiImage;
    [SerializeField] TMP_Text wifiText;

    [Header("Battle Searches")]
    [SerializeField] private List<string> waitingRoomIDs = new List<string>();

    [Header("Friends List")]
    [SerializeField] private TMP_InputField friendInput;
    [SerializeField] private GameObject friendsMenu;
    [SerializeField] private GameObject friendPrefab;
    [SerializeField] private List<string> friendsList = new List<string>();
    [SerializeField] private List<string> friendsListID = new List<string>();
    private GameObject currentFren;
    private List<GameObject> friendsListGameobjects = new List<GameObject>();

    private PhotonView _view;
    private menuScript _MS;
    private int _selectedLobby;



    void Start()
    {
        bJoiningFightRoom = false;

        loading = GameObject.Find("LoadingScreen").GetComponent<loadingScript>();

        battleMenuManager = GameObject.FindGameObjectWithTag("BattleMenuManager").GetComponent<BattleMenuManager>();
        NotificationScript = GameObject.FindGameObjectWithTag("Canvas").GetComponent<notifScript>();
        fireBaseScript = GameObject.FindGameObjectWithTag("Data").GetComponent<FirebaseScript>();
        MenuScript = GameObject.FindGameObjectWithTag("Canvas").transform.Find("menu").GetComponent<menuScript>();

        _view = GetComponent<PhotonView>();
        _MS = GetComponent<menuScript>();

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

        if (bJoiningFightRoom == true)
        {
            roomOptions.MaxPlayers = 4;
            PhotonNetwork.JoinOrCreateRoom(fightRoomID, roomOptions, TypedLobby.Default);
        }
        else
        {
            PhotonNetwork.JoinOrCreateRoom("LobbyRoom", roomOptions, TypedLobby.Default);
            roomOptions.MaxPlayers = 20;
        }
        changeWifiState();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        PhotonNetwork.JoinLobby();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) { whosInLobby(); }
    public override void OnPlayerLeftRoom(Player newPlayer) { whosInLobby(); }
    public override void OnJoinedRoom() 
    {
        whosInLobby(); 
        
        if(bJoiningFightRoom == true)
        {
            bJoiningFightRoom = false;

            List<string> names = fightRoomID.ToString().Split('õ').ToList();
            GameObject.FindGameObjectWithTag("BattleField").GetComponent<battleMaster>().setPlayerData(names[0], names[1]);
            StartCoroutine(battleMenuManager.setUpFight());
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        NotificationScript.createNotif($"{message}", Color.red);
    }

    public void whosInLobby()
    {
        playersInLobby.text = "Players In Lobby: " + (PhotonNetwork.CurrentRoom.PlayerCount - 1);

        foreach (GameObject friendGameObject in friendsListGameobjects)
        {
            friendGameObject.transform.Find("status").GetComponent<Image>().color = Color.red;
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            foreach (GameObject friendGameObject in friendsListGameobjects)
            {
                if (friendGameObject.name == player.NickName)
                {
                    friendGameObject.transform.Find("status").GetComponent<Image>().color = Color.green;
                }
            }
        }
    }

    public void quickPlaySearch()
    {
        if (PhotonNetwork.InRoom == false) return;

        foreach(string waitingID in waitingRoomIDs)
        {
            if(waitingID == PhotonNetwork.NickName)
            {
                return;
            }
            else if(waitingID != PhotonNetwork.NickName) // do some MMR matchmaking here
            {
                this.photonView.RPC("updateSearchWaitingRPC", RpcTarget.AllBufferedViaServer, waitingID, false, 0);
                this.photonView.RPC("matchFoundRPC", RpcTarget.All, PhotonNetwork.NickName, waitingID);
                return;
            }
        }

        this.photonView.RPC("updateSearchWaitingRPC", RpcTarget.AllBufferedViaServer, PhotonNetwork.NickName, true, 0);
    }

    public void setNickName(string nickname)
    {
        PhotonNetwork.NickName = nickname;
    }

    public override void OnDisconnected(DisconnectCause cause) { changeWifiState(); }

    public void sendFriendInvite()
    {
        if (PhotonNetwork.InRoom == false) return;

        if (friendInput.text == "")
        {
            NotificationScript.createNotif($"Input field is empty", Color.red);
            return;
        }

        foreach(string name in friendsList)
        {
            if(name == friendInput.text)
            {
                NotificationScript.createNotif($"Already friends with {friendInput.text}", Color.yellow);
                return;
            }
        }

        foreach(Player p in PhotonNetwork.PlayerList)
        {
            if(p.NickName == friendInput.text)
            {
                this.photonView.RPC("sendFriendInviteRPC", RpcTarget.Others, friendInput.text, MenuScript.UsernameField.text, fireBaseScript.GetUserID());
                NotificationScript.createNotif($"Friend request sent!", Color.green);
                return;
            }
        }

        NotificationScript.createNotif($"Player is not online, or name is incorrect", Color.yellow);
    }

    public void addFriend(string friendUsernameID)
    {
        friendsListID.Add(friendUsernameID);
        StartCoroutine(fireBaseScript.UpdateObject("friends", string.Join("õ", friendsListID)));

        GameObject newFren = Instantiate(friendPrefab);
        newFren.transform.SetParent(friendsMenu.transform);
        newFren.transform.localScale = new Vector3(1, 1, 1);

        float yPlacement = (friendsListID.Count == 1) ? 25 : 25 - ((friendsListID.Count - 1) * (75));
        newFren.transform.localPosition = new Vector2(-50, yPlacement);

        friendsListGameobjects.Add(newFren);

        currentFren = newFren;
        StartCoroutine(fireBaseScript.LoadOtherPlayersData(friendUsernameID, "username"));
    }

    private void changeWifiState()
    {
        if(PhotonNetwork.IsConnected)
        {
            wifiImage.color = new Color(0, 255, 0, 255);
            wifiText.text = "Online";
        }
        else
        {
            wifiImage.color = new Color(255, 0, 31, 255);
            wifiText.text = "Offline";
        }
    }

    public void LoadData(Dictionary<string, object> dataDictionary)
    {
        int friendsInList = 0;
        while (dataDictionary.ContainsKey("startSongs" + friendsInList))
        {
            if (dataDictionary["startSongs" + friendsInList].ToString() == "") return;
            addFriend(dataDictionary["startSongs" + friendsInList].ToString());
            friendsInList++;
        }
    }

    public void LoadOtherPlayersData(string key, object data)
    {
        if(key == "username")
        {
            friendsList.Add(data.ToString());
            currentFren.name = data.ToString();
            currentFren.transform.Find("name").GetComponent<TMP_Text>().text = data.ToString();
        }
    }

    public void updateFriendsName(string newName)
    {
        if (PhotonNetwork.InRoom == false) return;

        this.photonView.RPC("updateFriendsNameRPC", RpcTarget.Others, PhotonNetwork.NickName, newName);
    }

    [PunRPC]
    void matchFoundRPC(string player1ID, string player2ID)
    {
        if (PhotonNetwork.NickName != player1ID && PhotonNetwork.NickName != player2ID) return;

        bJoiningFightRoom = true;
        fightRoomID = player1ID + "õ" + player2ID;
        PhotonNetwork.LeaveRoom();
    }

    [PunRPC]
    void updateSearchWaitingRPC(string roomID, bool bAdd, int whichMode)
    {
        switch(whichMode)
        {
            case 0:
                if (bAdd) waitingRoomIDs.Add(roomID);
                else waitingRoomIDs.Remove(roomID);
                break;
        }
    }

    [PunRPC]
    void updateFriendsNameRPC(string oldName, string newName)
    {
        foreach (GameObject friendGameObject in friendsListGameobjects)
        {
            if (friendGameObject.name == oldName)
            {
                friendGameObject.transform.Find("name").GetComponent<TMP_Text>().text = newName;
            }
        }

        for(int i = 0; i < friendsList.Count; ++i)
        {
            if (friendsList[i] == oldName)
            {
                friendsList[i] = newName;
            }
        }
    }

    [PunRPC]
    void sendFriendInviteRPC(string name, string senderName, string senderID)
    {
        if (name == MenuScript.UsernameField.text)
        {
            //I'm just going to automatically assume y'all are good, even though you should probably ask before becoming friends first
            addFriend(senderID);

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p.NickName == senderName)
                {
                    this.photonView.RPC("acceptedFriendInviteRPC", RpcTarget.Others, senderName, fireBaseScript.GetUserID());
                    NotificationScript.createNotif($"Friend request accepted!", Color.green);
                    return;
                }
            }

            NotificationScript.createNotif($"Player is not online anymore", Color.yellow);
        }
    }

    [PunRPC]
    void acceptedFriendInviteRPC(string name, string senderID)
    {
        if (name == MenuScript.UsernameField.text)
        {
            addFriend(senderID);
        }
    }
}

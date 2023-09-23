using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using System.Linq;
using Unity.VisualScripting;

public class onlineScript : MonoBehaviourPunCallbacks, IDataPersistence
{
    notifScript NotificationScript;

    //Firebase Data
    FirebaseScript fireBaseScript;

    menuScript MenuScript;

    [SerializeField] TMP_Text playersInLobby;

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
        NotificationScript = GameObject.FindGameObjectWithTag("Canvas").GetComponent<notifScript>();
        fireBaseScript = GameObject.FindGameObjectWithTag("Data").GetComponent<FirebaseScript>();
        MenuScript = GameObject.FindGameObjectWithTag("Canvas").transform.Find("menu").GetComponent<menuScript>();

        _view = GetComponent<PhotonView>();
        _MS = GetComponent<menuScript>();

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() { PhotonNetwork.JoinLobby(); }

    public override void OnJoinedLobby() 
    { 
        changeWifiState();
    }

    public void joinLobbyRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 20;
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;

        PhotonNetwork.JoinOrCreateRoom("LobbyRoom", roomOptions, TypedLobby.Default);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) { whosInLobby(); }
    public override void OnPlayerLeftRoom(Player newPlayer) { whosInLobby(); }
    public override void OnJoinedRoom() { whosInLobby(); }

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

    public void setNickName(string nickname)
    {
        PhotonNetwork.NickName = nickname;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        NotificationScript.createNotif($"{message}", Color.red);
    }

    public override void OnDisconnected(DisconnectCause cause) { changeWifiState(); }

    public void sendFriendInvite()
    {
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
        Image wifiImage = GameObject.Find("wifi").GetComponent<Image>();
        TextMeshProUGUI wifiText = GameObject.Find("wifi_Text").GetComponent<TextMeshProUGUI>();

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
        List<string> stringList = dataDictionary["friends"].ToString().Split('õ').ToList();
        if (stringList[0] == "") return;
        foreach(string friend in stringList)
        {
            addFriend(friend);
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
        this.photonView.RPC("updateFriendsNameRPC", RpcTarget.Others, PhotonNetwork.NickName, newName);
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

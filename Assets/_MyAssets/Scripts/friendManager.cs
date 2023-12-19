using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Firebase.Database;
using System;
using TMPro;
using static UnityEngine.Rendering.DebugUI;

public class friendManager : MonoBehaviourPunCallbacks, IDataPersistence
{
    [SerializeField] lobbyManager lobbyManagerScript;
    [SerializeField] FirebaseScript fireBaseScript;
    [SerializeField] profileSettings profileScript;
    notifScript NotificationScript;

    [SerializeField] Transform friendsMenuTransform;
    [SerializeField] Transform friendsButtonTransform;
    [SerializeField] GameObject notificationGameObject;
    [SerializeField] TextMeshProUGUI notifText;

    int notifAmount = 0;
    bool openFriendMenu;
    bool isConnectedToRoom;
    string friendSearch = "";

    string foundUsername;

    [SerializeField] friendPrefab friendPrefab;
    [SerializeField] friendRequest friendRequestPrefab;

    [SerializeField] Transform friendsList;

    private List<friendPrefab> friendPrefabList = new List<friendPrefab>();
    private List<friendRequest> friendRequestList = new List<friendRequest>();

    private List<string> friendsIDs = new List<string>();

    public void Start()
    {
        lobbyManagerScript.onUserJoinedRoom += JoinedRoom;
        lobbyManagerScript.onUserLeftRoom += LeftRoom;
        lobbyManagerScript.onOtherUserJoinedRoom += OtherJoinedRoom;
        lobbyManagerScript.onOtherUserLeftRoom += OtherLeftRoom;

        profileScript.onUsernameChanged += NewUsername;

        NotificationScript = GameObject.FindObjectOfType<notifScript>();
    }

    private void Update()
    {
        Vector2 friendsMenuLerp = (openFriendMenu) ? new Vector2(435, 0) : new Vector2(850, 0);
        friendsMenuTransform.localPosition = Vector3.Lerp(friendsMenuTransform.localPosition, friendsMenuLerp, 10 * Time.deltaTime);

        Vector2 friendsButtonLerp = (openFriendMenu) ? new Vector2(-180, 235) : new Vector2(-360, 235);
        friendsButtonTransform.localPosition = Vector3.Lerp(friendsButtonTransform.localPosition, friendsButtonLerp, 5 * Time.deltaTime);
    }

    public void FriendMenuState()
    {
        openFriendMenu = !openFriendMenu;
    }

    public void FriendSearchUpdate(string search)
    {
        friendSearch = search;
    }

    private void UpdateNotifs(int change)
    {
        notifAmount += change;

        if (notifAmount < 0)
            notifAmount = 0;

        bool isActive = (notifAmount == 0) ? false : true;

        notificationGameObject.SetActive(isActive);
        notifText.text = notifAmount + "";

    }

    public void SendFriendRequest()
    {
        if (isConnectedToRoom == false)
        {
            NotificationScript.createNotif($"No internet", Color.red);
            return;
        }

        if (friendSearch == "")
        {
            NotificationScript.createNotif($"Enter a name!", Color.red);
            return;
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {

            if (AlreadyHasFriendRequest(player.NickName))
                return;

            if (AlreadyFriends(player.NickName))
                return;

            if (player.NickName == friendSearch)
            {
                NotificationScript.createNotif($"Friend request sent to {friendSearch}!", Color.green);
                this.photonView.RPC("SendFriendRequestRPC", RpcTarget.Others, friendSearch, profileScript.GetUsername(), fireBaseScript.GetUserID());
                return;
            }
        }

        NotificationScript.createNotif($"{friendSearch} not found or is offline", Color.red);
    }

    private bool AlreadyHasFriendRequest(string requrestName)
    {
        if (friendRequestList.Count <= 0)
            return false;

        foreach (friendRequest friend in friendRequestList)
        {
            if (friend.GetRequestsName() == requrestName)
            {
                NotificationScript.createNotif($"Already have a friend invite by {friendSearch}", Color.yellow);
                return true;
            }
        }

        return false;
    }

    private bool AlreadyFriends(string friendName)
    {
        if (friendPrefabList.Count <= 0)
            return false;

        foreach (friendPrefab friend in friendPrefabList)
        {
            if (friend.GetFriendsName() == friendName)
            {
                NotificationScript.createNotif($"Already friends with {friendSearch}", Color.yellow);
                return true;
            }
        }

        return false;
    }

    private void JoinedRoom(bool isLobbyRoom)
    {
        isConnectedToRoom = true;
    }

    private void LeftRoom()
    {
        isConnectedToRoom = false;
    }

    private void OtherJoinedRoom(Player other)
    {
        SetFriendOnlineStatus(true, other.NickName);
    }

    private void OtherLeftRoom(Player other)
    {
        SetFriendOnlineStatus(false, other.NickName);
    }

    public void OtherPlayerLoadedIn(string nickname)
    {
        this.photonView.RPC("OtherPlayerLoadedInRPC", RpcTarget.Others, true, nickname);
    }

    private void SetFriendOnlineStatus(bool state, string nickname)
    {
        foreach (friendPrefab friend in friendPrefabList)
        {
            if (nickname == friend.GetFriendsName())
            {
                friend.SetOnlineStatus(state);
            }
        }
    }

    public IEnumerator LoadData(DataSnapshot data)
    {
        for (int i = 0; i < data.Child("friends").ChildrenCount; ++i)
        {
            string currentID = data.Child("friends").Child("" + i).Value.ToString();
            yield return StartCoroutine(fireBaseScript.LoadOtherPlayersData(currentID, "username"));
            CreateNewFriendPrefab(currentID, foundUsername);
        }

        yield return new WaitForEndOfFrame();
    }

    public void LoadOtherPlayersData(string key, object data)
    {
        if (key == "username")
        {
            foundUsername = data.ToString();
        }
    }

    private void CreateNewFriendPrefab(string friendID, string username)
    {
        friendPrefab friend = Instantiate(friendPrefab, friendsList);
        friend.Init(username, friendID);
        friendPrefabList.Add(friend);

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.NickName == username)
            {
                friend.SetOnlineStatus(true);
            }
        }
    }

    private IEnumerator AddFriendToCloud(string ID)
    {
        friendsIDs.Add(ID);
        StartCoroutine(fireBaseScript.UpdateObject("friends", friendsIDs));
        yield return StartCoroutine(fireBaseScript.LoadOtherPlayersData(ID, "username"));

        CreateNewFriendPrefab(ID, foundUsername);
    }

    public void AcceptFriend(friendRequest request, string senderName, string senderID, string yourName)
    {
        StartCoroutine(AddFriendToCloud(senderID));

        this.photonView.RPC("AcceptFriendRequestRPC", RpcTarget.Others, senderName, yourName, fireBaseScript.GetUserID());

        DeleteFriendRequest(request);
    }

    public void DeleteFriendRequest(friendRequest request)
    {
        friendRequestList.Remove(request);
        Destroy(request.gameObject);
        UpdateNotifs(-1);
    }

    private void NewUsername(string newUsername)
    {
        foreach(Player player in PhotonNetwork.PlayerList)
        {
            this.photonView.RPC("UpdateFriendsUsername", RpcTarget.Others, player.NickName, newUsername, fireBaseScript.GetUserID());
        }
    }

    [PunRPC]
    void SendFriendRequestRPC(string myName, string senderName, string senderID)
    {
        if (myName != profileScript.GetUsername())
            return;

        foreach(friendRequest request in friendRequestList)
        {
            if (request.GetRequestsName() == senderName)
                return;
        }


        UpdateNotifs(1);
        //play a sound plz

        friendRequest friend = Instantiate(friendRequestPrefab, friendsList);
        friend.Init(senderName, senderID, myName, this);
        friendRequestList.Add(friend);
    }

    [PunRPC]
    void AcceptFriendRequestRPC(string myName, string senderName, string senderID)
    {
        if (myName != profileScript.GetUsername())
            return;

        StartCoroutine(AddFriendToCloud(senderID));

        NotificationScript.createNotif($"{senderName} accepted your friend request!", Color.green);
    }

    [PunRPC]
    void UpdateFriendsUsername(string myName, string newUsername, string friendID)
    {
        if (myName != PhotonNetwork.NickName)
            return;

        foreach (friendRequest request in friendRequestList)
        {
            if (request.GetRequestsID() == friendID)
            {
                request.SetRequestsName(newUsername);
            }
        }

        foreach (friendPrefab friend in friendPrefabList)
        {
            if(friend.GetFriendsID() == friendID)
            {
                friend.SetFriendsName(newUsername);
            }
        }
    }

    [PunRPC]
    void OtherPlayerLoadedInRPC(bool state, string nickname)
    {
        SetFriendOnlineStatus(true, nickname);
    }
}

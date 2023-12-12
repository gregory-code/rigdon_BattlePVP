using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Firebase.Database;
using System;
using TMPro;
using UnityEditor;

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

    [SerializeField] friendPrefab friendPrefab;
    [SerializeField] friendRequest friendRequestPrefab;

    [SerializeField] Transform friendsList;

    private List<friendPrefab> friendsListObjects = new List<friendPrefab>();
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

        if(friendsListObjects.Count > 0)
        {
            foreach (friendPrefab friend in friendsListObjects)
            {
                if (friendSearch == friend.GetFriendsName())
                {
                    NotificationScript.createNotif($"Already friends with {friendSearch}", Color.yellow);
                    return;
                }
            }
        }

        if(friendRequestList.Count > 0)
        {
            foreach (friendRequest request in friendRequestList)
            {
                if (request.GetName() == friendSearch)
                {
                    NotificationScript.createNotif($"Already have a friend request from {friendSearch}", Color.yellow);
                    return;
                }
            }
        }

        foreach(Player player in PhotonNetwork.PlayerList)
        {
            if(player.NickName == friendSearch)
            {
                NotificationScript.createNotif($"Friend request sent to {friendSearch}!", Color.green);
                this.photonView.RPC("SendFriendRequestRPC", RpcTarget.Others, friendSearch, PhotonNetwork.NickName, fireBaseScript.GetUserID());
                return;
            }
        }

        NotificationScript.createNotif($"User {friendSearch} not found or is offline", Color.red);
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
        SetFriendOnlineStatus(true, other);
    }

    private void OtherLeftRoom(Player other)
    {
        SetFriendOnlineStatus(false, other);
    }

    private void SetFriendOnlineStatus(bool state, Player player)
    {
        foreach (friendPrefab friend in friendsListObjects)
        {
            if(player.NickName == friend.GetFriendsName())
            {
                friend.SetOnlineStatus(state);
            }
        }
    }

    public void LoadData(DataSnapshot data)
    {
        for (int i = 0; i < data.Child("friends").ChildrenCount; ++i)
        {
            StartCoroutine(fireBaseScript.LoadOtherPlayersData(data.Child("friends").Child("" + i).Value.ToString(), "username"));
        }
    }

    public void LoadOtherPlayersData(string key, object data)
    {
        if (key == "username")
        {
            friendPrefab friend = Instantiate(friendPrefab, friendsList);
            friend.Init(data.ToString());
            friendsListObjects.Add(friend);

            foreach(Player player in PhotonNetwork.PlayerList)
            {
                if(player.NickName == data.ToString())
                {
                    friend.SetOnlineStatus(true);
                }
            }
        }
    }

    private void AddFriendToCloud(string ID)
    {
        friendsIDs.Add(ID);
        StartCoroutine(fireBaseScript.UpdateObject("friends", friendsIDs));
        StartCoroutine(fireBaseScript.LoadOtherPlayersData(ID, "username"));
    }

    public void AcceptFriend(friendRequest request, string senderName, string senderID, string yourName)
    {
        AddFriendToCloud(senderID);

        this.photonView.RPC("AcceptFriendRequestRPC", RpcTarget.Others, senderName, yourName, fireBaseScript.GetUserID());

        DeleteFriendRequest(request);
    }

    public void DeleteFriendRequest(friendRequest request)
    {
        friendRequestList.Remove(request);
        Destroy(request.gameObject);
        UpdateNotifs(-1);
    }

    private bool AlreadyHasFriendRequest(string senderID)
    {
        foreach(friendRequest friend in friendRequestList)
        {
            if(friend.IsSenderID(senderID))
            {
                return true;
            }
        }

        return false;
    }

    private void NewUsername(string oldName, string newUsername)
    {
        foreach (friendPrefab friend in friendsListObjects)
        {
            this.photonView.RPC("UpdateFriendsUsername", RpcTarget.Others, friend.GetFriendsName(), oldName, newUsername);
        }
    }

    [PunRPC]
    void SendFriendRequestRPC(string myName, string senderName, string senderID)
    {
        if (myName != PhotonNetwork.NickName)
            return;
            
        if (AlreadyHasFriendRequest(senderID))
            return;

        UpdateNotifs(1);
        //play a sound plz

        friendRequest friend = Instantiate(friendRequestPrefab, friendsList);
        friend.Init(senderName, senderID, myName, this);
        friendRequestList.Add(friend);
    }

    [PunRPC]
    void AcceptFriendRequestRPC(string myName, string senderName, string senderID)
    {
        if (myName != PhotonNetwork.NickName)
            return;

        AddFriendToCloud(senderID);

        NotificationScript.createNotif($"{senderName} accepted your friend request!", Color.green);
    }

    [PunRPC]
    void UpdateFriendsUsername(string myName, string oldUsername, string newUsername)
    {
        if (myName != PhotonNetwork.NickName)
            return;

        foreach(friendPrefab friend in friendsListObjects)
        {
            if(friend.GetFriendsName() == oldUsername)
                friend.SetFriendsName(newUsername);
        }
    }
}

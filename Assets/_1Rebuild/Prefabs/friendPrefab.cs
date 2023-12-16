using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class friendPrefab : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;

    [SerializeField] GameObject isOnline;
    [SerializeField] GameObject isOffline;

    private string friendsName;
    private string friendID;

    public void Init(string nameText, string friendID)
    {
        SetFriendsName(nameText);
        this.friendID = friendID;
    }

    public void SetFriendsName(string newName)
    {
        this.nameText.text = newName;
        friendsName = newName;
    }

    public string GetFriendsID()
    {
        return friendID;
    }

    public string GetFriendsName()
    {
        return friendsName;
    }

    public void SetOnlineStatus(bool state)
    {
        isOnline.SetActive(state);
        isOffline.SetActive(!state);
    }
}

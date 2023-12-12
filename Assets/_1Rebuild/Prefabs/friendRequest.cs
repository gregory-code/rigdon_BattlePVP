using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class friendRequest : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Button acceptButton;
    [SerializeField] Button declineButton;

    private string requestName;

    private string senderID;

    public void Init(string senderName, string senderID, string myName, friendManager manager)
    {
        nameText.text = senderName;
        requestName = senderName;
        this.senderID = senderID;

        acceptButton.onClick.AddListener(() => manager.AcceptFriend(this, senderName, senderID, myName));
        declineButton.onClick.AddListener(() => manager.DeleteFriendRequest(this));
    }

    public string GetName()
    {
        return requestName;
    }

    public bool IsSenderID(string check)
    {
        return (senderID == check);
    }
}

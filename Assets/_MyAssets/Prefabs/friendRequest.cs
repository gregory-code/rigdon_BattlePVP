using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Xml.Linq;

public class friendRequest : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Button acceptButton;
    [SerializeField] Button declineButton;

    private string requestName;
    private string myName;
    private string requestID;

    friendManager friendManager;

    public void Init(string senderName, string requestID, string myName, friendManager manager)
    {
        this.requestID = requestID;
        this.myName = myName;
        friendManager = manager;
        SetRequestsName(senderName);
    }

    public void SetRequestsName(string newName)
    {
        acceptButton.onClick.RemoveAllListeners();
        declineButton.onClick.RemoveAllListeners();

        nameText.text = newName;
        requestName = newName;

        acceptButton.onClick.AddListener(() => friendManager.AcceptFriend(this, newName, requestID, myName));
        declineButton.onClick.AddListener(() => friendManager.DeleteFriendRequest(this));
    }

    public string GetRequestsName()
    {
        return requestName;
    }

    public string GetRequestsID()
    {
        return requestID;
    }
}

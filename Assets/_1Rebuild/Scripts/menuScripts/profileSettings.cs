using Firebase.Database;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class profileSettings : MonoBehaviour, IDataPersistence
{
    [SerializeField] notifScript NotificationScript;
    [SerializeField] FirebaseScript fireBaseScript;
    [SerializeField] friendManager friendScript;

    [SerializeField] Sprite[] themeFullRect;
    [SerializeField] Sprite[] themeHoloRect;
    [SerializeField] Sprite[] themeFullCircle;
    [SerializeField] Sprite[] themeHoloCircle;
    [SerializeField] Sprite[] themeBackground;

    GameObject[] fullRects;
    GameObject[] holoRects;
    GameObject[] fullCircles;
    GameObject[] holoCircles;

    [SerializeField] Image backgroundImage;

    [SerializeField] TMP_InputField usernameField;
    private string username;
    int theme = 0;

    public delegate void OnUsernameChanged(string newUsername);
    public event OnUsernameChanged onUsernameChanged;

    public void Start()
    {
        fullRects = GameObject.FindGameObjectsWithTag("FullRect");
        holoRects = GameObject.FindGameObjectsWithTag("HoloRect");
        fullCircles = GameObject.FindGameObjectsWithTag("FullCircle");
        holoCircles = GameObject.FindGameObjectsWithTag("HoloCircle");
    }

    public void UpdateUsername(string newName)
    {
        username = newName;
        StartCoroutine(fireBaseScript.UpdateUsernameAuth(username));
        StartCoroutine(fireBaseScript.UpdateObject("username", username));

        PhotonNetwork.NickName = username;

        onUsernameChanged?.Invoke(newName);
    }    

    public void ChangeTheme()
    {
        theme++;

        if (theme > themeFullRect.Length - 1)
            theme = 0;

        StartCoroutine(fireBaseScript.UpdateObject("theme", theme));

        UpdateTheme();
    }

    private void UpdateTheme()
    {
        backgroundImage.sprite = themeBackground[theme];
        ChangeTaggedSprites(fullRects, themeFullRect);
        ChangeTaggedSprites(holoRects, themeHoloRect);
        ChangeTaggedSprites(fullCircles, themeFullCircle);
        ChangeTaggedSprites(holoCircles, themeHoloCircle);
    }

    public string GetUsername()
    {
        return username;
    }

    private void ChangeTaggedSprites(GameObject[] gameObjectArray, Sprite[] library)
    {
        foreach (GameObject taggedObject in gameObjectArray)
        {
            taggedObject.GetComponent<Image>().sprite = library[theme];
        }
    }

    public IEnumerator LoadData(DataSnapshot data)
    {
        if (data.Child("theme").Exists)
        {
            string themeString = data.Child("theme").Value.ToString();
            theme = int.Parse(themeString);
        }

        if (data.Child("username").Exists)
        {
            username = data.Child("username").Value.ToString();
            usernameField.text = username;
            PhotonNetwork.NickName = username;
        }

        friendScript.OtherPlayerLoadedIn(username);

        UpdateTheme();

        yield return new WaitForEndOfFrame();
    }

    public void LoadOtherPlayersData(string key, object data)
    {
        
    }
}

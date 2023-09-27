using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using System;
using UnityEngine.UI;
using Firebase.Auth;
using System.Linq;

public class FirebaseScript : MonoBehaviour
{
    notifScript NotificationScript;
    loadingScript loading;

    private List<IDataPersistence> dataPersistenceObjects;

    [Header("Firebase")]
    public DatabaseReference dataBaseReference;

    //User
    [SerializeField] private FirebaseUser _user;

    [Header("login")]
    [SerializeField] GameObject loginMenu;

    public void Awake()
    {
        loading = GameObject.Find("LoadingScreen").GetComponent<loadingScript>();
        NotificationScript = GameObject.FindGameObjectWithTag("Canvas").GetComponent<notifScript>();

        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
    }

    public void InitializeDatabase() 
    { 
        dataBaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        NotificationScript.createNotif($"Database found.", Color.green);
    }
    public void GetUser(FirebaseUser user) { _user = user; }
    public string GetUserID() { return _user.UserId; }

    public void LoadCloudData()
    {
        StartCoroutine(LoadData());
    }

    public IEnumerator UpdateUsernameAuth(string username)
    {
        UserProfile profile = new UserProfile();
        profile.DisplayName = username;

        var profileTask = _user.UpdateUserProfileAsync(profile);

        yield return new WaitUntil(predicate: () => profileTask.IsCompleted);

        if(profileTask.Exception != null)
        {
            NotificationScript.createNotif($"Failed to complete task: {profileTask.Exception}", Color.red);
        }
        else
        {
            NotificationScript.createNotif($"Username is updated: {profileTask.Exception}", Color.green);
            //Auth username is now updated
        }
    }

    public IEnumerator UpdateObject(string ID, object value)
    {
        var dataBaseTask = dataBaseReference.Child("users").Child(_user.UserId).Child(ID).SetValueAsync(value);

        yield return new WaitUntil(predicate: () => dataBaseTask.IsCompleted);

        if (dataBaseTask.Exception != null)
        {
            NotificationScript.createNotif($"Failed to save {ID}: {dataBaseTask.Exception}", Color.red);
        }
        else { } // Data saved
    }

    private IEnumerator LoadData()
    {
        var dataBaseTask = dataBaseReference.Child("users").Child(_user.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => dataBaseTask.IsCompleted);

        loading.hide();
        loginMenu.SetActive(false);
        GameObject.FindGameObjectWithTag("Online").GetComponent<onlineScript>().joinLobbyRoom();

        if (dataBaseTask.Exception != null)
        {
            NotificationScript.createNotif($"Failed to load data: {dataBaseTask.Exception}", Color.red);
        }
        else if(dataBaseTask.Result.Value == null)
        {
            // No data exists yet, set all data to default
            Dictionary<string, object> dataDictionary = new Dictionary<string, object>();

            dataDictionary.Add("Kills", "0"); 
            StartCoroutine(UpdateObject("Kills", "0"));

            string name = GameObject.FindGameObjectWithTag("Canvas").transform.Find("menu").GetComponent<menuScript>().UsernameField.text;
            dataDictionary.Add("username", name);
            StartCoroutine(UpdateObject("username", name));

            dataDictionary.Add("friends", "");
            StartCoroutine(UpdateObject("friends", ""));

            dataDictionary.Add("startSongs", "");
            StartCoroutine(UpdateObject("startSongs", ""));
            dataDictionary.Add("middleSongs", "");
            StartCoroutine(UpdateObject("middleSongs", ""));
            dataDictionary.Add("finalSongs", "");
            StartCoroutine(UpdateObject("finalSongs", ""));
            dataDictionary.Add("randomSongs", "");
            StartCoroutine(UpdateObject("randomSongs", ""));
            dataDictionary.Add("menuSongs", "");
            StartCoroutine(UpdateObject("menuSongs", ""));

            foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
            {
                dataPersistenceObj.LoadData(dataDictionary);
            }
        }
        else
        {
            DataSnapshot snapShot = dataBaseTask.Result;

            //set all of the data, things like

            Dictionary<string, object> dataDictionary = new Dictionary<string, object>();
            dataDictionary.Add("Kills" ,snapShot.Child("Kills").Value); // This is for ints, "Kills" is the name and snapShot.Chlid("Kills") gets the value from that directoary
            dataDictionary.Add("username", snapShot.Child("username").Value);
            dataDictionary.Add("friends", snapShot.Child("friends").Value);
            dataDictionary.Add("startSongs", snapShot.Child("startSongs").Value);
            dataDictionary.Add("middleSongs", snapShot.Child("middleSongs").Value);
            dataDictionary.Add("finalSongs", snapShot.Child("finalSongs").Value);
            dataDictionary.Add("randomSongs", snapShot.Child("randomSongs").Value);
            dataDictionary.Add("menuSongs", snapShot.Child("menuSongs").Value);

            foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
            {
                dataPersistenceObj.LoadData(dataDictionary);
            }
        }
    }

    public IEnumerator LoadOtherPlayersData(string otherUserID, string ID)
    {
        var dataBaseTask = dataBaseReference.Child("users").Child(otherUserID).GetValueAsync();

        yield return new WaitUntil(predicate: () => dataBaseTask.IsCompleted);

        if (dataBaseTask.Exception != null)
        {
            NotificationScript.createNotif($"Failed to load data: {dataBaseTask.Exception}", Color.red);
        }
        else
        {
            DataSnapshot snapShot = dataBaseTask.Result;

            foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
            {
                dataPersistenceObj.LoadOtherPlayersData(ID, snapShot.Child(ID).Value);
            }
        }
    }

    private IEnumerator LoadAllDataTypeString(string toFind)
    {
        var dataBaseTask = dataBaseReference.Child("users").OrderByChild(toFind).GetValueAsync(); // gets all of the users based on highest kill count

        yield return new WaitUntil(predicate: () => dataBaseTask.IsCompleted);

        if(dataBaseTask.Exception != null)
        {
            NotificationScript.createNotif($"Failed to load all {toFind}: {dataBaseTask.Exception}", Color.red);
        }
        else
        {
            DataSnapshot snapShot = dataBaseTask.Result;

            foreach(Transform child in transform)
            {
                //example is for a scoreboard, you delete every existing prefab first
            }

            foreach(DataSnapshot childSnap in snapShot.Children.Reverse<DataSnapshot>())
            {
                //string username = childSnap.Child("username").Value.ToString();
                //int kills = int.Parse(childSnap.Child("Kills").Value.ToString());

                // instantiate the new scoreboard with all the data you want
            }
        }
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}

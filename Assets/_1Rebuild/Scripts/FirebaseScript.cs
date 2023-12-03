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
    [SerializeField] private FirebaseUser user;

    [Header("login")]
    [SerializeField] GameObject loginMenu;

    public void Awake()
    {
        loading = GameObject.FindObjectOfType<loadingScript>();
        NotificationScript = GameObject.FindGameObjectWithTag("Canvas").GetComponent<notifScript>();

        this.dataPersistenceObjects = FindAllDataPersistenceObjects();
    }

    public void InitializeDatabase() 
    { 
        dataBaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void GetUser(FirebaseUser user) { this.user = user; }
    public string GetUserID() { return user.UserId; }

    public void LoadCloudData()
    {
        StartCoroutine(LoadData());
    }

    public IEnumerator UpdateUsernameAuth(string username)
    {
        UserProfile profile = new UserProfile();
        profile.DisplayName = username;

        var profileTask = user.UpdateUserProfileAsync(profile);

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
        var dataBaseTask = dataBaseReference.Child("users").Child(user.UserId).Child(ID).SetValueAsync(value);

        yield return new WaitUntil(predicate: () => dataBaseTask.IsCompleted);

        if (dataBaseTask.Exception != null)
        {
            NotificationScript.createNotif($"Failed to save {ID}: {dataBaseTask.Exception}", Color.red);
        }
        else { } // Data saved
    }



    private IEnumerator LoadData()
    {
        var dataBaseTask = dataBaseReference.Child("users").Child(user.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => dataBaseTask.IsCompleted);

        loading.hide();
        loginMenu.SetActive(false);

        if (dataBaseTask.Exception != null)
        {
            NotificationScript.createNotif($"Failed to load data: {dataBaseTask.Exception}", Color.red);
        }
        else if(dataBaseTask.Result.Value == null)
        {
            // No data exists yet, set all data to default
            /*Dictionary<string, object> dataDictionary = new Dictionary<string, object>();


            dataDictionary.Add("Kills", "0"); 
            StartCoroutine(UpdateObject("Kills", "0"));

            List<string> emptyFriends = new List<string> { "" };
            dataDictionary.Add("friends", emptyFriends);
            StartCoroutine(UpdateObject("friends", emptyFriends));

            string name = GameObject.FindGameObjectWithTag("Canvas").transform.Find("menu").GetComponent<menuScript>().UsernameField.text;
            dataDictionary.Add("username", name);
            StartCoroutine(UpdateObject("username", name));

            List<string> emptyStart = new List<string> { "" };
            dataDictionary.Add("startSongs", emptyStart);
            StartCoroutine(UpdateObject("startSongs", emptyStart));

            List<string> emptyMiddle = new List<string> { "" };
            dataDictionary.Add("middleSongs", emptyMiddle);
            StartCoroutine(UpdateObject("middleSongs", emptyMiddle));

            List<string> emptyFinal = new List<string> { "" };
            dataDictionary.Add("finalSongs", emptyFinal);
            StartCoroutine(UpdateObject("finalSongs", emptyFinal));

            List<string> emptyRandom = new List<string> { "" };
            dataDictionary.Add("randomSongs", emptyRandom);
            StartCoroutine(UpdateObject("randomSongs", emptyRandom));

            List<string> emptyMenu = new List<string> { "" };
            dataDictionary.Add("menuSongs", emptyMenu);
            StartCoroutine(UpdateObject("menuSongs", emptyMenu));

            //builder                        | team 1 | team 2 | team 3 |

            List<string> emptyTeamNames = new List<string> { "", "", "" };
            dataDictionary.Add("teamNames0", emptyTeamNames[0]);
            dataDictionary.Add("teamNames1", emptyTeamNames[1]);
            dataDictionary.Add("teamNames2", emptyTeamNames[2]);
            StartCoroutine(UpdateObject("teamNames", emptyTeamNames));

            dataDictionary.Add("critterIDs", "-1_-1_-1*-1_-1_-1*-1_-1_-1");
            StartCoroutine(UpdateObject("critterIDs", "-1_-1_-1*-1_-1_-1*-1_-1_-1"));

            dataDictionary.Add("attackIDs", "-1_-1_-1*-1_-1_-1*-1_-1_-1");
            StartCoroutine(UpdateObject("attackIDs", "-1_-1_-1*-1_-1_-1*-1_-1_-1"));

            dataDictionary.Add("abilityIDs", "-1_-1_-1*-1_-1_-1*-1_-1_-1");
            StartCoroutine(UpdateObject("abilityIDs", "-1_-1_-1*-1_-1_-1*-1_-1_-1"));

            dataDictionary.Add("passiveIDs", "-1_-1_-1*-1_-1_-1*-1_-1_-1");
            StartCoroutine(UpdateObject("passiveIDs", "-1_-1_-1*-1_-1_-1*-1_-1_-1"));

            dataDictionary.Add("HPGrowth", "-1_-1_-1*-1_-1_-1*-1_-1_-1");
            StartCoroutine(UpdateObject("HPGrowth", "-1_-1_-1*-1_-1_-1*-1_-1_-1"));

            dataDictionary.Add("StrengthGrowth", "-1_-1_-1*-1_-1_-1*-1_-1_-1");
            StartCoroutine(UpdateObject("StrengthGrowth", "-1_-1_-1*-1_-1_-1*-1_-1_-1"));

            dataDictionary.Add("MagicGrowth", "-1_-1_-1*-1_-1_-1*-1_-1_-1");
            StartCoroutine(UpdateObject("MagicGrowth", "-1_-1_-1*-1_-1_-1*-1_-1_-1"));

            dataDictionary.Add("SpeedGrowth", "-1_-1_-1*-1_-1_-1*-1_-1_-1");
            StartCoroutine(UpdateObject("SpeedGrowth", "-1_-1_-1*-1_-1_-1*-1_-1_-1"));

            foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
            {
                dataPersistenceObj.LoadData(dataDictionary);
            }*/
        }
        else
        {
            DataSnapshot snapShot = dataBaseTask.Result;

            //set all of the data, things like

            /*Dictionary<string, object> dataDictionary = new Dictionary<string, object>();
            dataDictionary.Add("Kills" ,snapShot.Child("Kills").Value); // This is for ints, "Kills" is the name and snapShot.Chlid("Kills") gets the value from that directoary
            dataDictionary.Add("username", snapShot.Child("username").Value);

            for (int i = 0; i < snapShot.Child("friends").ChildrenCount; ++i)
            {
                dataDictionary.Add(("friends" + i), snapShot.Child("friends").Child("" + i).Value);
            }

            for (int i = 0; i < snapShot.Child("startSongs").ChildrenCount; ++i)
            {
                dataDictionary.Add(("startSongs" + i), snapShot.Child("startSongs").Child("" + i).Value);
            }
            for (int i = 0; i < snapShot.Child("middleSongs").ChildrenCount; ++i)
            {
                dataDictionary.Add(("middleSongs" + i), snapShot.Child("middleSongs").Child("" + i).Value);
            }
            for (int i = 0; i < snapShot.Child("finalSongs").ChildrenCount; ++i)
            {
                dataDictionary.Add(("finalSongs" + i), snapShot.Child("finalSongs").Child("" + i).Value);
            }
            for (int i = 0; i < snapShot.Child("randomSongs").ChildrenCount; ++i)
            {
                dataDictionary.Add(("randomSongs" + i), snapShot.Child("randomSongs").Child("" + i).Value);
            }
            for (int i = 0; i < snapShot.Child("menuSongs").ChildrenCount; ++i)
            {
                dataDictionary.Add(("menuSongs" + i), snapShot.Child("menuSongs").Child("" + i).Value);
            }

            dataDictionary.Add("critterIDs", snapShot.Child("critterIDs").Value);
            dataDictionary.Add("attackIDs", snapShot.Child("attackIDs").Value);
            dataDictionary.Add("abilityIDs", snapShot.Child("abilityIDs").Value);
            dataDictionary.Add("passiveIDs", snapShot.Child("passiveIDs").Value);
            dataDictionary.Add("HPGrowth", snapShot.Child("HPGrowth").Value);
            dataDictionary.Add("StrengthGrowth", snapShot.Child("StrengthGrowth").Value);
            dataDictionary.Add("MagicGrowth", snapShot.Child("MagicGrowth").Value);
            dataDictionary.Add("SpeedGrowth", snapShot.Child("SpeedGrowth").Value);

            dataDictionary.Add(("teamNames" + 0), snapShot.Child("teamNames").Child("0").Value);
            dataDictionary.Add(("teamNames" + 1), snapShot.Child("teamNames").Child("1").Value);
            dataDictionary.Add(("teamNames" + 2), snapShot.Child("teamNames").Child("2").Value);
            Debug.Log("" + snapShot.Child("teamNames").Child("0").Value);
            Debug.Log("" + snapShot.Child("teamNames").Child("1").Value);
            Debug.Log("" + snapShot.Child("teamNames").Child("2").Value);*/


            foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
            {
                dataPersistenceObj.LoadData(snapShot);
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
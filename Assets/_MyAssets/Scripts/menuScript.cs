using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Audio;
using System.Linq;

public class menuScript : MonoBehaviour, IDataPersistence
{
    [SerializeField] private Sprite transparentSprite;

    notifScript NotificationScript;

    //Firebase Data
    FirebaseScript fireBaseScript;

    //Online
    onlineScript OnlineScript;

    //Music Menu
    musicMenu musicMenuScript;

    //Builder Menu
    builderMenu builderMenuScript;

    [Header("Sprites")]
    [SerializeField] private Sprite[] _background_Sprites;
    [SerializeField] private Sprite[] _button_Sprites;

    [Header("Cloud Variables")]
    public TMP_InputField UsernameField;
    public TMP_InputField KillsField;

    private string _fieldString;
    private int _fieldInt;
    private bool _bInt;

    [Header("Animations")]
    [SerializeField] TMP_Text titleText;
    private string currentMenu;
    private Animator menuAnimations;

    [Header("Menus Lerp")]
    [SerializeField] float friendSpeed = 6;
    private bool _bFriends;
    private Transform _friends;
    private Transform _friendsIcon;

    private void Awake()
    {
        _friends = transform.Find("friendsMenu").GetComponent<Transform>();
        _friendsIcon = transform.Find("friendsMenu").transform.Find("icon").GetComponent<Transform>();

        fireBaseScript = GameObject.FindGameObjectWithTag("Data").GetComponent<FirebaseScript>();
        OnlineScript = GameObject.FindGameObjectWithTag("Online").GetComponent<onlineScript>();

        NotificationScript = GameObject.FindGameObjectWithTag("Canvas").GetComponent<notifScript>();

        menuAnimations = GetComponent<Animator>();

        musicMenuScript = transform.Find("music").GetComponent<musicMenu>();
        builderMenuScript = transform.Find("builder").GetComponent<builderMenu>();
    }

    private void Start()
    {
        menuStyle(Random.Range(0, _background_Sprites.Length)); // picks a random style at game start
    }

    private void Update()
    {
        float friendsLerp = (_bFriends) ? 436 : 850;
        _friends.localPosition = Vector2.Lerp(_friends.localPosition, new Vector2(friendsLerp, 0), friendSpeed * Time.deltaTime);

        float friendsIconLerp = (_bFriends) ? -187 : -360;
        _friendsIcon.localPosition = Vector2.Lerp(_friendsIcon.localPosition, new Vector2(friendsIconLerp, 234), friendSpeed * Time.deltaTime);
    }

    private void menuStyle(int change)
    {
        transform.GetChild(0).GetComponent<Image>().sprite = _background_Sprites[change];

        GameObject[] buttonsToChange = GameObject.FindGameObjectsWithTag("button");
        foreach (GameObject obj in buttonsToChange)
        {
            obj.GetComponent<Image>().sprite = _button_Sprites[change];
        }
    }

    public void FriendsMenu()
    {
        _bFriends = !_bFriends;
    }

    public void OpenMenu(string name)
    {
        if (currentMenu == name) return;

        menuAnimations.SetTrigger("scatter");

        currentMenu = name;

        menuAnimations.SetBool("openBuilder", false);
        menuAnimations.SetBool("openBattle", false);
        menuAnimations.SetBool("openMusic", false);
        menuAnimations.SetBool("openProfile", false);
        menuAnimations.SetBool("open" + name, true);
    }

    public void returnMenu()
    {
        if (builderMenuScript.selectedTeam != -1)
        {
            menuAnimations.ResetTrigger("openTeam" + (builderMenuScript.selectedTeam + 1));
            menuAnimations.SetTrigger("closeTeam" + (builderMenuScript.selectedTeam + 1));
            //bSelectedTeam = false;
            //bCritterList = false;
            builderMenuScript.bEditCritter = false;
            builderMenuScript.selectedCritter = -1;
            //selectedTeam = -1;
        }
        else if(musicMenuScript.edittingPlaylist == true)
        {
            musicMenuScript.edittingPlaylist = false;
            menuAnimations.ResetTrigger("editPlaylist");
            menuAnimations.SetTrigger("exitPlaylist");
        }
        else
        {
            menuAnimations.ResetTrigger("scatter");
            menuAnimations.SetTrigger("leave" + currentMenu);
            currentMenu = "";
        }    
    }

    public void inBuilderSprites(int team)
    {
        builderMenuScript.inBuilderSprites(team);
    }

    public void inBuilderMenuSprites(int team)
    {
        builderMenuScript.inBuilderMenuSprites(team);
    }

    public void ReadStringInput(string s)
    {
        _fieldString = s;
        _bInt = false;

        if (!int.TryParse(s, out int example)) return;
        _fieldInt = System.Convert.ToInt32(s);
        _bInt = true;
    }

    public void CloudSave(string key)
    {
        if (key == "username")
        {
            OnlineScript.updateFriendsName(_fieldString);
            OnlineScript.setNickName(_fieldString);
            StartCoroutine(fireBaseScript.UpdateUsernameAuth(_fieldString));
        }

        if(_bInt)
        {
            StartCoroutine(fireBaseScript.UpdateObject(key, _fieldInt));
        }
        else
        {
            StartCoroutine(fireBaseScript.UpdateObject(key, _fieldString));
        }
    }

    public void LoadData(Dictionary<string, object> dataDictionary)
    {
        KillsField.text = dataDictionary["Kills"].ToString();

        UsernameField.text = dataDictionary["username"].ToString();
        OnlineScript.setNickName(dataDictionary["username"].ToString());
    }

    public void LoadOtherPlayersData(string key, object data)
    {
        
    }
}
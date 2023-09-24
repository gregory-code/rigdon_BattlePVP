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
    notifScript NotificationScript;

    //PlayerPref
    private float _musicLevel;
    private float _effectsLevel;

    //Firebase Data
    FirebaseScript fireBaseScript;

    //Online
    onlineScript OnlineScript;

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

    [Header("Audio")]
    [SerializeField] Image muteIcon;
    [SerializeField] Sprite mutedSprite;
    [SerializeField] Sprite unmutedSprite;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider effectsSlider;
    [SerializeField] AudioMixer MasterMixer;
    [SerializeField] AudioSource testEffect;
    private bool bReuglarMusic;
    [SerializeField] AudioMixerSnapshot Regular;
    [SerializeField] AudioMixerSnapshot Muted;

    private void Awake()
    {
        _friends = transform.Find("main").transform.Find("friendsMenu").GetComponent<Transform>();
        _friendsIcon = transform.Find("main").transform.Find("friendsMenu").transform.Find("icon").GetComponent<Transform>();

        fireBaseScript = GameObject.FindGameObjectWithTag("Data").GetComponent<FirebaseScript>();
        OnlineScript = GameObject.FindGameObjectWithTag("Online").GetComponent<onlineScript>();

        NotificationScript = GameObject.FindGameObjectWithTag("Canvas").GetComponent<notifScript>();

        menuAnimations = GetComponent<Animator>();
    }

    private void Start()
    {
        menuStyle(Random.Range(0, _background_Sprites.Length)); // picks a random style at game start

        //Setting music
        _musicLevel = PlayerPrefs.GetFloat("musicLevel");
        _effectsLevel = PlayerPrefs.GetFloat("effectsLevel");

        if (_musicLevel == 0) _musicLevel = 1;
        if (_effectsLevel == 0) _effectsLevel = 1;

        setMusicLevel(_musicLevel);
        setEffectsLevel(_effectsLevel);
        // ========
    }

    private void Update()
    {
        float friendsLerp = (_bFriends) ? 436 : 850;
        _friends.localPosition = Vector2.Lerp(_friends.localPosition, new Vector2(friendsLerp, 0), friendSpeed * Time.deltaTime);

        float friendsIconLerp = (_bFriends) ? -187 : -360;
        _friendsIcon.localPosition = Vector2.Lerp(_friendsIcon.localPosition, new Vector2(friendsIconLerp, 234), friendSpeed * Time.deltaTime);
    }

    #region menuing

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
        menuAnimations.ResetTrigger("scatter");
        menuAnimations.SetTrigger("leave" + currentMenu);
    }

    public void setTextToName()
    {
        titleText.text = currentMenu;
    }

    public void setTextToMain()
    {
        titleText.text = "Main Menu";
        menuAnimations.ResetTrigger("leave" + currentMenu);
        currentMenu = "";
    }

    #endregion

    #region Song Menu

    public void setMusicLevel(float sliderValue)
    {
        PlayerPrefs.SetFloat("musicLevel", sliderValue);
        MasterMixer.SetFloat("musicVol", Mathf.Log10(sliderValue) * 20);
        _musicLevel = sliderValue;
        musicSlider.value = sliderValue;
        checkMute();
    }

    public void setEffectsLevel(float sliderValue)
    {
        PlayerPrefs.SetFloat("effectsLevel", sliderValue);
        MasterMixer.SetFloat("effectsVol", Mathf.Log10(sliderValue) * 20);
        _effectsLevel = sliderValue;
        effectsSlider.value = sliderValue;
        testEffect.Play();
        
        checkMute();
    }

    private void checkMute()
    {
        if (muteIcon.sprite == mutedSprite) muteIcon.sprite = unmutedSprite;
        if (_effectsLevel == 0.0001f && _musicLevel == 0.0001f) muteIcon.sprite = mutedSprite;
    }

    public void mute()
    {
        if (muteIcon.sprite == mutedSprite)
        {
            setMusicLevel(1f);
            setEffectsLevel(1f);

        }
        else // unmutted
        {
            setMusicLevel(0.0001f);
            setEffectsLevel(0.0001f);
        }
    }

    #endregion

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

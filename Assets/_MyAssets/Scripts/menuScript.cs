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
    private int _songPreference;

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

    [SerializeField] private Image[] roundButtns;

    [SerializeField] TMP_Text roundNameText;

    private int currentSongList;

    public List<string> startSongs = new List<string>();
    public List<string> middleSongs = new List<string>();
    public List<string> finalSongs = new List<string>();
    public List<string> randomSongs = new List<string>();

    private List<GameObject> playListObjects = new List<GameObject>();
    [SerializeField] Transform playListParent;
    [SerializeField] GameObject playPrefab;

    [SerializeField] private AudioSource currentSong;
    [SerializeField] private Image[] seriesButtns;

    private List<GameObject> songGameObjects = new List<GameObject>();
    [SerializeField] Transform songListParent;
    [SerializeField] GameObject songPrefab;

    [SerializeField] song[] songLibrary;
    [SerializeField] Image randomImage;
    [SerializeField] Image perBattleImage;
    [SerializeField] TMP_Text preferenceText;
    [SerializeField] Image preferenceIcon;
    [SerializeField] Sprite perBattlePreference;
    [SerializeField] Sprite randomPreference;
    [SerializeField] Image muteIcon;
    [SerializeField] Sprite mutedSprite;
    [SerializeField] Sprite unmutedSprite;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider effectsSlider;
    [SerializeField] AudioMixer MasterMixer;
    [SerializeField] AudioSource testEffect;
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

        testEffect.volume = 0;
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

    private void InitalizeMusic()
    {
        _musicLevel = PlayerPrefs.GetFloat("musicLevel");
        _effectsLevel = PlayerPrefs.GetFloat("effectsLevel");
        _songPreference = PlayerPrefs.GetInt("songPreference");
        if (_songPreference == 1)
        {
            switchSongPrefernce();
        }
        else
        {
            switchPlayList(3);
        }

        if (_musicLevel == 0) _musicLevel = 1;
        if (_effectsLevel == 0) _effectsLevel = 1;

        setMusicLevel(_musicLevel);
        setEffectsLevel(_effectsLevel);

        switchSeriesMusic("sonic");
        // ========
    }

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
        if (testEffect.volume == 0) testEffect.volume = 1;
        
        checkMute();
    }

    private void checkMute()
    {
        if (muteIcon.sprite == mutedSprite) muteIcon.sprite = unmutedSprite;
        if (_effectsLevel == 0.0001f && _musicLevel == 0.0001f) muteIcon.sprite = mutedSprite;
    }

    public void switchSongPrefernce()
    {
        if(preferenceIcon.sprite == perBattlePreference) // per battle
        {
            preferenceIcon.sprite = randomPreference;
            preferenceText.text = "Song Prefernce: Random";
            perBattleImage.color = new Color(0, 0, 0, 0.5f);
            randomImage.color = new Color(1, 1, 1, 1);
            PlayerPrefs.SetInt("songPreference", 0);
            switchPlayList(3);
        }
        else // it's random
        {
            preferenceIcon.sprite = perBattlePreference;
            preferenceText.text = "Song Prefernce: Per Battle";
            randomImage.color = new Color(0, 0, 0, 0.5f);
            perBattleImage.color = new Color(1, 1, 1, 1);
            PlayerPrefs.SetInt("songPreference", 1);
            switchPlayList(0);
        }
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

    public void switchSeriesMusic(string seriesIndex)
    {
        foreach(GameObject song in songGameObjects)
        {
            Destroy(song);
        }
        songGameObjects.Clear();

        for(int i = 0; i < seriesButtns.Length; i++)
        {
            seriesButtns[i].color = new Color(0, 0, 0, 0.5f);
        }

        if(seriesIndex == "sonic") seriesButtns[0].color = new Color(1, 1, 1, 1);
        if (seriesIndex == "birdsEpic") seriesButtns[1].color = new Color(1, 1, 1, 1);
        if (seriesIndex == "pokemon") seriesButtns[2].color = new Color(1, 1, 1, 1);
        if (seriesIndex == "Limbus") seriesButtns[3].color = new Color(1, 1, 1, 1);

        foreach (song Song in songLibrary)
        {
            if(Song.gameSeries == (song.series)System.Enum.Parse(typeof(song.series), seriesIndex.ToString()))
            {
                GameObject newSong = Instantiate(songPrefab);

                newSong.transform.SetParent(songListParent.transform);
                newSong.transform.localScale = new Vector3(1, 1, 1);

                newSong.transform.Find("musicName").GetComponent<TMP_Text>().text = Song.songName;

                newSong.transform.Find("listenBtn").GetComponent<Image>().sprite = Song.borderSprite;
                newSong.transform.Find("addBtn").GetComponent<Image>().sprite = Song.borderSprite;

                newSong.transform.Find("listenBtn").GetComponent<Button>().onClick.AddListener(() => listenToSong(Song.songName));
                newSong.transform.Find("addBtn").GetComponent<Button>().onClick.AddListener(() => addSong(Song.songName));

                songGameObjects.Add(newSong);
            }
        }

        RectTransform rectSongList = songListParent.GetComponent<RectTransform>();
        rectSongList.sizeDelta = new Vector2(rectSongList.sizeDelta.x, songGameObjects.Count * 60);
    }

    private void listenToSong(string songName)
    {
        if (currentSong != null) currentSong.Stop();
        currentSong = GameObject.Find(songName).GetComponent<AudioSource>();
        currentSong.Play();
    }

    private void addSong(string songName)
    {
        List<string> desiredPlayList = new List<string>();
        switch (currentSongList)
        {
            case 0: desiredPlayList = startSongs; break;
            case 1: desiredPlayList = middleSongs; break;
            case 2: desiredPlayList = finalSongs; break;
            case 3: desiredPlayList = randomSongs; break;
        }

        if (desiredPlayList.Contains(songName)) return;

        desiredPlayList.Add(songName);

        addSongToList(songName, currentSongList);
}

    public void switchPlayList(int playList)
    {
        List<string> desiredPlayList = new List<string>();
        switch(playList)
        {
            case 0: 
                desiredPlayList = startSongs;
                roundNameText.text = "Round 1 Play List";
                break;
            case 1: 
                desiredPlayList = middleSongs;
                roundNameText.text = "Middle Rounds Play List";
                break;
            case 2: 
                desiredPlayList = finalSongs;
                roundNameText.text = "Final Round Play List";
                break;
            case 3: 
                desiredPlayList = randomSongs;
                roundNameText.text = "Song Play List";
                break;
        }

        foreach (GameObject song in playListObjects)
        {
            Destroy(song);
        }
        playListObjects.Clear();

        if(playList == 3)
        {
            for (int i = 0; i < roundButtns.Length; i++)
            {
                roundButtns[i].color = new Color(0, 0, 0, 0f);
                roundButtns[i].transform.GetChild(0).GetComponent<TMP_Text>().color = new Color(0, 0, 0, 0f);
            }
        }
        else
        {
            for (int i = 0; i < roundButtns.Length; i++)
            {
                roundButtns[i].color = new Color(0, 0, 0, 0.5f);
                roundButtns[i].transform.GetChild(0).GetComponent<TMP_Text>().color = new Color(1, 1, 1, 1f);
            }

            roundButtns[playList].color = new Color(1, 1, 1, 1);
            roundButtns[playList].color = new Color(1, 1, 1, 1);
            roundButtns[playList].color = new Color(1, 1, 1, 1);
            roundButtns[playList].color = new Color(1, 1, 1, 1);

        }

        currentSongList = playList;

        foreach (string songName in desiredPlayList)
        {
            addSongToList(songName, playList);
        }
    }

    private void addSongToList(string songName, int whichPlayList)
    {
        GameObject newSong = Instantiate(playPrefab);

        newSong.transform.SetParent(playListParent.transform);
        newSong.transform.localScale = new Vector3(1, 1, 1);

        newSong.transform.Find("musicName").GetComponent<TMP_Text>().text = songName;

        newSong.transform.Find("removeBtn").GetComponent<Button>().onClick.AddListener(() => removeSong(whichPlayList, songName, newSong));

        playListObjects.Add(newSong);

        CloudUpdateSongList(whichPlayList);

        RectTransform rectSongList = playListParent.GetComponent<RectTransform>();
        float difference = (playListObjects.Count == 0) ? 40 : playListObjects.Count * 40;
        rectSongList.sizeDelta = new Vector2(rectSongList.sizeDelta.x, difference);
    }

    private void removeSong(int whichPlaylist, string name, GameObject songObj)
    {
        List<string> desiredPlayList = new List<string>();
        switch (whichPlaylist)
        {
            case 0: desiredPlayList = startSongs; break;
            case 1: desiredPlayList = middleSongs; break;
            case 2: desiredPlayList = finalSongs; break;
            case 3: desiredPlayList = randomSongs; break;
        }

        desiredPlayList.Remove(name);
        Destroy(songObj);
        playListObjects.Remove(songObj);

        CloudUpdateSongList(whichPlaylist);
    }

    private void CloudUpdateSongList(int whichPlaylist)
    {
        switch (whichPlaylist)
        {
            case 0: StartCoroutine(fireBaseScript.UpdateObject("startSongs", string.Join("_", startSongs))); break;
            case 1: StartCoroutine(fireBaseScript.UpdateObject("middleSongs", string.Join("_", middleSongs))); break;
            case 2: StartCoroutine(fireBaseScript.UpdateObject("finalSongs", string.Join("_", finalSongs))); break;
            case 3: StartCoroutine(fireBaseScript.UpdateObject("randomSongs", string.Join("_", randomSongs))); break;
        }
    }

    private void deserializeSongList(List<string> list, List<string> desiredPlayList)
    {
        if (list[0] != "")
        {
            foreach (string song in list)
            {
                desiredPlayList.Add(song);
            }
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

        deserializeSongList(dataDictionary["startSongs"].ToString().Split('_').ToList(), startSongs);
        deserializeSongList(dataDictionary["middleSongs"].ToString().Split('_').ToList(), middleSongs);
        deserializeSongList(dataDictionary["finalSongs"].ToString().Split('_').ToList(), finalSongs);
        deserializeSongList(dataDictionary["randomSongs"].ToString().Split('_').ToList(), randomSongs);
        InitalizeMusic();
    }

    public void LoadOtherPlayersData(string key, object data)
    {
        
    }
}
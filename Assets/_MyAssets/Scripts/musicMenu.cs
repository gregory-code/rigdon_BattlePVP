using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.Linq;

public class musicMenu : MonoBehaviour, IDataPersistence
{

    //PlayerPref
    private float _musicLevel;
    private float _effectsLevel;
    private int _songPreference;

    notifScript NotificationScript;
    
    //Firebase Data
    FirebaseScript fireBaseScript;

    //Online
    onlineScript OnlineScript;

    //Aniamtor
    private Animator menuAnimations;


    [Header("Audio")]
    [SerializeField] private Image[] roundButtns;
    public bool edittingPlaylist;

    [SerializeField] TMP_Text roundNameText;

    private int currentSongList;

    public List<string> startSongs = new List<string>();
    public List<string> middleSongs = new List<string>();
    public List<string> finalSongs = new List<string>();
    public List<string> randomSongs = new List<string>();
    public List<string> menuSongs = new List<string>();

    private List<GameObject> playListObjects = new List<GameObject>();
    [SerializeField] Transform playListParent;
    [SerializeField] GameObject playPrefab;

    [SerializeField] private AudioSource currentSong;
    [SerializeField] private Image[] seriesButtns;

    private List<GameObject> songGameObjects = new List<GameObject>();
    [SerializeField] Transform songListParent;
    [SerializeField] GameObject songPrefab;

    [SerializeField] song[] songLibrary;
    private bool bBattlePlayListPreference;
    [SerializeField] Image randomImage;
    [SerializeField] Image perBattleImage;
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
        fireBaseScript = GameObject.FindGameObjectWithTag("Data").GetComponent<FirebaseScript>();
        OnlineScript = GameObject.FindGameObjectWithTag("Online").GetComponent<onlineScript>();

        NotificationScript = GameObject.FindGameObjectWithTag("Canvas").GetComponent<notifScript>();

        menuAnimations = GameObject.Find("menu").GetComponent<Animator>();
    }

    private void Start()
    {
        testEffect.volume = 0;
    }

    private void Update()
    {
        if (menuSongs.Count > 0)
        {
            if (currentSong != null && currentSong.isPlaying == false)
            {
                playRandomSong();
            }
        }
    }

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

    private void playRandomSong()
    {
        if (menuSongs.Count == 0) return;
        int randomSong = Random.Range(0, menuSongs.Count);

        listenToSong(menuSongs[randomSong]);
    }


    public void editPlayList()
    {
        edittingPlaylist = true;
        menuAnimations.SetTrigger("editPlaylist");
        menuAnimations.ResetTrigger("exitPlaylist");
        if (bBattlePlayListPreference == false)
        {
            switchPlayList(3);
        }
        else
        {
            switchPlayList(0);
        }
    }

    public void editMenuList()
    {
        edittingPlaylist = true;
        switchPlayList(4);
        menuAnimations.SetTrigger("editPlaylist");
        menuAnimations.ResetTrigger("exitPlaylist");
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
        if (bBattlePlayListPreference == true) // per battle
        {
            bBattlePlayListPreference = false;
            perBattleImage.color = new Color(0, 0, 0, 0.5f);
            randomImage.color = new Color(1, 1, 1, 1);
            PlayerPrefs.SetInt("songPreference", 0);
        }
        else // it's random
        {
            bBattlePlayListPreference = true;
            randomImage.color = new Color(0, 0, 0, 0.5f);
            perBattleImage.color = new Color(1, 1, 1, 1);
            PlayerPrefs.SetInt("songPreference", 1);
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
        foreach (GameObject song in songGameObjects)
        {
            Destroy(song);
        }
        songGameObjects.Clear();

        for (int i = 0; i < seriesButtns.Length; i++)
        {
            seriesButtns[i].color = new Color(0, 0, 0, 0.5f);
        }

        if (seriesIndex == "sonic") seriesButtns[0].color = new Color(1, 1, 1, 1);
        if (seriesIndex == "birdsEpic") seriesButtns[1].color = new Color(1, 1, 1, 1);
        if (seriesIndex == "pokemon") seriesButtns[2].color = new Color(1, 1, 1, 1);
        if (seriesIndex == "Limbus") seriesButtns[3].color = new Color(1, 1, 1, 1);

        foreach (song Song in songLibrary)
        {
            if (Song.gameSeries == (song.series)System.Enum.Parse(typeof(song.series), seriesIndex.ToString()))
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
        NotificationScript.createNotif($"Now Playing: {songName}", Color.magenta);
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
            case 4: desiredPlayList = menuSongs; break;
        }

        if (desiredPlayList.Contains(songName)) return;

        desiredPlayList.Add(songName);

        addSongToList(songName, currentSongList);
    }

    public void switchPlayList(int playList)
    {
        List<string> desiredPlayList = new List<string>();
        switch (playList)
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
                roundNameText.text = "Random Play List";
                break;
            case 4:
                desiredPlayList = menuSongs;
                roundNameText.text = "Menu Song List";
                break;
        }

        foreach (GameObject song in playListObjects)
        {
            Destroy(song);
        }
        playListObjects.Clear();

        if (playList == 3 || playList == 4)
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
            case 4: desiredPlayList = menuSongs; break;
        }

        desiredPlayList.Remove(name);
        Destroy(songObj);
        playListObjects.Remove(songObj);

        RectTransform rectSongList = playListParent.GetComponent<RectTransform>();
        float difference = (playListObjects.Count == 0) ? 40 : playListObjects.Count * 40;
        rectSongList.sizeDelta = new Vector2(rectSongList.sizeDelta.x, difference);

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
            case 4: StartCoroutine(fireBaseScript.UpdateObject("menuSongs", string.Join("_", menuSongs))); break;
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

    public void LoadData(Dictionary<string, object> dataDictionary)
    {
        deserializeSongList(dataDictionary["startSongs"].ToString().Split('_').ToList(), startSongs);
        deserializeSongList(dataDictionary["middleSongs"].ToString().Split('_').ToList(), middleSongs);
        deserializeSongList(dataDictionary["finalSongs"].ToString().Split('_').ToList(), finalSongs);
        deserializeSongList(dataDictionary["randomSongs"].ToString().Split('_').ToList(), randomSongs);
        deserializeSongList(dataDictionary["menuSongs"].ToString().Split('_').ToList(), menuSongs);

        InitalizeMusic();
        playRandomSong();
    }

    public void LoadOtherPlayersData(string key, object data)
    {
        
    }
}

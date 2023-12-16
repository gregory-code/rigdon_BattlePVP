using Firebase.Database;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class editPlayLists : MonoBehaviour, IDataPersistence
{
    public List<string> battleSongs = new List<string>();
    public List<string> menuSongs = new List<string>();

    private List<string> currentSongList = new List<string>();

    [SerializeField] FirebaseScript fireBaseScript;

    private List<GameObject> songList = new List<GameObject>();
    [SerializeField] Transform songListObject;
    [SerializeField] songButton songPrefab;

    private List<GameObject> seriesMusicList = new List<GameObject>();
    [SerializeField] Transform seriesMusicListObject;
    [SerializeField] seriesMusicButton seriesMusicPrefab;

    [SerializeField] private Image[] seriesButtons;
    [SerializeField] private Image[] playListButtons;

    [SerializeField] TMP_Text playListText;

    [SerializeField] song[] songLibrary;

    bool edittingMenuList;

    [SerializeField] AudioMixerSnapshot Regular;
    [SerializeField] AudioMixerSnapshot Muted;

    private void InitalizeMusic()
    {
        SwitchSeriesMusic(0);
        SwitchPlayList(0);
        PlayMenuSong();
    }

    private void PlayMenuSong()
    {
        if (menuSongs.Count >= 1 && jukeBox.IsSongPlaying() == false)
        {
            jukeBox.PlaySong(menuSongs[Random.Range(0, menuSongs.Count)]);
        }
    }

    public void SwitchSeriesMusic(int seriesIndex)
    {
        foreach (GameObject song in seriesMusicList)
        {
            Destroy(song);
        }
        seriesMusicList.Clear();

        for (int i = 0; i < seriesButtons.Length; i++)
        {
            seriesButtons[i].color = new Color(0, 0, 0, 0.5f);
        }

        seriesButtons[seriesIndex].color = new Color(1, 1, 1, 1);

        foreach (song Song in songLibrary)
        {
            if (Song.gameSeries == (song.series)System.Enum.Parse(typeof(song.series), seriesIndex.ToString()))
            {
                seriesMusicButton newSong = Instantiate(seriesMusicPrefab, seriesMusicListObject);
                newSong.transform.localScale = new Vector3(1, 1, 1);
                newSong.Init(Song.songName, Song.borderSprite, this);

                seriesMusicList.Add(newSong.gameObject);
            }
        }

        SetListTransform(seriesMusicListObject, 60, seriesMusicList.Count);
    }

    public void SwitchPlayList(int playList)
    {
        switch (playList)
        {
            case 0:
                currentSongList = battleSongs;
                edittingMenuList = false;
                playListText.text = "Editting: Battle Playlist";
                break;
            case 1:
                currentSongList = menuSongs;
                edittingMenuList = true;
                playListText.text = "Editting: Menu Playlist";
                break;
        }

        foreach (GameObject song in songList)
        {
            Destroy(song);
        }
        songList.Clear();

        foreach (Image buttonImage in playListButtons)
        {
            buttonImage.color = new Color(0, 0, 0, 0.5f);
        }

        playListButtons[playList].color = new Color(1, 1, 1, 1);

        foreach (string songName in currentSongList)
        {
            addSongToList(songName);
        }
    }

    public void AddSong(string songName)
    {
        if (currentSongList.Contains(songName))
            return;

        if (songName == "")
            return;

        currentSongList.Add(songName);

        addSongToList(songName);
    }

    private void addSongToList(string songName)
    {
        songButton newSong = Instantiate(songPrefab, songListObject);
        newSong.transform.localScale = new Vector3(1, 1, 1);
        newSong.Init(songName, this);

        songList.Add(newSong.gameObject);

        CloudUpdateSongList();

        SetListTransform(songListObject, 40, songList.Count);
    }

    public void RemoveSong(string name, GameObject songObj)
    {
        currentSongList.Remove(name);
        Destroy(songObj);
        songList.Remove(songObj);


        CloudUpdateSongList();

        SetListTransform(songListObject, 40, songList.Count);
    }

    private void SetListTransform(Transform list, float spacing, float count)
    {
        RectTransform rect = list.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, count * spacing);

    }

    private void CloudUpdateSongList()
    {
        if(edittingMenuList)
        {
            StartCoroutine(fireBaseScript.UpdateObject("menuSongs", menuSongs));
            menuSongs = currentSongList;
        }
        else
        {
            StartCoroutine(fireBaseScript.UpdateObject("battleSongs", battleSongs));
            battleSongs = currentSongList;
        }
    }

    public IEnumerator LoadData(DataSnapshot data)
    {
        for (int i = 0; i < data.Child("menuSongs").ChildrenCount; ++i)
        {
            menuSongs.Add(data.Child("menuSongs").Child("" + i).Value.ToString());
        }

        for (int i = 0; i < data.Child("battleSongs").ChildrenCount; ++i)
        {
            battleSongs.Add(data.Child("battleSongs").Child("" + i).Value.ToString());
        }

        InitalizeMusic();

        yield return new WaitForEndOfFrame();
    }

    public void LoadOtherPlayersData(string key, object data)
    {
        
    }
}

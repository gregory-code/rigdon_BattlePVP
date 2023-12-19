using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class battleMusicManager : MonoBehaviour
{
    public List<string> startSongs = new List<string>();
    public List<string> middleSongs = new List<string>();
    public List<string> finalSongs = new List<string>();
    public List<string> randomSongs = new List<string>();

    [SerializeField] private AudioSource currentSong;
    private List<string> currentPlayList = new List<string>();

    bool bInBattle;
    bool bRandoSongs;

    notifScript NotificationScript;

    public void Start()
    {
        NotificationScript = GameObject.FindGameObjectWithTag("Canvas").GetComponent<notifScript>();
        bInBattle = false;
    }

    public void setMusicPlaylists(List<string> start, List<string> middle, List<string> final, List<string> random, bool preference)
    {
        startSongs = start;
        middleSongs = middle;
        finalSongs = final;
        randomSongs = random;
        bRandoSongs = preference;
    }

    public void setGameState(string state)
    {
        bInBattle = true;

        switch(state)
        {
            case "middle": currentPlayList = middleSongs; break;
            case "random": currentPlayList = randomSongs; break;
            case "start": currentPlayList = startSongs; break;
            case "final": currentPlayList = finalSongs;  break;
            default: bInBattle = false; break;
        }

        if (bRandoSongs == true) currentPlayList = randomSongs;

        stopSong();
        playRandomSong();
    }

    public void Update()
    {
        if (currentSong == null) return;

        if (currentSong.isPlaying == false && bInBattle == true)
        {
            playRandomSong();
        }
    }

    public void stopSong()
    {
        if (currentSong != null)
        {
            if (currentSong.isPlaying) currentSong.Stop();
        }
    }

    private void playRandomSong()
    {
        if (currentPlayList.Count <= 0) return;
        if (currentPlayList[0] == "") return;
        int randomSong = UnityEngine.Random.Range(0, currentPlayList.Count);

        listenToSong(currentPlayList[randomSong]);
    }

    private void listenToSong(string song)
    {
        if (currentSong != null) currentSong.Stop();
        currentSong = GameObject.Find(song).GetComponent<AudioSource>();
        currentSong.Play();
        NotificationScript.createNotif($"Now Playing: {song}", Color.magenta);
    }
}

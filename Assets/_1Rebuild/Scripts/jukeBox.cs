using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class jukeBox
{
    private static AudioSource currentSong;
    private static notifScript NotificationScript;

    public static void PlaySong(string songName)
    {
        if (currentSong != null) 
            currentSong.Stop();

        currentSong = GameObject.Find(songName).GetComponent<AudioSource>();
        currentSong.Play();

        if(NotificationScript == null)
            NotificationScript = GameObject.FindObjectOfType<notifScript>();

        NotificationScript.createNotif($"Now Playing: {songName}", Color.magenta);
    }

    public static bool IsSongPlaying()
    {
        if (currentSong == null)
            return false;

        return currentSong.isPlaying;
    }

    public static void StopSong()
    {
        currentSong.Stop();
    }
}

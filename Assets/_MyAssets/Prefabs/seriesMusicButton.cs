using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class seriesMusicButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI songNameText;

    [SerializeField] Image listenImage;
    [SerializeField] Button listenButton;
    [SerializeField] Image addImage;
    [SerializeField] Button addButton;

    public void Init(string songName, Sprite borderSprite, editPlayLists ownerPlayList)
    {
        songNameText.text = songName;
        listenImage.sprite = borderSprite;
        addImage.sprite = borderSprite;
        listenButton.onClick.AddListener(() => jukeBox.PlaySong(songName));
        addButton.onClick.AddListener(() => ownerPlayList.AddSong(songName));
    }

}

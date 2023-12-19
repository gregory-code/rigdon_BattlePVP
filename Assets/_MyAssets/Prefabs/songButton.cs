using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class songButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI songNameText;
    [SerializeField] Button removeButton;

    public void Init(string songName, editPlayLists ownerPlayList)
    {
        songNameText.text = songName;
        removeButton.onClick.AddListener(() => ownerPlayList.RemoveSong(songName, this.gameObject));
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu(menuName = "Song")]
public class song : ScriptableObject
{
    public enum series
    {
        sonic,
        pokemon,
        birdsEpic,
        Limbus
    }

    public series gameSeries;
    public string songName;
    public Sprite borderSprite;
}

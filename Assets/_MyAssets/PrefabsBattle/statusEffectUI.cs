using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class statusEffectUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI statusCounter;
    [SerializeField] Image statusImage;
    [SerializeField] Sprite[] statusSprites;

    private int statusIndex;

    public void SetStatusIndex(int index, int statusCounter)
    {
        statusImage.sprite = statusSprites[index];
        statusIndex = index;
        UpdateStatusCounter(statusCounter);
    }

    public int GetIndex()
    {
        return statusIndex;
    }

    public void UpdateStatusCounter(int newCounter)
    {
        statusCounter.text = $"{newCounter}";
    }
}

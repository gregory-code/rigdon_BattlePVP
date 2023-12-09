using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class monsterSelectButton : MonoBehaviour
{
    [SerializeField] Button monsterButton;

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Image monsterImage;
    [SerializeField] Image outlineImage;
    [SerializeField] TextMeshProUGUI hpText;
    [SerializeField] TextMeshProUGUI strText;
    [SerializeField] TextMeshProUGUI magicText;
    [SerializeField] TextMeshProUGUI speedText;

    internal void Init(monster mon, teamBuilder builder)
    {
        monsterButton.onClick.AddListener(() => builder.CreateNewMonster(mon.GetMonsterID()));

        nameText.text = mon.GetMonsterName();
        monsterImage.sprite = mon.stages[0];
        outlineImage.sprite = mon.circleOutline;
        hpText.text = mon.GetInitialHP() + "";
        strText.text = mon.GetInitialStrength() + "";
        magicText.text = mon.GetInitialMagic() + "";
        speedText.text = mon.GetInitialSpeed() + "";
    }
}

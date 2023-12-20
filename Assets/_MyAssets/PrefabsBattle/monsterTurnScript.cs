using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class monsterTurnScript : MonoBehaviour
{
    [SerializeField] Image monsterImage;
    [SerializeField] GameObject friendlyBackground;
    [SerializeField] GameObject enemyBackground;

    public void Init(bool isMine, Sprite desiredImage)
    {
        transform.localScale = new Vector3(1, 1, 1);
        monsterImage.sprite = desiredImage;
        friendlyBackground.SetActive(isMine);
        enemyBackground.SetActive(!isMine);
    }
}

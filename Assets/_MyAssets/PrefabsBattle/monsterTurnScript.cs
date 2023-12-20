using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class monsterTurnScript : MonoBehaviour
{
    [SerializeField] Image monsterImage;
    [SerializeField] GameObject friendlyBackground;
    [SerializeField] GameObject enemyBackground;

    [SerializeField] Animator myAnimator;

    public void Init(bool isMine, Sprite desiredImage)
    {
        transform.localScale = new Vector3(1, 1, 1);
        monsterImage.sprite = desiredImage;
        friendlyBackground.SetActive(isMine);
        enemyBackground.SetActive(!isMine);
    }

    public void Discard()
    {
        myAnimator.SetTrigger("discard");
        Destroy(this.gameObject, 0.5f);
    }
}

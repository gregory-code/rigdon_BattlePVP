using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class monsterBase : MonoBehaviour
{
    [Header("BaseClass")]
    [SerializeField] bool bMouseOver;

    [SerializeField] Animator monsterAnimator;
    
    public GameMaster gameMaster { get; private set; }
    public lineScript redLine { get; private set; }
    public lineScript greenLine { get; private set; }
    public Camera renderCamera { get; private set; }

    [Header("Monster Prefab")]
    [SerializeField] monster myMonster;
    [SerializeField] SpriteRenderer monsterSprite;
    [SerializeField] Image health;
    [SerializeField] TextMeshProUGUI healthText;

    public void Init(monster myMonster, monsterPreferences myPref, GameMaster gameMaster, lineScript redLine, lineScript greenLine, Camera renderCamera)
    {
        this.myMonster = myMonster;
        this.gameMaster = gameMaster;
        this.redLine = redLine;
        this.greenLine = greenLine;
        this.renderCamera = renderCamera;

        int stage = myMonster.GetSpriteIndexFromLevel();
        monsterSprite.sprite = myMonster.stages[stage];
        if (myMonster.bFlipSprite[stage] == true)
            monsterSprite.flipX = !monsterSprite.flipX;

        if(myMonster.isMine() == false)
            monsterSprite.flipX = !monsterSprite.flipX;

        healthText.text = myMonster.GetCurrentHealth() + "";
        healthText.color = (myMonster.getHealthPercentage() >= 0.7f) ? new Vector4(0, 255, 0, 255) : new Vector4(255, 180, 180, 255);
    }

    void Update()
    {
        health.fillAmount = Mathf.Lerp(health.fillAmount, myMonster.getHealthPercentage(), 4 * Time.deltaTime);
    }

    public monster GetMyMonster()
    {
        return myMonster;
    }

    public void SetIsMouseOver(bool state)
    {
        bMouseOver = state;
    }

    public void OnMouseOver()
    {
        if (gameMaster.bRendering == false || bMouseOver == true) return;

        bMouseOver = true;
        gameMaster.targetedCritter = myMonster;

        if (myMonster.isMine())
        {
            redLine.enable(false, Vector2.zero);
            greenLine.enable(true, renderCamera.ScreenToWorldPoint(gameMaster.touchedPos));
            greenLine.focusTarget(true, gameObject.transform);

        }
        else
        {
            greenLine.enable(false, Vector2.zero);
            redLine.enable(true, renderCamera.ScreenToWorldPoint(gameMaster.touchedPos));
            redLine.focusTarget(true, gameObject.transform);
        }
    }

    public void OnMouseExit()
    {
        if (bMouseOver == false) return;

        bMouseOver = false;

        if (myMonster.isMine())
        {
            greenLine.focusTarget(false, gameObject.transform);
        }
        else
        {
            redLine.focusTarget(false, gameObject.transform);
        }
    }
}

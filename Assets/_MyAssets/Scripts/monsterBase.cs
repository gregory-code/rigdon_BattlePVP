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
    
    public GameMaster gameMaster { get; private set; }
    public lineScript redLine { get; private set; }
    public lineScript greenLine { get; private set; }
    public Camera renderCamera { get; private set; }

    [Header("Monster Prefab")]
    [SerializeField] monster myMonster;
    private SpriteRenderer monsterSprite;
    private Image health;
    private TextMeshProUGUI healthText;
    private TextMeshProUGUI nameText;
    private Transform HUD;
    private Animator monsterAnimator;

    public void Init(monster myMonster, GameMaster gameMaster, lineScript redLine, lineScript greenLine, Camera renderCamera)
    {
        this.myMonster = myMonster;
        this.gameMaster = gameMaster;
        this.redLine = redLine;
        this.greenLine = greenLine;
        this.renderCamera = renderCamera;

        monsterDependecy dependecies = GetComponent<monsterDependecy>();
        GrabDependecies(dependecies);

        int stage = myMonster.GetSpriteIndexFromLevel();
        monsterSprite.sprite = myMonster.stages[stage];
        if (myMonster.bFlipSprite[stage] == true)
            monsterSprite.flipX = !monsterSprite.flipX;

        if(myMonster.isMine() == false)
        {
            Vector3 flipRotation = new Vector3(0, 180, 0);
            transform.localEulerAngles = flipRotation;
            HUD.localEulerAngles = flipRotation;
        }

        nameText.text = myMonster.GetMonsterNickname();
        healthText.text = myMonster.GetCurrentHealth() + "";
        healthText.color = (myMonster.getHealthPercentage() >= 0.7f) ? new Vector4(0, 255, 0, 255) : new Vector4(255, 180, 180, 255);
    }

    private void GrabDependecies(monsterDependecy dependecy)
    {
        monsterSprite = dependecy.GetMonsterSprite();
        health = dependecy.GetHealthBar();
        healthText = dependecy.GetHealthText();
        nameText = dependecy.GetNameText();
        HUD = dependecy.GetHUD();
        monsterAnimator = dependecy.GetAnimator(myMonster.GetMonsterID(), myMonster.GetSpriteIndexFromLevel());
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
        gameMaster.targetedMonster = myMonster;

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

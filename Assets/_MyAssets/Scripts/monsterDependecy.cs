using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class monsterDependecy : MonoBehaviour
{
    [SerializeField] Animator monsterAnimator;
    [SerializeField] SpriteRenderer monsterSprite;
    [SerializeField] Image health;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Transform HUD;
    [SerializeField] Transform attackPoint;
    [SerializeField] RuntimeAnimatorController[] babyMonsterControllers;
    [SerializeField] RuntimeAnimatorController[] midMonsterControllers;
    [SerializeField] RuntimeAnimatorController[] finalMonsterControllers;

    [SerializeField] GameObject[] statusProcPrefabs;

    public Animator GetAnimator(int monsterID, int whichStage)
    {
        switch(whichStage)
        {
            case 0:
                monsterAnimator.runtimeAnimatorController = babyMonsterControllers[monsterID];
                break;

            case 1:
                monsterAnimator.runtimeAnimatorController = midMonsterControllers[monsterID];
                break;

            case 2:
                monsterAnimator.runtimeAnimatorController = finalMonsterControllers[monsterID];
                break;
        }

        return monsterAnimator;
    }

    public Image GetHealthBar()
    {
        return health;
    }

    public TextMeshProUGUI GetHealthText()
    {
        return healthText;
    }

    public SpriteRenderer GetMonsterSprite()
    {
        return monsterSprite;
    }

    public TextMeshProUGUI GetNameText()
    {
        return nameText;
    }

    public Transform GetHUD()
    {
        return HUD;
    }

    public Transform GetAttackPoint()
    {
        return attackPoint;
    }

    public GameObject[] GetStatusPrefabs()
    {
        return statusProcPrefabs;
    }
}

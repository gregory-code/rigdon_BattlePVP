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
    [SerializeField] SpriteRenderer monsterShadow;
    [SerializeField] Image health;
    [SerializeField] Image tempHealth;
    [SerializeField] Transform effectSpawn;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Transform HUD;
    [SerializeField] Transform attackPoint;
    [SerializeField] Transform effectsList;
    [SerializeField] statusEffectUI statusEffectPrefab;
    [SerializeField] RuntimeAnimatorController[] babyMonsterControllers;
    [SerializeField] RuntimeAnimatorController[] midMonsterControllers;
    [SerializeField] RuntimeAnimatorController[] finalMonsterControllers;

    [SerializeField] RuntimeAnimatorController[] babyEnemyControllers;
    [SerializeField] RuntimeAnimatorController[] midEnemyControllers;
    [SerializeField] RuntimeAnimatorController[] finalEnemyControllers;

    [SerializeField] GameObject[] statusProcPrefabs;

    [SerializeField] GameObject dirtSpray;

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

    public Animator GetEnemyAnimator(int monsterID, int whichStage)
    {
        switch (whichStage)
        {
            case 0:
                monsterAnimator.runtimeAnimatorController = babyEnemyControllers[monsterID];
                break;

            case 1:
                monsterAnimator.runtimeAnimatorController = midEnemyControllers[monsterID];
                break;

            case 2:
                monsterAnimator.runtimeAnimatorController = finalEnemyControllers[monsterID];
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

    public Image GetTempHealth()
    {
        return tempHealth;
    }

    public Transform GetEffectsList()
    {
        return effectsList;
    }

    public statusEffectUI GetStatusEffectPrefab()
    {
        return statusEffectPrefab;
    }

    public Transform GetEffectSpawn()
    {
        return effectSpawn;
    }

    public SpriteRenderer GetMonsterShadow()
    {
        return monsterShadow;
    }

    public void SprayDirt()
    {
        GameObject dirt = Instantiate(dirtSpray);
        Vector3 spawn = monsterSprite.transform.position;
        spawn.y -= 3.5f;
        dirt.transform.position = spawn;
    }
}

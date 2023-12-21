using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameMaster : MonoBehaviourPunCallbacks
{
    [Header("References")]
    [SerializeField] GameMenu gameMenu;
    [SerializeField] lineScript redLine;
    [SerializeField] lineScript greenLine;
    [SerializeField] Camera renderCamera;

    [SerializeField] Transform[] playerSpawns;
    [SerializeField] Transform[] enemySpawns;

    [SerializeField] GameObject monsterPrefab;

    [Header("Targeting")]
    public monsterBase selectedMonster;
    public monsterBase targetedMonster;

    public Vector3 touchedPos;
    public bool bRendering;

    [Header("Effects")]
    [SerializeField] damagePopScript damagePop;
    [SerializeField] projectileScript[] projectilePrefabs;

    public void StartFight()
    {
        spawnTeams();
        sortBySpeed();
        selectNew();
    }

    void Update()
    {
        handleTurnOrder();
    }

    public void spawnTeams()
    {
        for (int i = 0; i < 3; ++i)
        {
            GameObject newAlly = Instantiate(monsterPrefab, playerSpawns[i]);
            monsterAlly allyScript = AddMonsterScript(gameMenu.GetMyTeam()[i].GetMonsterID(), newAlly);
            allyScript.Init(gameMenu.GetMyTeam()[i], this, redLine, greenLine, renderCamera, damagePop);

            GameObject newEnemy = Instantiate(monsterPrefab, enemySpawns[i]);
            monsterBase enemyScript = AddMonsterScript(gameMenu.GetEnemyTeam()[i].GetMonsterID(), newEnemy);
            enemyScript.Init(gameMenu.GetEnemyTeam()[i], this, redLine, greenLine, renderCamera, damagePop);
        }
    }

    private monsterAlly AddMonsterScript(int monsterID, GameObject newMonster)
    {
        switch(monsterID)
        {
            case 2:
                return newMonster.AddComponent<draticAlly>();

            case 8:
                return newMonster.AddComponent<lusseliaAlly>();
        }

        return newMonster.AddComponent<draticAlly>();
    }

    [Header("Turn Order")]
    [SerializeField] Transform monsterTurnParent;
    [SerializeField] monsterTurnScript monsterTurnPrefab;
    [SerializeField] selectParticleScript selectParticlesPrefab;
    GameObject selectParticles;

    private List<monsterTurnScript> monsterTurnList = new List<monsterTurnScript>();

    public List<monster> activeMonsters = new List<monster>();

    private void addTurn(monster newMonster)
    {
        monsterTurnScript newMonsterTurn = Instantiate(monsterTurnPrefab, monsterTurnParent);
        newMonsterTurn.Init(newMonster.bMine, newMonster.stagesIcons[newMonster.GetSpriteIndexFromLevel()]);
        monsterTurnList.Add(newMonsterTurn);
    }

    private void deleteTurn(int where)
    {
        monsterTurnList[where].Discard();
        monsterTurnList.RemoveAt(where);
        activeMonsters.RemoveAt(where);
    }

    private void sortBySpeed()
    {
        activeMonsters.Clear();
        StatComparer comparer = new StatComparer();

        for (int i = 0; i < gameMenu.GetMyTeam().Length; ++i)
        {
            activeMonsters.Add(gameMenu.GetMyTeam()[i]);
            activeMonsters.Add(gameMenu.GetEnemyTeam()[i]);
        }

        activeMonsters.Sort(comparer);

        foreach (monster mon in activeMonsters)
        {
            addTurn(mon);
        }
    }

    private void selectNew()
    {
        selectParticleScript newSelect = Instantiate(selectParticlesPrefab, playerSpawns[0]); // give it a parent to go to
        newSelect.Init(activeMonsters[0].matchingColor);
        newSelect.transform.localPosition = Vector3.one;

        myTurn = activeMonsters[0].bMine;
        activeMonsters[0].canAct = true;

        if(selectParticles != null)
            Destroy(selectParticles);

        selectParticles = newSelect.gameObject;
    }

    public void removeFromCritterBase(monsterBase toRemove)
    {
        for (int i = 0; i < activeMonsters.Count; ++i)
        {
            if (toRemove.GetMyMonster() == activeMonsters[i])
            {
                deleteTurn(i);
                break;
            }
        }
    }

    private void handleTurnOrder()
    {
        if (monsterTurnList.Count <= 0)
            return;

        for (int i = 0; i < monsterTurnList.Count; ++i)
        {
            Vector3 lerp = (i == 0) ? new Vector3(-535, 77, 0) : new Vector3(-438 + (69 * (i - 1)), 70, 0);
            Vector3 size = (i == 0) ? new Vector3(1.357f, 1.357f, 1.357f) : Vector3.one ;

            monsterTurnList[i].transform.localPosition = lerp;
            monsterTurnList[i].transform.localScale = size;
        }
    }

    #region Commands

    [Header("Commands")]
    [SerializeField] bool myTurn;

    private monster GetSpecificMonster(bool bMine, int teamIndex) // bMine will be the same for both players RPC call
    {
        if (bMine && myTurn)
        {
            return gameMenu.GetMyTeam()[teamIndex];
        }

        if(bMine == false && myTurn == false)
        {
            return gameMenu.GetMyTeam()[teamIndex];
        }


        return gameMenu.GetEnemyTeam()[teamIndex];
    }

    public void NextTurn()
    {
        this.photonView.RPC("NextTurnRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void NextTurnRPC()
    {
        bool team1Alive = false;
        bool team2Alive = false;
        foreach (monster mon in activeMonsters)
        {
            if (mon.bMine == true) 
                team1Alive = true;

            if (mon.bMine == false) 
                team2Alive = true;
        }

        if (team1Alive == false || team2Alive == false)
        {
            // game over
            return;
        }

        monster finishedMonster = activeMonsters[0];
        deleteTurn(0);

        for (int i = 0; i < gameMenu.GetMyTeam().Length; ++i)
        {
            if (gameMenu.GetMyTeam()[i] == finishedMonster)
            {
                activeMonsters.Add(gameMenu.GetMyTeam()[i]);
                addTurn(gameMenu.GetMyTeam()[i]);
                break;
            }

            if (gameMenu.GetEnemyTeam()[i] == finishedMonster)
            {
                activeMonsters.Add(gameMenu.GetEnemyTeam()[i]);
                addTurn(gameMenu.GetEnemyTeam()[i]);
                break;
            }
        }

        selectNew();
    }

    public void MoveMonster(bool bMine, int teamIndex, Vector3 desiredLocation)
    {
        this.photonView.RPC("MoveMonsterRPC", RpcTarget.AllBuffered, bMine, teamIndex, desiredLocation);
    }

    [PunRPC]
    void MoveMonsterRPC(bool bMine, int teamIndex, Vector3 desiredLocation)
    {

    }

    public void AnimateMonster(bool bMine, int teamIndex, string animName)
    {
        this.photonView.RPC("AnimateMonsterRPC", RpcTarget.AllBuffered, bMine, teamIndex, animName);
    }

    [PunRPC]
    void AnimateMonsterRPC(bool bMine, int teamIndex, string animName)
    {
        GetSpecificMonster(bMine, teamIndex).PlayAnimation(animName);
    }

    public void ChangeMonsterHealth(bool bMine, int teamIndex, int healthChange)
    {
        this.photonView.RPC("ChangeMonsterHealthRPC", RpcTarget.AllBuffered, bMine, teamIndex, healthChange);
    }

    [PunRPC]
    void ChangeMonsterHealthRPC(bool bMine, int teamIndex, int healthChange)
    {
        GetSpecificMonster(bMine, teamIndex).ChangeHealth(healthChange);
    }

    public void ShootProjectile(bool bMine, int teamIndex, int projectileIndex, bool bMine2, int TargetIndex)
    {
        this.photonView.RPC("ShootProjectileRPC", RpcTarget.AllBuffered, bMine, teamIndex, projectileIndex, bMine2, TargetIndex);
    }

    [PunRPC]
    void ShootProjectileRPC(bool bMine, int teamIndex, int projectileIndex, bool bMine2, int TargetIndex)
    {
        Transform target = GetSpecificMonster(bMine2, TargetIndex).myBase.transform;
        GetSpecificMonster(bMine, teamIndex).ShootProjectile(projectilePrefabs[projectileIndex], target);
    }

    #endregion
}

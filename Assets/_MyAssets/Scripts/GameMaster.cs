using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameMaster : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public bool inInfoScreen;

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
    public bool waitForAction;

    [Header("Effects")]
    [SerializeField] damagePopScript damagePop;
    [SerializeField] projectileScript[] projectilePrefabs;
    [SerializeField] GameObject[] statusPrefabs;

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
            allyScript.Init(gameMenu.GetMyTeam()[i], this, redLine, greenLine, renderCamera, damagePop, playerSpawns[i]);

            GameObject newEnemy = Instantiate(monsterPrefab, enemySpawns[i]);
            monsterBase enemyScript = AddMonsterScript(gameMenu.GetEnemyTeam()[i].GetMonsterID(), newEnemy);
            enemyScript.Init(gameMenu.GetEnemyTeam()[i], this, redLine, greenLine, renderCamera, damagePop, enemySpawns[i]);
        }
    }

    public monster[] GetMonstersTeam(monster monToCheck)
    {
        monster[] myTeam = gameMenu.GetMyTeam();
        monster[] enemyTeam = gameMenu.GetEnemyTeam();

        for(int i = 0; i < myTeam.Length; i++)
        {
            if (myTeam[i] == monToCheck)
                return myTeam;

            if (enemyTeam[i] == monToCheck)
                return enemyTeam;
        }

        return myTeam;
    }

    public int GetRandomEnemyIndex(int indexToExclude, int otherIndexToExlude)
    {
        monster[] enemyTeam = gameMenu.GetEnemyTeam();
        int[] enemyIndexes = new int[3] { 0, 1, 2 };

        if (indexToExclude != -1)
            enemyIndexes[indexToExclude] = -1;

        if (otherIndexToExlude != -1)
            enemyIndexes[otherIndexToExlude] = -1;

        for (int i = 0; i < enemyTeam.Length; i++)
        {
            if (enemyTeam[i].GetCurrentHealth() <= 0)
            {
                enemyIndexes[i] = -1;
            }
        }

        if (enemyIndexes[0] == -1 && enemyIndexes[1] == -1 && enemyIndexes[2] == -1)
            return 5;

        int randomIndex = -1;
        while (randomIndex == -1)
        {
            randomIndex = enemyIndexes[Random.Range(0, 3)];
        }

        return randomIndex;
    }

    public int GetRandomConductiveEnemyIndex()
    {
        monster[] enemyTeam = gameMenu.GetEnemyTeam();
        int[] enemyIndexes = new int[3] { -1, -1, -1 };

        for (int i = 0; i < enemyTeam.Length; i++)
        {
            if (enemyTeam[i].GetStatus(0) != null && enemyTeam[i].GetCurrentHealth() > 0)
            {
                enemyIndexes[i] = enemyTeam[i].teamIndex;
            }
        }

        if (enemyIndexes[0] == -1 && enemyIndexes[1] == -1 && enemyIndexes[2] == -1)
            return 5;

        int randomIndex = -1;
        while (randomIndex == -1)
        {
            randomIndex = enemyIndexes[Random.Range(0, 3)];
        }

        return randomIndex;
    }

    private monsterAlly AddMonsterScript(int monsterID, GameObject newMonster)
    {
        switch(monsterID)
        {
            case 2:
                return newMonster.AddComponent<draticAlly>();

            case 8:
                return newMonster.AddComponent<lusseliaAlly>();

            case 9:
                return newMonster.AddComponent<minfurAlly>();
        }

        return newMonster.AddComponent<lusseliaAlly>();
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
        selectParticleScript newSelect = Instantiate(selectParticlesPrefab, activeMonsters[0].myBase.transform); // give it a parent to go to
        newSelect.Init(activeMonsters[0].matchingColor);
        Vector3 location = new Vector3(0, -12, 0);
        newSelect.transform.localPosition = location;

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
        StartCoroutine(ExecuteNextTurn());
    }

    private IEnumerator ExecuteNextTurn()
    {
        if (IsGameOver() == true)
        {

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

        foreach(monster mon in activeMonsters)
        {
            mon.NextTurn();
        }

        yield return new WaitForEndOfFrame();
    }

    private bool IsGameOver()
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
            return true;
        }

        return false;
    }

    public void UsedAction(bool bMine, int teamIndex, bool isAttack)
    {
        this.photonView.RPC("UsedActionRPC", RpcTarget.AllBuffered, bMine, teamIndex, isAttack);
    }

    [PunRPC]
    void UsedActionRPC(bool bMine, int teamIndex, bool isAttack)
    {
        GetSpecificMonster(bMine, teamIndex).UsedAction(isAttack);
    }

    public void MoveMonster(bool bMine, int teamIndex, bool goHome, bool bMine2, int targetIndex)
    {
        this.photonView.RPC("MoveMonsterRPC", RpcTarget.AllBuffered, bMine, teamIndex, goHome, bMine2, targetIndex);
    }

    [PunRPC]
    void MoveMonsterRPC(bool bMine, int teamIndex, bool goHome, bool bMine2, int targetIndex)
    {
        Vector3 pos = GetSpecificMonster(bMine2, targetIndex).spawnLocation;
        GetSpecificMonster(bMine, teamIndex).MovePosition(goHome, pos.x, pos.y);
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

    public void ChangeMonsterHealth(bool bMine, int userTeamIndex, bool bMine2, int targetTeamIndex, int healthChange)
    {
        this.photonView.RPC("ChangeMonsterHealthRPC", RpcTarget.AllBuffered, bMine, userTeamIndex, bMine2, targetTeamIndex, healthChange);
    }

    [PunRPC]
    void ChangeMonsterHealthRPC(bool bMine, int userTeamIndex, bool bMine2, int targetTeamIndex, int healthChange)
    {
        GetSpecificMonster(bMine2, targetTeamIndex).ChangeHealth(healthChange, bMine, userTeamIndex);
    }

    public void ApplyStatus(int statusIndex, bool bMine, int TargetIndex, int counter, int power)
    {
        this.photonView.RPC("ApplyStatusRPC", RpcTarget.AllBuffered, statusIndex, bMine, TargetIndex, counter, power);
    }

    [PunRPC]
    void ApplyStatusRPC(int statusIndex, bool bMine, int TargetIndex, int counter, int power)
    {
        GetSpecificMonster(bMine, TargetIndex).ApplyStatus(statusIndex, statusPrefabs[statusIndex], counter, power);
    }

    public void AttackAgain(bool bMine, int TargetIndex, int percentageMultiplier, bool bMine2, int TargetOfTargetIndex)
    {
        this.photonView.RPC("AttackAgainRPC", RpcTarget.AllBuffered, bMine, TargetIndex, percentageMultiplier, bMine2, TargetOfTargetIndex);
    }

    [PunRPC]
    void AttackAgainRPC(bool bMine, int TargetIndex, int percentageMultiplier, bool bMine2, int TargetOfTargetIndex)
    {
        GetSpecificMonster(bMine, TargetIndex).AttackAgain(percentageMultiplier, bMine2, TargetOfTargetIndex);
    }

    public void ShootProjectile(bool bMine, int userTeamIndex, int projectileIndex, bool bMine2, int TargetIndex)
    {
        this.photonView.RPC("ShootProjectileRPC", RpcTarget.AllBuffered, bMine, userTeamIndex, projectileIndex, bMine2, TargetIndex);
    }

    [PunRPC]
    void ShootProjectileRPC(bool bMine, int teamIndex, int projectileIndex, bool bMine2, int TargetIndex)
    {
        Transform target = GetSpecificMonster(bMine2, TargetIndex).myBase.transform;
        GetSpecificMonster(bMine, teamIndex).ShootProjectile(projectilePrefabs[projectileIndex], target);
    }

    #endregion
}

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
    [SerializeField] Transform[] uniqueLocations;

    [SerializeField] GameObject monsterPrefab;

    private bool fightingAI;

    [SerializeField] List<GameObject> monsterBasesToDeleteLater = new List<GameObject>();

    [Header("Targeting")]
    public monsterBase selectedMonster;
    public monsterBase targetedMonster;

    public Vector3 touchedPos;
    public bool bRendering;
    public bool waitingForAction;
    public monster redirectedMon = null;
    public int estimatedDamage;

    [Header("Effects")]
    [SerializeField] damagePopScript damagePop;
    [SerializeField] projectileScript[] projectilePrefabs;
    [SerializeField] GameObject[] statusPrefabs;

    public GameObject regularDeath;
    public GameObject grimmetalDeath;
    public bool bRegularDeath = true;
    public bool holdItDeerCrossing = false;

    public void StartFight(bool againstAI)
    {
        gameMenu.enemyInAICombat = (againstAI);
        bRegularDeath = true;
        movingToNewGame = false;
        spawnTeams(againstAI);
        sortBySpeed();
        selectNew();
    }

    void Update()
    {
        handleTurnOrder();
        handleReticle();
    }

    private void handleTurnOrder()
    {
        if (monsterTurnList.Count <= 0)
            return;

        for (int i = 0; i < monsterTurnList.Count; ++i)
        {
            Vector3 lerp = (i == 0) ? new Vector3(-535, 77, 0) : new Vector3(-438 + (69 * (i - 1)), 70, 0);
            Vector3 size = (i == 0) ? new Vector3(1.357f, 1.357f, 1.357f) : Vector3.one;

            monsterTurnList[i].transform.localPosition = lerp;
            monsterTurnList[i].transform.localScale = size;
        }
    }

    private void handleReticle()
    {
        if (bRendering == false || redLine.IsHoveringOverTarget() || greenLine.IsHoveringOverTarget()) 
            return;

        Vector3 touchPos = Input.mousePosition;
        redLine.updateReticleLocation(renderCamera.ScreenToWorldPoint(touchPos));
        greenLine.updateReticleLocation(renderCamera.ScreenToWorldPoint(touchPos));
    }

    public void spawnTeams(bool againstAI)
    {
        fightingAI = againstAI;
        createTeam(gameMenu.GetMyTeam(), playerSpawns, false, false);
        createTeam(gameMenu.GetEnemyTeam(), enemySpawns, true, againstAI);
    }

    private void createTeam(monster[] teamToCreate, Transform[] spawns, bool enemy, bool isAI)
    {
        for(int i = 0; i < teamToCreate.Length; i++)
        {
            if (teamToCreate[i] == null)
                continue;

            GameObject newMon = Instantiate(monsterPrefab, spawns[i]);
            if(enemy == false)
            {
                monsterAlly allyScript = AddMonsterScript(teamToCreate[i].GetMonsterID(), newMon);
                allyScript.Init(teamToCreate[i], this, redLine, greenLine, renderCamera, damagePop, spawns[i]);
            }
            else if(isAI)
            {
                monsterEnemyAI enemyAI = AddEnemyScript(teamToCreate[i].GetMonsterID(), newMon);
                enemyAI.Init(teamToCreate[i], this, redLine, greenLine, renderCamera, damagePop, spawns[i]);
            }
            else
            {
                monsterBase enemyScript = AddMonsterScript(teamToCreate[i].GetMonsterID(), newMon);
                enemyScript.Init(teamToCreate[i], this, redLine, greenLine, renderCamera, damagePop, spawns[i]);
            }
            monsterBasesToDeleteLater.Add(newMon);
        }
    }

    private monsterAlly AddMonsterScript(int monsterID, GameObject newMonster)
    {
        switch(monsterID)
        {
            case 2:
                return newMonster.AddComponent<draticAlly>();

            case 4:
                return newMonster.AddComponent<grimmetalAlly>();

            case 6:
                return newMonster.AddComponent<incanteerAlly>();

            case 8:
                return newMonster.AddComponent<lusseliaAlly>();

            case 9:
                return newMonster.AddComponent<minfurAlly>();
        }

        return newMonster.AddComponent<lusseliaAlly>();
    }

    private monsterEnemyAI AddEnemyScript(int monsterID, GameObject newMonster)
    {
        switch (monsterID)
        {
            case 1:
                return newMonster.AddComponent<minfurEnemy>();
        }

        return newMonster.AddComponent<minfurEnemy>();
    }

    [Header("Turn Order")]
    [SerializeField] Transform monsterTurnParent;
    [SerializeField] monsterTurnScript monsterTurnPrefab;
    [SerializeField] selectParticleScript selectParticlesPrefab;
    GameObject selectParticles;

    public bool movingToNewGame;

    private List<monsterTurnScript> monsterTurnList = new List<monsterTurnScript>();

    public List<monster> activeMonsters = new List<monster>();

    private void addTurn(monster newMonster, bool insertFirst)
    {
        monsterTurnScript newMonsterTurn = Instantiate(monsterTurnPrefab, monsterTurnParent);
        newMonsterTurn.Init(newMonster.isPlayer1(), newMonster.stagesIcons[newMonster.GetSpriteIndexFromLevel()]);

        if(insertFirst)
        {
            monsterTurnList.Insert(0, newMonsterTurn);
        }
        else
        {
            monsterTurnList.Add(newMonsterTurn);
        }
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

        List<monster> bothTeams = gameMenu.GetBothTeams();

        for (int i = 0; i < bothTeams.Count; ++i)
        {
            activeMonsters.Add(bothTeams[i]);
        }

        activeMonsters.Sort(comparer);

        foreach (monster mon in activeMonsters)
        {
            addTurn(mon, false);
        }
    }

    private void selectNew()
    {
        if (activeMonsters[0].isDead())
        {
            deleteTurn(0);
        }

        selectParticleScript newSelect = Instantiate(selectParticlesPrefab, activeMonsters[0].ownerTransform); // give it a parent to go to
        newSelect.Init(activeMonsters[0].matchingColor);
        Vector3 location = new Vector3(0, -12, 0);
        newSelect.transform.localPosition = location;

        gameMenu.SetFilter(!activeMonsters[0].GetOwnership());
        activeMonsters[0].SetAct(true);

        if (activeMonsters[0].IsAI())
            StartCoroutine(CheckAI());

        if (selectParticles != null)
            Destroy(selectParticles);

        selectParticles = newSelect.gameObject;
    }

    private IEnumerator CheckAI()
    {
        yield return new WaitForSeconds(2f);
        monsterEnemyAI[] enemyAIs = GameObject.FindObjectsOfType<monsterEnemyAI>();
        foreach (monsterEnemyAI ai in enemyAIs)
        {
            if (ai.GetMonster() == activeMonsters[0])
            {
                ai.AITurn();
            }
        }
    }

    public void SetFilter(bool regular)
    {
        gameMenu.SetFilter(regular);
    }

    public void removeFromMonsterBase(monsterBase toRemove)
    {
        StartCoroutine(removeFromBaseWait(toRemove));
    }

    private IEnumerator removeFromBaseWait(monsterBase toRemove)
    {
        while(waitingQueue == true)
        {
            yield return new WaitForEndOfFrame();
        }

        for (int i = 0; i < activeMonsters.Count; ++i)
        {
            if (toRemove.GetMonster() == activeMonsters[i])
            {
                deleteTurn(i);
                break;
            }
        }
    }

    private void ClearField()
    {
        foreach (monster mon in activeMonsters)
        {
            mon.RemoveConnections();
        }
        activeMonsters.Clear();

        foreach (GameObject mon in monsterBasesToDeleteLater)
        {
            Destroy(mon);
        }
        monsterBasesToDeleteLater.Clear();


        foreach (monsterTurnScript turn in monsterTurnList)
        {
            Destroy(turn.gameObject);
        }
        monsterTurnList.Clear();
    }

    private bool waitingQueue;
    private bool addToFront;
    private monster monsterToAdd;
    private IEnumerator ExecuteNextTurn()
    {
        while (waitingQueue == true)
        {
            yield return new WaitForEndOfFrame();
        }

        monster finishedMonster = activeMonsters[0];
        deleteTurn(0);

        for (int i = 0; i < gameMenu.GetMyTeam().Length; ++i)
        {
            if (gameMenu.GetMyTeam()[i] == finishedMonster)
            {
                activeMonsters.Add(gameMenu.GetMyTeam()[i]);
                addTurn(gameMenu.GetMyTeam()[i], false);
                break;
            }

            if (gameMenu.GetEnemyTeam()[i] == finishedMonster)
            {
                activeMonsters.Add(gameMenu.GetEnemyTeam()[i]);
                addTurn(gameMenu.GetEnemyTeam()[i], false);
                break;
            }
        }

        if (addToFront)
        {
            if (monsterToAdd.GetCurrentHealth() > 0)
            {
                activeMonsters.Insert(0, monsterToAdd);
                addTurn(monsterToAdd, true);
            }
            addToFront = false;
        }

        waitingQueue = true;

        foreach (monster mon in activeMonsters)
        {
            mon.NextTurn();
        }

        selectNew();
        waitingQueue = false;

        yield return new WaitForSeconds(0.5f);

        if (IsGameOver() == true && movingToNewGame == false)
        {
            movingToNewGame = true;

            yield return new WaitForSeconds(4f);

            waitingQueue = false;
            ClearField();

            if(fightingAI)
            {
                gameMenu.FinishedRound();
            }
            else
            {
                gameMenu.StartIntermission();
            }
        }
    }

    private bool IsGameOver()
    {
        bool team1Alive = false;
        bool team2Alive = false;
        foreach (monster mon in activeMonsters)
        {
            if (mon.isPlayer1() == true)
                team1Alive = true;

            if (mon.isPlayer1() == false)
                team2Alive = true;
        }

        if (team1Alive == false)
        {
            gameMenu.PlayerWon(false);
            return true;
        }

        if(team2Alive == false)
        {
            gameMenu.PlayerWon(true);
            return true;
        }

        return false;
    }

    #region Commands

    public monster GetMonster(bool isPlayer1, int index)
    {
        return gameMenu.GetMonsterFromReference(isPlayer1, index);
    }

    public bool DoIOwnThis(bool isPlayer1)
    {
        return gameMenu.OwnerShipCheck(isPlayer1);
    }

    public monster[] GetMonstersTeam(monster monToCheck)
    {
        monster[] myTeam = gameMenu.GetMyTeam();
        monster[] enemyTeam = gameMenu.GetEnemyTeam();

        for (int i = 0; i < myTeam.Length; i++)
        {
            if (myTeam[i] == monToCheck)
                return myTeam;

            if (enemyTeam[i] == monToCheck)
                return enemyTeam;
        }

        return myTeam;
    }

    public monster GetRandomEnemy(int indexToExclude, int otherIndexToExlude, bool allyTeam)
    {
        monster[] enemyTeam = gameMenu.GetEnemyTeam();

        if (allyTeam)
            enemyTeam = gameMenu.GetMyTeam();

        int[] enemyIndexes = new int[3] { 0, 1, 2 };

        if (indexToExclude != -1)
            enemyIndexes[indexToExclude] = -1;

        if (otherIndexToExlude != -1)
            enemyIndexes[otherIndexToExlude] = -1;

        for (int i = 0; i < enemyTeam.Length; i++)
        {
            if (enemyTeam[i] == null)
            {
                enemyIndexes[i] = -1;
            }
            else if (enemyTeam[i].isDead())
            {
                enemyIndexes[i] = -1;
            }
        }

        if (enemyIndexes[0] == -1 && enemyIndexes[1] == -1 && enemyIndexes[2] == -1)
            return null;

        int randomIndex = -1;
        while (randomIndex == -1)
        {
            randomIndex = enemyIndexes[Random.Range(0, 3)];
        }

        return enemyTeam[randomIndex];
    }

    public monster GetRedirectedMonster(monster currentTarget)
    {
        if (redirectedMon != null)
        {
            currentTarget = redirectedMon;
            redirectedMon = null;
        }

        return currentTarget;
    }

    public monster GetRandomConductiveEnemy()
    {
        monster[] enemyTeam = gameMenu.GetEnemyTeam();
        int[] enemyIndexes = new int[3] { -1, -1, -1 };

        for (int i = 0; i < enemyTeam.Length; i++)
        {
            statusEffectUI conductive = enemyTeam[i].GetStatus(0);

            if (conductive != null && enemyTeam[i].isDead() == false)
            {
                enemyIndexes[i] = enemyTeam[i].GetIndex();
            }
        }

        if (enemyIndexes[0] == -1 && enemyIndexes[1] == -1 && enemyIndexes[2] == -1)
            return null;

        int randomIndex = -1;
        while (randomIndex == -1)
        {
            randomIndex = enemyIndexes[Random.Range(0, 3)];
        }

        return enemyTeam[randomIndex];
    }

    public void NextTurn()
    {
        if(fightingAI)
        {
            StartCoroutine(ExecuteNextTurn());
        }
        else
        {
            this.photonView.RPC("NextTurnRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void NextTurnRPC()
    {
        StartCoroutine(ExecuteNextTurn());
    }

    public void UsedAction(monster mon, monster targetOfAction, bool isAttack)
    {
        if (fightingAI)
        {
            holdItDeerCrossing = false;
            mon.UsedAction(targetOfAction, isAttack);
        }
        else
        {
            this.photonView.RPC("UsedActionRPC", RpcTarget.AllBuffered, mon.isPlayer1(), mon.GetIndex(), isAttack, targetOfAction.isPlayer1(), targetOfAction.GetIndex());
        }
    }

    [PunRPC]
    void UsedActionRPC(bool isPlayer1, int teamIndex, bool isAttack, bool isTargetOfAction, int targetTeamIndex)
    {
        holdItDeerCrossing = false;
        GetMonster(isPlayer1, teamIndex).UsedAction(GetMonster(isTargetOfAction, targetTeamIndex), isAttack);
    }

    public void MoveMonster(monster monMoved, monster monLocation, int uniquePos)
    {
        if (fightingAI)
        {
            bool doIOwnThis = DoIOwnThis(monMoved);
            Vector3 pos = monLocation.spawnLocation.position;

            switch (uniquePos)
            {
                case 1:
                    pos = monMoved.spawnLocation.position;
                    break;

                case 2:
                    pos = uniqueLocations[uniquePos].position;
                    break;

                case 3:
                    pos = (doIOwnThis == true) ? uniqueLocations[uniquePos].position : uniqueLocations[uniquePos + 1].position;
                    break;

                case 5:
                    pos = uniqueLocations[uniquePos].position;
                    break;
            }

            pos.x += (doIOwnThis) ? -3 : 3;

            monMoved.MovePosition(pos.x, pos.y);
        }
        else
        {
            this.photonView.RPC("MoveMonsterRPC", RpcTarget.AllBuffered, monMoved.isPlayer1(), monMoved.GetIndex(), monLocation.isPlayer1(), monLocation.GetIndex(), uniquePos);
        }
    }

    [PunRPC]
    void MoveMonsterRPC(bool isPlayer1Moved, int teamIndexMoved, bool isPlayer1Location, int teamIndexLocation, int uniquePos) // unique pos 0 = go to target // unique pos 1 = go home
    {
        bool doIOwnThis = DoIOwnThis(isPlayer1Moved);
        Vector3 pos = GetMonster(isPlayer1Location, teamIndexLocation).spawnLocation.position;

        switch(uniquePos)
        {
            case 1:
                pos = GetMonster(isPlayer1Moved, teamIndexMoved).spawnLocation.position;
                break;

            case 2:
                pos = uniqueLocations[uniquePos].position;
                break;

            case 3:
                pos = (doIOwnThis == true) ? uniqueLocations[uniquePos].position : uniqueLocations[uniquePos + 1].position;
                break;

            case 5:
                pos = uniqueLocations[uniquePos].position;
                break;
        }

        pos.x += (doIOwnThis) ? -3 : 3 ;

        GetMonster(isPlayer1Moved, teamIndexMoved).MovePosition(pos.x, pos.y);
    }

    public void AnimateMonster(monster monToAnimate, string animName)
    {
        if (fightingAI)
        {
            monToAnimate.PlayAnimation(animName);
        }
        else
        {
            this.photonView.RPC("AnimateMonsterRPC", RpcTarget.AllBuffered, monToAnimate.isPlayer1(), monToAnimate.GetIndex(), animName);
        }
    }

    [PunRPC]
    void AnimateMonsterRPC(bool isPlayer1, int index, string animName)
    {
        GetMonster(isPlayer1, index).PlayAnimation(animName);
    }

    public void DamageMonster(monster usingMon, monster recivingMon, int damage, bool crit)
    {
        if (fightingAI)
        {
            monster target = recivingMon;

            if (target.isDead())
                return;

            target.TakeDamage(usingMon, damage, crit);
        }
        else
        {
            this.photonView.RPC("DamageMonsterRPC", RpcTarget.AllBuffered, usingMon.isPlayer1(), usingMon.GetIndex(), recivingMon.isPlayer1(), recivingMon.GetIndex(), damage, crit);
        }
    }

    [PunRPC]
    void DamageMonsterRPC(bool isUsing, int usingIndex, bool isReciving, int recivingIndex, int damage, bool crit)
    {
        monster target = (GetMonster(isReciving, recivingIndex));

        if (target.isDead())
            return;

        target.TakeDamage(GetMonster(isUsing, usingIndex), damage, crit);
    }

    public void HealMonster(monster usingMon, monster recivingMon, int heal)
    {
        if (fightingAI)
        {
            monster target = recivingMon;

            if (target.isDead())
                return;

            target.HealHealth(usingMon, heal);
        }
        else
        {
            this.photonView.RPC("HealMonsterRPC", RpcTarget.AllBuffered, usingMon.isPlayer1(), usingMon.GetIndex(), recivingMon.isPlayer1(), recivingMon.GetIndex(), heal);
        }
    }

    [PunRPC]
    void HealMonsterRPC(bool isUsing, int usingIndex, bool isReciving, int recivingIndex, int heal)
    {
        monster target = (GetMonster(isReciving, recivingIndex));

        if (target.isDead())
            return;

        target.HealHealth(GetMonster(isUsing, usingIndex), heal);
    }

    public void DeclaringDamage(monster usingMon, monster recivingMon, int damage, bool destroyShields, bool crit)
    {
        if (fightingAI)
        {
            monster target = recivingMon;

            if (target.isDead())
                return;

            target.DelcaringDamage(usingMon, damage, destroyShields, crit);
        }
        else
        {
            this.photonView.RPC("DeclaringDamageRPC", RpcTarget.AllBuffered, usingMon.isPlayer1(), usingMon.GetIndex(), recivingMon.isPlayer1(), recivingMon.GetIndex(), damage, destroyShields, crit);
        }
    }

    [PunRPC]
    void DeclaringDamageRPC(bool isUsing, int usingIndex, bool isReciving, int recivingIndex, int damage, bool destroyShields, bool crit)
    {
        monster target = (GetMonster(isReciving, recivingIndex));

        if (target.isDead())
            return;

        target.DelcaringDamage(GetMonster(isUsing, usingIndex), damage, destroyShields, crit);
    }

    public void ApplyStatus(monster usingMon, monster recivingMon, int statusIndex, int counter, int power)
    {
        if (fightingAI)
        {
            monster target = recivingMon;

            if (target.isDead())
                return;

            target.ApplyStatus(usingMon, statusIndex, statusPrefabs[statusIndex], counter, power);
        }
        else
        {
            this.photonView.RPC("ApplyStatusRPC", RpcTarget.AllBuffered, usingMon.isPlayer1(), usingMon.GetIndex(), recivingMon.isPlayer1(), recivingMon.GetIndex(), statusIndex, counter, power);
        }
    }

    [PunRPC]
    void ApplyStatusRPC(bool isUsing, int usingIndex, bool isReciving, int recivingIndex, int statusIndex, int counter, int power)
    {
        monster target = (GetMonster(isReciving, recivingIndex));

        if (target.isDead())
            return;

        target.ApplyStatus(GetMonster(isUsing, usingIndex), statusIndex, statusPrefabs[statusIndex], counter, power);
    }

    public void TryRemoveStatus(monster recivingMon, int statusIndex)
    {
        if (fightingAI)
        {
            recivingMon.TryRemoveStatus(statusIndex, true);
        }
        else
        {
            this.photonView.RPC("TryRemoveStatusRPC", RpcTarget.AllBuffered, recivingMon.isPlayer1(), recivingMon.GetIndex(), statusIndex);
        }
    }

    [PunRPC]
    void TryRemoveStatusRPC(bool isReciving, int recivingIndex, int statusIndex)
    {
        GetMonster(isReciving, recivingIndex).TryRemoveStatus(statusIndex, true);
    }

    public void AttackAgain(monster recivingMon, monster targetMon, int extraDamage)
    {
        if (fightingAI)
        {
            recivingMon.AttackAgain(targetMon, extraDamage);
        }
        else
        {
            this.photonView.RPC("AttackAgainRPC", RpcTarget.AllBuffered, recivingMon.isPlayer1(), recivingMon.GetIndex(), targetMon.isPlayer1(), targetMon.GetIndex(), extraDamage);
        }
    }

    [PunRPC]
    void AttackAgainRPC(bool isReciving, int recivingIndex, bool isTarget, int targetIndex, int extraDamage)
    {
        GetMonster(isReciving, recivingIndex).AttackAgain(GetMonster(isTarget, targetIndex), extraDamage);
    }

    public delegate void OnMonsterDied(monster whoDied);
    public event OnMonsterDied onMonsterDied;

    public void GiveKillExp(monster monWhoKilled)
    {
        if (fightingAI)
        {
            onMonsterDied?.Invoke(monWhoKilled);
            monWhoKilled.GetExpHold(1).GainExp();
        }
        else
        {
            this.photonView.RPC("GiveKillExpRPC", RpcTarget.AllBuffered, monWhoKilled.isPlayer1(), monWhoKilled.GetIndex());
        }
    }

    [PunRPC]
    void GiveKillExpRPC(bool isMonWhoKilled, int monWhoKilledIndex)
    {
        onMonsterDied?.Invoke(GetMonster(isMonWhoKilled, monWhoKilledIndex));
        GetMonster(isMonWhoKilled, monWhoKilledIndex).GetExpHold(1).GainExp();
    }

    public void AdjustTurnOrder(monster gettingAdjusted, bool goFirst, bool goLast)
    {
        if (fightingAI)
        {
            monster target = gettingAdjusted;

            for (int i = 0; i < activeMonsters.Count; i++)
            {
                if (activeMonsters[i] == target)
                {
                    activeMonsters.Remove(activeMonsters[i]);
                    monsterTurnList[i].Discard();
                    monsterTurnList.RemoveAt(i);
                }
            }

            if (goFirst)
            {
                addToFront = true;
                monsterToAdd = target;
            }
            else if (goLast)
            {
                activeMonsters.Add(target);
                addTurn(target, false);
            }
        }
        else
        {
            this.photonView.RPC("AdjustTurnOrderRPC", RpcTarget.AllBuffered, gettingAdjusted.isPlayer1(), gettingAdjusted.GetIndex(), goFirst, goLast);
        }
    }

    [PunRPC]
    void AdjustTurnOrderRPC(bool isGettingAdjusted, int gettingAdjustedIndex, bool goFirst, bool goLast)
    {
        monster target = GetMonster(isGettingAdjusted, gettingAdjustedIndex);

        for(int i = 0; i < activeMonsters.Count; i++)
        {
            if(activeMonsters[i] == target)
            {
                activeMonsters.Remove(activeMonsters[i]);
                monsterTurnList[i].Discard();
                monsterTurnList.RemoveAt(i);
            }
        }

        if(goFirst)
        {
            addToFront = true;
            monsterToAdd = target;
        }
        else if(goLast)
        {
            activeMonsters.Add(target);
            addTurn(target, false);
        }
    }

    public void ShootProjectile(monster usingMon, monster recivingMon, int projectileIndex, int uniqueSpawn)
    {
        if (fightingAI)
        {
            Transform target = recivingMon.ownerTransform;
            Transform spawn = usingMon.attackPoint;
            bool doIOwnThis = DoIOwnThis(usingMon);

            switch (uniqueSpawn)
            {
                case 2:
                    spawn = uniqueLocations[uniqueSpawn];
                    break;

                case 3:
                    spawn = (doIOwnThis) ? uniqueLocations[uniqueSpawn] : uniqueLocations[uniqueSpawn + 1];
                    break;

                case 5:
                    spawn = uniqueLocations[uniqueSpawn];
                    break;
            }

            usingMon.ShootProjectile(projectilePrefabs[projectileIndex], target, spawn);
        }
        else
        {
            this.photonView.RPC("ShootProjectileRPC", RpcTarget.AllBuffered, usingMon.isPlayer1(), usingMon.GetIndex(), recivingMon.isPlayer1(), recivingMon.GetIndex(), projectileIndex, uniqueSpawn);
        }
    }

    [PunRPC]
    void ShootProjectileRPC(bool isUsing, int usingIndex, bool isReciving, int recivingIndex, int projectileIndex, int uniqueSpawn) // 0 is from using mon // 1 is nothing
    {
        Transform target = GetMonster(isReciving, recivingIndex).ownerTransform;
        Transform spawn = GetMonster(isUsing, usingIndex).attackPoint;
        bool doIOwnThis = DoIOwnThis(isUsing);

        switch (uniqueSpawn)
        {
            case 2:
                spawn = uniqueLocations[uniqueSpawn];
                break;

            case 3:
                spawn = (doIOwnThis) ? uniqueLocations[uniqueSpawn] : uniqueLocations[uniqueSpawn + 1];
                break;

            case 5:
                spawn = uniqueLocations[uniqueSpawn];
                break;
        }

        GetMonster(isUsing, usingIndex).ShootProjectile(projectilePrefabs[projectileIndex], target, spawn);
    }

    #endregion
}

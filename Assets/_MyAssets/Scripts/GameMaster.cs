using Photon.Pun;
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

    [SerializeField] monsterAlly[] monsterAllies; // like each seperate script

    [Header("Targeting")]
    public monster selectedMonster;
    public monster targetedMonster;

    public Vector3 touchedPos;
    public bool bRendering;

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
            monsterAlly allyScript = newAlly.AddComponent<monsterAlly>();
            allyScript.Init(gameMenu.GetMyTeam()[i], this, redLine, greenLine, renderCamera);

            GameObject newEnemy = Instantiate(monsterPrefab, enemySpawns[i]);
            monsterBase enemyScript = newEnemy.AddComponent<monsterBase>();
            enemyScript.Init(gameMenu.GetEnemyTeam()[i], this, redLine, greenLine, renderCamera);
        }
    }

    [Header("Turn Order")]
    [SerializeField] Transform monsterTurnParent;
    [SerializeField] monsterTurnScript monsterTurnPrefab;
    [SerializeField] selectParticleScript selectParticlesPrefab;
    GameObject selectParticles;
    bool myTurn;

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
        selectParticles = newSelect.gameObject;
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
}

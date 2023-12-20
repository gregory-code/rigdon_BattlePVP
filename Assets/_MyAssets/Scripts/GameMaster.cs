using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviourPunCallbacks
{
    [Header("References")]
    [SerializeField] lineScript redLine;
    [SerializeField] lineScript greenLine;
    [SerializeField] Camera renderCamera;

    [Header("Targeting")]
    public monster selectedCritter;
    public monster targetedCritter;

    public Vector3 touchedPos;
    public bool bRendering;

    [Header("Team Info")]
    [SerializeField] private monster[] player1Team = new monster[3];
    [SerializeField] private monster[] player2Team = new monster[3];

    void Start()
    {
        
    }


    void Update()
    {
        
    }

    [Header("Turn Order")]
    [SerializeField] Transform monsterTurnParent;
    [SerializeField] monsterTurnScript monsterTurnPrefab;

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
        monsterTurnList[where].GetComponent<Animator>().SetTrigger("discard");
        Destroy(monsterTurnList[where], 0.5f);
        monsterTurnList.RemoveAt(where);
        activeMonsters.RemoveAt(where);
    }

    private void SortBySpeed()
    {
        activeMonsters.Clear();
        StatComparer comparer = new StatComparer();

        for (int i = 0; i < player1Team.Length; ++i)
        {
            activeMonsters.Add(player1Team[i]);
            activeMonsters.Add(player2Team[i]);
        }

        activeMonsters.Sort(comparer);

        foreach (monster mon in activeMonsters)
        {
            addTurn(mon);
        }
    }
}

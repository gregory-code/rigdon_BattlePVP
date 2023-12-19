using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviourPunCallbacks
{
    [Header("Targeting")]
    public monster selectedCritter;
    public monster targetedCritter;

    public Vector3 touchedPos;
    public bool bRendering;

    [Header("Turn Order Stuff")]
    public List<critter> activeMonsters = new List<critter>();

    void Start()
    {
        
    }


    void Update()
    {
        
    }
}

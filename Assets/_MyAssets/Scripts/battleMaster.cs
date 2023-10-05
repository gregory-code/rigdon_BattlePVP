using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public class battleMaster : MonoBehaviourPunCallbacks
{
    builderMenu BuilderMenu;

    [Header("Critters")]
    [SerializeField] critter[] critterCollection;

    [SerializeField] private critterBuild[] player1Team;
    [SerializeField] private critterBuild[] player2Team;

    [SerializeField] private string player1;
    [SerializeField] private string player2;

    private void Awake()
    {
        BuilderMenu = GameObject.Find("builder").GetComponent<builderMenu>();
    }

    public void setPlayerData(string player_1, string player_2) // MAKE SURE IT HAS THE PHOTONVIEW ASSET
    {
        player1 = player_1;
        player2 = player_2;

        BuilderMenu.setActiveTeam(0);
        player1Team = BuilderMenu.activeCritterTeam;

        sendMyTeamData();
    }

    private byte[] SerializeTeam(critterBuild obj)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, obj);
        return ms.ToArray();
    }

    private critterBuild DeserializeTeam(byte[] byteArray)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(byteArray);
        object obj = bf.Deserialize(ms);
        return (critterBuild)obj;
    }

    private void sendMyTeamData()
    {
        byte[] byteArray = SerializeTeam(player1Team[0]);
        string seralizedCritter1 = Convert.ToBase64String(byteArray);

        byte[] byte2Array = SerializeTeam(player1Team[1]);
        string seralizedCritter2 = Convert.ToBase64String(byte2Array);

        byte[] byte3Array = SerializeTeam(player1Team[2]);
        string seralizedCritter3 = Convert.ToBase64String(byte3Array);

        this.photonView.RPC("recieveEnemyCritterTeamRPC", RpcTarget.Others, seralizedCritter1, seralizedCritter2, seralizedCritter3);
    }

    [PunRPC]
    void recieveEnemyCritterTeamRPC(string enemyCritter1, string enemyCritter2, string enemyCritter3)
    {
        byte[] dataArray = Convert.FromBase64String(enemyCritter1);
        player2Team[0] = DeserializeTeam(dataArray);

        byte[] data2Array = Convert.FromBase64String(enemyCritter2);
        player2Team[1] = DeserializeTeam(data2Array);

        byte[] data3Array = Convert.FromBase64String(enemyCritter3);
        player2Team[2] = DeserializeTeam(data3Array);
    }
}

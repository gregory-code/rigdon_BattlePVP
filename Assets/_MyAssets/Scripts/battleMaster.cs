using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using System.IO;
using Unity.VisualScripting;

public class battleMaster : MonoBehaviourPunCallbacks
{
    builderMenu BuilderMenu;

    notifScript NotificationScript;

    [Header("Camera Shake")]
    float shakeTime;
    float shakePower;
    float shakeFade;
    float shakeRotation;
    [SerializeField] Camera mainCamera;

    [Header("Critters")]
    [SerializeField] critter[] allyCritterCollection;

    [SerializeField] private critter[] player1Team = new critter[3];
    [SerializeField] private critter[] player2Team = new critter[3];

    [SerializeField] private critterBuild proxyBuild; // for storing info to put into a function

    public GameObject critterPrefab;
    public GameObject enemyCritterPrefab;

    [SerializeField] private string player1;
    [SerializeField] private string player2;
    private bool bIsPlayer1;

    public Vector3 touchedPos;
    public bool bRendering;

    private bool bGameStart = false;

    private void Awake()
    {
        BuilderMenu = GameObject.Find("builder").GetComponent<builderMenu>();

        NotificationScript = GameObject.FindGameObjectWithTag("Canvas").GetComponent<notifScript>();

        bGameStart = true;
    }

    private void LateUpdate()
    {
        if (bGameStart == false) return;

        Vector3 cameraLerp = Vector3.Lerp(mainCamera.transform.position, new Vector3(0, 0, -100), 2 * Time.deltaTime);
        mainCamera.transform.position = cameraLerp;

        cameraShake();
    }

    private void cameraShake()
    {
        if (shakeTime > 0)
        {
            float xAmount = Random.Range(-1f, 1f) * shakePower;
            float yAmount = Random.Range(-1f, 1f) * shakePower;

            mainCamera.transform.position += new Vector3(xAmount, yAmount);
            mainCamera.transform.rotation = Quaternion.Euler(0f, 0f, shakeRotation * Random.Range(-1f, 1f));

            shakePower = Mathf.MoveTowards(shakePower, 0f, shakeFade * Time.deltaTime);
            shakeRotation = Mathf.MoveTowards(shakeRotation, 0f, shakeFade * 5 * Time.deltaTime);

            shakeTime -= 1;

            if (shakeTime <= 0)
            {
                mainCamera.transform.position =  new Vector3(0,0,-100);
                mainCamera.transform.rotation = Quaternion.Euler(0,0,0);
            }
        }
    }

    public void setCameraShake(float length, float power)
    {
        shakeTime = length;

        shakePower = power;

        shakeFade = power / length;

        shakeRotation = power * 5;
    }

    public void setPlayerData(string player_1, string player_2)
    {
        bGameStart = true;

        player1 = player_1;
        player2 = player_2;
        bIsPlayer1 = (PhotonNetwork.NickName == player_1) ? true : false ;
        int playerNum = (bIsPlayer1) ? 1 : 2 ;
        NotificationScript.createNotif($"Loaded as player {playerNum}", Color.green);

        BuilderMenu.setActiveTeam(0); // you can change this when selecting teams, it's set to 0
        sendMyTeam();

        for (int i = 0; i < 3; ++i) // gets my team
        {
            player1Team[i] = allyCritterCollection[BuilderMenu.activeCritterTeam[i].critterValue[0]];
            player1Team[i].SetFromCritterBuild(BuilderMenu.activeCritterTeam[i]);
        }

        InitalizeFight();
    }

    public void InitalizeFight()
    {
        for(int i = 0; i < 3; ++i)
        {
            GameObject critter = Instantiate(critterPrefab, transform.position, transform.rotation);
            critter.transform.SetParent(transform.Find("player" + (i+1) + "Spawn").transform);
            critter.transform.localPosition = Vector3.zero;
            critter.transform.localScale = new Vector3(1, 1, 1);
            critterController critterCon = critter.GetComponent<critterController>();
            critterCon.setCritter(player1Team[i]);

            GameObject enemyCritter = Instantiate(enemyCritterPrefab, transform.position, transform.rotation);
            enemyCritter.transform.SetParent(transform.Find("enemy" + (i+1) + "Spawn").transform);
            enemyCritter.transform.localPosition = Vector3.zero;
            enemyCritter.transform.localScale = new Vector3(1, 1, 1);
            enemyCritter enemyCon = enemyCritter.GetComponent<enemyCritter>();
            enemyCon.setCritter(player2Team[i]);
        }
    }

    private void sendMyTeam()
    {
        for (int i = 0; i < 3; ++i)
        {
            int[] critterValues = BuilderMenu.activeCritterTeam[i].critterValue;
            string critterNickname = BuilderMenu.activeCritterTeam[i].critterNickname;
            this.photonView.RPC("recieveEnemyCritterRPC", RpcTarget.OthersBuffered, i, critterValues, critterNickname); // First person to search is player 2
        }
    }

    [PunRPC]
    void recieveEnemyCritterRPC(int whichMember, int[] enemyValues, string enemyNickname)
    {
        player2Team[whichMember] = allyCritterCollection[enemyValues[0]]; // careful if you both have the same critter it might reference the same scriptable object

        proxyBuild.critterNickname = enemyNickname;
        for (int i = 0; i < proxyBuild.critterValue.Length; ++i)
        {
            proxyBuild.critterValue[i] = enemyValues[i];
        }

        player2Team[whichMember].SetFromCritterBuild(proxyBuild);
    }
}

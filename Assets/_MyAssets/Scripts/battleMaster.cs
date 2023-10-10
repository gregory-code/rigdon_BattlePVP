using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;
using UnityEditor.Rendering;

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
    [SerializeField] Sprite[] critterIcons;
    [SerializeField] critter[] allyCritterCollection;
    [SerializeField] critter[] enemyCritterCollection;

    [SerializeField] private critter[] player1Team = new critter[3];
    [SerializeField] private critter[] player2Team = new critter[3];

    [SerializeField] private critterBuild proxyBuild; // for storing info to put into a function

    [SerializeField] GameObject critterPrefab;
    [SerializeField] GameObject enemyCritterPrefab;

    [Header("Player Data")]
    [SerializeField] private string player1;
    [SerializeField] private string player2;
    private bool bIsPlayer1;

    public Vector3 touchedPos;
    public bool bRendering;

    private bool bGameStart = false;
    private bool bBlockFirstLerp = false;

    [Header("Turn Order")]
    [SerializeField] Transform spawnLocation;
    [SerializeField] GameObject turnOrderGameObject;
    [SerializeField] GameObject turnOrderPrefab;
    [SerializeField] Sprite friendlyTurnSprite;
    [SerializeField] Sprite enemyTurnSprite;

    public List<critter> allCritters = new List<critter>();

    [SerializeField] private List<GameObject> turnOrderObjectList = new List<GameObject>();

    [Header("Particles")]
    [SerializeField] GameObject selectParticles;
    [SerializeField] GameObject currentSelectParticles;

    [Header("Targeting")]
    public critter selectedCritter;
    public critter targetedCritter;

    private void Awake()
    {
        BuilderMenu = GameObject.Find("builder").GetComponent<builderMenu>();

        NotificationScript = GameObject.FindGameObjectWithTag("Canvas").GetComponent<notifScript>();
    }

    private void LateUpdate()
    {
        if (bGameStart == false) return;

        Vector3 cameraLerp = Vector3.Lerp(mainCamera.transform.position, new Vector3(0, 0, -100), 2 * Time.deltaTime);
        mainCamera.transform.position = cameraLerp;

        HandleTurnOrder();
        cameraShake();
    }

    private void HandleTurnOrder()
    {
        for(int i = 0; i < turnOrderObjectList.Count; ++i)
        {
            if(i == 0 && bBlockFirstLerp == false)
            {
                Vector3 highlightLerp = Vector3.Lerp(turnOrderObjectList[i].transform.localPosition, new Vector3(-235.4f, 7.23f, 0), 5 * Time.deltaTime);
                Vector3 sizeLerp = Vector3.Lerp(turnOrderObjectList[i].transform.localScale, new Vector3(1.359545f, 1.359545f, 1.359545f), 5 * Time.deltaTime);
                turnOrderObjectList[i].transform.localPosition = highlightLerp;
                turnOrderObjectList[i].transform.localScale = sizeLerp;
            }
            else if(i != 0)
            {
                Vector3 lerp = Vector3.Lerp(turnOrderObjectList[i].transform.localPosition, new Vector3(-138.6f + (69 * (i - 1)), 0, 0), 5 * Time.deltaTime);
                Vector3 size = Vector3.Lerp(turnOrderObjectList[i].transform.localScale, new Vector3(1, 1, 1), 5 * Time.deltaTime);
                turnOrderObjectList[i].transform.localPosition = lerp;
                turnOrderObjectList[i].transform.localScale = size;
            }
        }
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

        StartCoroutine(catchupDelay());
    }

    private IEnumerator catchupDelay()
    {
        yield return new WaitForSeconds(0.3f);
        InitalizeFight();
    }

    public void InitalizeFight()
    {
        for(int i = 0; i < 3; ++i)
        {
            GameObject critter = Instantiate(critterPrefab, transform.position, transform.rotation);
            critterController critterCon = critter.GetComponent<critterController>();
            critter.transform.SetParent(transform.Find("player" + (i+1) + "Spawn").transform);
            critter.transform.localPosition = Vector3.zero;
            critter.transform.localScale = new Vector3(1, 1, 1);
            critterCon.setCritter(player1Team[i]);
            critterCon.myCritter.bMine = true;
            critterCon.myCritter.teamPlacement = i;

            GameObject enemyCritter = Instantiate(enemyCritterPrefab, transform.position, transform.rotation);
            enemyCritter enemyCon = enemyCritter.GetComponent<enemyCritter>();
            enemyCritter.transform.SetParent(transform.Find("enemy" + (i+1) + "Spawn").transform);
            enemyCritter.transform.localPosition = Vector3.zero;
            enemyCritter.transform.localScale = new Vector3(1, 1, 1);
            enemyCon.setCritter(player2Team[i]);
            enemyCon.myCritter.bMine = false;
            enemyCon.myCritter.teamPlacement = i;
        }

        SortBySpeed();
        selectNew();
    }

    private void SortBySpeed()
    {
        allCritters.Clear();
        StatComparer comparer = new StatComparer();
        for (int i = 0; i < player1Team.Length; ++i)
        {
            allCritters.Add(player1Team[i]);
            allCritters.Add(player2Team[i]);
        }
        allCritters.Sort(comparer);

        foreach (critter c in allCritters)
        {
            addToTurnOrder(c);
        }
    }

    private void addToTurnOrder(critter c)
    {
        GameObject newTurnOrder = Instantiate(turnOrderPrefab);
        newTurnOrder.transform.localScale = new Vector3(1, 1, 1);
        newTurnOrder.transform.SetParent(turnOrderGameObject.transform);
        newTurnOrder.transform.localPosition = spawnLocation.localPosition;
        newTurnOrder.GetComponent<Image>().sprite = (c.bMine) ? friendlyTurnSprite : enemyTurnSprite;
        newTurnOrder.transform.Find("critterGraphic").GetComponent<Image>().sprite = critterIcons[c.GetCritterID()]; // check levels to see if it's a higher evolution
        turnOrderObjectList.Add(newTurnOrder);
    }

    private void selectNew()
    {
        GameObject newSelect = Instantiate(selectParticles);
        string team = (allCritters[0].bMine) ? "player" : "enemy" ;
        newSelect.transform.SetParent(GameObject.Find(team + (allCritters[0].teamPlacement + 1) + "Spawn").transform);
        newSelect.transform.localPosition = selectParticles.transform.localPosition;
        newSelect.transform.localScale = selectParticles.transform.localScale;

        ParticleSystem system = newSelect.GetComponent<ParticleSystem>();
        var mainModule = system.main;
        mainModule.startColor = allCritters[0].matchingColor;

        foreach(Transform t in newSelect.transform)
        {
            ParticleSystem newSystem = t.GetComponent<ParticleSystem>();
            var module = newSystem.main;
            module.startColor = allCritters[0].matchingColor;
        }

        currentSelectParticles = newSelect;
    }

    public void disappearSelectParticles()
    {
        currentSelectParticles.GetComponent<Animator>().SetTrigger("disappear");
        Destroy(currentSelectParticles, 0.5f);
    }

    public void NextTurn()
    {
        this.photonView.RPC("nextTurnRPC", RpcTarget.AllBuffered);
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
    void nextTurnRPC()
    {
        disappearSelectParticles();
        turnOrderObjectList[0].GetComponent<Animator>().SetTrigger("discard");
        Destroy(turnOrderObjectList[0], 0.5f);
        turnOrderObjectList.RemoveAt(0);
        critter deletedCritter = allCritters[0];
        allCritters.RemoveAt(0);
        for (int i = 0; i < player1Team.Length; ++i)
        {
            if (player1Team[i] == deletedCritter)
            {
                allCritters.Add(player1Team[i]);
                addToTurnOrder(player1Team[i]);
                break;
            }

            if (player2Team[i] == deletedCritter)
            {
                allCritters.Add(player2Team[i]);
                addToTurnOrder(player2Team[i]);
                break;
            }
        }
        selectNew();
    }

    [PunRPC]
    void recieveEnemyCritterRPC(int whichMember, int[] enemyValues, string enemyNickname)
    {
        player2Team[whichMember] = enemyCritterCollection[enemyValues[0]]; // careful if you both have the same critter it might reference the same scriptable object

        proxyBuild.critterNickname = enemyNickname;
        for (int i = 0; i < proxyBuild.critterValue.Length; ++i)
        {
            proxyBuild.critterValue[i] = enemyValues[i];
        }

        player2Team[whichMember].SetFromCritterBuild(proxyBuild);
    }
}

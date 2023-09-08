using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Playables;

public class DataPersistenceManager : MonoBehaviour
{
    [Header("File Storage Config")]
    [SerializeField] private string fileName;

    public GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    public FileDataHandler dataHandler;

    public static DataPersistenceManager instance { get; private set; }

    private menuScript MS;


    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("Found more than one data persistence manager");
        }

        instance = this;
    }

    private void Start()
    {

        /*this.dataPersistenceObjects = FindAllDataPersistenceObjects();

        this.dataHandler = new FileDataHandler(Application.dataPath, fileName);

        MS = GameObject.FindGameObjectWithTag("menu").GetComponent<menuScript>();

        loadGame();*/
    }

    public void loadGame()
    {
        this.gameData = dataHandler.Load();

        if (this.gameData == null)
        {
            Debug.Log("No game data");
            return;
        }

        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            this.gameData = dataHandler.Load();
            dataPersistenceObj.LoadData(gameData);
        }
    }

    public void saveGame()
    {
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            gameData = dataPersistenceObj.SaveData(gameData);
        }

        dataHandler.Save(gameData);
    }

    private void OnApplicationQuit()
    {
        //saveGame();
    }

    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistenceObjects);
    }
}

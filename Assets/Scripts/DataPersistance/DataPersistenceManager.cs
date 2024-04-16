using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DataPersistenceManager : MonoBehaviour
{
    public static DataPersistenceManager instance { get; private set; }

    [Header("File Storage Config")]
    public string fileName;
    [SerializeField] bool useEncryption;

    GameData gameData;
    List<IDataPersistence> dataPersistanceObjects;
    FileDataHandler dataHandler;

    private void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("DataPersistenceManager");

        if (objs.Length > 1)
        {
            //DataPersistenceManager.instance.LoadGame();
            Destroy(gameObject);
            return;
        }

        //DontDestroyOnLoad(gameObject);

        instance = this;
    }

    void OnEnable()
    {
        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
        dataPersistanceObjects = FindAllDataPersistanceObjects();
        LoadGame();
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public void LoadGame()
    {
        gameData = dataHandler.Load();

        if (this.gameData == null) NewGame();

        foreach (var dataPersistanceObject in dataPersistanceObjects)
        {
            dataPersistanceObject.LoadData(gameData);
        }
    }

    public void SaveGame()
    {
        foreach (var dataPersistanceObject in dataPersistanceObjects)
        {
            dataPersistanceObject.SaveData(ref gameData);
        }

        dataHandler.Save(gameData);
    }

    List<IDataPersistence> FindAllDataPersistanceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistanceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistanceObjects);
    }
}

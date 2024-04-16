using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

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
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);

        dataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
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

        if (this.gameData == null) return;

        foreach (var dataPersistanceObject in dataPersistanceObjects)
        {
            dataPersistanceObject.LoadData(gameData);
        }
    }

    public void SaveGame()
    {
        if (this.gameData == null) return;

        foreach (var dataPersistanceObject in dataPersistanceObjects)
        {
            dataPersistanceObject.SaveData(gameData);
        }

        dataHandler.Save(gameData);
    }

    List<IDataPersistence> FindAllDataPersistanceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistanceObjects = FindObjectsOfType<MonoBehaviour>(true).OfType<IDataPersistence>();
        return new List<IDataPersistence>(dataPersistanceObjects);
    }
}

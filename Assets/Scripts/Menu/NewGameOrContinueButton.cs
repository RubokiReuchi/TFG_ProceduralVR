using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameOrContinueButton : MenuButton
{
    [SerializeField] TextMeshPro text;

    bool newGame = true;

    void Start()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, DataPersistenceManager.instance.fileName);
        if (File.Exists(fullPath))
        {
            text.text = "Continue";
            newGame = false;
        }
    }

    public override void ButtonHitted()
    {
        if (newGame)
        {
            DataPersistenceManager.instance.NewGame();
            DataPersistenceManager.instance.SaveGame();
        }

        SceneManager.LoadScene(0);
    }
}

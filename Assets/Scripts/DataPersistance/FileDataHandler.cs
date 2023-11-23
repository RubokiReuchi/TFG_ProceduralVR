using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.Profiling;
using Unity.VisualScripting;

public class FileDataHandler
{
    string dataDirPath = "";
    string dataFileName = "";

    bool useEncryption = false;

    readonly string encryptionCodeWord = "encryption";
    readonly string backpExtension = ".bak";

    public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEncryption;
    }

    public GameData Load(bool allowRestoreFromBackip = true)
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        GameData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";

                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                if (useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                if (allowRestoreFromBackip)
                {
                    Debug.LogWarning("Failed to load data file. Attempting to roll back.\n" + e);
                    bool rollbackSuccess = AttemptRollBack(fullPath);
                    if (rollbackSuccess)
                    {
                        loadedData = Load(false);
                    }
                }
                else
                {
                    Debug.LogError("Error occured when trying to load file at path: " + fullPath + " and backup did not work.\n" + e);
                }
            }
        }
        return loadedData;
    }

    public void Save(GameData data)
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);
        string backupPath = fullPath + backpExtension;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToStore = JsonUtility.ToJson(data, true);

            if (useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }

            GameData verifiedGameData = Load();
            if (verifiedGameData != null)
            {
                File.Copy(fullPath, backupPath, true);
            }
            else
            {
                throw new Exception("Save file cound not be verified and backup could not be created.");
            }
        }
        catch (Exception e)
        {

            Debug.LogError("Error ocurred when trying to save data to file: " + fullPath + "\n" + e);
        }
    }

    string EncryptDecrypt(string data)
    {
        string modifiedData = "";
        for (int i = 0; i < data.Length; i++)
        {
            modifiedData += (char)(data[i] ^ encryptionCodeWord[i % encryptionCodeWord.Length]);
        }
        return modifiedData;
    }

    bool AttemptRollBack(string fullPath)
    {
        bool success = false;
        string backupPath = fullPath + backpExtension;
        try
        {
            if (File.Exists(backupPath))
            {
                File.Copy(backupPath, fullPath, true);
                success = true;
                Debug.LogWarning("Had to roll back to backup file at: " + backupPath);
            }
            else
            {
                throw new Exception("Tried to roll back, but no backup file exists to roll back to.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to roll back to backup file at: " + backupPath + "\n" + e);
        }

        return success;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;

    [SerializeField] GameObject schemePane;
    [SerializeField] TextMeshProUGUI[] volumeValues;

    [SerializeField] GameObject leftArrow;
    [SerializeField] GameObject rightArrow;

    [Header("File Storage Config")]
    public string fileName;
    string fullPath;
    ConfigData configData = null;

    AudioManager audioManager;

    void Awake()
    {
        fullPath = Path.Combine(Application.persistentDataPath, fileName);
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

                configData = JsonUtility.FromJson<ConfigData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError("Error occured when trying to load file at path: " + fullPath + " data will be reset.\n" + e);
            }
        }
        if (configData == null)
        {
            configData = new ConfigData();

            SaveConfigData();
        }
    }

    void Start()
    {
        audioManager = AudioManager.instance;

        mixer.SetFloat("MasterVolume", Mathf.Log10(configData.master) * 20);
        volumeValues[0].text = (configData.master * 100).ToString();
        mixer.SetFloat("MusicVolume", Mathf.Log10(configData.music) * 20);
        volumeValues[1].text = (configData.music * 100).ToString();
        mixer.SetFloat("EfectsVolume", Mathf.Log10(configData.sfx) * 20);
        volumeValues[2].text = (configData.sfx * 100).ToString();
        mixer.SetFloat("ProjectileVolume", Mathf.Log10(configData.projectiles) * 20);
        volumeValues[3].text = (configData.projectiles * 100).ToString();
        mixer.SetFloat("EnemiesVolume", Mathf.Log10(configData.enemies) * 20);
        volumeValues[4].text = (configData.enemies * 100).ToString();
        mixer.SetFloat("EnviromentVolume", Mathf.Log10(configData.enviroment) * 20);
        volumeValues[5].text = (configData.enviroment * 100).ToString();
    }

    public void OpenScheme()
    {
        schemePane.SetActive(true);
        schemePane.GetComponent<Animator>().SetBool("Opened", true);

        leftArrow.SetActive(false);
        rightArrow.SetActive(false);

        audioManager.PlaySound("SelectOption");
    }

    public void CloseScheme()
    {
        StartCoroutine(CloseScheme_Co());

        leftArrow.SetActive(true);
        rightArrow.SetActive(true);

        audioManager.PlaySound("CancelOption");
    }

    IEnumerator CloseScheme_Co()
    {
        schemePane.GetComponent<Animator>().SetBool("Opened", false);
        yield return new WaitForSeconds(0.3f);
        schemePane.SetActive(false);
    }

    // 0 --> master, 1 --> musci, 2 --> sfx, 3 --> projectiles,  4 --> enemies, 5 --> enviroment
    public void IncreaseVolume(int mixerGroup)
    {
        float currentValue = float.Parse(volumeValues[mixerGroup].text) / 100.0f;
        if (currentValue == 1.0f) return;

        volumeValues[mixerGroup].text = ((currentValue + 0.05f) * 100.0f).ToString();
        switch (mixerGroup)
        {
            case 0:
                configData.master = currentValue + 0.05f;
                mixer.SetFloat("MasterVolume", Mathf.Log10(configData.master) * 20);
                break;
            case 1:
                configData.music = currentValue + 0.05f;
                mixer.SetFloat("MusicVolume", Mathf.Log10(configData.music) * 20);
                break;
            case 2:
                configData.sfx = currentValue + 0.05f;
                mixer.SetFloat("EfectsVolume", Mathf.Log10(configData.sfx) * 20);
                break;
            case 3:
                configData.projectiles = currentValue + 0.05f;
                mixer.SetFloat("ProjectileVolume", Mathf.Log10(configData.projectiles) * 20);
                break;
            case 4:
                configData.enemies = currentValue + 0.05f;
                mixer.SetFloat("EnemiesVolume", Mathf.Log10(configData.enemies) * 20); break;
            case 5:
                configData.enviroment = currentValue + 0.05f;
                mixer.SetFloat("EnviromentVolume", Mathf.Log10(configData.enviroment) * 20);
                break;
            default:
                Debug.LogError("Value Wrong Asigned");
                break;
        }
        SaveConfigData();

        audioManager.PlaySound("SelectOption");
    }

    public void DecreaseVolume(int mixerGroup)
    {
        float currentValue = float.Parse(volumeValues[mixerGroup].text) / 100.0f;
        if (currentValue == 0.0f) return;

        volumeValues[mixerGroup].text = ((currentValue - 0.05f) * 100.0f).ToString();
        switch (mixerGroup)
        {
            case 0:
                configData.master = currentValue - 0.05f;
                mixer.SetFloat("MasterVolume", (configData.master > 0.0f) ? Mathf.Log10(configData.master) * 20 : -80);
                break;
            case 1:
                configData.music = currentValue - 0.05f;
                mixer.SetFloat("MusicVolume", (configData.music > 0.0f) ? Mathf.Log10(configData.music) * 20 : -80);
                break;
            case 2:
                configData.sfx = currentValue - 0.05f;
                mixer.SetFloat("EfectsVolume", (configData.sfx > 0.0f) ? Mathf.Log10(configData.sfx) * 20 : -80);
                break;
            case 3:
                configData.projectiles = currentValue - 0.05f;
                mixer.SetFloat("ProjectileVolume", (configData.projectiles > 0.0f) ? Mathf.Log10(configData.projectiles) * 20 : -80);
                break;
            case 4:
                configData.enemies = currentValue - 0.05f;
                mixer.SetFloat("EnemiesVolume", (configData.enemies > 0.0f) ? Mathf.Log10(configData.enemies) * 20 : -80);
                break;
            case 5:
                configData.enviroment = currentValue - 0.05f;
                mixer.SetFloat("EnviromentVolume", (configData.enviroment > 0.0f) ? Mathf.Log10(configData.enviroment) * 20 : -80);
                break;
            default:
                Debug.LogError("Value Wrong Asigned");
                break;
        }
        SaveConfigData();

        audioManager.PlaySound("SelectOption");
    }

    void SaveConfigData()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

        string dataToStore = JsonUtility.ToJson(configData, true);

        using (FileStream stream = new FileStream(fullPath, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(dataToStore);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConfigData
{
    public float master;
    public float music;
    public float sfx;
    public float projectiles;
    public float enemies;
    public float enviroment;

    // new data values
    public ConfigData()
    {
        master = 1;
        music = 1;
        sfx = 1;
        projectiles = 1;
        enemies = 1;
        enviroment = 1;
    }
}

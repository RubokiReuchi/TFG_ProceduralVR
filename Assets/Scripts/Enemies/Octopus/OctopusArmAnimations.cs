using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctopusArmAnimations : MonoBehaviour
{
    [SerializeField] Octopus script;
    [SerializeField] bool commanderArm;

    public void Idle()
    {
        if (commanderArm) script.Idle();
    }

    public void SpawnMeteorite()
    {
        if (commanderArm) script.SpawnMeteorite();
    }

    public void SpawnNuke()
    {
        if (commanderArm) script.SpawnNuke();
    }

    public void StartSlowdownRings()
    {
        if (commanderArm) script.StartSlowdownRings();
    }

    public void SpawnSonnar()
    {
        if (commanderArm) script.SpawnSonnar();
    }
}

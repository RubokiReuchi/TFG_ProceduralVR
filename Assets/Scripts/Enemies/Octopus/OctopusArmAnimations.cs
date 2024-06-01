using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctopusArmAnimations : MonoBehaviour
{
    [SerializeField] Octopus script;
    public bool commanderArm;
    [SerializeField] GameObject homingBomb;
    [SerializeField] Transform projectileOrigin;
    [SerializeField] ParticleSystem launchHomingBombPs;
    [SerializeField] ParticleSystem launchRainPs;

    public void IdleAllRows()
    {
        if (commanderArm) script.Idle();
    }

    public void IdleFirstRow()
    {
        if (commanderArm) script.Idle(true, false, false, false);
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

    public void SpawnHomingBomb()
    {
        GameObject.Instantiate(homingBomb, projectileOrigin.position, projectileOrigin.rotation);
        launchHomingBombPs.Play();
    }

    public void CheckHomingBombLoop()
    {
        if (commanderArm && Random.Range(0, 3) == 0) script.Idle(true, false, false, false);
    }

    public void SpawnRain()
    {
        script.SpawnRain();
        launchRainPs.Play();
    }

    public void CheckRainLoop()
    {
        if (commanderArm && Random.Range(0, 3) == 0)
        {
            script.Idle(false, true, false, false);
            commanderArm = false;
        }
    }
}

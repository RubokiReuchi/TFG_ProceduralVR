using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctopusArmAnimations : MonoBehaviour
{
    [SerializeField] Octopus script;
    public bool commanderArm;
    float currentLoop = 0;
    [SerializeField] GameObject homingBomb;
    [SerializeField] Transform projectileOrigin;
    [SerializeField] ParticleSystem launchHomingBombPs;
    [SerializeField] float maxHomingLoops;
    [SerializeField] ParticleSystem launchRainPs;
    [SerializeField] float maxRainLoops;
    [SerializeField] ParticleSystem launchMinionPs;
    [SerializeField] float maxMinionLoops;

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
        if (commanderArm)
        {
            if (Random.Range(0, 3) == 0 || currentLoop == maxHomingLoops)
            {
                currentLoop = 0;
                script.Idle(true, false, false, false);
                commanderArm = false;
            }
            else currentLoop++;
        }
    }

    public void SpawnRain()
    {
        script.SpawnRain();
        launchRainPs.Play();
    }

    public void CheckRainLoop()
    {
        if (commanderArm)
        {
            if (Random.Range(0, 3) == 0 || currentLoop == maxRainLoops)
            {
                currentLoop = 0;
                script.Idle(false, true, false, false);
                commanderArm = false;
            }
            else currentLoop++;
        }
    }

    public void SpawnMinion()
    {
        script.SpawnMinion(projectileOrigin);
        launchMinionPs.Play();
    }

    public void CheckMinionLoop()
    {
        if (commanderArm)
        {
            if (Random.Range(0, 2) == 0 || currentLoop == maxMinionLoops)
            {
                currentLoop = 0;
                script.Idle(false, false, false, true);
                commanderArm = false;
            }
            else currentLoop++;
        }
    }

    public void CollectMinionWave()
    {
        if (commanderArm) script.CollectMinionWave();
    }

    public void LaunchMinionWave()
    {
        if (commanderArm)
        {
            script.Idle();
            script.LaunchMinionWave();
        }
    }
}

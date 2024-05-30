using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctopusArmAnimations : MonoBehaviour
{
    [SerializeField] Octopus script;
    [SerializeField] bool commanderArm;
    [SerializeField] GameObject homingBomb;
    [SerializeField] Transform homingBombOrigin;
    [SerializeField] ParticleSystem launchHomingBombPs;

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

    public void SpawnHomingBomb()
    {
        GameObject.Instantiate(homingBomb, homingBombOrigin.position, homingBombOrigin.rotation);
        launchHomingBombPs.Play();
    }

    public void CheckHomingBombLoop()
    {
        if (commanderArm && Random.Range(0, 3) == 0) script.Idle();
    }
}

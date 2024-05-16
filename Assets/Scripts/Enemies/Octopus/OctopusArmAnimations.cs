using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctopusArmAnimations : MonoBehaviour
{
    [SerializeField] Octopus script;

    public void Idle()
    {
        script.Idle();
    }

    public void SpawnMeteorite()
    {
        script.SpawnMeteorite();
    }
}

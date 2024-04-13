using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnimSphereRobot : MonoBehaviour
{
    SphereRobot script;
    [SerializeField] ParticleSystem shootPs;

    // Start is called before the first frame update
    void Start()
    {
        script = transform.parent.GetComponent<SphereRobot>();
    }

    public void CheckOtions()
    {
        if (script) script.StartCheckOptions();
    }

    public void SpawnRay()
    {
        script.SpawnRay();
    }

    public void Stop()
    {
        script.Stop();
    }

    public void Destroyed()
    {
        script.Destroyed();
    }

    public void RollSpeed()
    {
        script.RollSpeed();
    }

    public void PlayRing()
    {
        shootPs.Play();
    }

    public void StopRing()
    {
        shootPs.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }
}

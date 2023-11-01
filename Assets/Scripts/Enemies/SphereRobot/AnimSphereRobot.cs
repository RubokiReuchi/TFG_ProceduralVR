using System.Collections;
using System.Collections.Generic;
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
        script.StartCheckOptions();
    }

    public void SpawnRay()
    {
        script.SpawnRay();
    }

    public void Stop()
    {
        script.Stop();
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

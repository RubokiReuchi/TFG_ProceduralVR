using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockedGate : MonoBehaviour
{
    [SerializeField] ParticleSystem psClose;
    [SerializeField] ParticleSystem psOpen;

    public void PlayCloseParticles()
    {
        psClose.Play();
    }

    public void PlayOpenParticles()
    {
        psOpen.Play();
    }

    public void StopOpenParticles()
    {
        psOpen.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

}

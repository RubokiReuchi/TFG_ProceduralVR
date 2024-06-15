using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockedGate : MonoBehaviour
{
    [SerializeField] ParticleSystem psClose;
    [SerializeField] ParticleSystem psOpen;
    [SerializeField] AudioSource closeSource;
    [SerializeField] AudioSource openSource;

    public void PlayCloseParticles()
    {
        psClose.Play();
    }

    public void PlayOpenParticles()
    {
        psOpen.Play();
        openSource.Play();
    }

    public void StopOpenParticles()
    {
        psOpen.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void PlayCloseSound()
    {
        closeSource.Play();
    }
}

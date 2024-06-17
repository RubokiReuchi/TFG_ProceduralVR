using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctopusSonnar : MonoBehaviour
{
    [SerializeField] ParticleSystem ps;
    [SerializeField] Animator animator;

    [Header("Audio")]
    AudioSource waveSource;

    void Start()
    {
        waveSource = GetComponent<AudioSource>();
    }

    public void LaunchWave()
    {
        ps.Play();
        waveSource.Play();
    }

    public void DisableAnimator()
    {
        animator.SetBool("Enabled", false);
    }
}

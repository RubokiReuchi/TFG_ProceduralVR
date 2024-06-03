using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctopusSonnar : MonoBehaviour
{
    [SerializeField] ParticleSystem ps;
    [SerializeField] Animator animator;

    public void LaunchWave()
    {
        ps.Play();
    }

    public void DisableAnimator()
    {
        animator.SetBool("Enabled", false);
    }
}

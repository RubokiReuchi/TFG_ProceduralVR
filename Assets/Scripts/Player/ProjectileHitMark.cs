using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ProjectileHitMark : MonoBehaviour
{
    VisualEffect vfx;

    // Start is called before the first frame update
    void OnEnable()
    {
        vfx = GetComponent<VisualEffect>();
        vfx.Play();
    }

    void Update()
    {
        if (vfx.aliveParticleCount == 0) Destroy(gameObject);
    }
}

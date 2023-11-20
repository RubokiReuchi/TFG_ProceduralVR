using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ProjectileHitMark : MonoBehaviour
{
    VisualEffect vfx;
    float lifeTime = 2.0f;

    // Start is called before the first frame update
    void OnEnable()
    {
        vfx = GetComponent<VisualEffect>();
        vfx.Play();
    }

    void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime < 0)
        {
            Destroy(gameObject);
        }
    }
}

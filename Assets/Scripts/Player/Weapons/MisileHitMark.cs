using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MisileHitMark : MonoBehaviour
{
    float lifeTime = 2.0f;
    bool stopped = false;

    // Update is called once per frame
    void Update()
    {
        lifeTime -= Time.deltaTime;
        if (!stopped && lifeTime < 0.6f)
        {
            stopped = true;
            GetComponent<ParticleSystem>().Stop();
        }
        if (lifeTime < 0) Destroy(gameObject);
    }
}

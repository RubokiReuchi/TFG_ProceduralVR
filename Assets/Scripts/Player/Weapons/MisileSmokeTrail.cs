using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class missileSmokeTrail : MonoBehaviour
{
    float lifeTime = 6;

    // Update is called once per frame
    void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0) Destroy(gameObject);
    }
}

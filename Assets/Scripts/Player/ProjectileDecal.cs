using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ProjectileDecal : MonoBehaviour
{
    DecalProjector decal;
    [SerializeField] float speed;
    float lifeTime = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        decal = GetComponent<DecalProjector>();
    }

    // Update is called once per frame
    void Update()
    {
        lifeTime -= Time.deltaTime * speed;
        if (lifeTime < 1.0f) decal.fadeFactor = lifeTime;
        if (lifeTime < 0)
        {
            Destroy(transform.root.gameObject);
        }
    }
}

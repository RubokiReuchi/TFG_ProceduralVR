using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ProjectileDecal : MonoBehaviour
{
    [SerializeField] float speed;
    float lifeTime = 2.0f;
    Material material;

    // Start is called before the first frame update
    void Start()
    {
        material = new Material(GetComponent<Renderer>().material);
        GetComponent<Renderer>().material = material;
    }

    // Update is called once per frame
    void Update()
    {
        lifeTime -= Time.deltaTime * speed;
        if (lifeTime < 1.0f) material.SetFloat("_Alpha", lifeTime);
        if (lifeTime < 0)
        {
            Destroy(transform.root.gameObject);
        }
    }
}

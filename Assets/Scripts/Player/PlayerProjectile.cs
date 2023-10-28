using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public GUN_TYPE selectedGunType;
    Rigidbody rb;
    [SerializeField] float speed;
    float lifeTime = 2;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
    }

    void Update()
    {
        rb.velocity = transform.forward * speed;
        lifeTime -= Time.deltaTime;
        if (lifeTime < 0) Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // gun type changes
        Destroy(this.gameObject);
    }
}

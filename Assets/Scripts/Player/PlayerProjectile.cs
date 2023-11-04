using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerProjectile : Projectile
{
    public GUN_TYPE selectedGunType;
    [SerializeField] GameObject decal;

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Foundations"))
        {
            GameObject.Instantiate(decal, new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.01f), transform.rotation);
        }
        Destroy(this.gameObject);
    }
}

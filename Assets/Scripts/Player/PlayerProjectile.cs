using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerProjectile : Projectile
{
    public GUN_TYPE selectedGunType;
    [SerializeField] float damage;
    [SerializeField] GameObject decal;
    [SerializeField] GameObject hitMark;

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
        if (collision.gameObject.CompareTag("FoundationsF") || collision.gameObject.CompareTag("FoundationsW"))
        {
            Physics.Raycast(transform.position, transform.forward, out RaycastHit hit);
            GameObject.Instantiate(decal, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal) * Quaternion.AngleAxis(90, Vector3.right));
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            GameObject.Instantiate(hitMark, collision.contacts[0].point, Quaternion.identity);
            Enemy script = collision.gameObject.GetComponent<Enemy>();
            if (script.enabled) script.TakeDamage(damage);
        }
        Destroy(this.gameObject);
    }
}

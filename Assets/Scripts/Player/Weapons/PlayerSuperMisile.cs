using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSuperMisile : Projectile
{
    [SerializeField] float delay;
    [SerializeField] float damage;
    [SerializeField] GameObject decal;
    [SerializeField] GameObject smokeHitMark;
    [SerializeField] GameObject hitMark;
    bool playVFXs = false;
    float cdVFX = 0;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * 0.1f;
    }

    void Update()
    {
        if (delay > 0)
        {
            delay -= Time.deltaTime;
            delay -= Time.deltaTime;
            if (delay <= 0) playVFXs = true;
            else return;
        }
        
        if (playVFXs)
        {
            cdVFX -= Time.deltaTime;
            if (cdVFX <= 0)
            {
                GameObject.Instantiate(hitMark, transform.position, Quaternion.identity);
                cdVFX = 0.1f;
            }
        }

        rb.velocity += transform.forward * speed;
        lifeTime -= Time.deltaTime;
        if (lifeTime < 0) Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("FoundationsF") || collision.gameObject.CompareTag("FoundationsW"))
        {
            Physics.Raycast(transform.position, transform.forward, out RaycastHit hit);
            GameObject.Instantiate(decal, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal) * Quaternion.AngleAxis(90, Vector3.right));
            GameObject.Instantiate(smokeHitMark, collision.contacts[0].point, Quaternion.identity);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            GameObject.Instantiate(hitMark, collision.contacts[0].point, Quaternion.identity);
            GameObject.Instantiate(smokeHitMark, collision.contacts[0].point, Quaternion.identity);
            Enemy script = collision.gameObject.GetComponent<Enemy>();
            if (script.enabled) script.TakeDamage(damage);
        }
        Destroy(this.gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerProjectile : Projectile
{
    [SerializeField] float damage;
    [SerializeField] GameObject decal;
    [SerializeField] GameObject hitMark;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * speed;
    }

    void Update()
    {
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
            if (script.enabled)
            {
                script.TakeDamage(damage);
                if (gameObject.CompareTag("BlueProjectile")) script.TakeFreeze(damage);
            }
        }
        else if (collision.gameObject.CompareTag("EnemyShield"))
        {
            GameObject.Instantiate(hitMark, collision.contacts[0].point, Quaternion.identity);
            float finalDamage = !gameObject.CompareTag("RedProjectile") ? damage / 10.0f : damage;
            EnemyShield script = collision.gameObject.GetComponent<EnemyShield>();
            if (script.enabled) script.TakeDamage(finalDamage);
        }
        else if (collision.gameObject.CompareTag("PowerChecker"))
        {
            GameObject.Instantiate(hitMark, collision.contacts[0].point, Quaternion.identity);
            PowerChecker script = collision.gameObject.GetComponent<PowerChecker>();
            if (script.enabled)
            {
                script.TakeDamage(damage);
                if (gameObject.CompareTag("BlueProjectile")) script.TakeFreeze(damage);
            }
        }
        else if (collision.gameObject.CompareTag("DeflectProjectile"))
        {
            Vector3 deflectDirection = Vector3.Reflect(transform.forward, collision.contacts[0].normal);
            transform.forward = deflectDirection;
            return;
        }
        Destroy(this.gameObject);
    }
}

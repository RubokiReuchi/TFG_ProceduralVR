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
    bool shockwave = false;
    [SerializeField] GameObject shockwavePrefab;

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
            if (shockwave)
            {
                PlayerShockwave shockwave = GameObject.Instantiate(shockwavePrefab, collision.contacts[0].point, Quaternion.identity).GetComponent<PlayerShockwave>();
                shockwave.SetDamage(damage / 2.0f);
            }
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            GameObject.Instantiate(hitMark, collision.contacts[0].point, Quaternion.identity);
            GameObject.Instantiate(smokeHitMark, collision.contacts[0].point, Quaternion.identity);
            Enemy script = collision.gameObject.GetComponent<Enemy>();
            if (script.enabled)
            {
                script.TakeDamage(damage);
                if (gameObject.CompareTag("BlueProjectile")) script.TakeFreeze(damage);
            }
            if (shockwave)
            {
                PlayerShockwave shockwave = GameObject.Instantiate(shockwavePrefab, collision.contacts[0].point, Quaternion.identity).GetComponent<PlayerShockwave>();
                shockwave.SetDamage(damage / 2.0f);
            }
        }
        else if (collision.gameObject.CompareTag("EnemyShield"))
        {
            GameObject.Instantiate(hitMark, collision.contacts[0].point, Quaternion.identity);
            GameObject.Instantiate(smokeHitMark, collision.contacts[0].point, Quaternion.identity);
            float finalDamage = !gameObject.CompareTag("RedProjectile") ? damage / 10.0f : damage;
            EnemyShield script = collision.gameObject.GetComponent<EnemyShield>();
            if (script.enabled) script.TakeDamage(finalDamage);
            if (shockwave)
            {
                PlayerShockwave shockwave = GameObject.Instantiate(shockwavePrefab, collision.contacts[0].point, Quaternion.identity).GetComponent<PlayerShockwave>();
                shockwave.SetDamage(damage / 2.0f);
            }
        }
        else if (collision.gameObject.CompareTag("PowerChecker"))
        {
            GameObject.Instantiate(hitMark, collision.contacts[0].point, Quaternion.identity);
            GameObject.Instantiate(smokeHitMark, collision.contacts[0].point, Quaternion.identity);
            PowerChecker script = collision.gameObject.GetComponent<PowerChecker>();
            if (script.enabled)
            {
                script.TakeDamage(damage);
                if (gameObject.CompareTag("BlueProjectile")) script.TakeFreeze(damage);
            }
            if (shockwave)
            {
                PlayerShockwave shockwave = GameObject.Instantiate(shockwavePrefab, collision.contacts[0].point, Quaternion.identity).GetComponent<PlayerShockwave>();
                shockwave.SetDamage(damage / 2.0f);
            }
        }
        else if (collision.gameObject.CompareTag("DeflectProjectile"))
        {
            Vector3 deflectDirection = Vector3.Reflect(transform.forward, collision.contacts[0].normal);
            transform.forward = deflectDirection;
            return;
        }
        else if (collision.gameObject.CompareTag("IceSpike"))
        {
            GameObject.Instantiate(hitMark, collision.contacts[0].point, Quaternion.identity);
            GameObject.Instantiate(smokeHitMark, collision.contacts[0].point, Quaternion.identity);
            IceSpike script = collision.gameObject.GetComponent<IceSpike>();
            if (script.enabled && gameObject.CompareTag("RedProjectile"))
            {
                foreach (IceSpike spike in script.allSpikes) spike.Melt(damage);
                script.meltPs.Play();
            }
            if (shockwave)
            {
                PlayerShockwave shockwave = GameObject.Instantiate(shockwavePrefab, collision.contacts[0].point, Quaternion.identity).GetComponent<PlayerShockwave>();
                shockwave.SetDamage(damage / 2.0f);
            }
        }
        else if (collision.gameObject.CompareTag("Thorns"))
        {
            GameObject.Instantiate(hitMark, collision.contacts[0].point, Quaternion.identity);
            GameObject.Instantiate(smokeHitMark, collision.contacts[0].point, Quaternion.identity);
            Thorns script = collision.gameObject.GetComponent<Thorns>();
            if (script.enabled && gameObject.CompareTag("GreenProjectile"))
            {
                script.Disintegrate();
            }
            if (shockwave)
            {
                PlayerShockwave shockwave = GameObject.Instantiate(shockwavePrefab, collision.contacts[0].point, Quaternion.identity).GetComponent<PlayerShockwave>();
                shockwave.SetDamage(damage / 2.0f);
            }
        }
        Destroy(this.gameObject);
    }

    public void AddShockwave()
    {
        shockwave = true;
    }
}

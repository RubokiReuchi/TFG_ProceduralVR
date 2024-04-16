using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMissile : Projectile
{
    [SerializeField] float delay;
    [SerializeField] float damage;
    [SerializeField] GameObject decal;
    [SerializeField] GameObject smokeHitMark;
    [SerializeField] GameObject hitMark;
    [SerializeField] GameObject smokeTrail;
    [SerializeField] GameObject damageText;
    PlayerSkills skills;

    // Start is called before the first frame update
    void Start()
    {
        // Power Increase
        skills = PlayerSkills.instance;
        damage += (skills.attackLevel * 0.05f) * damage;
        speed += (skills.proyectileSpeedLevel * 0.1f) * speed;
        delay -= (Mathf.CeilToInt(skills.missileModeLevel / 2.0f) * 0.1f) * delay;

        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * 0.1f;
        smokeTrail = GameObject.Instantiate(smokeTrail, transform);
    }

    void Update()
    {
        if (delay > 0)
        {
            delay -= Time.deltaTime;
            if (delay <= 0)
            {
                smokeTrail.GetComponent<ParticleSystem>().Play();
            }
            else return;
        }
        

        rb.velocity += transform.forward * speed;
        lifeTime -= Time.deltaTime;
        if (lifeTime < 0)
        {
            smokeTrail.transform.SetParent(null, true);
            smokeTrail.GetComponent<ParticleSystem>().Stop();
            Destroy(gameObject);
        }
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
            if (script.enabled)
            {
                if (gameObject.CompareTag("GreenProjectile") && script.type == COIN.BIOMATTER) damage += (skills.greenBeamLevel * 0.2f) * damage;
                script.TakeDamage(damage, damageText);
                if (gameObject.CompareTag("BlueProjectile")) script.TakeFreeze(damage + (skills.blueBeamLevel * 0.05f) * damage);
            }
        }
        else if (collision.gameObject.CompareTag("EnemyShield"))
        {
            GameObject.Instantiate(hitMark, collision.contacts[0].point, Quaternion.identity);
            GameObject.Instantiate(smokeHitMark, collision.contacts[0].point, Quaternion.identity);
            float finalDamage = !gameObject.CompareTag("RedProjectile") ? damage / 2.0f : damage + (skills.redBeamLevel * 0.1f) * damage;
            EnemyShield script = collision.gameObject.GetComponent<EnemyShield>();
            if (script.enabled) script.TakeDamage(finalDamage, damageText);
        }
        else if (collision.gameObject.CompareTag("DeflectProjectile"))
        {
            Vector3 deflectDirection = Vector3.Reflect(transform.forward, collision.contacts[0].normal);
            transform.forward = deflectDirection;
            return;
        }
        else if (collision.gameObject.CompareTag("Puzzle"))
        {
            GameObject.Instantiate(hitMark, collision.contacts[0].point, Quaternion.identity);
            GameObject.Instantiate(smokeHitMark, collision.contacts[0].point, Quaternion.identity);
            Puzzle script = collision.gameObject.GetComponent<PuzzleParent>().puzzle;
            if (script.enabled) script.HitPuzzle(damage, gameObject.tag);
        }
        else if (collision.gameObject.CompareTag("OtherNonFoundations"))
        {
            GameObject.Instantiate(hitMark, collision.contacts[0].point, Quaternion.identity);
            GameObject.Instantiate(smokeHitMark, collision.contacts[0].point, Quaternion.identity);
        }
        smokeTrail.transform.SetParent(null);
        smokeTrail.GetComponent<ParticleSystem>().Stop();
        Destroy(this.gameObject);
    }
}

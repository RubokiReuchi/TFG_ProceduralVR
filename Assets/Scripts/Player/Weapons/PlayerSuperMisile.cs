using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSuperMissile : Projectile
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
    PlayerSkills skills;

    // Start is called before the first frame update
    void Start()
    {
        // Power Increase
        skills = PlayerSkills.instance;
        damage += (skills.attackLevel * 0.05f) * damage;
        speed += (skills.proyectileSpeedLevel * 0.1f) * speed;
        delay -= (Mathf.FloorToInt(skills.missileModeLevel / 2.0f) * 0.15f) * delay;

        rb = GetComponent<Rigidbody>();
        rb.velocity = transform.forward * 0.1f;
    }

    void Update()
    {
        if (delay > 0)
        {
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
                if (gameObject.CompareTag("GreenProjectile") && script.type == COIN.BIOMATTER) damage += (skills.greenBeamLevel * 0.2f) * damage;
                script.TakeDamage(damage);
                if (gameObject.CompareTag("BlueProjectile")) script.TakeFreeze((damage / 2.0f) + (skills.blueBeamLevel * 0.05f) * (damage / 2.0f));
                float lifeChargeAmount = PlayerSkills.instance.lifeChargeLevel * 0.01f;
                if (lifeChargeAmount > 0) PlayerState.instance.HealPercentage(lifeChargeAmount);
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
            float finalDamage = !gameObject.CompareTag("RedProjectile") ? damage / 2.0f : damage + (skills.redBeamLevel * 0.1f) * damage;
            EnemyShield script = collision.gameObject.GetComponent<EnemyShield>();
            if (script.enabled) script.TakeDamage(finalDamage);
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
        else if (collision.gameObject.CompareTag("Puzzle"))
        {
            GameObject.Instantiate(hitMark, collision.contacts[0].point, Quaternion.identity);
            GameObject.Instantiate(smokeHitMark, collision.contacts[0].point, Quaternion.identity);
            Puzzle script = collision.gameObject.GetComponent<PuzzleParent>().puzzle;
            if (script.enabled) script.HitPuzzle(damage, gameObject.tag);
            if (shockwave)
            {
                PlayerShockwave shockwave = GameObject.Instantiate(shockwavePrefab, collision.contacts[0].point, Quaternion.identity).GetComponent<PlayerShockwave>();
                shockwave.SetDamage(damage / 2.0f);
            }
        }
        else if (collision.gameObject.CompareTag("OtherNonFoundations"))
        {
            GameObject.Instantiate(hitMark, collision.contacts[0].point, Quaternion.identity);
            GameObject.Instantiate(smokeHitMark, collision.contacts[0].point, Quaternion.identity);
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

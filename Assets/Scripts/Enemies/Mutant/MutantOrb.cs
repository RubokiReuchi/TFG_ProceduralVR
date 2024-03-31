using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MutantOrb : Enemy
{
    [HideInInspector] public Mutant owner;
    [Header("Orb")]
    [SerializeField] GameObject ps;
    bool growed = false;
    [SerializeField] float growSpeed;
    [SerializeField] ParticleSystem destroyPs;

    void Start()
    {
        owner.activeOrbs.Add(this);

        currentHealth = maxHealth;

        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        if (alive)
        {
            if (!growed)
            {
                float newSize = transform.localScale.x;
                newSize += Time.deltaTime * growSpeed;
                if (newSize >= 0.5f)
                {
                    newSize = 0.5f;
                    growed = true;
                    ps.SetActive(true);
                }
                transform.localScale = new Vector3(newSize, newSize, newSize);
            }
        }
        else if (!destroyPs.isPlaying)
        {
            Destroy(gameObject);
        }

        destroyPs.transform.rotation = Quaternion.identity;
    }

    public override void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            owner.activeOrbs.Remove(this);
            Die();
        }
    }
    public override void Die()
    {
        destroyPs.Play();
        alive = false;
        transform.localScale = Vector3.zero;
    }
}

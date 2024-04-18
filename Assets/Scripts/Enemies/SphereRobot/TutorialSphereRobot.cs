using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TutorialSphereRobot : Enemy
{
    enum STATE
    {
        REST,
        WALK
    }

    Transform player;
    [NonEditable][SerializeField] STATE state;
    Animator animator;
    [SerializeField] GameObject corpsPrefab;

    private void OnEnable()
    {
        if (!firstEnable) return;
        firstEnable = false;

        animator = transform.GetChild(0).GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        material = new Material(originalMaterial);
        foreach (var materialGO in materialGameObjects) materialGO.GetComponent<MeshRenderer>().material = material;
        state = STATE.REST;

        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case STATE.REST:
                animator.SetTrigger("Activate");
                Invoke("StartWalk", 1.7f);
                break;
            case STATE.WALK:
                agent.destination = player.position;
                break;
            default:
                break;
        }
    }

    void StartWalk()
    {
        animator.SetTrigger("Walk");
        state = STATE.WALK;
        agent.speed = 1.2f;
        animator.speed = 0.31f;
    }

    public override void StartCheckOptions()
    {
        
    }

    public override void TakeDamage(float amount, GameObject damageText)
    {
        if (!enabled || invulneravilityTime > 0) return;

        if (freezePercentage == 100)
        {
            currentHealth -= amount * 5.0f;
            if (damageText != null)
            {
                FloatingDamageText text = GameObject.Instantiate(damageText, damageTextCenter.position + Vector3.one * Random.Range(-0.2f, 0.2f), Quaternion.identity).GetComponentInChildren<FloatingDamageText>();
                text.damage = amount * 5.0f;
            }
            freezePercentage = 0;
            material.SetFloat("_FreezeInterpolation", 0);
            if (currentHealth < 0)
            {
                GameObject.Instantiate(iceBlocksParticlesPrefab, iceBlocksParticlesSpawn.position, Quaternion.identity);
                GiveCoin();
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            currentHealth -= amount;
            if (damageText != null)
            {
                FloatingDamageText text = GameObject.Instantiate(damageText, damageTextCenter.position + Vector3.one * Random.Range(-0.2f, 0.2f), Quaternion.identity).GetComponentInChildren<FloatingDamageText>();
                text.damage = amount;
            }
        }
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public override void TakeFreeze(float amount)
    {
        if (freezePercentage == 100) return;

        freezePercentage += amount - freezeResistance;
        if (freezePercentage >= 100) // freeze
        {
            freezePercentage = 100;
            freezedTime = freezeDuration;
            invulneravilityTime = 1.0f;
        }
        material.SetFloat("_FreezeInterpolation", freezePercentage / 100.0f);
        recoverTime = recoverDelay;
        freezeApplied = false;
    }

    public override void Die()
    {
        animator.speed = 1.0f;
        animator.SetTrigger("Destroy");
        alive = false;
        agent.speed = 0;
        Invoke("Destroyed", 1.45f);
    }

    void Destroyed()
    {
        GameObject.Instantiate(corpsPrefab, transform.position, transform.rotation, transform.parent);
        GiveCoin();
        Destroy(gameObject);
    }
}

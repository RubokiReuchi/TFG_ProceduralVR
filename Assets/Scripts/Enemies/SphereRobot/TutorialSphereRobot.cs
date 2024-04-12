using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TutorialSphereRobot : Enemy
{
    Transform player;
    Transform playerHead;
    Animator animator;
    [SerializeField] GameObject corpsPrefab;

    private void OnEnable()
    {
        if (!firstEnable) return;
        firstEnable = false;

        animator = transform.GetChild(0).GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHead = GameObject.FindGameObjectWithTag("PlayerHead").transform;
        material = new Material(originalMaterial);
        foreach (var materialGO in materialGameObjects) materialGO.GetComponent<MeshRenderer>().material = material;

        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        agent.destination = player.position;
    }

    public override void StartCheckOptions()
    {
        animator.SetTrigger("Walk");
    }

    public override void TakeDamage(float amount)
    {
        if (!enabled || invulneravilityTime > 0) return;

        if (freezePercentage == 100)
        {
            currentHealth -= amount * 5.0f;
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
        else currentHealth -= amount;
        
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
        animator.SetTrigger("Destroy");
        transform.GetComponentInChildren<AnimSphereRobot>().StopRing();
        alive = false;
    }

    public void Destroyed()
    {
        GameObject.Instantiate(corpsPrefab, transform.position, transform.rotation, transform.parent);
        GiveCoin();
        Destroy(gameObject);
    }
}

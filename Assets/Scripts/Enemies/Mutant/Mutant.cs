using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Mutant : Enemy
{
    enum STATE
    {
        REST,
        FALLING,
        WALK_SHOOTING_LEFT,
        WALK_SHOOTING_RIGHT,
        RUNNING,
        ATTACKING,
        BACKFLIPPNG,
        DIYING,
        WAITING
    }

    Transform player;
    Transform playerHead;
    [NonEditable][SerializeField] STATE state;
    Animator animator;
    Rigidbody rb;
    [SerializeField] float slashDistance;
    [SerializeField] Transform rayOriginLeft;
    [SerializeField] Transform rayOriginRight;
    [SerializeField] GameObject rayPrefab;
    [SerializeField] GameObject orbPrefab;
    [SerializeField] ParticleSystem roarPs;
    bool touchFloorOnSpawn;
    bool lastActionWasRoar;

    private void OnEnable()
    {
        if (!firstEnable) return;
        firstEnable = false;

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHead = GameObject.FindGameObjectWithTag("PlayerHead").transform;
        //material = new Material(originalMaterial);
        //foreach (var materialGO in materialGameObjects) materialGO.GetComponent<MeshRenderer>().material = material;
        state = STATE.REST;
        touchFloorOnSpawn = false;
        lastActionWasRoar = false;

        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        // freeze
        freezeSlow = 1.0f - (freezePercentage / 100.0f);
        animator.speed = freezeSlow;
        if (!freezeApplied)
        {
            agent.speed = defaultSpeed * freezeSlow;
            freezeApplied = true;
        }

        switch (state)
        {
            case STATE.REST:
                animator.SetTrigger("Activate");
                rb.useGravity = true;
                state = STATE.FALLING;
                break;
            case STATE.FALLING:
                if (touchFloorOnSpawn)
                {
                    animator.SetTrigger("Landed");
                    state = STATE.WAITING;
                    agent.enabled = true;
                }
                break;
            case STATE.WALK_SHOOTING_LEFT:
                break;
            case STATE.WALK_SHOOTING_RIGHT:
                break;
            case STATE.RUNNING:
                break;
            case STATE.ATTACKING:
                break;
            case STATE.BACKFLIPPNG:
                break;
            case STATE.DIYING:
                break;
            case STATE.WAITING:
                break;
            default:
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("FoundationsF")) touchFloorOnSpawn = true;
    }

    public void FirstStartCheckOptions()
    {
        StartCoroutine(CheckOptions(true));
    }

    public override void StartCheckOptions()
    {
        StartCoroutine(CheckOptions(false));
    }

    public IEnumerator CheckOptions(bool landed)
    {
        if (landed)
        {
            int rand = Random.Range(0, 100);
            if (rand < 80)
            {
                // Roar
                animator.SetTrigger("Roar");
                state = STATE.WAITING;
                lastActionWasRoar = true;
                yield break;
            }
        }
        else if (!lastActionWasRoar)
        {
            int rand = Random.Range(0, 100);
            if (rand < 20)
            {
                // Roar
                animator.SetTrigger("Roar");
                state = STATE.WAITING;
                lastActionWasRoar = true;
                yield break;
            }
        }

        agent.destination = player.position;
        yield return null;
        if (agent.hasPath && agent.remainingDistance <= slashDistance)
        {
            MeleeOptions();
        }
        else
        {
            //RangeOptions();
        }
    }

    void MeleeOptions()
    {
        int rand = Random.Range(0, 1);

        if (rand == 0) // slash
        {
            animator.SetTrigger("Slash");
            state = STATE.ATTACKING;
        }
        else // backflip
        {

        }
    }

    /*void RangeOptions()
    {
        if (lastCanShoot == 1)
        {
            animator.SetTrigger("Start Side Roll");
            state = STATE.SIDE_ROLLING;
            // set end roll location
            int min = Mathf.RoundToInt(minSideRollDistance * 10.0f);
            int max = Mathf.RoundToInt(maxSideRollDistance * 10.0f);
            bool error = true;
            int loops = 0;
            NavMeshHit hit = new();
            while (loops < 10 && error) // if in 10 loops didn't find a good path explote
            {
                float distance = Random.Range(min, max) / 10.0f;
                int direction = Random.Range(0, 2);
                Quaternion q;
                if (direction == 0) q = Quaternion.AngleAxis(90, Vector3.up);
                else q = Quaternion.AngleAxis(270, Vector3.up);
                Vector3 directionVector = transform.position + q * (player.position - transform.position).normalized;
                Vector3 finalPos = directionVector * distance;
                float lenght = Vector3.Distance(finalPos, transform.position);
                NavMesh.SamplePosition(finalPos, out hit, lenght, 1);
                error = Physics.Raycast(transform.position, finalPos, lenght - 0.1f, foundationsLayers);
                loops++;
            }

            if (error) // explote
            {
                StartCoroutine(Exploting());
                return;
            }

            sideRollingDestination = hit.position;
            lastCanShoot = 2;
        }
        else
        {
            animator.SetTrigger("Aiming");
            state = STATE.AIMING;
            lastCanShoot = 1;
        }
    }
    
    public void SpawnRay()
    {
        GameObject.Instantiate(rayPrefab, rayOrigin.position, Quaternion.LookRotation((playerHead.position - rayOrigin.position).normalized));
    }

    public void Stop()
    {
        agent.speed = 0;
    }

    public void RollSpeed()
    {
        agent.speed = rollSpeed;
        defaultSpeed = rollSpeed;
        freezeApplied = false;
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
                GameObject.Instantiate(iceBlocksParticlesPrefab, transform.position, Quaternion.identity);
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
        state = STATE.DESTROYING;
        alive = false;
    }

    public void Destroyed()
    {
        GameObject.Instantiate(corpsPrefab, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    IEnumerator Exploting()
    {
        exploting = true;
        agent.speed = 0;
        animator.SetTrigger("Explote");
        transform.GetComponentInChildren<AnimSphereRobot>().StopRing();
        state = STATE.WAITING;
        float value = 1;
        while (value > 0)
        {
            value -= Time.deltaTime * 2.0f;
            if (value < 0) value = 0;
            material.SetColor("_Color", new Color(1, value, value));
            yield return null;
        }
        for (int i = 0; i < 50; i++)
        {
            if (i % 10 == 0) // i --> 0, 10, 20, 30...
            {
                material.SetColor("_Color", new Color(1, 0.0f, 0));
            }
            else if (i % 10 == 1)
            {
                material.SetColor("_Color", new Color(1, 0.145f, 0));
            }
            else if (i % 10 == 2)
            {
                material.SetColor("_Color", new Color(1, 0.29f, 0));
            }
            else if (i % 10 == 3)
            {
                material.SetColor("_Color", new Color(1, 0.435f, 0));
            }
            else if (i % 10 == 4)
            {
                material.SetColor("_Color", new Color(1, 0.58f, 0));
            }
            else if (i % 10 == 5)
            {
                material.SetColor("_Color", new Color(1, 0.725f, 0));
            }
            else if (i % 10 == 6)
            {
                material.SetColor("_Color", new Color(1, 0.58f, 0));
            }
            else if (i % 10 == 7)
            {
                material.SetColor("_Color", new Color(1, 0.435f, 0));
            }
            else if (i % 10 == 8)
            {
                material.SetColor("_Color", new Color(1, 0.29f, 0));
            }
            else
            {
                material.SetColor("_Color", new Color(1, 0.145f, 0));
            }
            yield return new WaitForSeconds(0.007f);
        }
        GameObject.Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }*/
}

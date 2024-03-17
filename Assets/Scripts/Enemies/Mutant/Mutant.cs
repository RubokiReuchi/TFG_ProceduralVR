using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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
        ROARING,
        LEFT_ATTACKING,
        RIGHT_ATTACKING,
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
    [SerializeField] float backflipSpeed;
    [SerializeField] float minDistanceToShot;
    [SerializeField] float walkShootingSpeed;
    [SerializeField] float runSpeed;
    [SerializeField] Transform rayOriginLeft;
    [SerializeField] Transform rayOriginRight;
    [SerializeField] GameObject rayPrefab;
    [SerializeField] Transform orbOrigin;
    [SerializeField] GameObject orbPrefab;
    [SerializeField] ParticleSystem roarPs;
    bool touchFloorOnSpawn;
    bool usedAuxiliarRoar;

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
        usedAuxiliarRoar = false;

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
                if (agent.hasPath && agent.remainingDistance <= 0.1f)
                {
                    StartCheckOptions();
                }
                break;
            case STATE.WALK_SHOOTING_RIGHT:
                if (agent.hasPath && agent.remainingDistance <= 0.1f)
                {
                    StartCheckOptions();
                }
                break;
            case STATE.RUNNING:
                break;
            case STATE.ROARING:
                agent.speed = 0;
                break;
            case STATE.LEFT_ATTACKING:
                break;
            case STATE.RIGHT_ATTACKING:
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
        //if (landed)
        //{
        //    int rand = Random.Range(0, 100);
        //    if (rand < 80)
        //    {
        //        // Roar
        //        animator.SetTrigger("Roar");
        //        state = STATE.ROARING;
        //        yield break;
        //    }
        //}
        //else if (state != STATE.ROARING)
        //{
        //    int rand = Random.Range(0, 100);
        //    if (rand < 20)
        //    {
        //        // Roar
        //        animator.SetTrigger("Roar");
        //        state = STATE.ROARING;
        //        yield break;
        //    }
        //}
        /*else */if (usedAuxiliarRoar)
        {
            RangeOptions();
            usedAuxiliarRoar = false;
            yield break;
        }

        agent.destination = player.position;
        yield return null;
        /*if (agent.hasPath && agent.remainingDistance <= slashDistance)
        {
            MeleeOptions();
        }
        else if (state == STATE.LEFT_ATTACKING || state == STATE.RIGHT_ATTACKING) // never do range options after slash
        {
            // Roar
        //        animator.SetTrigger("Roar");
        //        state = STATE.ROARING;
        //        yield break;
        }
        else
        {
            RangeOptions();
        }*/
        //Test///////////////////////////
        /*if (state != STATE.BACKFLIPPNG) // slash
        {
            animator.SetTrigger("Backflip");
            state = STATE.BACKFLIPPNG;
            agent.speed = 0;
        }
        else
        {
            animator.SetTrigger("Roar");
            state = STATE.ROARING;
        }*/
        RangeOptions();
    }

    void MeleeOptions()
    {
        int rand = Random.Range(0, 2);

        if (rand == 0 && state != STATE.RIGHT_ATTACKING) // slash
        {
            animator.SetTrigger("Slash");
            if (state == STATE.LEFT_ATTACKING) state = STATE.RIGHT_ATTACKING;
            else state = STATE.LEFT_ATTACKING;
        }
        else // backflip
        {
            animator.SetTrigger("Backflip");
            state = STATE.BACKFLIPPNG;
            agent.speed = 0;
        }
    }

    void RangeOptions()
    {
        int rand = Random.Range(0, 2);

        if (usedAuxiliarRoar) rand = 1; // after auxiliar roar always walk shooting

        if (rand == 0 && state != STATE.RUNNING) // run
        {
            
        }
        else
        {
            if (state == STATE.WALK_SHOOTING_LEFT || state == STATE.WALK_SHOOTING_RIGHT) // auxiliar roar
            {
                usedAuxiliarRoar = true;
                animator.SetTrigger("AuxiliarRoar");
                state = STATE.ROARING;
            }
            else // shooting
            {
                // set end walk location
                bool error = true;
                NavMeshHit hit = new();
                float distance = 7.0f;
                int direction = Random.Range(0, 2);
                while (error && distance > minDistanceToShot) // if no enougth distance run
                {
                    Vector3 directionVector = (direction == 0) ? -transform.right : transform.right; // 0 --> left, 1 --> right
                    Vector3 finalPos = transform.position + directionVector * distance;
                    NavMesh.SamplePosition(finalPos, out hit, distance, 1);
                    error = Physics.Raycast(hit.position, -directionVector, Vector3.Distance(hit.position, transform.position) - 0.1f, foundationsLayers);
                    if (error)
                    {
                        distance -= 1.0f;
                        direction = 1 - direction; // invert
                    }
                }

                if (error) // run
                {

                }
                else
                {
                    if (direction == 0) // walk left
                    {
                        animator.SetTrigger("WalkShootingLeft");
                        state = STATE.WALK_SHOOTING_LEFT;
                    }
                    else // walk right
                    {
                        animator.SetTrigger("WalkShootingRight");
                        state = STATE.WALK_SHOOTING_RIGHT;
                    }
                    agent.destination = hit.position;
                    agent.updateRotation = false;
                    agent.speed = walkShootingSpeed;
                    defaultSpeed = walkShootingSpeed;
                    freezeApplied = false;
                }
            }
        }
    }

    public void StartBackflipMovement()
    {
        agent.updateRotation = false;
        agent.velocity = -transform.forward * 10;
        // APPLY SLOW////////////////////////////////////////////////////////////////////////////////
    }

    public void StopBackflipMovement()
    {
        agent.velocity = Vector3.zero;
    }

    public void AllowRotation()
    {
        agent.updateRotation = true;
    }

    /*public void SpawnRay()
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

using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class SphereRobot : Enemy
{
    public enum STATE
    {
        REST,
        ROLL,
        SIDE_ROLLING, // roll for change shooting position
        WALK,
        AIMING,
        SHOOTING,
        DESTROYING,
        WAITING
    }

    Transform player;
    Transform playerHead;
    [NonEditable][SerializeField] STATE state;
    Animator animator;
    NavMeshAgent agent;
    [SerializeField] float walkDistance;
    [SerializeField] float shootDistance;
    [SerializeField] float minSideRollDistance;
    [SerializeField] float maxSideRollDistance;
    [SerializeField] float rotationSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] float rollSpeed;
    [SerializeField] float explosionDistance;
    int lastCanShoot; // 0 --> side roll, 1 --> shoot
    [SerializeField] Transform rayOrigin;
    [SerializeField] GameObject rayPrefab;
    [SerializeField] GameObject materialGO;
    Material material;
    bool exploting;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] GameObject corpsPrefab;

    private void OnEnable()
    {
        animator = transform.GetChild(0).GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHead = GameObject.FindGameObjectWithTag("PlayerHead").transform;
        material = new Material(materialGO.GetComponent<Renderer>().material);
        materialGO.GetComponent<Renderer>().material = material;
        exploting = false;
        state = STATE.REST;
        lastCanShoot = 0;

        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (exploting) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer < explosionDistance && !Physics.Raycast(transform.position, player.position, distanceToPlayer, foundationsLayers))
        {
            StartCoroutine(Exploting());
            return;
        }

        switch (state)
        {
            case STATE.REST:
                animator.SetTrigger("Activate");
                state = STATE.WAITING;
                break;
            case STATE.ROLL:
                agent.destination = player.position;
                if (agent.hasPath && agent.remainingDistance <= walkDistance)
                {
                    animator.SetTrigger("Stop Roll");
                    state = STATE.WAITING;
                }
                break;
            case STATE.SIDE_ROLLING:
                if (agent.hasPath && agent.remainingDistance <= 0.1f)
                {
                    animator.SetTrigger("Stop Roll");
                    state = STATE.WAITING;
                }
                break;
            case STATE.WALK:
                agent.destination = player.position;
                if (agent.hasPath)
                {
                    if (agent.remainingDistance <= shootDistance)
                    {
                        CanShoot();
                    }
                    else if (agent.remainingDistance > walkDistance)
                    {
                        animator.SetTrigger("Start Roll");
                        state = STATE.ROLL;
                    }
                }
                break;
            case STATE.AIMING:
                agent.speed = 0;
                Vector3 lookAtPos = new Vector3(player.position.x, transform.position.y, player.position.z);
                Quaternion targetRotation = Quaternion.LookRotation(lookAtPos - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                if (Quaternion.Angle(transform.rotation, targetRotation) < 30.0f)
                {
                    animator.SetTrigger("Shoot");
                    state = STATE.SHOOTING;
                }
                break;
            case STATE.SHOOTING:
            case STATE.DESTROYING:
                agent.speed = 0;
                break;
            case STATE.WAITING:
            default:
                break;
        }
    }

    public void StartCheckOptions()
    {
        StartCoroutine(CheckOptions());
    }

    public IEnumerator CheckOptions()
    {
        agent.destination = player.position;
        yield return null;
        if (agent.hasPath && agent.remainingDistance <= shootDistance)
        {
            CanShoot();
        }
        else if (agent.hasPath && agent.remainingDistance <= walkDistance)
        {
            animator.SetTrigger("Walk");
            state = STATE.WALK;
            agent.speed = walkSpeed;
        }
        else
        {
            animator.SetTrigger("Start Roll");
            state = STATE.ROLL;
        }
    }

    void CanShoot()
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
                error = Physics.Raycast(transform.position, finalPos, lenght, foundationsLayers);
                loops++;
            }

            if (error) // explote
            {
                StartCoroutine(Exploting());
                return;
            }
            
            agent.destination = hit.position;
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
    }

    public override void TakeDamage(float amount)
    {
        if (!enabled) return;
        currentHealth -= amount;
        // change color???
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public override void Die()
    {
        animator.SetTrigger("Destroy");
        transform.GetComponentInChildren<AnimSphereRobot>().StopRing();
        state = STATE.DESTROYING;
    }

    public void Destroyed()
    {
        GameObject.Instantiate(corpsPrefab, transform.position, transform.rotation);
        Destroy(this.gameObject);
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
            material.color = new Color(1, value, value);
            yield return null;
        }
        for (int i = 0; i < 50; i++)
        {
            if (i % 10 == 0) // i --> 0, 10, 20, 30...
            {
                material.color = new Color(1, 0.0f, 0);
            }
            else if (i % 10 == 1)
            {
                material.color = new Color(1, 0.145f, 0);
            }
            else if (i % 10 == 2)
            {
                material.color = new Color(1, 0.29f, 0);
            }
            else if (i % 10 == 3)
            {
                material.color = new Color(1, 0.435f, 0);
            }
            else if (i % 10 == 4)
            {
                material.color = new Color(1, 0.58f, 0);
            }
            else if (i % 10 == 5)
            {
                material.color = new Color(1, 0.725f, 0);
            }
            else if (i % 10 == 6)
            {
                material.color = new Color(1, 0.58f, 0);
            }
            else if (i % 10 == 7)
            {
                material.color = new Color(1, 0.435f, 0);
            }
            else if (i % 10 == 8)
            {
                material.color = new Color(1, 0.29f, 0);
            }
            else
            {
                material.color = new Color(1, 0.145f, 0);
            }
            yield return new WaitForSeconds(0.007f);
        }
        GameObject.Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}

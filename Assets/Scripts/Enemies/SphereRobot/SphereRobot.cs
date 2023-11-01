using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class SphereRobot : Enemy
{
    public enum STATE
    {
        REST,
        IDLE,
        ROLL,
        SIDE_ROLLING, // roll for change shooting position
        WALK,
        SHOOTING,
        DESTROYING
    }

    [SerializeField] STATE initialState;
    Transform player;
    STATE state;
    Animator animator;
    [SerializeField] float walkDistance;
    [SerializeField] float shootDistance;
    Vector3 sideRollTarget;

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        state = initialState;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case STATE.REST:
                animator.SetTrigger("Active");
                break;
            case STATE.IDLE:
                CheckOptions();
                break;
            case STATE.ROLL:
                // seak player
                if (Vector3.Distance(transform.position, player.position) <= walkDistance)
                {
                    animator.SetTrigger("Stop Roll");
                }
                break;
            case STATE.SIDE_ROLLING:
                // seak side roll target
                if (Vector3.Distance(transform.position, sideRollTarget) <= 0.1f)
                {
                    animator.SetTrigger("Stop Roll");
                    CheckOptions();
                }
                break;
            case STATE.WALK:
                // seak player
                if (Vector3.Distance(transform.position, player.position) <= shootDistance)
                {
                    CanShoot();
                }
                else if (Vector3.Distance(transform.position, player.position) > walkDistance)
                {
                    animator.SetTrigger("Start Roll");
                    state = STATE.ROLL;
                }
                break;
            case STATE.SHOOTING:
            case STATE.DESTROYING:
            default:
                break;
        }
    }

    public void CheckOptions()
    {
        if (Vector3.Distance(transform.position, player.position) <= shootDistance)
        {
            CanShoot();
        }
        else if (Vector3.Distance(transform.position, player.position) <= walkDistance)
        {
            animator.SetTrigger("Walk");
            state = STATE.WALK;
        }
        else
        {
            animator.SetTrigger("Start Roll");
            state = STATE.ROLL;
        }
    }

    public void CanShoot()
    {
        int rand = Random.Range(0, 10);
        if (rand < 5)
        {
            animator.SetTrigger("Start Side Roll");
            state = STATE.SIDE_ROLLING;
            // set end roll location
        }
        else
        {
            animator.SetTrigger("Shoot");
            state = STATE.SHOOTING;
        }
    }

    public override void TakeDamage(float damage)
    {

    }

    public override void Die()
    {
        // destroy anim
        state = STATE.DESTROYING;
    }
}

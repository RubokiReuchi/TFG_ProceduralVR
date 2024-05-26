using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Octopus : Enemy
{
    enum STATE
    {
        REST,
        WAITING
    }

    Transform player;
    [NonEditable][SerializeField] STATE state;
    [SerializeField] Animator[] animators;
    [SerializeField] GameObject meteorite;
    [SerializeField] GameObject nuke;

    private void OnEnable()
    {
        if (!firstEnable) return;
        firstEnable = false;

        state = STATE.REST;
        foreach (var animator in animators)
        {
            animator.SetInteger("IdleAnimation", Random.Range(0, 12));
        }

        currentHealth = maxHealth;
        Invoke("StartCheckOptions", 3.0f);
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case STATE.REST:
                break;
            case STATE.WAITING:
                break;
            default:
                break;
        }
    }

    public override void StartCheckOptions()
    {
        // meteorite
        foreach (var animator in animators)
        {
            animator.SetBool("Idle", false);
            animator.SetTrigger("Meteorite");
        }
        // nuke
        /*foreach (var animator in animators)
        {
            animator.SetBool("Idle", false);
            animator.SetTrigger("Nuke");
        }*/
    }

    public void Idle()
    {
        foreach (var animator in animators)
        {
            animator.SetBool("Idle", true);
        }
    }

    public void SpawnMeteorite()
    {
        GameObject.Instantiate(meteorite);
    }

    public void SpawnNuke()
    {
        GameObject.Instantiate(nuke, new Vector3(transform.position.x, 0, transform.position.z), Quaternion.identity);
    }

    public override void TakeDamage(float amount, GameObject damageText = null)
    {
        if (!enabled || invulneravilityTime > 0) return;

        currentHealth -= amount;
        if (damageText != null)
        {
            FloatingDamageText text = GameObject.Instantiate(damageText, damageTextCenter.position + Vector3.one * Random.Range(-0.2f, 0.2f), Quaternion.identity).GetComponentInChildren<FloatingDamageText>();
            text.damage = amount;
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public override void TakeFreeze(float amount)
    {
        
    }

    public override void Die()
    {
        alive = false;
    }
}

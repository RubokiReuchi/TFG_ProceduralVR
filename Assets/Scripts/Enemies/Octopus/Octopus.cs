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
    [SerializeField] ParticleSystem launchMeteoritePs;
    [SerializeField] GameObject nuke;
    [SerializeField] Transform energyShield;
    [SerializeField] Transform physicShield;

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
        launchMeteoritePs.Play();
    }

    public void SpawnNuke()
    {
        GameObject.Instantiate(nuke, new Vector3(transform.position.x, 0, transform.position.z), Quaternion.identity);
    }

    IEnumerator CreateEnergyShield()
    {
        float size = 0.0f;
        energyShield.gameObject.SetActive(true);
        while (size < 1.0f)
        {
            size += Time.deltaTime;
            energyShield.localScale = new Vector3(size, size, size);
            yield return null;
        }
    }

    IEnumerator OpenPhysicShield()
    {
        float size = 0.0f;
        physicShield.gameObject.SetActive(true);
        while (size < 3.75f)
        {
            size += Time.deltaTime * 3.75f;
            if (size > 3.75f) size = 3.75f;
            physicShield.localScale = new Vector3(size, size, size);
            yield return null;
        }
    }

    IEnumerator ClosePhysicShield()
    {
        float size = 3.75f;
        while (size > 0.0f)
        {
            size -= Time.deltaTime * 3.75f;
            if (size < 0.0f) size = 0.0f;
            physicShield.localScale = new Vector3(size, size, size);
            yield return null;
        }
        physicShield.gameObject.SetActive(false);
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

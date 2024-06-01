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
    [SerializeField] GameObject[] slowdownRings;
    [SerializeField] Animator sonnar;
    [SerializeField] GameObject rain;

    private void OnEnable()
    {
        if (!firstEnable) return;
        firstEnable = false;

        player = GameObject.FindGameObjectWithTag("Player").transform;

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
        /*foreach (var animator in animators)
        {
            animator.SetBool("Idle", false);
            animator.SetTrigger("Meteorite");
        }
        // nuke
        foreach (var animator in animators)
        {
            animator.SetBool("Idle", false);
            animator.SetTrigger("Nuke");
        }*/
        // slowdownRings
        //animators[0].SetBool("Idle", false);
        //animators[0].SetTrigger("SlowdownRingsRight");
        //animators[4].SetBool("Idle", false);
        //animators[4].SetTrigger("SlowdownRingsLeft");
        // sonnar
        /*for (int i = 0; i < 4; i++)
        {
            animators[i].SetBool("Idle", false);
            animators[i].SetTrigger("SonnarRight");
        }
        for (int i = 4; i < 8; i++)
        {
            animators[i].SetBool("Idle", false);
            animators[i].SetTrigger("SonnarLeft");
        }
        // homing bomb
        StartCoroutine(StartHomingBombSequence());*/
        // rain
        StartCoroutine(StartRainSequence());
    }

    public void Idle(bool row0 = true, bool row1 = true, bool row2 = true, bool row3 = true)
    {
        if (row0)
        {
            animators[0].SetBool("Idle", true);
            animators[4].SetBool("Idle", true);
        }
        if (row1)
        {
            animators[1].SetBool("Idle", true);
            animators[5].SetBool("Idle", true);
        }
        if (row2)
        {
            animators[2].SetBool("Idle", true);
            animators[6].SetBool("Idle", true);
        }
        if (row3)
        {
            animators[3].SetBool("Idle", true);
            animators[7].SetBool("Idle", true);
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

    public void StartSlowdownRings()
    {
        foreach (var ring in slowdownRings)
        {
            ring.SetActive(true);
        }
        Invoke("EndSlowdownRings", 10.0f);
    }

    void EndSlowdownRings()
    {
        foreach (var ring in slowdownRings)
        {
            ring.SetActive(false);
        }
    }

    public void SpawnSonnar()
    {
        sonnar.enabled = true;
    }

    public IEnumerator StartHomingBombSequence()
    {
        animators[0].SetBool("Idle", false);
        animators[0].SetTrigger("HomingBomb");
        yield return new WaitForSeconds(0.8f);
        animators[4].SetBool("Idle", false);
        animators[4].SetTrigger("HomingBomb");
    }

    public IEnumerator StartRainSequence()
    {
        animators[1].SetBool("Idle", false);
        animators[1].SetTrigger("Rain");
        animators[1].GetComponent<OctopusArmAnimations>().commanderArm = true;
        yield return new WaitForSeconds(0.8f);
        animators[5].SetBool("Idle", false);
        animators[5].SetTrigger("Rain");
    }

    public void SpawnRain()
    {
        GameObject.Instantiate(rain, new Vector3(player.position.x + Random.Range(-5, 6), 0, player.position.z + Random.Range(-5, 6)), Quaternion.identity);
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

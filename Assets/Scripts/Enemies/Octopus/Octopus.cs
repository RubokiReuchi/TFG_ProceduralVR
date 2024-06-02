using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Octopus : Enemy
{
    enum STATE
    {
        REST,
        PANNING,
        CENTRING,
        UNCENTRING,
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
    [SerializeField] GameObject sphereRobot;
    [HideInInspector] public List<GameObject> balls;
    [SerializeField] ParticleSystem minionWavePs;
    [SerializeField] GameObject[] corners;
    int currentCorner = 0;
    float pathTime = 0;
    bool canRotate = true;

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
        state = STATE.PANNING;
    }

    // Update is called once per frame
    void Update()
    {
        if (canRotate) transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

        switch (state)
        {
            case STATE.REST:
                break;
            case STATE.PANNING:
                Move(true);
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
        StartCoroutine(StartHomingBombSequence());
        // rain
        StartCoroutine(StartRainSequence());
        // minion
        StartCoroutine(StartMinionSequence());*/
        // minion wave
        foreach (var animator in animators)
        {
            animator.SetBool("Idle", false);
            animator.SetTrigger("MinionWave");
        }
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

    public IEnumerator StartMinionSequence()
    {
        animators[3].SetBool("Idle", false);
        animators[3].SetTrigger("Minion");
        animators[3].GetComponent<OctopusArmAnimations>().commanderArm = true;
        yield return new WaitForSeconds(0.8f);
        animators[7].SetBool("Idle", false);
        animators[7].SetTrigger("Minion");
    }

    public void SpawnMinion(Transform origin)
    {
        Rigidbody rb = GameObject.Instantiate(sphereRobot, origin.position, Quaternion.identity).GetComponent<Rigidbody>();
        rb.AddForce(transform.right * Random.Range(450, 550));
        rb.AddForce(transform.up * Random.Range(450, 550));
    }

    public void CollectMinionWave()
    {
        StartCoroutine(AddInverseImpulseToBall());
        StartCoroutine(MinionWaveParticlesIn());
    }

    IEnumerator AddInverseImpulseToBall()
    {
        foreach (var ball in balls)
        {
            Vector3 direction = Vector3.Normalize(new Vector3(ball.transform.position.x - transform.position.x, 0, ball.transform.position.z - transform.position.z));
            ball.GetComponent<Rigidbody>().mass = 1.0f;
            OctopusBall script = ball.GetComponent<OctopusBall>();
            yield return null;
            ball.GetComponent<Rigidbody>().AddForce(-direction * 1000);
            ball.GetComponent<Rigidbody>().AddForce(Vector3.up * 1000);
            yield return null;
            script.launching = true;
        }
    }

    IEnumerator MinionWaveParticlesIn()
    {
        minionWavePs.gameObject.SetActive(true);
        float size = 6.5f;
        while (size > 0)
        {
            size -= Time.deltaTime * 6.5f;
            minionWavePs.transform.localScale = new Vector3(size, size, size);
            yield return null;
        }
    }

    public void LaunchMinionWave()
    {
        StartCoroutine(AddImpulseToBall());
        StartCoroutine(MinionWaveParticlesOut());
    }

    IEnumerator AddImpulseToBall()
    {
        foreach (var ball in balls)
        {
            Vector3 direction = Vector3.Normalize(new Vector3(ball.transform.position.x - transform.position.x, 0, ball.transform.position.z - transform.position.z));
            ball.GetComponent<Rigidbody>().mass = 1.0f;
            OctopusBall script = ball.GetComponent<OctopusBall>();
            yield return null;
            ball.GetComponent<Rigidbody>().AddForce(direction * 1500);
            yield return null;
            script.launching = true;
        }
    }

    IEnumerator MinionWaveParticlesOut()
    {
        minionWavePs.gameObject.SetActive(true);
        float size = 0;
        while (size < 30.0f)
        {
            size += Time.deltaTime * 40.0f;
            minionWavePs.transform.localScale = new Vector3(size, size, size);
            if (size >= 30.0f) minionWavePs.gameObject.SetActive(false);
            yield return null;
        }
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

    void Move(bool rightDirection)
    {
        int targetCorner = currentCorner + 1;
        if (targetCorner == 4) targetCorner = 0;
        transform.position = Vector3.Lerp(transform.position, corners[targetCorner].transform.position, pathTime);
        pathTime += Time.deltaTime * 0.3f;
        if (pathTime >= 1.0f)
        {
            currentCorner += 1;
            if (currentCorner == 4) currentCorner = 0;
            pathTime = 0.0f;
        }
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

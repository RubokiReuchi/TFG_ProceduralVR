using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

public class Octopus : Enemy
{
    enum STATE
    {
        REST,
        PANNING,
        CENTRING,
        SLOW_RINGS,
        SONNAR,
        HOMING_BOMB,
        RAIN,
        MINION,
        MINION_WAVE,
        METEORITE,
        NUKE
    }

    Transform player;
    [SerializeField] GameObject portal;
    [NonEditable][SerializeField] STATE state;
    [SerializeField] Animator[] animators;
    [SerializeField] GameObject meteorite;
    [SerializeField] ParticleSystem launchMeteoritePs;
    [HideInInspector] public List<GameObject> meteorites = new();
    [SerializeField] GameObject nuke;
    [SerializeField] Transform energyShield;
    [SerializeField] Transform physicShield;
    [SerializeField] GameObject[] slowdownRings;
    [SerializeField] Animator sonnar;
    [SerializeField] GameObject rain;
    [SerializeField] GameObject sphereRobot;
    [HideInInspector] public List<GameObject> balls = new();
    [SerializeField] ParticleSystem minionWavePs;
    Animator pathAnimator;
    float panningTime;
    STATE lastAttack = STATE.REST;
    float pathDirection = 1;
    Vector3 centerPos;
    List<STATE> ensureAttack = new();
    float shieldCd = 30.0f;
    bool physicalShieldActive = false;
    [SerializeField] ParticleSystem[] deathExplosion;
    int explosionNum = 0;

    private void OnEnable()
    {
        if (!firstEnable) return;
        firstEnable = false;

        player = GameObject.FindGameObjectWithTag("Player").transform;
        pathAnimator = GetComponent<Animator>();
        StartCoroutine(EnterSequence());

        state = STATE.REST;
        transform.localScale = Vector3.zero;
        centerPos = new Vector3(0.0f, 7.0f, 19.5f);
        foreach (var animator in animators)
        {
            animator.SetInteger("IdleAnimation", Random.Range(0, 12));
        }

        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) TakeDamage(315);

        if (!alive) return;
        if (!hasShield && shieldCd > 0) shieldCd -= Time.deltaTime;

        switch (state)
        {
            case STATE.PANNING:
                if (panningTime > 0) panningTime -= Time.deltaTime;
                else StartCheckOptions();
                break;
            case STATE.CENTRING:
                if (Vector3.Distance(transform.position, centerPos) < 0.01)
                {
                    switch (lastAttack)
                    {
                        case STATE.SONNAR:
                            for (int i = 0; i < 4; i++)
                            {
                                animators[i].SetBool("Idle", false);
                                animators[i].SetTrigger("SonnarRight");
                            }
                            for (int i = 4; i < 8; i++)
                            {
                                animators[i].SetBool("Idle", false);
                                animators[i].SetTrigger("SonnarLeft");
                            }
                            pathAnimator.SetFloat("Speed", 0);
                            panningTime = (currentHealth > maxHealth / 2.0f) ? 10.0f : 8.0f;
                            break;
                        case STATE.MINION_WAVE:
                            foreach (var animator in animators)
                            {
                                animator.SetBool("Idle", false);
                                animator.SetTrigger("MinionWave");
                            }
                            pathAnimator.SetFloat("Speed", 0);
                            panningTime = 8.0f;
                            break;
                        case STATE.NUKE:
                            foreach (var animator in animators)
                            {
                                animator.SetBool("Idle", false);
                                animator.SetTrigger("Nuke");
                            }
                            pathAnimator.SetFloat("Speed", 0);
                            panningTime = 8.0f;
                            break;
                        default:
                            break;
                    }
                    state = lastAttack;
                }
                break;
            default:
                break;
        }

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
        transform.Rotate(new Vector3(0, -90, 0));
    }

    IEnumerator EnterSequence()
    {
        float size = 0;
        while (size < 18)
        {
            size += Time.deltaTime * 5.0f;
            portal.transform.localScale = new Vector3(size, size, size);
            yield return null;
        }
        size = 0;
        while (size < 1)
        {
            size += Time.deltaTime;
            if (size > 1.0f) size = 1.0f;
            transform.localScale = new Vector3(size, size, size);
            yield return null;
        }
        pathAnimator.enabled = true;
        yield return new WaitForSeconds(0.3f);
        size = 18;
        while (size > 0)
        {
            size -= Time.deltaTime * 5.0f;
            portal.transform.localScale = new Vector3(size, size, size);
            yield return null;
        }
        portal.SetActive(false);
        state = STATE.PANNING;
        panningTime = 1.5f;
    }

    public override void StartCheckOptions()
    {
        /*if (!hasShield && shieldCd <= 0)
        {
            if (Random.Range(0, 2) == 0) StartCoroutine(CreateEnergyShield());
            else StartCoroutine(OpenPhysicShield());
            hasShield = true;
        }
        */
        // meteorite
        if (ensureAttack.Count > 0 && ensureAttack[0] == STATE.METEORITE)
        {
            foreach (var animator in animators)
            {
                animator.SetBool("Idle", false);
                animator.SetTrigger("Meteorite");
            }
            state = STATE.METEORITE;
            lastAttack = STATE.METEORITE;
            pathAnimator.SetFloat("Speed", 0);
            panningTime = 4.5f;
            ensureAttack.Remove(ensureAttack[0]);
            return;
        }
        // nuke
        else if (ensureAttack.Count > 0 && ensureAttack[0] == STATE.NUKE)
        {
            pathAnimator.SetBool("Center", true);
            state = STATE.CENTRING;
            lastAttack = STATE.NUKE;
            ensureAttack.Remove(ensureAttack[0]);
            return;
        }

        // minion wave
        if (balls.Count > 3 && lastAttack != STATE.MINION_WAVE && Random.Range(0, 100) < 20)
        {
            pathAnimator.SetBool("Center", true);
            state = STATE.CENTRING;
            lastAttack = STATE.MINION_WAVE;
            return;
        }
        
        float rand = Random.Range(0, 100);
        // slowdownRings
        if (rand < 20)
        {
            if (lastAttack == STATE.SLOW_RINGS)
            {
                StartCheckOptions();
                return;
            }
            animators[0].SetBool("Idle", false);
            animators[0].SetTrigger("SlowdownRingsRight");
            animators[4].SetBool("Idle", false);
            animators[4].SetTrigger("SlowdownRingsLeft");
            state = STATE.SLOW_RINGS;
            lastAttack = STATE.SLOW_RINGS;
            pathAnimator.SetFloat("Speed", 0);
            panningTime = 0.2f;
        }
        // sonnar
        else if (rand < 40)
        {
            if (lastAttack == STATE.SONNAR)
            {
                StartCheckOptions();
                return;
            }
            pathAnimator.SetBool("Center", true);
            state = STATE.CENTRING;
            lastAttack = STATE.SONNAR;
        }
        // homing bomb
        else if (rand < 60)
        {
            if (lastAttack == STATE.HOMING_BOMB)
            {
                StartCheckOptions();
                return;
            }
            StartCoroutine(StartHomingBombSequence());
            state = STATE.HOMING_BOMB;
            lastAttack = STATE.HOMING_BOMB;
            pathAnimator.SetFloat("Speed", 0);
            panningTime = 5.0f;
        }
        // rain
        else if (rand < 80)
        {
            if (lastAttack == STATE.RAIN)
            {
                StartCheckOptions();
                return;
            }
            StartCoroutine(StartRainSequence());
            state = STATE.RAIN;
            lastAttack = STATE.RAIN;
            pathAnimator.SetFloat("Speed", 0);
            panningTime = 7.5f;
        }
        // minion
        else
        {
            if (lastAttack == STATE.MINION || physicalShieldActive)
            {
                StartCheckOptions();
                return;
            }
            StartCoroutine(StartMinionSequence());
            state = STATE.MINION;
            lastAttack = STATE.MINION;
            pathAnimator.SetFloat("Speed", 0);
            panningTime = 15.0f;
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

        switch (state)
        {
            case STATE.SLOW_RINGS:
            case STATE.SONNAR:
            case STATE.MINION_WAVE:
            case STATE.METEORITE:
            case STATE.NUKE:
                ContinueMovement();
                break;
            case STATE.HOMING_BOMB:
            case STATE.RAIN:
            case STATE.MINION:
                Invoke("ContinueMovement", 3.0f);
                break;
            default:
                break;
        }
    }

    void ContinueMovement()
    {
        if (state != STATE.SLOW_RINGS)
        {
            int direction = Random.Range(0, 10);
            pathAnimator.SetFloat("Speed", (direction < 3) ? -pathDirection : pathDirection);
        }
        if (state == STATE.SONNAR || state == STATE.MINION_WAVE || state == STATE.NUKE) pathAnimator.SetBool("Center", false);
        state = STATE.PANNING;
    }

    public void SpawnMeteorite()
    {
        meteorites.Add(GameObject.Instantiate(meteorite));
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
        sonnar.SetBool("Enabled", true);
    }

    public IEnumerator StartHomingBombSequence()
    {
        animators[0].SetBool("Idle", false);
        animators[0].SetTrigger("HomingBomb");
        yield return new WaitForSeconds(1.3f);
        animators[4].SetBool("Idle", false);
        animators[4].SetTrigger("HomingBomb");
    }

    public IEnumerator StartRainSequence()
    {
        animators[1].SetBool("Idle", false);
        animators[1].SetTrigger("Rain");
        animators[1].GetComponent<OctopusArmAnimations>().commanderArm = true;
        yield return new WaitForSeconds(1.3f);
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
        yield return new WaitForSeconds(1.3f);
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

    public void EnergyShieldBreak()
    {
        hasShield = false;
        shieldCd = 30.0f;
    }

    IEnumerator OpenPhysicShield()
    {
        physicalShieldActive = true;
        float size = 0.0f;
        physicShield.gameObject.SetActive(true);
        while (size < 4.5f)
        {
            size += Time.deltaTime * 4.5f;
            if (size > 4.5f) size = 4.5f;
            physicShield.localScale = new Vector3(size, size, size);
            yield return null;
        }
        yield return new WaitForSeconds(30.0f);
        while (size > 0.0f)
        {
            size -= Time.deltaTime * 4.5f;
            if (size < 0.0f) size = 0.0f;
            physicShield.localScale = new Vector3(size, size, size);
            yield return null;
        }
        physicShield.gameObject.SetActive(false);
        physicalShieldActive = true;
        hasShield = false;
        shieldCd = 30.0f;
    }

    public override void TakeDamage(float amount, GameObject damageText = null)
    {
        if (!enabled || invulneravilityTime > 0 || !alive) return;

        if (damageText != null)
        {
            FloatingDamageText text = GameObject.Instantiate(damageText, damageTextCenter.position + Vector3.one * Random.Range(-2.0f, 2.0f), Quaternion.identity).GetComponentInChildren<FloatingDamageText>();
            text.damage = amount;
            text.scaleMultiplier = 5.0f;
        }

        if (currentHealth == 0) return;
        currentHealth -= amount;
        

        if (currentHealth + amount >= (maxHealth / 8) * 7 && currentHealth < (maxHealth / 8) * 7) ensureAttack.Add(STATE.METEORITE);
        else if (currentHealth + amount >= (maxHealth / 8) * 6 && currentHealth < (maxHealth / 8) * 6) ensureAttack.Add(STATE.METEORITE);
        else if (currentHealth + amount >= (maxHealth / 8) * 5 && currentHealth < (maxHealth / 8) * 5) ensureAttack.Add(STATE.METEORITE);
        else if (currentHealth + amount >= (maxHealth / 8) * 4 && currentHealth < (maxHealth / 8) * 4) ensureAttack.Add(STATE.NUKE);
        else if (currentHealth + amount >= (maxHealth / 8) * 3 && currentHealth < (maxHealth / 8) * 3) ensureAttack.Add(STATE.METEORITE);
        else if (currentHealth + amount >= (maxHealth / 8) * 2 && currentHealth < (maxHealth / 8) * 2) ensureAttack.Add(STATE.METEORITE);
        else if (currentHealth + amount >= (maxHealth / 8) * 1 && currentHealth < (maxHealth / 8) * 1) ensureAttack.Add(STATE.METEORITE);

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
        ensureAttack.Add(STATE.NUKE);
    }

    public void DeathPath()
    {
        if (currentHealth == 0)
        {
            pathAnimator.SetBool("Death", true);
            alive = false;
        }
    }

    public void DeathExplosion()
    {
        deathExplosion[explosionNum].Play();
        explosionNum++;
    }

    public void DestroyEnemy()
    {
        Destroy(gameObject);
    }
}

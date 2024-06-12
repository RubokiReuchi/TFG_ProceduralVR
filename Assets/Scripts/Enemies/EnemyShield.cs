using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyShield : MonoBehaviour
{
    enum ON_DEATH
    {
        DESTROY,
        HIDE_AND_REVIVE
    }

    [SerializeField] protected Enemy enemyScript;
    [SerializeField] protected float maxHealth;
    protected float currentHealth;
    [SerializeField] ON_DEATH onDeath;
    PlayerState playerState;
    bool xRayLayer = false;
    Material material;
    [SerializeField] protected float dissolveSpeed;
    [SerializeField] float damageTextScale = 1.0f;

    public virtual void Start()
    {
        enemyScript.hasShield = true;
        enemyScript.shield = this;
        material = new Material(GetComponent<Renderer>().material);
        GetComponent<Renderer>().material = material;
        playerState = PlayerState.instance;

        currentHealth = maxHealth;
    }

    public virtual void Update()
    {
        if (!xRayLayer && playerState.xRayVisionActive)
        {
            gameObject.layer = LayerMask.NameToLayer("XRayEnemy");
            xRayLayer = true;
        }
        else if (xRayLayer && !playerState.xRayVisionActive)
        {
            gameObject.layer = LayerMask.NameToLayer("EnemyBarrier");
            xRayLayer = false;
        }
    }

    public virtual void TakeDamage(float amount, GameObject damageText = null)
    {
        if (!enabled) return;
        currentHealth -= amount;
        if (damageText != null)
        {
            FloatingDamageText text = GameObject.Instantiate(damageText, enemyScript.damageTextCenter.position + Vector3.one * Random.Range(-0.2f, 0.2f) + Vector3.up * 0.5f, Quaternion.identity).GetComponentInChildren<FloatingDamageText>();
            text.damage = amount;
            text.scaleMultiplier = damageTextScale;
        }

        if (currentHealth <= 0)
        {
            if (onDeath == ON_DEATH.DESTROY)
            {
                currentHealth = 0;
                StartCoroutine(Destroy());
            }
            else if (onDeath == ON_DEATH.HIDE_AND_REVIVE)
            {
                StartCoroutine(Hide());
            }
        }
    }

    IEnumerator Destroy()
    {
        float percentage = material.GetFloat("_DissolvePercentage");
        while (percentage > 0)
        {
            percentage -= (percentage > 0.8f) ? Time.deltaTime * dissolveSpeed * 5.0f : Time.deltaTime * dissolveSpeed;
            if (percentage < 0) percentage = 0;
            material.SetFloat("_DissolvePercentage", percentage);
            yield return null;
        }
        enemyScript.hasShield = false;
        transform.parent.gameObject.SetActive(false);
    }

    IEnumerator Hide()
    {
        float percentage = material.GetFloat("_DissolvePercentage");
        while (percentage > 0)
        {
            percentage -= (percentage > 0.8f) ? Time.deltaTime * dissolveSpeed * 5.0f : Time.deltaTime * dissolveSpeed;
            if (percentage < 0) percentage = 0;
            material.SetFloat("_DissolvePercentage", percentage);
            yield return null;
        }
        currentHealth = maxHealth;
        material.SetFloat("_DissolvePercentage", 1.0f);
        transform.parent.localScale = Vector3.zero;
        transform.parent.gameObject.SetActive(false);
        if (enemyScript is Octopus)
        {
            Octopus octopus = (Octopus)enemyScript;
            octopus.EnergyShieldBreak();
        }
    }
}

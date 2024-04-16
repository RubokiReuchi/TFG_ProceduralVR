using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShield : MonoBehaviour
{
    [SerializeField] Enemy enemyScript;
    [SerializeField] float maxHealth;
    float currentHealth;
    PlayerState playerState;
    bool xRayLayer = false;
    Material material;
    [SerializeField] float dissolveSpeed;

    void Start()
    {
        enemyScript.hasShield = true;
        enemyScript.shield = this;
        material = new Material(GetComponent<Renderer>().material);
        GetComponent<Renderer>().material = material;
        playerState = PlayerState.instance;

        currentHealth = maxHealth;
    }

    private void Update()
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

    public void TakeDamage(float amount, GameObject damageText = null)
    {
        if (!enabled) return;
        currentHealth -= amount;
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            StartCoroutine(Destroy());     
        }
    }

    IEnumerator Destroy()
    {
        float percentage = material.GetFloat("_DissolvePercentage");
        while (percentage > 0)
        {
            percentage -= Time.deltaTime * dissolveSpeed;
            if (percentage < 0) percentage = 0;
            material.SetFloat("_DissolvePercentage", percentage);
            yield return null;
        }
        enemyScript.hasShield = false;
        transform.parent.gameObject.SetActive(false);
    }
}

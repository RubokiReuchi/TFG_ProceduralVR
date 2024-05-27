using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctopusShield : EnemyShield
{
    public override void Start()
    {
        currentHealth = maxHealth;
    }

    public override void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(1000);
        }
    }

    public override void TakeDamage(float amount, GameObject damageText = null)
    {
        if (!enabled) return;
        currentHealth -= amount;
        if (damageText != null)
        {
            FloatingDamageText text = GameObject.Instantiate(damageText, enemyScript.damageTextCenter.position + Vector3.one * Random.Range(-0.2f, 0.2f) + Vector3.up * 0.5f, Quaternion.identity).GetComponentInChildren<FloatingDamageText>();
            text.damage = amount * 5.0f;
        }

        if (currentHealth <= 0)
        {
            StartCoroutine(Hide());
        }
    }

    IEnumerator Hide()
    {
        float size = transform.localScale.x;
        while (size > 0.0f)
        {
            size -= Time.deltaTime * dissolveSpeed;
            transform.localScale = new Vector3(size, size, size);
            yield return null;
        }
        currentHealth = maxHealth;
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
}

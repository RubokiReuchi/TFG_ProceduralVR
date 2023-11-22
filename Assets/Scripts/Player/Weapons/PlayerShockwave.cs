using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShockwave : MonoBehaviour
{
    float damage;
    List<Transform> entityDamaged = new();
    bool increase = true;
    [HideInInspector] public float maxSize = 2.0f;

    private void OnEnable()
    {
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        if (increase)
        {
            transform.localScale += Vector3.one * 4.0f * Time.deltaTime;
            if (transform.localScale.x >= maxSize) increase = false;
        }
        else
        {
            transform.localScale -= Vector3.one * 4.0f * Time.deltaTime;
            if (transform.localScale.x <= 0.0f) Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (entityDamaged.Contains(other.transform.root)) return;
        if (other.CompareTag("Player") || other.CompareTag("PlayerHead"))
        {
            other.transform.root.GetComponent<PlayerState>().TakeDamage(damage / 5.0f);
            entityDamaged.Add(other.transform.root);
        }
        else if (other.CompareTag("Enemy"))
        {
            Enemy script = other.transform.root.GetComponent<Enemy>();
            if (script.enabled) script.TakeDamage(damage);
            entityDamaged.Add(other.transform.root);
        }
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    public void SetSize(float size)
    {
        maxSize = size;
    }
}

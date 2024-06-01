using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctopusRain : MonoBehaviour
{
    [SerializeField] float damage;
    float lifeTime = 4.1f;
    string UUID;
    CapsuleCollider col;

    void Start()
    {
        UUID = System.Guid.NewGuid().ToString();
        col = GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0) Destroy(gameObject);
        else if (lifeTime < 0.5f)
        {
            if (col.enabled) col.enabled = false;
        }
        else if (lifeTime < 3.75f)
        {
            if (!col.enabled) col.enabled = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerState>().TakeAreaDamage(damage, UUID);
        }
    }
}

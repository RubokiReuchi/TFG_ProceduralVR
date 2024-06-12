using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteoriteFlames : MonoBehaviour
{
    [SerializeField] float damage;
    string UUID;
    ParticleSystem ps;

    void Start()
    {
        UUID = System.Guid.NewGuid().ToString();
        ps = GetComponent<ParticleSystem>();
        Invoke("EnableCollision", 1.0f);
    }

    void Update()
    {
        if (ps.isStopped) gameObject.SetActive(false);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerHead") || other.CompareTag("NormalHand"))
        {
            other.transform.root.GetComponent<PlayerState>().TakeAreaDamage(damage, UUID);
        }
    }

    void EnableCollision()
    {
        GetComponent<SphereCollider>().enabled = true;
    }
}

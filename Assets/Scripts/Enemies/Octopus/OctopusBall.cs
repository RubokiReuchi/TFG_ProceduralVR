using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OctopusBall : MonoBehaviour
{
    [SerializeField] float damage;
    [HideInInspector] public bool launching = false;
    Rigidbody rb;
    string UUID;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        UUID = System.Guid.NewGuid().ToString();
    }

    void Update()
    {
        if (launching && rb.velocity.magnitude < 0.1f)
        {
            launching = false;
            rb.mass = 10000.0f;
            rb.velocity = Vector3.zero;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (launching)
        {
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("PlayerHead") || collision.gameObject.CompareTag("NormalHand"))
            {
                collision.transform.root.GetComponent<PlayerState>().TakeAreaDamage(damage, UUID);
            }
        }
    }
}

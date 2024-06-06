using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] float damage;
    [SerializeField] GameObject breakPs;
    string UUID;
    public bool landed = false;

    void Start()
    {
        UUID = System.Guid.NewGuid().ToString();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!landed)
        {
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("PlayerHead") || collision.gameObject.CompareTag("NormalHand"))
            {
                collision.transform.root.GetComponent<PlayerState>().TakeAreaDamage(damage, UUID);
                BreakMeteorite();
            }
        }
        if (collision.gameObject.CompareTag("Meteorite"))
        {
            BreakMeteorite();
        }
    }

    public void BreakMeteorite()
    {
        Instantiate(breakPs, transform.position, Quaternion.identity);
        Destroy(transform.root.gameObject);
    }
}

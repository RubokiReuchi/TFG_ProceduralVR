using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] float damage;
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
                StartCoroutine(BreakMeteorite());
            }
        }
        if (collision.gameObject.CompareTag("Meteorite"))
        {
            StartCoroutine(BreakMeteorite());
        }
    }

    IEnumerator BreakMeteorite()
    {
        yield return null;
        Destroy(transform.root.gameObject);
    }
}

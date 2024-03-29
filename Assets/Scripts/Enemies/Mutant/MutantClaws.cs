using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutantClaws : MonoBehaviour
{
    [SerializeField] float damage;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerState>().TakeDamage(damage);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutantClaws : MonoBehaviour
{
    [SerializeField] float damage;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerMeleeCollision"))
        {
            other.transform.root.GetComponent<PlayerState>().TakeDamage(damage);
        }
    }
}

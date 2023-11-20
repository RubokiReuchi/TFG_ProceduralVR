using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLaser : MonoBehaviour
{
    [SerializeField] float damage;
    //[SerializeField] GameObject decal;
    [SerializeField] GameObject hitMark;

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Enemy"))
        {
            GameObject.Instantiate(hitMark, transform.position, Quaternion.identity);
            Enemy script = other.GetComponent<Enemy>();
            if (script.enabled) script.TakeDamage(damage);
        }
    }
}

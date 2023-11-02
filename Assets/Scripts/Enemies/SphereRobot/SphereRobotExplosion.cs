using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SphereRobotExplosion : MonoBehaviour
{
    ParticleSystem ps;
    List<Transform> entityDamaged = new();

    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!ps.IsAlive()) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (entityDamaged.Contains(other.transform.root)) return;
        if (other.CompareTag("Player") || other.CompareTag("PlayerHead"))
        {
            Debug.Log("Player Damaged");
            entityDamaged.Add(other.transform.root);
        }
        else if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy Damaged");
            entityDamaged.Add(other.transform.root);
        }
    }
}

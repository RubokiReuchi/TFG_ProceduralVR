using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctopusSonnarWave : MonoBehaviour
{
    ParticleSystem ps;
    [SerializeField] float damage;
    string UUID;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        ps.trigger.AddCollider(GameObject.FindGameObjectWithTag("Player").transform);

        StartCoroutine(GetNewUUID());
    }

    void Update()
    {
        transform.rotation = Quaternion.identity;

    }

    private void OnParticleTrigger()
    {
        List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
        int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter, out var data);

        for (int i = 0; i < numEnter; i++)
        {
            for (int j = 0; j < data.GetColliderCount(i); j++)
            {
                if (data.GetCollider(i, j).CompareTag("Player"))
                {
                    data.GetCollider(i, j).GetComponent<PlayerState>().TakeAreaDamage(damage, UUID);
                }
            }
        }
    }

    IEnumerator GetNewUUID()
    {
        while (transform.parent.gameObject.activeSelf)
        {
            UUID = System.Guid.NewGuid().ToString();
            yield return new WaitForSeconds(0.5f);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutantOrbWave : MonoBehaviour
{
    ParticleSystem ps;
    [SerializeField] float waveDamage;
    [SerializeField] float waveHeal;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
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
                    data.GetCollider(i, j).GetComponent<PlayerState>().TakeDamage(waveDamage);
                }
                else if (data.GetCollider(i, j).CompareTag("Enemy"))
                {
                    data.GetCollider(i, j).GetComponent<Enemy>().TakeHeal(waveHeal);
                }
            }
        }
    }
}

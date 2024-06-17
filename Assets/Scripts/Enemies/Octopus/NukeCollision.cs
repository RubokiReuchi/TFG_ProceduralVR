using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class NukeCollision : MonoBehaviour
{
    [SerializeField] LayerMask layerMask;

    [Header("Audio")]
    [SerializeField] AudioClip explosion;
    AudioSource source;

    private void OnEnable()
    {
        Invoke("ExplosionSound", 2.85f);
        Invoke("CheckPlayer", 4.5f);
        Invoke("Destroy", 12.0f);

        source = GetComponent<AudioSource>();
    }

    void CheckPlayer()
    {
        PlayerState player = PlayerState.instance;
        Physics.Raycast(transform.position + Vector3.up, player.transform.position - (transform.position + Vector3.up), out RaycastHit hit, float.MaxValue, layerMask);
        if (hit.collider.transform.root.CompareTag("Player"))
        {
            player.TakeDamage(1000);
            player.TakeDamage(1000);
        }

        Octopus octopusScript = GameObject.Find("Octopus").GetComponent<Octopus>();
        while(octopusScript.meteorites.Count > 0)
        {
            octopusScript.meteorites[0].GetComponentInChildren<Asteroid>().BreakMeteorite();
        }
    }

    void Destroy()
    {
        Destroy(gameObject);
    }

    void ExplosionSound()
    {
        source.clip = explosion;
        source.Play();
    }
}

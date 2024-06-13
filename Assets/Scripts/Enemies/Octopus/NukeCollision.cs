using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NukeCollision : MonoBehaviour
{
    [SerializeField] LayerMask layerMask;

    private void OnEnable()
    {
        Invoke("CheckPlayer", 4.5f);
        Invoke("Destroy", 12.0f);
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
}

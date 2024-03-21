using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutantArtifact : MonoBehaviour
{
    [SerializeField] Transform ring0;
    [SerializeField] float ring0Speed;
    [SerializeField] Transform ring1;
    [SerializeField] float ring1Speed;
    [SerializeField] Transform ring2;
    [SerializeField] float ring2Speed;
    bool spawning = false;
    bool despawning = false;
    [SerializeField] float spawnSpeed;
    float newScale;
    [SerializeField] GameObject rayPrefab;
    [SerializeField] float cadence; // projectiles/sec
    float projectileCd;

    void Update()
    {
        transform.rotation = Quaternion.identity;
        ring0.Rotate(Vector3.right * ring0Speed);
        ring1.Rotate(Vector3.up * ring1Speed);
        ring2.Rotate(Vector3.forward * ring2Speed);

        if (spawning)
        {
            newScale += Time.deltaTime * spawnSpeed;
            if (newScale >= 1.0f)
            {
                newScale = 1.0f;
                spawning = false;
                projectileCd = 0;
            }
            transform.localScale = Vector3.one * newScale;
        }
        else if (despawning)
        {
            newScale -= Time.deltaTime * spawnSpeed;
            if (newScale <= 0.0f)
            {
                newScale = 0.0f;
                despawning = false;
                gameObject.SetActive(false);
            }
            transform.localScale = Vector3.one * newScale;
        }
        else
        {
            projectileCd -= Time.deltaTime;
            if (projectileCd <= 0.0f)
            {
                GameObject.Instantiate(rayPrefab, transform.position, Quaternion.Euler(-Vector3.right * 90));
                projectileCd = 1 / cadence;
            }
        }
    }

    public void Spawn()
    {
        newScale = 0.0f;
        spawning = true;
    }

    public void Despawn()
    {
        newScale = 1.0f;
        despawning = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutantArtifact : MonoBehaviour
{
    bool spawning = false;
    bool despawning = false;
    [SerializeField] float spawnSpeed;
    [SerializeField] float desiredScale;
    float newScale;
    [SerializeField] GameObject rayPrefab;
    [SerializeField] float cadence; // projectiles/sec
    float projectileCd;
    public GameObject trial;

    void Update()
    {
        transform.rotation = Quaternion.identity;

        if (spawning)
        {
            newScale += Time.deltaTime * spawnSpeed * desiredScale;
            if (newScale >= desiredScale)
            {
                newScale = desiredScale;
                spawning = false;
                projectileCd = 0;
            }
            transform.localScale = Vector3.one * newScale;
        }
        else if (despawning)
        {
            newScale -= Time.deltaTime * spawnSpeed * desiredScale;
            if (newScale <= 0.0f)
            {
                newScale = 0.0f;
                despawning = false;
                gameObject.SetActive(false);
                trial.SetActive(false);
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
        transform.localScale = Vector3.zero;
        spawning = true;
        trial.SetActive(true);
    }

    public void Despawn()
    {
        newScale = desiredScale;
        despawning = true;
    }
}

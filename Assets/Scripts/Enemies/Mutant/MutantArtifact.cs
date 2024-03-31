using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutantArtifact : MonoBehaviour
{
    [SerializeField] ParticleSystem ps;
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
                ps.Stop();
                gameObject.SetActive(false);
            }
            transform.localScale = Vector3.one * newScale;
        }
        else
        {
            projectileCd -= Time.deltaTime;
            if (projectileCd <= 0.0f)
            {
                GameObject.Instantiate(rayPrefab, transform.position, Quaternion.identity);//Quaternion.Euler(-Vector3.right * 90));
                projectileCd = 1 / cadence;
            }
        }
    }

    public void Spawn()
    {
        newScale = 0.0f;
        spawning = true;
        ps.Play();
    }

    public void Despawn()
    {
        newScale = 1.0f;
        despawning = true;
    }
}

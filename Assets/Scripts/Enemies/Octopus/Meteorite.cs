using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteorite : MonoBehaviour
{
    Transform player;
    [SerializeField] GameObject asteroid;
    [SerializeField] ParticleSystem asteroidTrail;
    [SerializeField] GameObject smoke;
    [SerializeField] GameObject mark;
    [SerializeField] ParticleSystem smallCirclePs;
    [SerializeField] GameObject explosion;
    [SerializeField] GameObject[] residualFlames;
    Vector3 asteroidOriginalPos;
    Vector3 asteroidFinalPos;
    float timer = 0.0f;
    bool staticState = false;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        asteroidOriginalPos = asteroid.transform.localPosition;
        asteroidFinalPos = Vector3.zero + Vector3.up * 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        if (staticState) return;
        if (timer <= 1.0f)
        {
            if (timer < 0.2f) asteroid.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, timer * 5.0f);
            if (timer < 0.6f)
            {
                gameObject.transform.position = new Vector3(player.position.x, 0.0f, player.position.z);
                if (timer + Time.deltaTime * 0.25f >= 0.6f) smallCirclePs.Pause();
            }
            if (timer > 0.8f && !smoke.activeSelf)
            {
                smoke.SetActive(true);
                foreach (var flame in residualFlames) flame.SetActive(true);
            }
            asteroid.transform.localPosition = Vector3.Lerp(asteroidOriginalPos, asteroidFinalPos, timer);
            timer += Time.deltaTime * 0.25f;
        }
        else
        {
            asteroidTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            mark.SetActive(false);
            explosion.SetActive(true);
            asteroid.GetComponent<Asteroid>().landed = true;
            asteroid.GetComponent<Rigidbody>().isKinematic = true;
            staticState = true;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteorite : MonoBehaviour
{
    [SerializeField] GameObject asteroid;
    [SerializeField] ParticleSystem asteroidTrail;
    [SerializeField] GameObject smoke;
    [SerializeField] GameObject mark;
    [SerializeField] GameObject markCircle;
    [SerializeField] GameObject explosion;
    [SerializeField] GameObject[] residualFlames;
    Vector3 asteroidOriginalPos;
    Vector3 asteroidFinalPos;
    float timer = 0.0f;
    bool staticState = false;

    // Start is called before the first frame update
    void Start()
    {
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
            if (timer > 0.8f && !smoke.activeSelf)
            {
                smoke.SetActive(true);
                foreach (var flame in residualFlames) flame.SetActive(true);
            }
            markCircle.transform.localScale = new Vector3(timer, timer, timer);
            asteroid.transform.localPosition = Vector3.Lerp(asteroidOriginalPos, asteroidFinalPos, timer);
            timer += Time.deltaTime * 0.25f;
        }
        else
        {
            asteroidTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            mark.SetActive(false);
            markCircle.SetActive(false);
            explosion.SetActive(true);
            staticState = true;
        }
    }
}

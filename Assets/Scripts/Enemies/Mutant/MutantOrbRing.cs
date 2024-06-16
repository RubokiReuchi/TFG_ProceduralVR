using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class MutantOrbRing : MonoBehaviour
{
    [SerializeField] Material growingRingMat;
    Material material;
    [SerializeField] float ringGrowSpeed;
    [SerializeField] float ringAlphaSpeed;
    [SerializeField] float scaleOffset;
    float currentScale;
    float alpha;
    Color ringColor;
    [SerializeField] ParticleSystem ps;

    AudioSource source;

    void Start()
    {
        material = new Material(growingRingMat);
        GetComponent<MeshRenderer>().material = material;
        currentScale = scaleOffset;
        alpha = 0.45f;
        ringColor = growingRingMat.GetColor("_TintColor");

        if (ps) source = GetComponent<AudioSource>();
    }

    
    void Update()
    {
        if (ps && currentScale == 0.0f)
        {
            ps.Play();
            StartCoroutine(WaveSound());
        }

        if (currentScale < 0.1f)
        {
            currentScale += Time.deltaTime * ringGrowSpeed;
            transform.localScale = new Vector3(currentScale, currentScale, currentScale);
        }

        if (currentScale > 0.08f)
        {
            alpha -= Time.deltaTime * ringAlphaSpeed;
            if (alpha < 0.0f)
            {
                alpha = 0.45f;
                currentScale = 0.0f;
                transform.localScale = new Vector3(currentScale, currentScale, currentScale);
            }
            material.SetColor("_TintColor", new Color(ringColor.r, ringColor.g, ringColor.b, alpha));
        }
    }

    IEnumerator WaveSound()
    {
        source.Play();
        yield return new WaitForSeconds(0.2f);
        source.Play();
        yield return new WaitForSeconds(0.2f);
        source.Play();
    }
}

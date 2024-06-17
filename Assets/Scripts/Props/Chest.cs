using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Chest : MonoBehaviour
{
    [SerializeField] COIN type;
    [SerializeField] int amount;
    [SerializeField] Renderer handPrintRenderer;
    [SerializeField] BoxCollider detectorCollider;
    Bounds colliderBounds;
    CapsuleCollider leftHandCollider;
    float handInTime;
    Material material;
    [SerializeField] AudioSource scannerSource;
    [SerializeField] AudioSource openSource;
    bool handPlaced = false;

    // Start is called before the first frame update
    void Start()
    {
        colliderBounds = detectorCollider.GetComponent<BoxCollider>().bounds;
        material = new Material(handPrintRenderer.materials[1]);
        Material[] auxArray = { handPrintRenderer.materials[0], material };
        handPrintRenderer.materials = auxArray;
        material.SetColor("_GridColor", new Color(0.35f, 1, 0.54f, 1));

        leftHandCollider = GameObject.Find("LeftHandCollision").GetComponent<CapsuleCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (colliderBounds.Contains(leftHandCollider.bounds.center))
        {
            if (handInTime == 0.0f) scannerSource.Play();
            handInTime += Time.deltaTime;

            if (handInTime >= 1)
            {
                handInTime = 1;
                GetComponent<Animator>().enabled = true;
                if (type == COIN.BIOMATTER) PlayerSkills.instance.AddBiomatter(amount);
                else PlayerSkills.instance.AddGear(amount);
                enabled = false;
                openSource.Play();
            }
            material.SetColor("_GridColor", new Color(0.35f, 1, 0.54f + 0.46f * handInTime, 1));
            handPlaced = true;
        }
        else if (handInTime > 0)
        {
            if (handPlaced)
            {
                StartCoroutine(FadeOutScanner());
                handPlaced = false;
            }
            handInTime -= Time.deltaTime;
            if (handInTime < 0) handInTime = 0;
            material.SetColor("_GridColor", new Color(0.35f, 1, 0.54f + 0.23f * handInTime, 1));
        }
    }

    IEnumerator FadeOutScanner()
    {
        float initialVolume = scannerSource.volume;
        float volume = initialVolume;
        while (volume > 0.0f)
        {
            volume -= Time.deltaTime * 2.0f;
            if (volume < 0.0f) volume = 0.0f;
            scannerSource.volume = volume;
            yield return null;
        }
        scannerSource.Stop();
        scannerSource.volume = initialVolume;
    }
}

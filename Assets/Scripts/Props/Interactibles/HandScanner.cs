using System.Collections;
using UnityEngine;

public class HandScanner : Puzzle
{
    [SerializeField] BoxCollider detectorCollider;
    Bounds colliderBounds;
    CapsuleCollider leftHandCollider;
    float handInTime = 0.0f;
    [SerializeField] EnergyBarrier barrier;
    Material material;
    [SerializeField] AudioSource scannerSource;
    [SerializeField] AudioSource completedSource;
    bool handPlaced = false;

    // Start is called before the first frame update
    void Start()
    {
        colliderBounds = detectorCollider.GetComponent<BoxCollider>().bounds;
        material = new Material(GetComponent<Renderer>().materials[2]);
        Material[] auxArray = { GetComponent<Renderer>().materials[0], GetComponent<Renderer>().materials[1], material };
        GetComponent<Renderer>().materials = auxArray;
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
                barrier.PuzzleCompleted();
                enabled = false;
                scannerSource.Stop();
                completedSource.Play();
            }
            material.SetColor("_GridColor", new Color(0.35f , 1, 0.54f + 0.46f * handInTime, 1));
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

    public override void StartPuzzle()
    {
        enabled = true;
        barrier.PuzzleStarted();
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

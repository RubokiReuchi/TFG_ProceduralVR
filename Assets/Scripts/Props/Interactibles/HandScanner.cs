using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandScanner : MonoBehaviour
{
    Bounds colliderBounds;
    [SerializeField] CapsuleCollider leftHandCollider;
    float handInTime;
    [SerializeField] EnergyBarrier barrier;
    Material material;

    // Start is called before the first frame update
    void Start()
    {
        colliderBounds = GetComponent<BoxCollider>().bounds;
        material = new Material(GetComponent<Renderer>().material);
        GetComponent<Renderer>().material = material;
        material.SetColor("_Color", Color.red);
    }

    // Update is called once per frame
    void Update()
    {
        if (colliderBounds.Contains(leftHandCollider.bounds.center))
        {
            handInTime += Time.deltaTime;
            handInTime += Time.deltaTime;

            if (handInTime >= 2)
            {
                handInTime = 2;
                barrier.PuzzleCompleted();
                enabled = false;
            }
            material.SetColor("_Color", new Color(1, handInTime / 2.0f, 0, 1));
        }
        else handInTime = 0;
    }

    public void StartPuzzle()
    {
        enabled = true;
        barrier.PuzzleStarted();
    }
}

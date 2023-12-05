using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class HandScanner : Puzzle
{
    [SerializeField] BoxCollider detectorCollider;
    Bounds colliderBounds;
    [SerializeField] CapsuleCollider leftHandCollider;
    float handInTime;
    [SerializeField] EnergyBarrier barrier;
    Material material;

    // Start is called before the first frame update
    void Start()
    {
        colliderBounds = detectorCollider.GetComponent<BoxCollider>().bounds;
        material = new Material(GetComponent<Renderer>().materials[2]);
        Material[] auxArray = { GetComponent<Renderer>().materials[0], GetComponent<Renderer>().materials[1], material };
        GetComponent<Renderer>().materials = auxArray;
        material.SetColor("_GridColor", new Color(0.35f, 1, 0.54f, 1));
    }

    // Update is called once per frame
    void Update()
    {
        if (colliderBounds.Contains(leftHandCollider.bounds.center))
        {
            handInTime += Time.deltaTime;

            if (handInTime >= 1)
            {
                handInTime = 1;
                barrier.PuzzleCompleted();
                enabled = false;
            }
            material.SetColor("_GridColor", new Color(0.35f , 1, 0.54f + 0.46f * handInTime, 1));
        }
        else if (handInTime > 0)
        {
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interruptor : MonoBehaviour
{
    [SerializeField] EnergyBarrier barrier;
    Material material;

    void Start()
    {
        material = new Material(GetComponent<Renderer>().material);
        GetComponent<Renderer>().material = material;
    }

    public void StartPuzzle()
    {
        barrier.PuzzleStarted();
    }

    public void PuzzleCompleted()
    {
        material.color = Color.green;
        barrier.PuzzleCompleted();
        enabled = false;
    }
}

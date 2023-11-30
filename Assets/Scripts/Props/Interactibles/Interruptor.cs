using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interruptor : Puzzle
{
    [SerializeField] EnergyBarrier barrier;
    Material material;

    void Start()
    {
        material = new Material(GetComponent<Renderer>().material);
        GetComponent<Renderer>().material = material;
    }

    public override void StartPuzzle()
    {
        barrier.PuzzleStarted();
    }

    public override void HitPuzzle(float damage, string projectileTag)
    {
        material.color = Color.green;
        barrier.PuzzleCompleted();
        enabled = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interruptor : Puzzle
{
    [SerializeField] Barrier barrier;
    [SerializeField] GameObject orb;
    Material material;

    void Start()
    {
        material = new Material(orb.GetComponent<Renderer>().material);
        orb.GetComponent<Renderer>().material = material;
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
        GetComponent<AudioSource>().Play();
    }
}

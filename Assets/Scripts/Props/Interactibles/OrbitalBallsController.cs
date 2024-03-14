using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalBallsController : Puzzle
{
    [SerializeField] Barrier barrier;
    [SerializeField] GameObject[] orbitalBalls;
    bool puzzleStarted = false;

    void Update()
    {
        if (!puzzleStarted) return;
        foreach (var ball in orbitalBalls)
        {
            if (ball.activeSelf) return;
        }

        barrier.PuzzleCompleted();
        enabled = false;
    }

    public override void StartPuzzle()
    {
        barrier.PuzzleStarted();
        foreach (var ball in orbitalBalls)
        {
            ball.SetActive(true);
        }
        puzzleStarted = true;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerCheckerPuzzle : MonoBehaviour
{
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PuzzleStarted()
    {
        animator.SetTrigger("Close");
    }

    public void PuzzleCompleted()
    {
        animator.SetTrigger("Open");
    }

    public void AnimationEnded()
    {
        gameObject.SetActive(false);
    }
}

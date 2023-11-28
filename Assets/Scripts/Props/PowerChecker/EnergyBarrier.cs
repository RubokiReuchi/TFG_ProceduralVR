using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBarrier : MonoBehaviour
{
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PuzzleStarted()
    {
        gameObject.SetActive(true);
        animator.SetTrigger("Close");
    }

    public void PuzzleCompleted()
    {
        animator.SetTrigger("Open");
        Invoke("AnimationEnded", 2.0f);
    }

    public void AnimationEnded()
    {
        gameObject.SetActive(false);
    }
}

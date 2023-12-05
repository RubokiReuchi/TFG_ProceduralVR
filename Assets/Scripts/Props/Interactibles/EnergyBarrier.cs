using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyBarrier : Barrier
{
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public override void PuzzleStarted()
    {
        gameObject.SetActive(true);
        StartCoroutine(Closeanim());
    }

    IEnumerator Closeanim()
    {
        yield return null;
        animator.SetTrigger("Close");
    }

    public override void PuzzleCompleted()
    {
        animator.SetTrigger("Open");
        Invoke("AnimationEnded", 2.0f);
    }

    public void AnimationEnded()
    {
        gameObject.SetActive(false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSpikesPuzzle : MonoBehaviour
{
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void StartPuzzle()
    {
        animator.enabled = true;
    }
}

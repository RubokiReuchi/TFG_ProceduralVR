using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Animator))]
public class LeftHand : MonoBehaviour
{
    [SerializeField] float animationSpeed;
    Animator animator;
    float indexTarget;
    float gripTarget;
    float indexCurrent;
    float gripCurrent;
    PlayerState state;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        state = PlayerState.instance;
    }

    // Update is called once per frame
    void Update()
    {
        AnimateHand();
        CalculateState();
    }

    public void SetIndex(float value)
    {
        indexTarget = value;
    }

    public void SetGrip(float value)
    {
        gripTarget = value;
    }

    void AnimateHand()
    {
        if (indexCurrent != indexTarget)
        {
            indexCurrent = Mathf.MoveTowards(indexCurrent, indexTarget, Time.deltaTime * animationSpeed);
            animator.SetFloat("Index", indexCurrent);
        }

        if (gripCurrent != gripTarget)
        {
            gripCurrent = Mathf.MoveTowards(gripCurrent, gripTarget, Time.deltaTime * animationSpeed);
            animator.SetFloat("Grip", gripCurrent);
        }
    }

    void CalculateState()
    {
        if (indexTarget > 0.9f && gripTarget > 0.9f) state.leftHandPose = LEFT_HAND_POSE.CLOSE;
        else if (indexTarget < 0.1f && gripTarget > 0.9f) state.leftHandPose = LEFT_HAND_POSE.INDEX;
        else if (indexTarget > 0.9f && gripTarget < 0.1f) state.leftHandPose = LEFT_HAND_POSE.OK;
        else state.leftHandPose = LEFT_HAND_POSE.OPEN;
    }
}

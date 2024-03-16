using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutantAccelerate : StateMachineBehaviour
{
    float accelerateDelay;
    float accelerateDuration;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        accelerateDelay = 0;
        accelerateDuration = 0;
        animator.speed = 0.3f;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (accelerateDelay < 0.75f)
        {
            accelerateDelay += Time.deltaTime;
        }
        else if (accelerateDuration < 0.7f)
        {
            accelerateDuration += Time.deltaTime;
            if (accelerateDuration > 0.7f) accelerateDuration = 0.7f;
            animator.speed = 0.3f + accelerateDuration;
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RightHand : MonoBehaviour
{
    [SerializeField] float animationSpeed;
    Animator handAnimator;
    float indexTarget;
    float indexCurrent;
    [SerializeField] Animator gunTriggerAnimator;
    float gunTriggerTarget;
    float gunTriggerCurrent;

    // Start is called before the first frame update
    void Start()
    {
        handAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        AnimateHand();
    }

    public void SetIndex(float value)
    {
        indexTarget = value;
        gunTriggerTarget = value;
    }

    public void Dash()
    {
        
    }

    void AnimateHand()
    {
        if (indexCurrent != indexTarget)
        {
            indexCurrent = Mathf.MoveTowards(indexCurrent, indexTarget, Time.deltaTime * animationSpeed);
            handAnimator.SetFloat("Trigger", indexCurrent);
        }
        if (gunTriggerCurrent != gunTriggerTarget)
        {
            gunTriggerCurrent = Mathf.MoveTowards(gunTriggerCurrent, gunTriggerTarget, Time.deltaTime * animationSpeed);
            gunTriggerAnimator.SetFloat("Trigger", gunTriggerCurrent);
        }
    }
}

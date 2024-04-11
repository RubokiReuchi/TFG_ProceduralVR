using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialExplanation : MonoBehaviour
{
    [SerializeField] Animator markAnimator;
    [SerializeField] Animator textAnimator;
    [SerializeField] float triggerDistance;
    bool showing = false;
    [SerializeField] bool active;
    [SerializeField] GameObject activeOnDestroy;

    void Start()
    {
        if (!active) GetComponent<Canvas>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!active && activeOnDestroy != null) return;
        if (!active)
        {
            GetComponent<Canvas>().enabled = true;
            active = true;
        }

        Vector3 distance = Camera.main.transform.position - transform.position;
        if (distance.magnitude < triggerDistance)
        {
            if (showing) return;
            markAnimator.SetBool("Shown", false);
            textAnimator.SetBool("Shown", true);
            showing = true;
        }
        else
        {
            if (!showing) return;
            markAnimator.SetBool("Shown", true);
            textAnimator.SetBool("Shown", false);
            showing = false;
        }
    }
}

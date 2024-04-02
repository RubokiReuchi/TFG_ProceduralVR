using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public class LobbyPanel : MonoBehaviour
{
    [SerializeField] Animator mark;
    [SerializeField] int numOfFadeAnimations;
    [SerializeField] float triggerDistance;
    [SerializeField] float delay;
    Animator[] animators;
    bool showing = false;
    Coroutine coroutine;

    // Start is called before the first frame update
    void Start()
    {
        animators = GetComponentsInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 distance = Camera.main.transform.position - transform.position;
        if (distance.magnitude < triggerDistance)
        {
            if (showing) return;
            if (coroutine != null) StopCoroutine(coroutine);
            coroutine = StartCoroutine(Swap(true));
        }
        else
        {
            if (!showing) return;
            if (coroutine != null) StopCoroutine(coroutine);
            coroutine = StartCoroutine(Swap(false));
        }
    }

    public IEnumerator Swap(bool show)
    {
        showing = show;
        mark.SetBool("Shown", !showing);
        yield return new WaitForSeconds(delay);
        foreach (var animator in animators)
        {
            animator.SetInteger("Value", Random.Range(0, numOfFadeAnimations));
            animator.SetBool("Shown", showing);
            yield return new WaitForSeconds(delay);
        }
    }
}

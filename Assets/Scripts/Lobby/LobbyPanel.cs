using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Animations;

public class LobbyPanel : MonoBehaviour
{
    [SerializeField] Animator mark;
    [SerializeField] IncreaseDescription description;
    [SerializeField] int numOfFadeAnimations;
    [SerializeField] float delay;
    Animator[] animators;
    bool showing = false;
    Coroutine coroutine;

    // Start is called before the first frame update
    void Start()
    {
        animators = GetComponentsInChildren<Animator>();
        Shuffle(animators);
    }

    // Update is called once per frame
    public void Display(bool open)
    {
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(Swap(open));
    }

    public IEnumerator Swap(bool show)
    {
        showing = show;
        if (mark && showing) mark.SetBool("Shown", false);
        description.HidePanel();
        yield return new WaitForSeconds(delay);
        foreach (var animator in animators)
        {
            animator.SetInteger("Value", Random.Range(0, numOfFadeAnimations));
            animator.SetBool("Shown", showing);
            animator.SetBool("Backed", false);
            yield return new WaitForSeconds(delay);
        }
        if (mark && !showing) mark.SetBool("Shown", true);
    }

    public void MoveBack()
    {
        foreach (var animator in animators)
        {
            animator.SetInteger("Value", -1);
            animator.SetBool("Backed", true);
        }
    }

    public void MoveFront()
    {
        foreach (var animator in animators)
        {
            animator.SetTrigger("MoveFront");
            animator.SetBool("Backed", false);
        }
    }

    void Shuffle(Animator[] elements)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < elements.Length; t++)
        {
            Animator tmp = elements[t];
            int r = Random.Range(t, elements.Length);
            elements[t] = elements[r];
            elements[r] = tmp;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class StatusMenuPoints : MonoBehaviour
{
    public static StatusMenuPoints instance;

    public GameObject[] exclamations;
    public GameObject[] panels;
    int currentIndex = -1; // -1 --> no selected

    [SerializeField] GameObject leftArrow;
    [SerializeField] GameObject rightArrow;

    private void Awake()
    {
        instance = this;
    }

    public void SelectPoint(int index)
    {
        panels[index].SetActive(true);
        panels[index].GetComponent<Animator>().SetBool("Opened", true);
        currentIndex = index;

        leftArrow.SetActive(false);
        rightArrow.SetActive(false);

        if (exclamations[index].activeSelf) exclamations[index].SetActive(false);
    }


    public void ClosePanel()
    {
        StartCoroutine(ClosePanelCo(currentIndex));
        currentIndex = -1;

        leftArrow.SetActive(true);
        rightArrow.SetActive(true);
    }

    IEnumerator ClosePanelCo(int index)
    {
        panels[index].GetComponent<Animator>().SetBool("Opened", false);
        yield return new WaitForSeconds(0.35f);
        panels[index].SetActive(false);
    }

    public void SetExclamation(int panel)
    {
        exclamations[panel].SetActive(true);
    }
}

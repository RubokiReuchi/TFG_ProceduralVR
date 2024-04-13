using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusMenuPoints : MonoBehaviour
{
    [SerializeField] GameObject[] panels;
    int currentIndex = -1; // -1 --> no selected

    public void SelectPoint(int index)
    {
        panels[index].SetActive(true);
        panels[index].GetComponent<Animator>().SetBool("Opened", true);
        currentIndex = index;
    }


    public void ClosePanel()
    {
        StartCoroutine(ClosePanelCo(currentIndex));
        currentIndex = -1;
    }

    IEnumerator ClosePanelCo(int index)
    {
        panels[index].GetComponent<Animator>().SetBool("Opened", false);
        yield return new WaitForSeconds(0.35f);
        panels[index].SetActive(false);
    }
}

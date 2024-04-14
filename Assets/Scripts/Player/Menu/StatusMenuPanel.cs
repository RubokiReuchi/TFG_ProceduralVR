using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.EventSystems;

public class StatusMenuPanel : MonoBehaviour
{
    [SerializeField] GameObject[] buttons;
    [SerializeField] GameObject[] panel;
    int currentIndex = 0;

    private void OnEnable()
    {
        panel[currentIndex].SetActive(true);
        panel[currentIndex].GetComponent<Animator>().SetBool("Opened", true);
        EventSystem.current.SetSelectedGameObject(buttons[currentIndex]);
    }

    public void SelectButton(int index)
    {
        if (index == currentIndex) return;
        StartCoroutine(OpenMenu(index, currentIndex));
        currentIndex = index;
    }

    IEnumerator OpenMenu(int newIndex, int oldIndex)
    {
        panel[oldIndex].GetComponent<Animator>().SetBool("Opened", false);
        yield return new WaitForSeconds(0.3f);
        panel[newIndex].SetActive(true);
        panel[newIndex].GetComponent<Animator>().SetBool("Opened", true);
        panel[oldIndex].SetActive(false);
    }

    public void UpdateInformation(int index, float[] values)
    {
        if (!buttons[index].activeSelf) buttons[index].SetActive(true);
        if (values != null) panel[index].GetComponent<StatusMenuDescription>().UpdateData(values);
    }
}

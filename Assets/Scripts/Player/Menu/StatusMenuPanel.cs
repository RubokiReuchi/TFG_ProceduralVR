using System;
using System.Collections;
using UnityEngine;

public class StatusMenuPanel : MonoBehaviour
{
    [SerializeField] GameObject[] buttons;
    [SerializeField] GameObject[] panel;
    [SerializeField] GameObject mark;
    int currentIndex = 0;

    private void OnEnable()
    {
        if (!buttons[currentIndex].activeSelf)
        {
            mark.SetActive(false);
            return;
        }
        mark.SetActive(true);
        panel[currentIndex].SetActive(true);
        panel[currentIndex].GetComponent<Animator>().SetBool("Opened", true);
    }

    private void Update()
    {
        if (mark.activeSelf) mark.transform.position = buttons[currentIndex].transform.position;
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

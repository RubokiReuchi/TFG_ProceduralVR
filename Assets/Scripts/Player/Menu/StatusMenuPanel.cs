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
        int currentButton = (currentIndex > 8) ? 8 : currentIndex;
        EventSystem.current.SetSelectedGameObject(buttons[currentButton]);
    }

    public void SelectButton(int index)
    {
        if (index == currentIndex) return;
        if (index == 8)
        {
            PROJECTILE_TYPE projectileType = GameObject.Find("RightHand").GetComponent<PlayerGun>().projectileType;
            switch (projectileType)
            {
                case PROJECTILE_TYPE.AUTOMATIC: index = 8; break;
                case PROJECTILE_TYPE.TRIPLE: index = 9; break;
                case PROJECTILE_TYPE.MISSILE: index = 10; break;
                default: Debug.LogError("Something does wrong!"); break;
            }
        }
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
        if (index == 10) buttons[8].GetComponentInChildren<TextMeshProUGUI>().text = "Missile Mode";
        else if (index == 9) buttons[8].GetComponentInChildren<TextMeshProUGUI>().text = "Triple Shot Mode";
        else if (index == 8) buttons[8].GetComponentInChildren<TextMeshProUGUI>().text = "Automatic Mode";
        int currentButton = (index > 8) ? 8 : index;
        if (!buttons[currentButton].activeSelf) buttons[currentButton].SetActive(true);
        if (values != null) panel[index].GetComponent<StatusMenuDescription>().UpdateData(values);
    }
}

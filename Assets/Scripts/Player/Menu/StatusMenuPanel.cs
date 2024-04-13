using System;
using System.Collections;
using System.Collections.Generic;
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
}

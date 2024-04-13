using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusMenuPanel : MonoBehaviour
{
    [SerializeField] GameObject[] buttons;
    [SerializeField] GameObject[] panel;
    int currentIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
        OpenMenu(index, currentIndex);
        currentIndex = index;
    }

    IEnumerator OpenMenu(int newIndex, int oldIndex)
    {
        panel[oldIndex].GetComponent<Animator>().SetBool("Opened", false);
        yield return new WaitForSeconds(0.3f);
        panel[newIndex].GetComponent<Animator>().SetBool("Opened", true);
        panel[newIndex].SetActive(true);
        panel[oldIndex].SetActive(false);
    }
}

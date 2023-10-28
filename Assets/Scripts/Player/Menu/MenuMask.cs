using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class MenuMask : MonoBehaviour
{
    public RectTransform menusParentTransform;
    public GameObject[] menus;
    int indexOnScreen = 0;

    float targetPosition;
    [SerializeField] float transitionSpeed;

    // Start is called before the first frame update
    void Start()
    {
        SetUp();
    }

    // Update is called once per frame
    void Update()
    {
        if (targetPosition != menusParentTransform.position.x)
        {
            if (targetPosition + 1 > menusParentTransform.localPosition.x && targetPosition - 1 < menusParentTransform.localPosition.x) menusParentTransform.localPosition = new Vector3(targetPosition, menusParentTransform.localPosition.y, menusParentTransform.localPosition.z);
            else if (targetPosition < menusParentTransform.localPosition.x) menusParentTransform.localPosition -= Vector3.right * Time.deltaTime * transitionSpeed;
            else menusParentTransform.localPosition += Vector3.right * Time.deltaTime * transitionSpeed;
        }

        if (Input.GetKeyDown(KeyCode.E)) PrevMenu();
        if (Input.GetKeyDown(KeyCode.R)) NextMenu();
    }

    void SetUp()
    {
        for (int i = 0; i < menus.Length - 1; i++)
        {
            menus[i].GetComponent<RectTransform>().localPosition = i * 100 * Vector3.right;
        }
        menus[menus.Length - 1].GetComponent<RectTransform>().localPosition = Vector3.right * -100;
    }

    public void NextMenu()
    {
        if (targetPosition == menusParentTransform.localPosition.x) targetPosition = menusParentTransform.localPosition.x - 100;
        UpdateIndexOnScreen(1);
    }

    public void PrevMenu()
    {
        if (targetPosition == menusParentTransform.localPosition.x) targetPosition = menusParentTransform.localPosition.x + 100;
        UpdateIndexOnScreen(-1);
    }

    void UpdateIndexOnScreen(int value)
    {
        indexOnScreen += value;
        if (indexOnScreen < 0) indexOnScreen = menus.Length - 1;
        else if (indexOnScreen > menus.Length - 1) indexOnScreen = 0;

        if (value == 1 && indexOnScreen == FindHead())
        {
            menus[FindTail()].GetComponent<RectTransform>().localPosition = menus[indexOnScreen].GetComponent<RectTransform>().localPosition + Vector3.right * 100;
        }
        if (value == -1 && indexOnScreen == FindTail())
        {
            menus[FindHead()].GetComponent<RectTransform>().localPosition = menus[indexOnScreen].GetComponent<RectTransform>().localPosition - Vector3.right * 100;
        }
    }

    int FindHead() // return head index
    {
        int index = 0;
        for (int i = 0; i < menus.Length; i++)
        {
            int aux = index + 1;
            if (aux == menus.Length) aux = 0;
            if (menus[index].GetComponent<RectTransform>().localPosition.x < menus[aux].GetComponent<RectTransform>().localPosition.x)
            {
                index++;
            }
            else return index;
        }
        return index;
    }

    int FindTail() // return tail index
    {
        int index = menus.Length - 1;
        for (int i = 0; i < menus.Length; i++)
        {
            int aux = index - 1;
            if (aux == -1) aux = menus.Length - 1;
            if (menus[index].GetComponent<RectTransform>().localPosition.x > menus[aux].GetComponent<RectTransform>().localPosition.x)
            {
                index--;
            }
            else return index;
        }
        return index;
    }
}

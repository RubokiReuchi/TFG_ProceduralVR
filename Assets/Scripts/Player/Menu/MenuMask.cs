using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class MenuMask : MonoBehaviour
{
    public GameObject[] menus;
    int indexOnScreen = 0;

    float targetScale;
    [SerializeField] float transitionSpeed;

    // Start is called before the first frame update
    void Start()
    {
        SetUp();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetUp()
    {
        for (int i = 1; i < menus.Length; i++)
        {
            menus[i].GetComponent<RectTransform>().localScale = Vector3.zero;
        }
    }

    public void NextMenu()
    {
        StopAllCoroutines();
        StartCoroutine(ChangeMenuCo(1));
    }

    public void PrevMenu()
    {
        StopAllCoroutines();
        StartCoroutine(ChangeMenuCo(-1));
    }

    void UpdateIndexOnScreen(int value)
    {
        indexOnScreen += value;
        if (indexOnScreen < 0) indexOnScreen = menus.Length - 1;
        else if (indexOnScreen > menus.Length - 1) indexOnScreen = 0;
    }

    IEnumerator ChangeMenuCo(int value)
    {
        targetScale = 0;
        RectTransform rect = menus[indexOnScreen].GetComponent<RectTransform>();
        while (targetScale != rect.localScale.x)
        {
            rect.localScale -= Time.deltaTime * transitionSpeed * Vector3.one;
            if (rect.localScale.x < 0) rect.localScale = Vector3.zero;
            yield return null;
        }

        UpdateIndexOnScreen(value);

        targetScale = 1;
        rect = menus[indexOnScreen].GetComponent<RectTransform>();
        while (targetScale != rect.localScale.x)
        {
            rect.localScale += Time.deltaTime * transitionSpeed * Vector3.one;
            if (rect.localScale.x > 1) rect.localScale = Vector3.one;
            yield return null;
        }
    }
}

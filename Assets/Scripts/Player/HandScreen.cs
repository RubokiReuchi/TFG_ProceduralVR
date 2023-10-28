using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandScreen : MonoBehaviour
{
    [Header("Gun Type")]
    [SerializeField] List<MeshRenderer> screenBlocks;
    [SerializeField] Material yellowMat, blueMat, redMat, purpleMat, greenMat;

    [Header("Menu")]
    [SerializeField] Material openMenuMat;
    [SerializeField] float openMenuSpeed;
    [NonEditable][SerializeField] bool menuOpened;

    // Start is called before the first frame update
    void Start()
    {
        openMenuMat.SetFloat("_DisplayHeight", 0);
        menuOpened = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenMenu()
    {
        if (!menuOpened) StartCoroutine(OpenMenuCo());
        else StartCoroutine(CloseMenuCo());
    }

    IEnumerator OpenMenuCo()
    {
        float openMenuProgress = openMenuMat.GetFloat("_DisplayHeight");
        while (openMenuProgress < 1)
        {
            openMenuProgress += Time.deltaTime * openMenuSpeed;
            if (openMenuProgress > 1) openMenuProgress = 1;
            openMenuMat.SetFloat("_DisplayHeight", openMenuProgress);
            yield return null;
        }
    }

    public IEnumerator CloseMenuCo()
    {
        float openMenuProgress = openMenuMat.GetFloat("_DisplayHeight");
        while (openMenuProgress < 1)
        {
            openMenuProgress += Time.deltaTime * openMenuSpeed;
            if (openMenuProgress > 1) openMenuProgress = 1;
            openMenuMat.SetFloat("_DisplayHeight", openMenuProgress);
            yield return null;
        }
    }

    public void SetScreen(GUN_TYPE type)
    {
        for (int i = 0; i < 10; i++)
        {
            switch (type)
            {
                case GUN_TYPE.YELLOW:
                    screenBlocks[i].GetComponent<MeshRenderer>().material = yellowMat;
                    break;
                case GUN_TYPE.BLUE:
                    screenBlocks[i].GetComponent<MeshRenderer>().material = blueMat;
                    break;
                case GUN_TYPE.RED:
                    screenBlocks[i].GetComponent<MeshRenderer>().material = redMat;
                    break;
                case GUN_TYPE.PURPLE:
                    screenBlocks[i].GetComponent<MeshRenderer>().material = purpleMat;
                    break;
                case GUN_TYPE.GREEN:
                    screenBlocks[i].GetComponent<MeshRenderer>().material = greenMat;
                    break;
                default:
                    break;
            }
        }
    }
}

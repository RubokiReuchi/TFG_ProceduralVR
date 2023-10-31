using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandScreen : MonoBehaviour
{
    [Header("Gun Type")]
    [SerializeField] List<MeshRenderer> screenBlocks;
    [SerializeField] Material yellowMat, blueMat, redMat, purpleMat, greenMat;

    [Header("Menu")]
    [SerializeField] Material openMenuMat;
    [SerializeField] Material panelMenuMat;
    [SerializeField] Material frameMenuMat;
    [SerializeField] Material backgroundMenuMat;
    [SerializeField] Image arrow1MenuImage;
    [SerializeField] Image arrow2MenuImage;
    Material arrows1MenuMat;
    Material arrows2MenuMat;
    [SerializeField] RectTransform mapFix;
    [SerializeField] RectTransform map;
    Vector3 saveMapPosition;
    [SerializeField] TextMeshProUGUI[] texts;
    List<float> textsFontSize = new();
    float fontSizePercentage = 0;
    [SerializeField] float openMenuSpeed;
    [SerializeField] float textSpeed;
    [NonEditable][SerializeField] bool menuOpened;

    // Start is called before the first frame update
    void Start()
    {
        openMenuMat.SetFloat("_DisplayHeight", 0);
        panelMenuMat.SetFloat("_DisplayHeight", 0);
        frameMenuMat.SetFloat("_DisplayHeight", 0);
        backgroundMenuMat.SetFloat("_DisplayHeight", 0);
        arrows1MenuMat = arrow1MenuImage.material;
        arrows2MenuMat = arrow2MenuImage.material;
        arrows1MenuMat.SetFloat("_DisplayHeight", 0);
        arrows2MenuMat.SetFloat("_DisplayHeight", 0);
        mapFix.localScale = new Vector3(1, 0, 1);
        saveMapPosition = Vector3.zero;

        for (int i = 0; i < texts.Length; i++)
        {
            textsFontSize.Add(texts[i].fontSize);
            texts[i].fontSize = 0;
        }

        menuOpened = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OpenMenu()
    {
        StopAllCoroutines();
        if (!menuOpened) StartCoroutine(OpenMenuCo());
        else StartCoroutine(CloseMenuCo());
    }

    IEnumerator OpenMenuCo()
    {
        menuOpened = true;

        float openMenuProgress = openMenuMat.GetFloat("_DisplayHeight");
        while (openMenuProgress < 0.999f) // ramp
        {
            openMenuProgress += Time.deltaTime * openMenuSpeed;
            if (openMenuProgress > 0.999f) openMenuProgress = 0.999f;
            openMenuMat.SetFloat("_DisplayHeight", openMenuProgress);
            yield return null;
        }

        float panelMenuProgress = panelMenuMat.GetFloat("_DisplayHeight");
        float arrowsMenuProgress = arrows1MenuMat.GetFloat("_DisplayHeight");
        while (panelMenuProgress < 0.999f) // panel
        {
            panelMenuProgress += Time.deltaTime * openMenuSpeed;
            if (panelMenuProgress > 0.999f) panelMenuProgress = 0.999f;
            panelMenuMat.SetFloat("_DisplayHeight", panelMenuProgress);
            frameMenuMat.SetFloat("_DisplayHeight", panelMenuProgress);
            backgroundMenuMat.SetFloat("_DisplayHeight", panelMenuProgress);
            if (panelMenuProgress > 0.35f && panelMenuProgress < 0.65f)
            {
                arrowsMenuProgress += Time.deltaTime * openMenuSpeed * 3;
                if (arrowsMenuProgress > 0.9f) arrowsMenuProgress = 1;
                arrows1MenuMat.SetFloat("_DisplayHeight", arrowsMenuProgress);
                arrows2MenuMat.SetFloat("_DisplayHeight", arrowsMenuProgress);
            }
            else if (panelMenuProgress >= 0.65f && arrowsMenuProgress != 1)
            {
                arrowsMenuProgress = 1;
                arrows1MenuMat.SetFloat("_DisplayHeight", arrowsMenuProgress);
                arrows2MenuMat.SetFloat("_DisplayHeight", arrowsMenuProgress);
            }
            mapFix.localScale = new Vector3(1, panelMenuProgress, 1);
            map.localPosition = saveMapPosition;
            yield return null;
        }

        while (fontSizePercentage < 1) // texts
        {
            fontSizePercentage += Time.deltaTime * textSpeed;
            if (fontSizePercentage > 1) fontSizePercentage = 1;
            for (int i = 0; i < texts.Length; i++) texts[i].fontSize = fontSizePercentage * textsFontSize[i];
            yield return null;
        }
    }

    public IEnumerator CloseMenuCo()
    {
        menuOpened = false;
        saveMapPosition = map.localPosition;

        while (fontSizePercentage > 0) // texts
        {
            fontSizePercentage -= Time.deltaTime * textSpeed;
            if (fontSizePercentage < 0) fontSizePercentage = 0;
            for (int i = 0; i < texts.Length; i++) texts[i].fontSize = fontSizePercentage * textsFontSize[i];
            yield return null;
        }

        float panelMenuProgress = panelMenuMat.GetFloat("_DisplayHeight");
        float arrowsMenuProgress = arrows1MenuMat.GetFloat("_DisplayHeight");
        while (panelMenuProgress > 0) // panel
        {
            panelMenuProgress -= Time.deltaTime * openMenuSpeed;
            if (panelMenuProgress < 0) panelMenuProgress = 0;
            panelMenuMat.SetFloat("_DisplayHeight", panelMenuProgress);
            frameMenuMat.SetFloat("_DisplayHeight", panelMenuProgress);
            backgroundMenuMat.SetFloat("_DisplayHeight", panelMenuProgress);
            if (panelMenuProgress > 0.35f && panelMenuProgress < 0.65f)
            {
                arrowsMenuProgress -= Time.deltaTime * openMenuSpeed * 3;
                arrows1MenuMat.SetFloat("_DisplayHeight", arrowsMenuProgress);
                arrows2MenuMat.SetFloat("_DisplayHeight", arrowsMenuProgress);
            }
            else if (panelMenuProgress <= 0.35f && arrowsMenuProgress != 0)
            {
                arrowsMenuProgress = 0;
                arrows1MenuMat.SetFloat("_DisplayHeight", arrowsMenuProgress);
                arrows2MenuMat.SetFloat("_DisplayHeight", arrowsMenuProgress);
            }
            mapFix.localScale = new Vector3(1, panelMenuProgress, 1);
            yield return null;
        }

        float openMenuProgress = openMenuMat.GetFloat("_DisplayHeight");
        while (openMenuProgress > 0)
        {
            openMenuProgress -= Time.deltaTime * openMenuSpeed;
            if (openMenuProgress < 0) openMenuProgress = 0;
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

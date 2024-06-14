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
    [SerializeField] Animator menuAnimator;
    [SerializeField] Transform mapCenter;
    [SerializeField] Transform playerMark;
    [SerializeField] Transform centerPosition;
    ScrollRect scrollRect;
    Vector3 mapCenterTarget;
    [NonEditable][SerializeField] bool menuOpened;

    [Header("Audio")]
    AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        menuOpened = false;
        if (centerPosition != null)
        {
            scrollRect = centerPosition.GetComponent<ScrollRect>();
            mapCenter.position += centerPosition.position - playerMark.position;
        }

        audioManager = AudioManager.instance;
    }

    public void OpenMenu()
    {
        if (!menuOpened)
        {
            menuAnimator.SetBool("Opened", true);
            menuOpened = true;
            audioManager.PlaySound("OpenMenu");
        }
        else
        {
            menuAnimator.SetBool("Opened", false);
            menuOpened = false;
            audioManager.PlaySound("CloseMenu");
        }
    }

    public void CenterMap()
    {
        mapCenterTarget = mapCenter.position + centerPosition.position - playerMark.position;
        StartCoroutine(CenterMapCo());
    }

    IEnumerator CenterMapCo()
    {
        scrollRect.enabled = false;
        while (Vector3.Distance(mapCenter.position, mapCenterTarget) > 0.005f)
        {
            mapCenter.position += (centerPosition.position - playerMark.position) * 0.05f;
            yield return null;
        }
        scrollRect.enabled = true;
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

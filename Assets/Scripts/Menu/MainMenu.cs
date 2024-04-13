using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string fileName;
    [SerializeField] Animator platformAnimator;
    [SerializeField] Material fadeMat;
    [SerializeField] Animator newGameAnimator;
    [SerializeField] Animator continueAnimator;
    [SerializeField] CanvasGroup continueGroup;
    [SerializeField] Animator confirmAnimator;
    [SerializeField] Animator cancelAnimator;
    [SerializeField] Animator deleteAdviceAnimator;

    string fullPath;
    
    bool existingSaveFile = false;
    bool delete; // true --> new game, false --> exit

    // Start is called before the first frame update
    void Start()
    {
        fullPath = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(fullPath)) existingSaveFile = true;
        else
        {
            continueAnimator.enabled = false;
            continueGroup.alpha = 0.3f;
            continueGroup.interactable = false;
            continueGroup.blocksRaycasts = false;
        }
    }

    public void NewGame()
    {
        if (!existingSaveFile)
        {
            StartCoroutine(FadeOut(1)); // pre tutorial level
            platformAnimator.SetTrigger("Move");
            newGameAnimator.SetBool("Open", false);
            continueAnimator.SetBool("Open", false);
            confirmAnimator.SetBool("Open", false);
            cancelAnimator.SetBool("Open", false);
            deleteAdviceAnimator.SetBool("Open", false);
        }
        else
        {
            newGameAnimator.SetTrigger("Fade");
            newGameAnimator.SetBool("Open", false);
            continueAnimator.SetBool("Open", false);
            confirmAnimator.SetBool("Open", true);
            cancelAnimator.SetBool("Open", true);
            deleteAdviceAnimator.SetBool("Open", true);
            delete = true;
        }
    }

    public void Continue()
    {
        StartCoroutine(FadeOut(3)); // lobby level
        platformAnimator.SetTrigger("Move");
        newGameAnimator.SetBool("Open", false);
        continueAnimator.SetBool("Open", false);
        confirmAnimator.SetBool("Open", false);
        cancelAnimator.SetBool("Open", false);
        deleteAdviceAnimator.SetBool("Open", false);
    }

    public void Confirm()
    {
        if (delete)
        {
            File.Delete(fullPath);
            StartCoroutine(FadeOut(1)); // pre tutorial level
            platformAnimator.SetTrigger("Move");
            newGameAnimator.SetBool("Open", false);
            continueAnimator.SetBool("Open", false);
            confirmAnimator.SetBool("Open", false);
            cancelAnimator.SetBool("Open", false);
            deleteAdviceAnimator.SetBool("Open", false);
        }
        else
        {
            Application.Quit();
        }
    }

    public void Cancel()
    {
        newGameAnimator.SetBool("Open", true);
        continueAnimator.SetBool("Open", true);
        confirmAnimator.SetBool("Open", false);
        cancelAnimator.SetBool("Open", false);
        deleteAdviceAnimator.SetBool("Open", false);
    }

    IEnumerator FadeOut(int level)
    {
        yield return new WaitForSeconds(3.0f);

        float opacity = 0.0f;
        while (opacity < 1)
        {
            opacity += Time.deltaTime * 0.5f;
            if (opacity > 1) opacity = 1;
            fadeMat.SetFloat("_Opacity", opacity);
            yield return null;
        }

        SceneManager.LoadScene(level);
    }
}

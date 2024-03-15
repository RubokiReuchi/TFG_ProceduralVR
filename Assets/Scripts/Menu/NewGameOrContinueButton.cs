using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameOrContinueButton : MenuButton
{
    [SerializeField] string fileName;
    [SerializeField] TextMeshPro text;
    [SerializeField] Animator anim;
    [SerializeField] Material fadeMat;

    bool newGame = true;

    void Start()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(fullPath))
        {
            text.text = "Continue";
            newGame = false;
        }
    }

    public override void ButtonHitted()
    {
        StartCoroutine(FadeOut());

        anim.SetTrigger("Move");
    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(3.0f);

        float opacity = 0.0f;
        while (opacity < 1)
        {
            opacity += Time.deltaTime * 1;
            if (opacity > 1) opacity = 1;
            fadeMat.SetFloat("_Opacity", opacity);
            yield return null;
        }

        if (newGame)
        {
            // to tutorial
        }
        else
        {
            // to lobby
        }
    }
}

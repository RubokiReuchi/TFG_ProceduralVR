using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameButton : MenuButton
{
    [SerializeField] string fileName;
    [SerializeField] TextMeshPro text;
    [SerializeField] Animator anim;
    [SerializeField] Material fadeMat;

    string fullPath;

    bool existingSaveFile = false;
    bool sure = false;
    bool paused = false;

    void Start()
    {
        fullPath = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(fullPath))
        {
            existingSaveFile = true;
        }
    }

    public override void ButtonHitted()
    {
        if (!existingSaveFile)
        {
            StartCoroutine(FadeOut());
            anim.SetTrigger("Move");
        }
        else if (!paused)
        {
            if (!sure)
            {
                paused = true;
                text.text = "Existing data\nwill be deleted.\nAre you Sure?";
                Invoke("AreYouSure", 0.5f);
            }
            else
            {
                File.Delete(fullPath);
                StartCoroutine(FadeOut());
                anim.SetTrigger("Move");
            }
        }
    }

    IEnumerator FadeOut()
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

        SceneManager.LoadScene(1); // tutorial scene
    }

    void AreYouSure()
    {
        paused = false;
        sure = true;
        Invoke("NotSure", 3.0f);
    }

    void NotSure()
    {
        sure = false;
        text.text = "New Game";
    }
}

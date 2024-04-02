using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueButton : MenuButton
{
    [SerializeField] string fileName;
    [SerializeField] Animator anim;
    [SerializeField] Material fadeMat;

    void Start()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, fileName);
        if (!File.Exists(fullPath)) gameObject.SetActive(false);
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
            opacity += Time.deltaTime * 0.5f;
            if (opacity > 1) opacity = 1;
            fadeMat.SetFloat("_Opacity", opacity);
            yield return null;
        }

        SceneManager.LoadScene(2); // lobby scene
    }
}

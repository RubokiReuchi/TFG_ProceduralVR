using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PickGun : MonoBehaviour
{
    [SerializeField] Material fadeMaterial;
    bool activated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") && !activated) return;
        StartCoroutine(FadeIn());
        activated = true;
    }

    IEnumerator FadeIn()
    {
        float opacity = 0.0f;
        while (opacity < 1)
        {
            opacity += Time.deltaTime * 2.0f;
            if (opacity > 1) opacity = 1;
            fadeMaterial.SetFloat("_Opacity", opacity);
            yield return null;
        }
        SceneManager.LoadScene(2); // tutorial
    }
}

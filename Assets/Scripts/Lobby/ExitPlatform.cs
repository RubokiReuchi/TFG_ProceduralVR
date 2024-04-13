using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitPlatform : MonoBehaviour
{
    [SerializeField] GameObject playerLocomotionSystem;
    [SerializeField] Material fadeMat;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.parent = transform;
            other.GetComponent<PlayerMovement>().enabled = false;
            playerLocomotionSystem.SetActive(false);
            StartCoroutine(FadeOut());
            GetComponent<Animator>().SetTrigger("Move");
            DataPersistenceManager.instance.SaveGame();
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

        SceneManager.LoadScene(4); // level scene
    }
}

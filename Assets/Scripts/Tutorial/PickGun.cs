using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickGun : MonoBehaviour
{
    [SerializeField] GameObject playerWithoutGun;
    [SerializeField] GameObject player;
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
            opacity += Time.deltaTime * 1.0f;
            if (opacity > 1) opacity = 1;
            fadeMaterial.SetFloat("_Opacity", opacity);
            yield return null;
        }
        playerWithoutGun.SetActive(false);
        player.SetActive(true);
        player.transform.SetPositionAndRotation(playerWithoutGun.transform.position, playerWithoutGun.transform.rotation);
        Destroy(gameObject);
    }
}

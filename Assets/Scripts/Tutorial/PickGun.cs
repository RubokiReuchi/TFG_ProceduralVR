using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickGun : MonoBehaviour
{
    [SerializeField] GameObject playerWithoutGun;
    [SerializeField] GameObject player;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerWithoutGun.SetActive(false);
        player.SetActive(true);
        player.transform.SetPositionAndRotation(playerWithoutGun.transform.position, playerWithoutGun.transform.rotation);
        Destroy(gameObject);
    }
}

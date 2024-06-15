using System.Collections;
using UnityEngine;

public class EnterPlatform : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] GameObject playerLocomotionSystem;

    [SerializeField] AudioSource platformSource;
    [SerializeField] AudioSource tubeSource;

    public void PlayerCanMove()
    {
        playerLocomotionSystem.SetActive(true);
        player.GetComponent<PlayerMovement>().enabled = true;
        player.transform.parent = null;
        Destroy(gameObject);
    }

    public void PlayTubeSound()
    {
        tubeSource.Play();
    }
}

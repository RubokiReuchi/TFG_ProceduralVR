using UnityEngine;

public class EnterPlatform : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] GameObject playerLocomotionSystem;

    public void PlayerCanMove()
    {
        playerLocomotionSystem.SetActive(true);
        player.GetComponent<PlayerMovement>().enabled = true;
        player.transform.parent = null;
        Destroy(gameObject);
    }
}

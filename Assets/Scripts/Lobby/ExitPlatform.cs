using UnityEngine;

public class ExitPlatform : MonoBehaviour
{
    [SerializeField] GameObject playerLocomotionSystem;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.parent = transform;
            other.GetComponent<PlayerMovement>().enabled = false;
            playerLocomotionSystem.SetActive(false);
            GetComponent<Animator>().SetTrigger("Move");
        }
    }
}

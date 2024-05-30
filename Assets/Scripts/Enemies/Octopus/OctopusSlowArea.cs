using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctopusSlowArea : MonoBehaviour
{
    [SerializeField] float slowPercentage;
    [SerializeField] float slowDuration;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.root.GetComponentInChildren<PlayerMovement>().TakeSlow(slowPercentage, slowDuration);
        }
    }
}

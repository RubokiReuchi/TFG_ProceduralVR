using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateQuad : MonoBehaviour
{
    GateBehaviour gateBehaviour;

    void Start()
    {
        gateBehaviour = transform.parent.GetComponent<GateBehaviour>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (gateBehaviour.opened) return;
        if (!collision.gameObject.CompareTag("YellowProjectile") && !collision.gameObject.CompareTag("BlueProjectile") && !collision.gameObject.CompareTag("RedProjectile") && !collision.gameObject.CompareTag("PurpleProjectile") && !collision.gameObject.CompareTag("GreenProjectile")) return;

        gateBehaviour.OpenTriggered(collision.gameObject);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitalBall : MonoBehaviour
{
    [SerializeField] Transform rotateAround;

    // Update is called once per frame
    void Update()
    {
        transform.RotateAround(rotateAround.position, Vector3.up, 1.0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody.CompareTag("ShieldedHand"))
        {
            rotateAround.gameObject.SetActive(false);
            AudioManager.instance.PlaySound("DeflectProjectile");
        }
    }
}

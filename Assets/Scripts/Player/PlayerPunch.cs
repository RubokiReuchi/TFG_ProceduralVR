using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPunch : MonoBehaviour
{
    [SerializeField] Rigidbody rigidBody;
    [SerializeField] float punchForce;
    [SerializeField] float giantPunchForce;
    [HideInInspector] public bool giantPunch = false;

    private void OnTriggerEnter(Collider other)
    {
        Vector3 leftHandVelocity = rigidBody.velocity;
        if (PlayerState.instance.leftHandPose != LEFT_HAND_POSE.CLOSE || leftHandVelocity.magnitude < 2) return;

        if (other.CompareTag("Enemy"))
        {
            float finalForce = !giantPunch ? punchForce : giantPunchForce;
            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (Mathf.Abs(leftHandVelocity.y) > new Vector2(leftHandVelocity.x, leftHandVelocity.z).magnitude)
            {
                if (leftHandVelocity.y > 0)
                {
                    rb.AddForce(Vector3.up * finalForce, ForceMode.VelocityChange);
                }
                //else shield damage
            }
            else
            {
                rb.AddForce(new Vector3(leftHandVelocity.x, 0, leftHandVelocity.z).normalized * finalForce, ForceMode.VelocityChange);
            }
        }
    }
}

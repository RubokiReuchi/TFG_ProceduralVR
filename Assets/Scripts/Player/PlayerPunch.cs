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
        if (PlayerState.instance.leftHandPose != LEFT_HAND_POSE.CLOSE || leftHandVelocity.magnitude < 1.5f) return;

        if (other.CompareTag("Enemy"))
        {
            Enemy script = other.GetComponent<Enemy>();
            float finalForce = !giantPunch ? punchForce : giantPunchForce;
            if (Mathf.Abs(leftHandVelocity.y) > new Vector2(leftHandVelocity.x, leftHandVelocity.z).magnitude)
            {
                if (leftHandVelocity.y > 0)
                {
                    if (!script.verticalPushed)
                    {
                        script.Airbourne(finalForce, giantPunch);
                        script.verticalPushed = true;
                    }
                    if (script.hasShield)
                    {
                        float finalShieldDamage = !giantPunch ? 50 : 200;
                        script.shield.TakeDamage(finalShieldDamage);
                    }
                }
                else if (script.hasShield)
                {
                    float finalShieldDamage = !giantPunch ? 50 : 200;
                    script.shield.TakeDamage(finalShieldDamage);
                }
            }
            else
            {
                if (!script.horizontalPushed)
                {
                    script.Pushed(new Vector3(leftHandVelocity.x, 0, leftHandVelocity.z).normalized * finalForce, giantPunch);
                    script.horizontalPushed = true;
                }
                if (script.hasShield)
                {
                    float finalShieldDamage = !giantPunch ? 50 : 200;
                    script.shield.TakeDamage(finalShieldDamage);
                }
            }
        }
        else if (other.CompareTag("EnemyShield"))
        {
            EnemyShield script = other.GetComponent<EnemyShield>();
            float finalShieldDamage = !giantPunch ? 50 : 200;
            script.TakeDamage(finalShieldDamage);
        }
    }
}

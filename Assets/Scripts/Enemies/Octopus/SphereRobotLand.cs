using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SphereRobotLand : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("FoundationsF"))
        {
            GetComponent<SphereRobot>().enabled = true;
            GetComponent<NavMeshAgent>().enabled = true;
            GetComponent<Rigidbody>().mass = 10000.0f;
            enabled = false;
        }
    }
}

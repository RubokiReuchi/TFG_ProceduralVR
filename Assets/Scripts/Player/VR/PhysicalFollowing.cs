using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalFollowing : MonoBehaviour
{
    [SerializeField] Transform controller;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.velocity = (controller.position - transform.position) / Time.fixedDeltaTime * 10.0f;

        transform.rotation = controller.rotation;
    }
}

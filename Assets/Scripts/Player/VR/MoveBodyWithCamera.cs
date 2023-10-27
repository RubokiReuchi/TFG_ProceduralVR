using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBodyWithCamera : MonoBehaviour
{
    public Transform mainCamera;
    Vector3 positionOffset;

    // Start is called before the first frame update
    void Start()
    {
        positionOffset = transform.position - mainCamera.position;
    }

    // Update is called once per frame
    void Update()
    {
        // position
        transform.position = mainCamera.position + positionOffset;

        // rotation
        if (transform.rotation.y < mainCamera.rotation.y - 0.5f) transform.rotation = new Quaternion(0, mainCamera.rotation.y - 0.5f, 0, mainCamera.rotation.w);
        else if (transform.rotation.y > mainCamera.rotation.y + 0.5f) transform.rotation = new Quaternion(0, mainCamera.rotation.y + 0.5f, 0, mainCamera.rotation.w);
    }
}

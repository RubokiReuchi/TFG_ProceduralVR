using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBodyWithCamera : MonoBehaviour
{
    public GameObject mainCamera;
    Vector3 positionOffset;

    // Start is called before the first frame update
    void Start()
    {
        positionOffset = transform.position - mainCamera.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = mainCamera.transform.position + positionOffset;
        transform.rotation = new Quaternion(0, mainCamera.transform.rotation.y, 0, mainCamera.transform.rotation.w);
    }
}

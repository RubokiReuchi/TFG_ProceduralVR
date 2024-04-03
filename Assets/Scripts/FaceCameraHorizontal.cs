using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCameraHorizontal : MonoBehaviour
{
    GameObject cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera");
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(new Vector3(cam.transform.position.x, transform.position.y, cam.transform.position.z));
    }
}
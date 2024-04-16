using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositionOnMap : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Transform cam;

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = new Vector3(player.position.x / 3, player.position.z / 3, 0);
        transform.localRotation = new Quaternion(0, 0, -cam.rotation.y, cam.rotation.w);
    }
}

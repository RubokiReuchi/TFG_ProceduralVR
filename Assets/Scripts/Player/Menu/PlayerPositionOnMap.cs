using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositionOnMap : MonoBehaviour
{
    [SerializeField] Transform player;

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = new Vector3(transform.position.x / 3, transform.position.z / 3, 0);
    }
}

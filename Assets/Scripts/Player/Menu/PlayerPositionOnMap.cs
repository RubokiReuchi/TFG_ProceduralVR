using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPositionOnMap : MonoBehaviour
{
    [SerializeField] Transform player;

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = new Vector3(player.position.x / 3, player.position.z / 3, 0);
    }
}

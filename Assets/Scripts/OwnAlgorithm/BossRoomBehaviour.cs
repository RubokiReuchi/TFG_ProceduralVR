using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoomBehaviour : RoomBehaviour
{
    [HideInInspector] public RoomGenarator manager;
    public int width; // in tiles
    public int height; // in tiles

    // Start is called before the first frame update
    void Start()
    {
        if (!RoomGenarator.instance.activeRooms.Contains(this)) gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 GetEnterDoorPosition()
    {
        return doorsTransform[0].position;
    }
}

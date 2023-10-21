using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OwnRoomGenarator : MonoBehaviour
{
    public ReadRoomsInfo roomsInfo;
    public GameObject startRoomPrefab;
    public GameObject[] bossRoomsPrefabs;
    public GameObject[] roomsPrefabs;

    int roomsNormalWidth = 7;
    int roomsNormalHeight = 5;

    [Header("Rooms Between Start and Boss Rooms")]
    [Range(0, 9)][SerializeField] int roomsBetween;

    List<RoomBehaviour> roomsScripts = new();

    // Start is called before the first frame update
    void Start()
    {
        CreateMainPath();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateMainPath()
    {
        // start room
        GameObject room = GameObject.Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        RoomBehaviour script = room.GetComponent<RoomBehaviour>();
        script.manager = this;
        script.SetDoors();

        Door unfilledDoor;
        GameObject auxGO;

        // start-boss path rooms
        while (roomsBetween > 0)
        {
            if (script.doorsFilled) Debug.LogError("Logic Error");
            unfilledDoor = script.GetRandomUnfilledDoor();

            auxGO = CreateNextRoom(unfilledDoor);
            if (auxGO.name != "NULL_GAMEOBJECT")
            {
                unfilledDoor.state = DOOR_STATE.YELLOW;
                room = auxGO;
                script = room.GetComponent<RoomBehaviour>();
                roomsScripts.Add(script);
                roomsBetween--;
            }
            else
            {
                GameObject.Destroy(auxGO);
            }
        }

        // boss room
        if (script.doorsFilled) Debug.LogError("Logic Error");
        unfilledDoor = script.GetRandomUnfilledDoor();

        auxGO = CreateNextRoom(unfilledDoor);
        if (auxGO.name != "NULL_GAMEOBJECT")
        {
            unfilledDoor.state = DOOR_STATE.YELLOW;
        }
        else
        {
            GameObject.Destroy(auxGO);
        }
    }

    GameObject CreateNextRoom(Door door)
    {
        List<GameObject> posibleRooms = roomsPrefabs.ToList<GameObject>();
        int newRoomTypeID = -1;

        switch (door.direction)
        {
            case DOOR_DIRECTION.TOP:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == 0) return ReturnNullGameobjectAndDestroyDoor(door);

                    newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    RoomInfo roomInfo = roomsInfo.GetRoomByType(newRoomTypeID);

                    if (roomInfo.jointRoomTypeID == -1) // no joint room
                    {
                        Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + 2/*HallwaySize*/ + roomsNormalHeight);
                        Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth, 5, roomsNormalHeight));
                        if (colliding.Length == 0) // no room there
                        {
                            if (roomInfo.downDoor == 1) // if room has a door down
                            {
                                Vector3 roomPosition = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + 2);
                                GameObject newRoom = GameObject.Instantiate(posibleRooms[newRoomTypeID], roomPosition, Quaternion.identity);
                                RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                                script.SetDoors();
                                script.NullifyDoor(DOOR_DIRECTION.DOWN);
                                return newRoom;
                            }
                            else
                            {
                                posibleRooms.RemoveAt(newRoomTypeID);
                                newRoomTypeID = -1;
                                continue;
                            }
                        }
                        else
                        {
                            return ReturnNullGameobjectAndDestroyDoor(door);
                        }
                    }
                    else
                    {

                    }
                }
                break;
            case DOOR_DIRECTION.DOWN:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == 0) return ReturnNullGameobjectAndDestroyDoor(door);

                    newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    RoomInfo roomInfo = roomsInfo.GetRoomByType(newRoomTypeID);

                    if (roomInfo.jointRoomTypeID == -1) // no joint room
                    {
                        Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z - 2/*HallwaySize*/ - roomsNormalHeight);
                        Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth, 5, roomsNormalHeight));
                        if (colliding.Length == 0) // no room there
                        {
                            if (roomInfo.topDoor == 1) // if room has a door at top
                            {
                                Vector3 roomPosition = new Vector3(door.position.x, door.position.y/*0*/, door.position.z - 2 - roomsNormalHeight * 2);
                                GameObject newRoom = GameObject.Instantiate(posibleRooms[newRoomTypeID], roomPosition, Quaternion.identity);
                                RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                                script.SetDoors();
                                script.NullifyDoor(DOOR_DIRECTION.TOP);
                                return newRoom;
                            }
                            else
                            {
                                posibleRooms.RemoveAt(newRoomTypeID);
                                newRoomTypeID = -1;
                                continue;
                            }
                        }
                        else
                        {
                            return ReturnNullGameobjectAndDestroyDoor(door);
                        }
                    }
                    else
                    {

                    }
                }
                break;
            case DOOR_DIRECTION.RIGHT:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == 0) return ReturnNullGameobjectAndDestroyDoor(door);

                    newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    RoomInfo roomInfo = roomsInfo.GetRoomByType(newRoomTypeID);

                    if (roomInfo.jointRoomTypeID == -1) // no joint room
                    {
                        Vector3 roomCenter = new Vector3(door.position.x + 2/*HallwaySize*/ + roomsNormalWidth, door.position.y/*0*/, door.position.z);
                        Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth, 5, roomsNormalHeight));
                        if (colliding.Length == 0) // no room there
                        {
                            if (roomInfo.leftDoor == 1) // if room has a door at left
                            {
                                Vector3 roomPosition = new Vector3(door.position.x + 2 + roomsNormalWidth, door.position.y/*0*/, door.position.z - roomsNormalHeight);
                                GameObject newRoom = GameObject.Instantiate(posibleRooms[newRoomTypeID], roomPosition, Quaternion.identity);
                                RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                                script.SetDoors();
                                script.NullifyDoor(DOOR_DIRECTION.LEFT);
                                return newRoom;
                            }
                            else
                            {
                                posibleRooms.RemoveAt(newRoomTypeID);
                                newRoomTypeID = -1;
                                continue;
                            }
                        }
                        else
                        {
                            return ReturnNullGameobjectAndDestroyDoor(door);
                        }
                    }
                    else
                    {

                    }
                }
                break;
            case DOOR_DIRECTION.LEFT:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == 0) return ReturnNullGameobjectAndDestroyDoor(door);

                    newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    RoomInfo roomInfo = roomsInfo.GetRoomByType(newRoomTypeID);

                    if (roomInfo.jointRoomTypeID == -1) // no joint room
                    {
                        Vector3 roomCenter = new Vector3(door.position.x - 2/*HallwaySize*/ - roomsNormalWidth, door.position.y/*0*/, door.position.z);
                        Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth, 5, roomsNormalHeight));
                        if (colliding.Length == 0) // no room there
                        {
                            if (roomInfo.rightDoor == 1) // if room has a door at right
                            {
                                Vector3 roomPosition = new Vector3(door.position.x - 2 - roomsNormalWidth, door.position.y/*0*/, door.position.z - roomsNormalHeight);
                                GameObject newRoom = GameObject.Instantiate(posibleRooms[newRoomTypeID], roomPosition, Quaternion.identity);
                                RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                                script.SetDoors();
                                script.NullifyDoor(DOOR_DIRECTION.RIGHT);
                                return newRoom;
                            }
                            else
                            {
                                posibleRooms.RemoveAt(newRoomTypeID);
                                newRoomTypeID = -1;
                                continue;
                            }
                        }
                        else
                        {
                            return ReturnNullGameobjectAndDestroyDoor(door);
                        }
                    }
                    else
                    {

                    }
                }
                break;
        }
        return ReturnNullGameobjectAndDestroyDoor(door);
    }



    GameObject CreateBossRoom(Door door)
    {
        List<GameObject> posibleRooms = bossRoomsPrefabs.ToList<GameObject>();
        int newRoomTypeID = -1;

        switch (door.direction)
        {
            case DOOR_DIRECTION.TOP:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == 0) return ReturnNullGameobjectAndDestroyDoor(door);

                    newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    RoomInfo roomInfo = roomsInfo.GetRoomByType(newRoomTypeID);

                    if (roomInfo.jointRoomTypeID == -1) // no joint room
                    {
                        Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + 2/*HallwaySize*/ + roomsNormalHeight);
                        Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth, 5, roomsNormalHeight));
                        if (colliding.Length == 0) // no room there
                        {
                            if (roomInfo.downDoor == 1) // if room has a door down
                            {
                                Vector3 roomPosition = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + 2);
                                GameObject newRoom = GameObject.Instantiate(posibleRooms[newRoomTypeID], roomPosition, Quaternion.identity);
                                newRoom.GetComponent<RoomBehaviour>().NullifyDoor(DOOR_DIRECTION.DOWN);
                                return newRoom;
                            }
                            else
                            {
                                posibleRooms.RemoveAt(newRoomTypeID);
                                newRoomTypeID = -1;
                                continue;
                            }
                        }
                        else
                        {
                            return ReturnNullGameobjectAndDestroyDoor(door);
                        }
                    }
                    else
                    {

                    }
                }
                break;
            case DOOR_DIRECTION.DOWN:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == 0) return ReturnNullGameobjectAndDestroyDoor(door);

                    newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    RoomInfo roomInfo = roomsInfo.GetRoomByType(newRoomTypeID);

                    if (roomInfo.jointRoomTypeID == -1) // no joint room
                    {
                        Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z - 2/*HallwaySize*/ - roomsNormalHeight);
                        Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth, 5, roomsNormalHeight));
                        if (colliding.Length == 0) // no room there
                        {
                            if (roomInfo.topDoor == 1) // if room has a door at top
                            {
                                Vector3 roomPosition = new Vector3(door.position.x, door.position.y/*0*/, door.position.z - 2);
                                GameObject newRoom = GameObject.Instantiate(posibleRooms[newRoomTypeID], roomPosition, Quaternion.identity);
                                newRoom.GetComponent<RoomBehaviour>().NullifyDoor(DOOR_DIRECTION.TOP);
                                return newRoom;
                            }
                            else
                            {
                                posibleRooms.RemoveAt(newRoomTypeID);
                                newRoomTypeID = -1;
                                continue;
                            }
                        }
                        else
                        {
                            return ReturnNullGameobjectAndDestroyDoor(door);
                        }
                    }
                    else
                    {

                    }
                }
                break;
            case DOOR_DIRECTION.RIGHT:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == 0) return ReturnNullGameobjectAndDestroyDoor(door);

                    newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    RoomInfo roomInfo = roomsInfo.GetRoomByType(newRoomTypeID);

                    if (roomInfo.jointRoomTypeID == -1) // no joint room
                    {
                        Vector3 roomCenter = new Vector3(door.position.x + 2/*HallwaySize*/ + roomsNormalWidth, door.position.y/*0*/, door.position.z);
                        Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth, 5, roomsNormalHeight));
                        if (colliding.Length == 0) // no room there
                        {
                            if (roomInfo.leftDoor == 1) // if room has a door at left
                            {
                                Vector3 roomPosition = new Vector3(door.position.x + 2, door.position.y/*0*/, door.position.z);
                                GameObject newRoom = GameObject.Instantiate(posibleRooms[newRoomTypeID], roomPosition, Quaternion.identity);
                                newRoom.GetComponent<RoomBehaviour>().NullifyDoor(DOOR_DIRECTION.LEFT);
                                return newRoom;
                            }
                            else
                            {
                                posibleRooms.RemoveAt(newRoomTypeID);
                                newRoomTypeID = -1;
                                continue;
                            }
                        }
                        else
                        {
                            return ReturnNullGameobjectAndDestroyDoor(door);
                        }
                    }
                    else
                    {

                    }
                }
                break;
            case DOOR_DIRECTION.LEFT:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == 0) return ReturnNullGameobjectAndDestroyDoor(door);

                    newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    RoomInfo roomInfo = roomsInfo.GetRoomByType(newRoomTypeID);

                    if (roomInfo.jointRoomTypeID == -1) // no joint room
                    {
                        Vector3 roomCenter = new Vector3(door.position.x - 2/*HallwaySize*/ - roomsNormalWidth, door.position.y/*0*/, door.position.z);
                        Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth, 5, roomsNormalHeight));
                        if (colliding.Length == 0) // no room there
                        {
                            if (roomInfo.rightDoor == 1) // if room has a door at right
                            {
                                Vector3 roomPosition = new Vector3(door.position.x - 2, door.position.y/*0*/, door.position.z);
                                GameObject newRoom = GameObject.Instantiate(posibleRooms[newRoomTypeID], roomCenter, Quaternion.identity);
                                newRoom.GetComponent<RoomBehaviour>().NullifyDoor(DOOR_DIRECTION.RIGHT);
                                return newRoom;
                            }
                            else
                            {
                                posibleRooms.RemoveAt(newRoomTypeID);
                                newRoomTypeID = -1;
                                continue;
                            }
                        }
                        else
                        {
                            return ReturnNullGameobjectAndDestroyDoor(door);
                        }
                    }
                    else
                    {

                    }
                }
                break;
        }
        return ReturnNullGameobjectAndDestroyDoor(door);
    }

    GameObject ReturnNullGameobjectAndDestroyDoor(Door door)
    {
        door.state = DOOR_STATE.DESTROYED;
        return new GameObject("NULL_GAMEOBJECT");
    }
}

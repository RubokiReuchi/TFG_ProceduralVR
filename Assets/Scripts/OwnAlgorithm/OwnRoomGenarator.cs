using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum FOUR_DIRECTIONS
{
    TOP,
    DOWN,
    RIGHT,
    LEFT,
    NONE
}

public class OwnRoomGenarator : MonoBehaviour
{
    public ReadRoomsInfo roomsInfo;
    public GameObject startRoomPrefab;
    public GameObject[] roomsPrefabs;
    public GameObject[] bossRoomsPrefabs;
    public GameObject[] jointsPrefabs;

    int roomsNormalWidth = 7;
    int roomsNormalHeight = 5;

    [Header("Rooms Between Start and Boss Rooms")]
    [Range(0, 9)][SerializeField] int minRoomsBetween;
    [Range(0, 9)][SerializeField] int maxRoomsBetween;
    [NonEditable][SerializeField] public int totalRoomsBetween = 0;
    int roomsBetween = 0;

    List<RoomBehaviour> roomsScripts = new();

    int lastRoomCreated = -1; // make imposible to have the same room in sequence

    // Start is called before the first frame update
    void Start()
    {
        totalRoomsBetween = Random.Range(minRoomsBetween, maxRoomsBetween);
        roomsBetween = totalRoomsBetween;
        CreateMainPath();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    int GetScriptIndex(RoomBehaviour script)
    {
        for (int i = 0; i < roomsScripts.Count; i++)
        {
            if (script.GetInstanceID() == roomsScripts[i].GetInstanceID()) return i;
        }

        Debug.Log("Logic Error");
        return -1;
    }

    void CreateMainPath()
    {
        // start room
        GameObject room = GameObject.Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        RoomBehaviour script = room.GetComponent<RoomBehaviour>();
        script.manager = this;
        script.SetDoors();

        Door unfilledDoor;
        RoomBehaviour auxScript;

        // start-boss path rooms
        while (roomsBetween > 0)
        {
            if (script.GetDoorsFilled())
            {
                if (!FindRoomScriptWithDoors(ref script)) return;
            }
            unfilledDoor = script.GetRandomUnfilledDoor();

            auxScript = CreateNextRoom(unfilledDoor);
            if (auxScript != null)
            {
                lastRoomCreated = auxScript.roomTypeID;
                unfilledDoor.state = DOOR_STATE.YELLOW;
                script = auxScript;
                roomsBetween--;
            }
        }

        // boss room
        //if (script.doorsFilled)
        //{
        //    Debug.LogError("Logic Error");
        //    return;
        //}
        //unfilledDoor = script.GetRandomUnfilledDoor();

        //auxScript = CreateNextRoom(unfilledDoor);
        //if (auxScript != null)
        //{
        //    unfilledDoor.state = DOOR_STATE.YELLOW;
        //}
    }

    bool FindRoomScriptWithDoors(ref RoomBehaviour script)
    {
        RoomInfo roomInfo = roomsInfo.roomInfoList[script.roomTypeID];
        if (roomInfo.IsJointRoom())
        {
            int currentScriptIndex = GetScriptIndex(script);
            if (currentScriptIndex == -1)
            {
                Debug.LogError("Logic Error: Script not Stored on scriptList");
                return false;
            }

            for (int i = 1; i < roomInfo.jointRoomNumber; i++) // start at 1 to ignore it self
            {
                if (!roomsScripts[currentScriptIndex + i].GetDoorsFilled())
                {
                    script = roomsScripts[currentScriptIndex + i];
                    return true;
                }
            }

            Debug.LogError("Logic Error");
            return false;
        }
        else
        {
            // Retroceder en las salas
            return false;
        }
    }

    RoomBehaviour CreateNextRoom(Door door)
    {
        List<GameObject> posibleRooms = roomsPrefabs.ToList<GameObject>();
        List<int> imposibleRooms = new();
        if (lastRoomCreated != -1) imposibleRooms.Add(lastRoomCreated);
        int newRoomTypeID = -1;

        switch (door.direction)
        {
            case FOUR_DIRECTIONS.TOP:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == imposibleRooms.Count) return ReturnNullGameobjectAndHoldDoor(door);
                    do
                    {
                        newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    } while (imposibleRooms.Contains(newRoomTypeID));
                    RoomInfo roomInfo = roomsInfo.roomInfoList[newRoomTypeID];

                    if (!roomInfo.IsJointRoom()) // no joint room
                    {
                        Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + 2/*HallwaySize*/ + roomsNormalHeight);
                        Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth, 5, roomsNormalHeight));
                        if (colliding.Length == 0) // no room there
                        {
                            if (roomInfo.downDoor == 1) // if room has a door down
                            {
                                Vector3 roomPosition = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + 2);
                                GameObject newRoom = GameObject.Instantiate(roomsPrefabs[newRoomTypeID], roomPosition, Quaternion.identity);
                                RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                                script.SetDoors();
                                script.NullifyDoor(FOUR_DIRECTIONS.DOWN);
                                roomsScripts.Add(script);
                                return script;
                            }
                            else
                            {
                                imposibleRooms.Add(newRoomTypeID);
                                newRoomTypeID = -1;
                                continue;
                            }
                        }
                        else
                        {
                            return ReturnNullGameobjectAndHoldDoor(door);
                        }
                    }
                    else
                    {
                        if (roomInfo.downDoor != 1)
                        {
                            imposibleRooms.Add(newRoomTypeID);
                            newRoomTypeID = -1;
                            continue;
                        }

                        Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + 2/*HallwaySize*/ + roomsNormalHeight);
                        List<Vector3> localRooms = new(); // contains all central room points that form the joint
                        CalculateJointedRoomGrid(newRoomTypeID, roomCenter, ref localRooms);

                        if (JointRoomOverlapping(localRooms)) // no space for joint room
                        {
                            imposibleRooms.Add(newRoomTypeID);
                            newRoomTypeID = -1;
                            continue;
                        }
                        else
                        {
                            List<Vector3> storeRooms = new(); // prevent to duplicate rooms
                            int lastRoomScriptIndex = roomsScripts.Count - 1;
                            BuildJointRoom(newRoomTypeID, roomCenter, FOUR_DIRECTIONS.DOWN, storeRooms);
                            return roomsScripts[lastRoomScriptIndex + 1];
                        }
                    }
                }
                break;
            case FOUR_DIRECTIONS.DOWN:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == imposibleRooms.Count) return ReturnNullGameobjectAndHoldDoor(door);
                    do
                    {
                        newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    } while (imposibleRooms.Contains(newRoomTypeID));
                    RoomInfo roomInfo = roomsInfo.roomInfoList[newRoomTypeID];

                    if (!roomInfo.IsJointRoom()) // no joint room
                    {
                        Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z - 2/*HallwaySize*/ - roomsNormalHeight);
                        Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth, 5, roomsNormalHeight));
                        if (colliding.Length == 0) // no room there
                        {
                            if (roomInfo.topDoor == 1) // if room has a door at top
                            {
                                Vector3 roomPosition = new Vector3(door.position.x, door.position.y/*0*/, door.position.z - 2 - roomsNormalHeight * 2);
                                GameObject newRoom = GameObject.Instantiate(roomsPrefabs[newRoomTypeID], roomPosition, Quaternion.identity);
                                RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                                script.SetDoors();
                                script.NullifyDoor(FOUR_DIRECTIONS.TOP);
                                roomsScripts.Add(script);
                                return script;
                            }
                            else
                            {
                                imposibleRooms.Add(newRoomTypeID);
                                newRoomTypeID = -1;
                                continue;
                            }
                        }
                        else
                        {
                            return ReturnNullGameobjectAndHoldDoor(door);
                        }
                    }
                    else
                    {
                        if (roomInfo.topDoor != 1)
                        {
                            imposibleRooms.Add(newRoomTypeID);
                            newRoomTypeID = -1;
                            continue;
                        }

                        Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + 2/*HallwaySize*/ + roomsNormalHeight);
                        List<Vector3> localRooms = new(); // contains all central room points that form the joint
                        CalculateJointedRoomGrid(newRoomTypeID, roomCenter, ref localRooms);

                        if (JointRoomOverlapping(localRooms)) // no space for joint room
                        {
                            imposibleRooms.Add(newRoomTypeID);
                            newRoomTypeID = -1;
                            continue;
                        }
                        else
                        {
                            List<Vector3> storeRooms = new(); // prevent to duplicate rooms
                            int lastRoomScriptIndex = roomsScripts.Count - 1;
                            BuildJointRoom(newRoomTypeID, roomCenter, FOUR_DIRECTIONS.DOWN, storeRooms);
                            return roomsScripts[lastRoomScriptIndex + 1];
                        }
                    }
                }
                break;
            case FOUR_DIRECTIONS.RIGHT:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == imposibleRooms.Count) return ReturnNullGameobjectAndHoldDoor(door);
                    do
                    {
                        newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    } while (imposibleRooms.Contains(newRoomTypeID));
                    RoomInfo roomInfo = roomsInfo.roomInfoList[newRoomTypeID];

                    if (!roomInfo.IsJointRoom()) // no joint room
                    {
                        Vector3 roomCenter = new Vector3(door.position.x + 2/*HallwaySize*/ + roomsNormalWidth, door.position.y/*0*/, door.position.z);
                        Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth, 5, roomsNormalHeight));
                        if (colliding.Length == 0) // no room there
                        {
                            if (roomInfo.leftDoor == 1) // if room has a door at left
                            {
                                Vector3 roomPosition = new Vector3(door.position.x + 2 + roomsNormalWidth, door.position.y/*0*/, door.position.z - roomsNormalHeight);
                                GameObject newRoom = GameObject.Instantiate(roomsPrefabs[newRoomTypeID], roomPosition, Quaternion.identity);
                                RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                                script.SetDoors();
                                script.NullifyDoor(FOUR_DIRECTIONS.LEFT);
                                roomsScripts.Add(script);
                                return script;
                            }
                            else
                            {
                                imposibleRooms.Add(newRoomTypeID);
                                newRoomTypeID = -1;
                                continue;
                            }
                        }
                        else
                        {
                            return ReturnNullGameobjectAndHoldDoor(door);
                        }
                    }
                    else
                    {
                        if (roomInfo.leftDoor != 1)
                        {
                            imposibleRooms.Add(newRoomTypeID);
                            newRoomTypeID = -1;
                            continue;
                        }

                        Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + 2/*HallwaySize*/ + roomsNormalHeight);
                        List<Vector3> localRooms = new(); // contains all central room points that form the joint
                        CalculateJointedRoomGrid(newRoomTypeID, roomCenter, ref localRooms);

                        if (JointRoomOverlapping(localRooms)) // no space for joint room
                        {
                            imposibleRooms.Add(newRoomTypeID);
                            newRoomTypeID = -1;
                            continue;
                        }
                        else
                        {
                            List<Vector3> storeRooms = new(); // prevent to duplicate rooms
                            int lastRoomScriptIndex = roomsScripts.Count - 1;
                            BuildJointRoom(newRoomTypeID, roomCenter, FOUR_DIRECTIONS.DOWN, storeRooms);
                            return roomsScripts[lastRoomScriptIndex + 1];
                        }
                    }
                }
                break;
            case FOUR_DIRECTIONS.LEFT:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == imposibleRooms.Count) return ReturnNullGameobjectAndHoldDoor(door);
                    do
                    {
                        newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    } while (imposibleRooms.Contains(newRoomTypeID));
                    RoomInfo roomInfo = roomsInfo.roomInfoList[newRoomTypeID];

                    if (!roomInfo.IsJointRoom()) // no joint room
                    {
                        Vector3 roomCenter = new Vector3(door.position.x - 2/*HallwaySize*/ - roomsNormalWidth, door.position.y/*0*/, door.position.z);
                        Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth, 5, roomsNormalHeight));
                        if (colliding.Length == 0) // no room there
                        {
                            if (roomInfo.rightDoor == 1) // if room has a door at right
                            {
                                Vector3 roomPosition = new Vector3(door.position.x - 2 - roomsNormalWidth, door.position.y/*0*/, door.position.z - roomsNormalHeight);
                                GameObject newRoom = GameObject.Instantiate(roomsPrefabs[newRoomTypeID], roomPosition, Quaternion.identity);
                                RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                                script.SetDoors();
                                script.NullifyDoor(FOUR_DIRECTIONS.RIGHT);
                                roomsScripts.Add(script);
                                return script;
                            }
                            else
                            {
                                imposibleRooms.Add(newRoomTypeID);
                                newRoomTypeID = -1;
                                continue;
                            }
                        }
                        else
                        {
                            return ReturnNullGameobjectAndHoldDoor(door);
                        }
                    }
                    else
                    {
                        if (roomInfo.rightDoor != 1)
                        {
                            imposibleRooms.Add(newRoomTypeID);
                            newRoomTypeID = -1;
                            continue;
                        }

                        Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + 2/*HallwaySize*/ + roomsNormalHeight);
                        List<Vector3> localRooms = new(); // contains all central room points that form the joint
                        CalculateJointedRoomGrid(newRoomTypeID, roomCenter, ref localRooms);

                        if (JointRoomOverlapping(localRooms)) // no space for joint room
                        {
                            imposibleRooms.Add(newRoomTypeID);
                            newRoomTypeID = -1;
                            continue;
                        }
                        else
                        {
                            List<Vector3> storeRooms = new(); // prevent to duplicate rooms
                            int lastRoomScriptIndex = roomsScripts.Count - 1;
                            BuildJointRoom(newRoomTypeID, roomCenter, FOUR_DIRECTIONS.DOWN, storeRooms);
                            return roomsScripts[lastRoomScriptIndex + 1];
                        }
                    }
                }
                break;
        }
        return ReturnNullGameobjectAndHoldDoor(door);
    }

    RoomBehaviour ReturnNullGameobjectAndHoldDoor(Door door)
    {
        door.state = DOOR_STATE.HOLDED; // later I will check if door is next to another or next to the wall
        return null;
    }

    void CalculateJointedRoomGrid(int currentRoomTypeID, Vector3 currentGridLocation, ref List<Vector3> localRooms)
    {
        if (localRooms.Contains(currentGridLocation)) return;
        else localRooms.Add(currentGridLocation);

        RoomInfo currentRoomInfo = roomsInfo.roomInfoList[currentRoomTypeID];

        // top
        if (currentRoomInfo.jointRoomTypeIdTop != -1)
        {
            JointInfo jointInfo = roomsInfo.jointInfoList[currentRoomInfo.jointRoomTypeIdTop];
            if (jointInfo.head.objectTypeID == currentRoomTypeID) // room is the head
            {
                OBJECT_TYPE objectType = jointInfo.tail.objectType;
                switch (objectType)
                {
                    case OBJECT_TYPE.ROOM:
                    case OBJECT_TYPE.BOSS_ROOM:
                        Vector3 roomCenter = new Vector3(currentGridLocation.x, 0, currentGridLocation.z + 2 + roomsNormalHeight * 2);
                        CalculateJointedRoomGrid(jointInfo.tail.objectTypeID, roomCenter, ref localRooms);
                        break;
                    case OBJECT_TYPE.JOINT:
                        break;
                    default:
                        Debug.LogError("Data Store Error");
                        break;
                }
            }
            else if (jointInfo.tail.objectTypeID == currentRoomTypeID) // room is the tail
            {
                OBJECT_TYPE objectType = jointInfo.head.objectType;
                switch (objectType)
                {
                    case OBJECT_TYPE.ROOM:
                    case OBJECT_TYPE.BOSS_ROOM:
                        Vector3 roomCenter = new Vector3(currentGridLocation.x, 0, currentGridLocation.z + 2 + roomsNormalHeight * 2);
                        CalculateJointedRoomGrid(jointInfo.head.objectTypeID, roomCenter, ref localRooms);
                        break;
                    case OBJECT_TYPE.JOINT:
                        break;
                    default:
                        Debug.LogError("Data Store Error");
                        break;
                }
            }
            else
            {
                Debug.LogError("Logic Error");
            }
        }

        // down
        if (currentRoomInfo.jointRoomTypeIdDown != -1)
        {
            JointInfo jointInfo = roomsInfo.jointInfoList[currentRoomInfo.jointRoomTypeIdDown];
            if (jointInfo.head.objectTypeID == currentRoomTypeID) // room is the head
            {
                OBJECT_TYPE objectType = jointInfo.tail.objectType;
                switch (objectType)
                {
                    case OBJECT_TYPE.ROOM:
                    case OBJECT_TYPE.BOSS_ROOM:
                        Vector3 roomCenter = new Vector3(currentGridLocation.x, 0, currentGridLocation.z - 2 - roomsNormalHeight * 2);
                        CalculateJointedRoomGrid(jointInfo.tail.objectTypeID, roomCenter, ref localRooms);
                        break;
                    case OBJECT_TYPE.JOINT:
                        break;
                    default:
                        Debug.LogError("Data Store Error");
                        break;
                }
            }
            else if (jointInfo.tail.objectTypeID == currentRoomTypeID) // room is the tail
            {
                OBJECT_TYPE objectType = jointInfo.head.objectType;
                switch (objectType)
                {
                    case OBJECT_TYPE.ROOM:
                    case OBJECT_TYPE.BOSS_ROOM:
                        Vector3 roomCenter = new Vector3(currentGridLocation.x, 0, currentGridLocation.z - 2 - roomsNormalHeight * 2);
                        CalculateJointedRoomGrid(jointInfo.head.objectTypeID, roomCenter, ref localRooms);
                        break;
                    case OBJECT_TYPE.JOINT:
                        break;
                    default:
                        Debug.LogError("Data Store Error");
                        break;
                }
            }
            else
            {
                Debug.LogError("Logic Error");
            }
        }

        // right
        if (currentRoomInfo.jointRoomTypeIdRight != -1)
        {
            JointInfo jointInfo = roomsInfo.jointInfoList[currentRoomInfo.jointRoomTypeIdRight];
            if (jointInfo.head.objectTypeID == currentRoomTypeID) // room is the head
            {
                OBJECT_TYPE objectType = jointInfo.tail.objectType;
                switch (objectType)
                {
                    case OBJECT_TYPE.ROOM:
                    case OBJECT_TYPE.BOSS_ROOM:
                        Vector3 roomCenter = new Vector3(currentGridLocation.x + 2 + roomsNormalWidth * 2, 0, currentGridLocation.z);
                        CalculateJointedRoomGrid(jointInfo.tail.objectTypeID, roomCenter, ref localRooms);
                        break;
                    case OBJECT_TYPE.JOINT:
                        break;
                    default:
                        Debug.LogError("Data Store Error");
                        break;
                }
            }
            else if (jointInfo.tail.objectTypeID == currentRoomTypeID) // room is the tail
            {
                OBJECT_TYPE objectType = jointInfo.head.objectType;
                switch (objectType)
                {
                    case OBJECT_TYPE.ROOM:
                    case OBJECT_TYPE.BOSS_ROOM:
                        Vector3 roomCenter = new Vector3(currentGridLocation.x + 2 + roomsNormalWidth * 2, 0, currentGridLocation.z);
                        CalculateJointedRoomGrid(jointInfo.head.objectTypeID, roomCenter, ref localRooms);
                        break;
                    case OBJECT_TYPE.JOINT:
                        break;
                    default:
                        Debug.LogError("Data Store Error");
                        break;
                }
            }
            else
            {
                Debug.LogError("Logic Error");
            }
        }

        // left
        if (currentRoomInfo.jointRoomTypeIdLeft != -1)
        {
            JointInfo jointInfo = roomsInfo.jointInfoList[currentRoomInfo.jointRoomTypeIdLeft];
            if (jointInfo.head.objectTypeID == currentRoomTypeID) // room is the head
            {
                OBJECT_TYPE objectType = jointInfo.tail.objectType;
                switch (objectType)
                {
                    case OBJECT_TYPE.ROOM:
                    case OBJECT_TYPE.BOSS_ROOM:
                        Vector3 roomCenter = new Vector3(currentGridLocation.x - 2 - roomsNormalWidth * 2, 0, currentGridLocation.z);
                        CalculateJointedRoomGrid(jointInfo.tail.objectTypeID, roomCenter, ref localRooms);
                        break;
                    case OBJECT_TYPE.JOINT:
                        break;
                    default:
                        Debug.LogError("Data Store Error");
                        break;
                }
            }
            else if (jointInfo.tail.objectTypeID == currentRoomTypeID) // room is the tail
            {
                OBJECT_TYPE objectType = jointInfo.head.objectType;
                switch (objectType)
                {
                    case OBJECT_TYPE.ROOM:
                    case OBJECT_TYPE.BOSS_ROOM:
                        Vector3 roomCenter = new Vector3(currentGridLocation.x - 2 - roomsNormalWidth * 2, 0, currentGridLocation.z);
                        CalculateJointedRoomGrid(jointInfo.head.objectTypeID, roomCenter, ref localRooms);
                        break;
                    case OBJECT_TYPE.JOINT:
                        break;
                    default:
                        Debug.LogError("Data Store Error");
                        break;
                }
            }
            else
            {
                Debug.LogError("Logic Error");
            }
        }
    }

    bool JointRoomOverlapping(List<Vector3> localRooms)
    {
        for (int i = 0; i < localRooms.Count; i++)
        {
            Collider[] colliding = Physics.OverlapBox(localRooms[i], new Vector3(roomsNormalWidth, 5, roomsNormalHeight));
            if (colliding.Length > 0) return true;
        }
        return false;
    }

    void BuildJointRoom(int currentRoomTypeID, Vector3 currentGridLocation, FOUR_DIRECTIONS nullifyDoor, List<Vector3> localRooms)
    {
        if (localRooms.Contains(currentGridLocation)) return;
        else localRooms.Add(currentGridLocation);

        RoomInfo currentRoomInfo = roomsInfo.roomInfoList[currentRoomTypeID];

        Vector3 roomPosition = new Vector3(currentGridLocation.x, 0, currentGridLocation.z - roomsNormalHeight);
        GameObject newRoom = GameObject.Instantiate(roomsPrefabs[currentRoomTypeID], roomPosition, Quaternion.identity);
        RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
        script.SetDoors();
        if (nullifyDoor != FOUR_DIRECTIONS.NONE) script.NullifyDoor(nullifyDoor);
        roomsScripts.Add(script);

        // top
        if (currentRoomInfo.jointRoomTypeIdTop != -1)
        {
            JointInfo jointInfo = roomsInfo.jointInfoList[currentRoomInfo.jointRoomTypeIdTop];
            if (jointInfo.head.objectTypeID == currentRoomTypeID) // room is the head
            {
                OBJECT_TYPE objectType = jointInfo.tail.objectType;
                switch (objectType)
                {
                    case OBJECT_TYPE.ROOM:
                    case OBJECT_TYPE.BOSS_ROOM:
                        Vector3 roomCenter = new Vector3(currentGridLocation.x, 0, currentGridLocation.z + roomsNormalHeight * 2);
                        if (!localRooms.Contains(roomCenter)) GameObject.Instantiate(jointsPrefabs[currentRoomInfo.jointRoomTypeIdTop], new Vector3(currentGridLocation.x, 0, currentGridLocation.z + 2 + roomsNormalHeight), Quaternion.identity);
                        BuildJointRoom(jointInfo.tail.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms);
                        break;
                    case OBJECT_TYPE.JOINT:
                        break;
                    default:
                        Debug.LogError("Data Store Error");
                        break;
                }
            }
            else if (jointInfo.tail.objectTypeID == currentRoomTypeID) // room is the tail
            {
                OBJECT_TYPE objectType = jointInfo.head.objectType;
                switch (objectType)
                {
                    case OBJECT_TYPE.ROOM:
                    case OBJECT_TYPE.BOSS_ROOM:
                        Vector3 roomCenter = new Vector3(currentGridLocation.x, 0, currentGridLocation.z + roomsNormalHeight * 2);
                        if (!localRooms.Contains(roomCenter)) GameObject.Instantiate(jointsPrefabs[currentRoomInfo.jointRoomTypeIdTop], new Vector3(currentGridLocation.x, 0, currentGridLocation.z + 2 + roomsNormalHeight), Quaternion.identity);
                        BuildJointRoom(jointInfo.head.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms);
                        break;
                    case OBJECT_TYPE.JOINT:
                        break;
                    default:
                        Debug.LogError("Data Store Error");
                        break;
                }
            }
            else
            {
                Debug.LogError("Logic Error");
            }
        }

        // down
        if (currentRoomInfo.jointRoomTypeIdDown != -1)
        {
            JointInfo jointInfo = roomsInfo.jointInfoList[currentRoomInfo.jointRoomTypeIdDown];
            if (jointInfo.head.objectTypeID == currentRoomTypeID) // room is the head
            {
                OBJECT_TYPE objectType = jointInfo.tail.objectType;
                switch (objectType)
                {
                    case OBJECT_TYPE.ROOM:
                    case OBJECT_TYPE.BOSS_ROOM:
                        Vector3 roomCenter = new Vector3(currentGridLocation.x, 0, currentGridLocation.z - 2 - roomsNormalHeight * 2);
                        if (!localRooms.Contains(roomCenter)) GameObject.Instantiate(jointsPrefabs[currentRoomInfo.jointRoomTypeIdDown], new Vector3(currentGridLocation.x, 0, currentGridLocation.z - 2 - roomsNormalHeight), Quaternion.identity);
                        BuildJointRoom(jointInfo.tail.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms);
                        break;
                    case OBJECT_TYPE.JOINT:
                        break;
                    default:
                        Debug.LogError("Data Store Error");
                        break;
                }
            }
            else if (jointInfo.tail.objectTypeID == currentRoomTypeID) // room is the tail
            {
                OBJECT_TYPE objectType = jointInfo.head.objectType;
                switch (objectType)
                {
                    case OBJECT_TYPE.ROOM:
                    case OBJECT_TYPE.BOSS_ROOM:
                        Vector3 roomCenter = new Vector3(currentGridLocation.x, 0, currentGridLocation.z - 2 - roomsNormalHeight * 2);
                        if (!localRooms.Contains(roomCenter)) GameObject.Instantiate(jointsPrefabs[currentRoomInfo.jointRoomTypeIdDown], new Vector3(currentGridLocation.x, 0, currentGridLocation.z - 2 - roomsNormalHeight), Quaternion.identity);
                        BuildJointRoom(jointInfo.head.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms);
                        break;
                    case OBJECT_TYPE.JOINT:
                        break;
                    default:
                        Debug.LogError("Data Store Error");
                        break;
                }
            }
            else
            {
                Debug.LogError("Logic Error");
            }
        }

        // right
        if (currentRoomInfo.jointRoomTypeIdRight != -1)
        {
            JointInfo jointInfo = roomsInfo.jointInfoList[currentRoomInfo.jointRoomTypeIdRight];
            if (jointInfo.head.objectTypeID == currentRoomTypeID) // room is the head
            {
                OBJECT_TYPE objectType = jointInfo.tail.objectType;
                switch (objectType)
                {
                    case OBJECT_TYPE.ROOM:
                    case OBJECT_TYPE.BOSS_ROOM:
                        Vector3 roomCenter = new Vector3(currentGridLocation.x + 2 + roomsNormalWidth * 2, 0, currentGridLocation.z);
                        if (!localRooms.Contains(roomCenter)) GameObject.Instantiate(jointsPrefabs[currentRoomInfo.jointRoomTypeIdRight], new Vector3(currentGridLocation.x + 1 + roomsNormalWidth, 0, currentGridLocation.z - roomsNormalHeight), Quaternion.identity);
                        BuildJointRoom(jointInfo.tail.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms);
                        break;
                    case OBJECT_TYPE.JOINT:
                        break;
                    default:
                        Debug.LogError("Data Store Error");
                        break;
                }
            }
            else if (jointInfo.tail.objectTypeID == currentRoomTypeID) // room is the tail
            {
                OBJECT_TYPE objectType = jointInfo.head.objectType;
                switch (objectType)
                {
                    case OBJECT_TYPE.ROOM:
                    case OBJECT_TYPE.BOSS_ROOM:
                        Vector3 roomCenter = new Vector3(currentGridLocation.x + 2 + roomsNormalWidth * 2, 0, currentGridLocation.z);
                        if (!localRooms.Contains(roomCenter)) GameObject.Instantiate(jointsPrefabs[currentRoomInfo.jointRoomTypeIdRight], new Vector3(currentGridLocation.x + 1 + roomsNormalWidth, 0, currentGridLocation.z - roomsNormalHeight), Quaternion.identity);
                        BuildJointRoom(jointInfo.head.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms);
                        break;
                    case OBJECT_TYPE.JOINT:
                        break;
                    default:
                        Debug.LogError("Data Store Error");
                        break;
                }
            }
            else
            {
                Debug.LogError("Logic Error");
            }
        }

        // left
        if (currentRoomInfo.jointRoomTypeIdLeft != -1)
        {
            JointInfo jointInfo = roomsInfo.jointInfoList[currentRoomInfo.jointRoomTypeIdLeft];
            if (jointInfo.head.objectTypeID == currentRoomTypeID) // room is the head
            {
                OBJECT_TYPE objectType = jointInfo.tail.objectType;
                switch (objectType)
                {
                    case OBJECT_TYPE.ROOM:
                    case OBJECT_TYPE.BOSS_ROOM:
                        Vector3 roomCenter = new Vector3(currentGridLocation.x - 2 - roomsNormalWidth * 2, 0, currentGridLocation.z);
                        if (!localRooms.Contains(roomCenter)) GameObject.Instantiate(jointsPrefabs[currentRoomInfo.jointRoomTypeIdLeft], new Vector3(currentGridLocation.x - 1 - roomsNormalWidth, 0, currentGridLocation.z - roomsNormalHeight), Quaternion.identity);
                        BuildJointRoom(jointInfo.tail.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms);
                        break;
                    case OBJECT_TYPE.JOINT:
                        break;
                    default:
                        Debug.LogError("Data Store Error");
                        break;
                }
            }
            else if (jointInfo.tail.objectTypeID == currentRoomTypeID) // room is the tail
            {
                OBJECT_TYPE objectType = jointInfo.head.objectType;
                switch (objectType)
                {
                    case OBJECT_TYPE.ROOM:
                    case OBJECT_TYPE.BOSS_ROOM:
                        Vector3 roomCenter = new Vector3(currentGridLocation.x - 2 - roomsNormalWidth * 2, 0, currentGridLocation.z);
                        if (!localRooms.Contains(roomCenter)) GameObject.Instantiate(jointsPrefabs[currentRoomInfo.jointRoomTypeIdLeft], new Vector3(currentGridLocation.x - 1 - roomsNormalWidth, 0, currentGridLocation.z - roomsNormalHeight), Quaternion.identity);
                        BuildJointRoom(jointInfo.head.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms);
                        break;
                    case OBJECT_TYPE.JOINT:
                        break;
                    default:
                        Debug.LogError("Data Store Error");
                        break;
                }
            }
            else
            {
                Debug.LogError("Logic Error");
            }
        }
    }
}

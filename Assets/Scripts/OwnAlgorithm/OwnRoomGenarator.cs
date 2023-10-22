using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] ReadRoomsInfo roomsInfo;
    [SerializeField] GameObject startRoomPrefab;
    [SerializeField] GameObject[] pathRoomsPrefabs; // room with 2 doors at least
    [SerializeField] GameObject[] endingRoomsPrefabs; // room with only one door
    GameObject[] roomsPrefabs; // pathRoomsPrefabs + endingRoomsPrefabs
    [SerializeField] GameObject[] bossRoomsPrefabs;
    [SerializeField] GameObject[] jointsPrefabs;

    int roomsNormalWidth = 7;
    int roomsNormalHeight = 5;

    [Header("Rooms Between Start and Boss Rooms")]
    [Range(0, 9)][SerializeField] int minRoomsBetween;
    [Range(0, 9)][SerializeField] int maxRoomsBetween;
    [NonEditable][SerializeField] int totalRoomsBetween = 0;
    int roomsBetween = 0;
    [SerializeField] int maxRooms;
    [NonEditable][SerializeField] int currentRooms = 0;

    Dictionary<int, TreeNode> roomsTree = new();
    GameObject bossRoom;

    int workingIndex = 0;
    int lastRoomCreated = -1; // make imposible to have the same room in sequence

    List<Door> draw = new();

    // Start is called before the first frame update
    void Start()
    {
        totalRoomsBetween = Random.Range(minRoomsBetween, maxRoomsBetween);
        roomsBetween = totalRoomsBetween;
        if (maxRooms < roomsBetween) maxRooms = roomsBetween;

        roomsPrefabs = pathRoomsPrefabs.Concat(endingRoomsPrefabs).ToArray();

        CreateMainPath();

        FillWithRooms();
    }

    // Update is called once per frame
    void Update()
    {
        // display tree
        for (int i = 0; i < draw.Count; i++)
        {
            Debug.DrawLine(draw[i].position, draw[i].position + Vector3.up * 5);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    int GetTreeIndex(RoomBehaviour script)
    {
        for (int i = 0; i < roomsTree.Count; i++)
        {
            if (roomsTree[i].script.GetInstanceID() == script.GetInstanceID()) return i;
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

        roomsTree.Add(0, new TreeNode(script, null));

        Door unfilledDoor;
        RoomBehaviour auxScript;

        // start-boss path rooms
        while (roomsBetween > 0)
        {
            if (script.GetDoorsFilled())
            {
                if (!FindRoomScriptWithDoors(ref script)) return; // error ocurred
            }
            workingIndex = GetTreeIndex(script);
            unfilledDoor = roomsTree[workingIndex].script.GetRandomUnfilledDoor();

            auxScript = CreateNextRoom(unfilledDoor, workingIndex, pathRoomsPrefabs);
            if (auxScript != null)
            {
                lastRoomCreated = auxScript.roomTypeID;
                unfilledDoor.state = DOOR_STATE.YELLOW;
                script = auxScript;
                roomsBetween--;
            }
        }

        // boss room
        CreateBossRoom(script);
    }

    bool FindRoomScriptWithDoors(ref RoomBehaviour script)
    {
        RoomInfo roomInfo = roomsInfo.roomInfoList[script.roomTypeID];
        if (roomInfo.IsJointRoom())
        {
            int currentTreeIndex = GetTreeIndex(script);
            if (currentTreeIndex == -1)
            {
                Debug.LogError("Logic Error: Script not Stored on treeList");
                return false;
            }

            for (int i = 1; i < roomInfo.jointRoomNumber; i++) // start at 1 to ignore it self
            {
                if (!roomsTree[currentTreeIndex + i].script.GetDoorsFilled())
                {
                    script = roomsTree[currentTreeIndex + i].script;
                    return true;
                }
            }

            // Retroceder en las salas
            RoomBehaviour auxScript = FindRoomScriptWithDoorsBacktracking(roomsTree[GetTreeIndex(script)]);
            if (auxScript != null)
            {
                script = auxScript;
                return true;
            }
            else
            {
                Debug.LogWarning("No more doors for fill");
                return false;
            }
        }
        else
        {
            // Retroceder en las salas
            RoomBehaviour auxScript = FindRoomScriptWithDoorsBacktracking(roomsTree[GetTreeIndex(script)]);
            if (auxScript != null)
            {
                script = auxScript;
                return true;
            }
            else
            {
                Debug.LogWarning("No more doors for fill");
                return false;
            }
        }
    }

    RoomBehaviour FindRoomScriptWithDoorsBacktracking(TreeNode node) // know initial node don't have doors for fill
    {
        TreeNode ret = node;
        while (ret.parent != null)
        {
            ret = ret.parent;
            if (!ret.script.GetDoorsFilled()) return ret.script;
        }

        return null; // no more doors for fill on linear backtraking
    }

    RoomBehaviour CreateNextRoom(Door door, int roomTreeIndex, GameObject[] roomsPool)
    {
        List<GameObject> posibleRooms = new(roomsPool.ToList());
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
                                GameObject newRoom = GameObject.Instantiate(roomsPool[newRoomTypeID], roomPosition, Quaternion.identity);
                                currentRooms++;
                                RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                                script.SetDoors();
                                script.NullifyDoor(FOUR_DIRECTIONS.DOWN);
                                TreeNode node = new TreeNode(script, roomsTree[roomTreeIndex]);
                                roomsTree[roomTreeIndex].children[0] = node; // 0 cause is top
                                roomsTree.Add(roomsTree.Count, node);
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
                            int lastRoomScriptIndex = roomsTree.Count - 1;
                            BuildJointRoom(newRoomTypeID, roomCenter, FOUR_DIRECTIONS.DOWN, storeRooms, roomTreeIndex, 0, roomsPool); // top
                            currentRooms++;
                            return roomsTree[lastRoomScriptIndex + 1].script;
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
                                GameObject newRoom = GameObject.Instantiate(roomsPool[newRoomTypeID], roomPosition, Quaternion.identity);
                                currentRooms++;
                                RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                                script.SetDoors();
                                script.NullifyDoor(FOUR_DIRECTIONS.TOP);
                                TreeNode node = new TreeNode(script, roomsTree[roomTreeIndex]);
                                roomsTree[roomTreeIndex].children[1] = node; // 1 cause is down
                                roomsTree.Add(roomsTree.Count, node);
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
                            int lastRoomScriptIndex = roomsTree.Count - 1;
                            BuildJointRoom(newRoomTypeID, roomCenter, FOUR_DIRECTIONS.DOWN, storeRooms, roomTreeIndex, 1, roomsPool); // down
                            currentRooms++;
                            return roomsTree[lastRoomScriptIndex + 1].script;
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
                                GameObject newRoom = GameObject.Instantiate(roomsPool[newRoomTypeID], roomPosition, Quaternion.identity);
                                currentRooms++;
                                RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                                script.SetDoors();
                                script.NullifyDoor(FOUR_DIRECTIONS.LEFT);
                                TreeNode node = new TreeNode(script, roomsTree[roomTreeIndex]);
                                roomsTree[roomTreeIndex].children[2] = node; // 2 cause is right
                                roomsTree.Add(roomsTree.Count, node);
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
                            int lastRoomScriptIndex = roomsTree.Count - 1;
                            BuildJointRoom(newRoomTypeID, roomCenter, FOUR_DIRECTIONS.DOWN, storeRooms, roomTreeIndex, 2, roomsPool); // right
                            currentRooms++;
                            return roomsTree[lastRoomScriptIndex + 1].script;
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
                                GameObject newRoom = GameObject.Instantiate(roomsPool[newRoomTypeID], roomPosition, Quaternion.identity);
                                currentRooms++;
                                RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                                script.SetDoors();
                                script.NullifyDoor(FOUR_DIRECTIONS.RIGHT);
                                TreeNode node = new TreeNode(script, roomsTree[roomTreeIndex]);
                                roomsTree[roomTreeIndex].children[3] = node; // 3 cause is left
                                roomsTree.Add(roomsTree.Count, node);
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
                            int lastRoomScriptIndex = roomsTree.Count - 1;
                            BuildJointRoom(newRoomTypeID, roomCenter, FOUR_DIRECTIONS.DOWN, storeRooms, roomTreeIndex, 3, roomsPool); // left
                            currentRooms++;
                            return roomsTree[lastRoomScriptIndex + 1].script;
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

    void BuildJointRoom(int currentRoomTypeID, Vector3 currentGridLocation, FOUR_DIRECTIONS nullifyDoor, List<Vector3> localRooms, int currentRoomTreeIndex, int currentRoomDirection, GameObject[] roomsPool)
    {
        if (localRooms.Contains(currentGridLocation)) return;
        else localRooms.Add(currentGridLocation);

        RoomInfo currentRoomInfo = roomsInfo.roomInfoList[currentRoomTypeID];

        Vector3 roomPosition = new Vector3(currentGridLocation.x, 0, currentGridLocation.z - roomsNormalHeight);
        GameObject newRoom = GameObject.Instantiate(roomsPool[currentRoomTypeID], roomPosition, Quaternion.identity);
        RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
        script.SetDoors();
        if (nullifyDoor != FOUR_DIRECTIONS.NONE) script.NullifyDoor(nullifyDoor);
        TreeNode node = new TreeNode(script, roomsTree[currentRoomTreeIndex]);
        roomsTree[currentRoomTreeIndex].children[currentRoomDirection] = node; // 0 --> this room is on top of last one
        roomsTree.Add(roomsTree.Count, node);

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
                        BuildJointRoom(jointInfo.tail.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms, roomsTree.Count - 1, 0, roomsPool); // top
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
                        BuildJointRoom(jointInfo.head.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms, roomsTree.Count - 1, 0, roomsPool); // top
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
                        BuildJointRoom(jointInfo.tail.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms, roomsTree.Count - 1, 1, roomsPool); // down
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
                        BuildJointRoom(jointInfo.head.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms, roomsTree.Count - 1, 1, roomsPool); // down
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
                        BuildJointRoom(jointInfo.tail.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms, roomsTree.Count - 1, 2, roomsPool); // right
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
                        BuildJointRoom(jointInfo.head.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms, roomsTree.Count - 1, 2, roomsPool); // right
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
                        BuildJointRoom(jointInfo.tail.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms, roomsTree.Count - 1, 3, roomsPool); // left
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
                        BuildJointRoom(jointInfo.head.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms, roomsTree.Count - 1, 3, roomsPool); // left
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

    void CreateBossRoom(RoomBehaviour lastRoomScript)
    {
        GameObject bossRoomPrefab = bossRoomsPrefabs[Random.Range(0, bossRoomsPrefabs.Length)];
        BossRoomBehaviour bossScript = bossRoomPrefab.GetComponent<BossRoomBehaviour>();

        // find position
        if (!CreateBossRoomLoop(roomsTree[GetTreeIndex(lastRoomScript)], bossRoomPrefab, bossScript)) Debug.LogError("Boss room was imposible to be created");
    }

    bool CreateBossRoomLoop(TreeNode node, GameObject bossRoomPrefab, BossRoomBehaviour bossScript) // true --> created, false --> imposible to create boss room
    {
        List<Door> lastRoomDoors = new(node.script.doors);
        for (int i = 0; i < lastRoomDoors.Count; i++)
        {
            if (lastRoomDoors[i].state != DOOR_STATE.FOR_FILL) lastRoomDoors.Remove(lastRoomDoors[i]);
        }
        while (lastRoomDoors.Count > 0) // Check all room doors
        {
            Door bossEntrance = lastRoomDoors[Random.Range(0, lastRoomDoors.Count)];
            Collider[] colliding;
            switch (bossEntrance.direction)
            {
                case FOUR_DIRECTIONS.TOP:
                    colliding = Physics.OverlapBox(new Vector3(bossEntrance.position.x, 0, bossEntrance.position.z + 2 + bossScript.height), new Vector3(bossScript.width, 5, bossScript.height), Quaternion.identity);
                    if (colliding.Length == 0)
                    {
                        bossRoom = GameObject.Instantiate(bossRoomPrefab, new Vector3(bossEntrance.position.x, 0, bossEntrance.position.z + 2), Quaternion.identity);
                        bossEntrance.state = DOOR_STATE.BOSS;
                        return true;
                    }
                    else // no space
                    {
                        lastRoomDoors.Remove(bossEntrance);
                    }
                    break;
                case FOUR_DIRECTIONS.DOWN:
                    colliding = Physics.OverlapBox(new Vector3(bossEntrance.position.x, 0, bossEntrance.position.z - 2 - bossScript.height), new Vector3(bossScript.width, 5, bossScript.height), Quaternion.AngleAxis(180, Vector3.up));
                    if (colliding.Length == 0)
                    {
                        bossRoom = GameObject.Instantiate(bossRoomPrefab, new Vector3(bossEntrance.position.x, 0, bossEntrance.position.z - 2), Quaternion.AngleAxis(180, Vector3.up));
                        bossEntrance.state = DOOR_STATE.BOSS;
                        return true;
                    }
                    else // no space
                    {
                        lastRoomDoors.Remove(bossEntrance);
                    }
                    break;
                case FOUR_DIRECTIONS.RIGHT:
                    colliding = Physics.OverlapBox(new Vector3(bossEntrance.position.x + 2 + bossScript.width, 0, bossEntrance.position.z), new Vector3(bossScript.width, 5, bossScript.height), Quaternion.AngleAxis(90, Vector3.up));
                    if (colliding.Length == 0)
                    {
                        bossRoom = GameObject.Instantiate(bossRoomPrefab, new Vector3(bossEntrance.position.x + 2, 0, bossEntrance.position.z), Quaternion.AngleAxis(90, Vector3.up));
                        bossEntrance.state = DOOR_STATE.BOSS;
                        return true;
                    }
                    else // no space
                    {
                        lastRoomDoors.Remove(bossEntrance);
                    }
                    break;
                case FOUR_DIRECTIONS.LEFT:
                    colliding = Physics.OverlapBox(new Vector3(bossEntrance.position.x - 2 - bossScript.width, 0, bossEntrance.position.z), new Vector3(bossScript.width, 5, bossScript.height), Quaternion.AngleAxis(270, Vector3.up));
                    if (colliding.Length == 0)
                    {
                        bossRoom = GameObject.Instantiate(bossRoomPrefab, new Vector3(bossEntrance.position.x - 2, 0, bossEntrance.position.z), Quaternion.AngleAxis(270, Vector3.up));
                        bossEntrance.state = DOOR_STATE.BOSS;
                        return true;
                    }
                    else // no space
                    {
                        lastRoomDoors.Remove(bossEntrance);
                    }
                    break;
                case FOUR_DIRECTIONS.NONE:
                default:
                    Debug.LogError("Door Direction Undefined");
                    break;
            }
        }

        // no doors on that room
        if (node.parent.parent == null) return false; // skip initial room
        else return CreateBossRoomLoop(node.parent, bossRoomPrefab, bossScript);
    }

    void FillWithRooms()
    {
        //while (currentRooms < maxRooms)
        {
            List<Door> forFillDoors = new();
            FindForFillDoors(roomsTree[1], ref forFillDoors); // 1 --> ignore start room
            draw = forFillDoors;
        }
    }

    void FindForFillDoors(TreeNode node, ref List<Door> forFillDoors)
    {
        RoomInfo roomInfo = roomsInfo.roomInfoList[node.script.roomTypeID];
        if (node.script.doors.Count == 1 && !roomInfo.IsJointRoom()) return; // ending room

        // top
        if (roomInfo.topDoor == 1)
        {
            Door door = node.script.GetDoorByDirection(FOUR_DIRECTIONS.TOP);
            if (door == null)
            {
                Debug.LogError("Door don't exist");
            }
            else
            {
                switch (door.state)
                {
                    case DOOR_STATE.FOR_FILL:
                        forFillDoors.Add(door);
                        break;
                    case DOOR_STATE.YELLOW:
                        FindForFillDoors(node.children[0], ref forFillDoors); // 0 --> top
                        break;
                    case DOOR_STATE.BOSS:
                    case DOOR_STATE.HOLDED:
                    case DOOR_STATE.DESTROYED:
                    case DOOR_STATE.NULL:
                    default:
                        break;
                }
            }
        }
        else if (roomInfo.topDoor == 2)
        {
            if (node.children[0] != null) FindForFillDoors(node.children[0], ref forFillDoors);  // if null means top room is the parent
        }

        // down
        if (roomInfo.downDoor == 1)
        {
            Door door = node.script.GetDoorByDirection(FOUR_DIRECTIONS.DOWN);
            if (door == null)
            {
                Debug.LogError("Door don't exist");
            }
            else
            {
                switch (door.state)
                {
                    case DOOR_STATE.FOR_FILL:
                        forFillDoors.Add(door);
                        break;
                    case DOOR_STATE.YELLOW:
                        FindForFillDoors(node.children[1], ref forFillDoors); // 1 --> down
                        break;
                    case DOOR_STATE.BOSS:
                    case DOOR_STATE.HOLDED:
                    case DOOR_STATE.DESTROYED:
                    case DOOR_STATE.NULL:
                    default:
                        break;
                }
            }
        }
        else if (roomInfo.downDoor == 2)
        {
            if (node.children[1] != null) FindForFillDoors(node.children[1], ref forFillDoors);  // if null means down room is the parent
        }

        // right
        if (roomInfo.rightDoor == 1)
        {
            Door door = node.script.GetDoorByDirection(FOUR_DIRECTIONS.RIGHT);
            if (door == null)
            {
                Debug.LogError("Door don't exist");
            }
            else
            {
                switch (door.state)
                {
                    case DOOR_STATE.FOR_FILL:
                        forFillDoors.Add(door);
                        break;
                    case DOOR_STATE.YELLOW:
                        FindForFillDoors(node.children[2], ref forFillDoors); // 2 --> right
                        break;
                    case DOOR_STATE.BOSS:
                    case DOOR_STATE.HOLDED:
                    case DOOR_STATE.DESTROYED:
                    case DOOR_STATE.NULL:
                    default:
                        break;
                }
            }
        }
        else if (roomInfo.rightDoor == 2)
        {
            if (node.children[2] != null) FindForFillDoors(node.children[2], ref forFillDoors); // if null means right room is the parent
        }

        // left
        if (roomInfo.leftDoor == 1)
        {
            Door door = node.script.GetDoorByDirection(FOUR_DIRECTIONS.LEFT);
            if (door == null)
            {
                Debug.LogError("Door don't exist");
            }
            else
            {
                switch (door.state)
                {
                    case DOOR_STATE.FOR_FILL:
                        forFillDoors.Add(door);
                        break;
                    case DOOR_STATE.YELLOW:
                        FindForFillDoors(node.children[3], ref forFillDoors); // 3 --> left
                        break;
                    case DOOR_STATE.BOSS:
                    case DOOR_STATE.HOLDED:
                    case DOOR_STATE.DESTROYED:
                    case DOOR_STATE.NULL:
                    default:
                        break;
                }
            }
        }
        else if (roomInfo.leftDoor == 2)
        {
            if (node.children[3] != null) FindForFillDoors(node.children[3], ref forFillDoors); // if null means left room is the parent
        }
    }
}

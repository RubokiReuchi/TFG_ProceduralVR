using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public enum FOUR_DIRECTIONS
{
    TOP,
    DOWN,
    RIGHT,
    LEFT,
    NONE
}

public class RoomGenarator : MonoBehaviour
{
    public static RoomGenarator instance;

    [SerializeField] ReadRoomsInfo roomsInfo;
    [SerializeField] PlaceGates placeGates;

    [Header("Rooms Prefabs")]
    [SerializeField] GameObject startRoomPrefab;
    [SerializeField] GameObject[] pathRoomsPrefabs; // room with 2 doors at least
    [SerializeField] GameObject[] endingRoomsPrefabs; // room with only one door
    GameObject[] roomsPrefabs; // pathRoomsPrefabs + endingRoomsPrefabs
    [SerializeField] GameObject[] powerUpPathRoomsPrefabs; // power up room with 2 doors at least
    [SerializeField] GameObject[] powerUpEndingRoomsPrefabs; // power up room with only one door
    GameObject[] powerUpRoomsPrefabs; // powerUpPathRoomsPrefabs + powerUpEndingRoomsPrefabs
    [SerializeField] GameObject[] upgradeRoomsPrefabs; //upgrade room always one door only
    [SerializeField] GameObject[] bossRoomsPrefabs;
    [SerializeField] GameObject[] jointsPrefabs;
    [SerializeField] GameObject hallwayPrefab;

    int powerUpRoomsLeft = 3;

    float roomsNormalWidth = 7;
    float roomsNormalHeight = 5;
    float tileSize = 3;

    [Header("Rooms Between Start and Boss Rooms")]
    [Range(0, 9)][SerializeField] int minRoomsBetween;
    [Range(0, 9)][SerializeField] int maxRoomsBetween;
    [NonEditable][SerializeField] int totalRoomsBetween = 0;
    int roomsBetween = 0;
    [Header("Number of Rooms")] // not counting start and boss
    [SerializeField] int maxRooms; // if a lot of ending rooms are created, it maybe less rooms
    [NonEditable][SerializeField] int currentRooms = 0;

    Dictionary<int, TreeNode> roomsTree = new();
    GameObject bossRoom;

    int workingIndex = 0;
    int lastRoomCreated = -1; // make imposible to have the same room in sequence

    [Range(0, 20)] public float breakDoorChance;
    public LayerMask doorLayer;
    List<HallwayBehaviour> hallways = new();
    List<Gate> draw = new();

    [Header("PowerUps & Upgrades")]
    [SerializeField] GameObject[] powerUpPrefabs;
    List<PowerUp> currentPowerUps = new();
    bool upgradeRoomPlaced = false;
    int upgradeLevel = 0;

    [Header("Map Rooms")]
    [SerializeField] Transform createMap;
    [SerializeField] GameObject mapStartRoomPrefab;
    [SerializeField] GameObject[] mapPathRoomsPrefabs;
    [SerializeField] GameObject[] mapEndingRoomsPrefabs;
    GameObject[] mapRoomsPrefabs; // mapPathRoomsPrefabs + mapEndingRoomsPrefabs
    [SerializeField] GameObject[] mapPowerUpPathRoomsPrefabs;
    [SerializeField] GameObject[] mapPowerUpEndingRoomsPrefabs;
    GameObject[] mapPowerUpRoomsPrefabs; // mapPowerUpPathRoomsPrefabs + mapPowerUpEndingRoomsPrefabs
    [SerializeField] GameObject[] mapUpgradeRoomsPrefabs; //upgrade room always one door only
    [SerializeField] GameObject[] mapBossRoomsPrefabs;
    [SerializeField] GameObject[] mapJointsPrefabs;
    [SerializeField] GameObject mapHallwayPrefab;
    [SerializeField] Transform mapCenterPos;
    List<GameObject> roomsInMap = new();
    [SerializeField] Transform playerMark;

    [HideInInspector] public List<RoomBehaviour> activeRooms = new();
    [HideInInspector] public List<HallwayBehaviour> activeHallways = new();

    [Header("Nav Mesh")]
    [SerializeField] NavMeshSurface[] navMeshSurfaces;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        totalRoomsBetween = Random.Range(minRoomsBetween, maxRoomsBetween);
        roomsBetween = totalRoomsBetween;
        if (maxRooms < roomsBetween) maxRooms = roomsBetween;

        roomsPrefabs = pathRoomsPrefabs.Concat(endingRoomsPrefabs).ToArray();
        mapRoomsPrefabs = mapPathRoomsPrefabs.Concat(mapEndingRoomsPrefabs).ToArray();

        powerUpRoomsPrefabs = powerUpPathRoomsPrefabs.Concat(powerUpEndingRoomsPrefabs).ToArray();
        mapPowerUpRoomsPrefabs = mapPowerUpPathRoomsPrefabs.Concat(mapPowerUpEndingRoomsPrefabs).ToArray();

        CreateMainPath();

        FillWithRooms();

        PlacePowerUps();

        foreach (var navMeshSurface in navMeshSurfaces) navMeshSurface.BuildNavMesh();

        SetMapSize(); // map

        CreateHallways();

        playerMark.SetParent(createMap); // map
        playerMark.localScale = Vector3.one;
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
        GameObject newMapRoom = GameObject.Instantiate(mapStartRoomPrefab, createMap);
        newMapRoom.transform.localPosition = Vector3.zero;
        RoomBehaviour script = room.GetComponent<RoomBehaviour>();
        script.SetDoors();
        script.roomInMap = newMapRoom;
        activeRooms.Add(script);

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

            bool powerUpRoom = !(Random.Range(0, 100) < 80) && powerUpRoomsLeft > 0;
            if (powerUpRoom)
            {
                auxScript = CreateNextPowerUpRoom(unfilledDoor, workingIndex, powerUpPathRoomsPrefabs, mapPowerUpPathRoomsPrefabs); // powerUp room
                if (auxScript != null)
                {
                    lastRoomCreated = auxScript.roomTypeID;
                    unfilledDoor.state = DOOR_STATE.YELLOW;
                    script = auxScript;
                    roomsBetween--;
                    powerUpRoomsLeft--;
                }
                else powerUpRoom = false;
            }
            if (!powerUpRoom)
            {
                auxScript = CreateNextRoom(unfilledDoor, workingIndex, pathRoomsPrefabs, mapPathRoomsPrefabs); // no powerUp room
                if (auxScript != null)
                {
                    lastRoomCreated = auxScript.roomTypeID;
                    unfilledDoor.state = DOOR_STATE.YELLOW;
                    script = auxScript;
                    roomsBetween--;
                }
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

    RoomBehaviour CreateNextRoom(Door door, int roomTreeIndex, GameObject[] roomsPool, GameObject[] mapRoomsPool)
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
                        Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + tileSize/*HallwaySize*/ + roomsNormalHeight / 2 * tileSize);
                        Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth / 2 * tileSize, 5, roomsNormalHeight / 2 * tileSize));
                        if (colliding.Length == 0) // no room there
                        {
                            if (roomInfo.downDoor == 1) // if room has a door down
                            {
                                Vector3 roomPosition = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + tileSize);
                                GameObject newRoom = GameObject.Instantiate(roomsPool[newRoomTypeID], roomPosition, Quaternion.identity);
                                GameObject newMapRoom = GameObject.Instantiate(mapRoomsPool[newRoomTypeID], createMap);
                                newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                                roomsInMap.Add(newMapRoom);
                                currentRooms++;
                                RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                                script.SetDoors();
                                script.NullifyDoor(FOUR_DIRECTIONS.DOWN);
                                script.roomInMap = newMapRoom;
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

                        Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + tileSize/*HallwaySize*/ + roomsNormalHeight / 2 * tileSize);
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
                            List<GameObject> jointedRooms = new();
                            List<GameObject> mapJoints = new();
                            BuildJointRoom(newRoomTypeID, roomCenter, FOUR_DIRECTIONS.DOWN, storeRooms, roomTreeIndex, 0, roomsPool, mapRoomsPool, ref jointedRooms, ref mapJoints); // top
                            StoreJointedRooms(jointedRooms, mapJoints);
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
                        Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z - tileSize/*HallwaySize*/ - roomsNormalHeight / 2 * tileSize);
                        Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth / 2 * tileSize, 5, roomsNormalHeight / 2 * tileSize));
                        if (colliding.Length == 0) // no room there
                        {
                            if (roomInfo.topDoor == 1) // if room has a door at top
                            {
                                Vector3 roomPosition = new Vector3(door.position.x, door.position.y/*0*/, door.position.z - tileSize - roomsNormalHeight * tileSize);
                                GameObject newRoom = GameObject.Instantiate(roomsPool[newRoomTypeID], roomPosition, Quaternion.identity);
                                GameObject newMapRoom = GameObject.Instantiate(mapRoomsPool[newRoomTypeID], createMap);
                                newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                                roomsInMap.Add(newMapRoom);
                                currentRooms++;
                                RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                                script.SetDoors();
                                script.NullifyDoor(FOUR_DIRECTIONS.TOP);
                                script.roomInMap = newMapRoom;
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

                        Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + tileSize/*HallwaySize*/ + roomsNormalHeight / 2 * tileSize);
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
                            List<GameObject> jointedRooms = new();
                            List<GameObject> mapJoints = new();
                            BuildJointRoom(newRoomTypeID, roomCenter, FOUR_DIRECTIONS.DOWN, storeRooms, roomTreeIndex, 1, roomsPool, mapRoomsPool, ref jointedRooms, ref mapJoints); // down
                            StoreJointedRooms(jointedRooms, mapJoints);
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
                        Vector3 roomCenter = new Vector3(door.position.x + tileSize/*HallwaySize*/ + roomsNormalWidth / 2 * tileSize, door.position.y/*0*/, door.position.z);
                        Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth / 2 * tileSize, 5, roomsNormalHeight / 2 * tileSize));
                        if (colliding.Length == 0) // no room there
                        {
                            if (roomInfo.leftDoor == 1) // if room has a door at left
                            {
                                Vector3 roomPosition = new Vector3(door.position.x + tileSize + roomsNormalWidth / 2 * tileSize, door.position.y/*0*/, door.position.z - roomsNormalHeight / 2 * tileSize);
                                GameObject newRoom = GameObject.Instantiate(roomsPool[newRoomTypeID], roomPosition, Quaternion.identity);
                                GameObject newMapRoom = GameObject.Instantiate(mapRoomsPool[newRoomTypeID], createMap);
                                newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                                roomsInMap.Add(newMapRoom);
                                currentRooms++;
                                RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                                script.SetDoors();
                                script.NullifyDoor(FOUR_DIRECTIONS.LEFT);
                                script.roomInMap = newMapRoom;
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

                        Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + tileSize/*HallwaySize*/ + roomsNormalHeight / 2 * tileSize);
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
                            List<GameObject> jointedRooms = new();
                            List<GameObject> mapJoints = new();
                            BuildJointRoom(newRoomTypeID, roomCenter, FOUR_DIRECTIONS.DOWN, storeRooms, roomTreeIndex, 2, roomsPool, mapRoomsPool, ref jointedRooms, ref mapJoints); // right
                            StoreJointedRooms(jointedRooms, mapJoints);
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
                        Vector3 roomCenter = new Vector3(door.position.x - 2/*HallwaySize*/ - roomsNormalWidth / 2 * tileSize, door.position.y/*0*/, door.position.z);
                        Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth / 2 * tileSize, 5, roomsNormalHeight / 2 * tileSize));
                        if (colliding.Length == 0) // no room there
                        {
                            if (roomInfo.rightDoor == 1) // if room has a door at right
                            {
                                Vector3 roomPosition = new Vector3(door.position.x - tileSize - roomsNormalWidth / 2 * tileSize, door.position.y/*0*/, door.position.z - roomsNormalHeight / 2 * tileSize);
                                GameObject newRoom = GameObject.Instantiate(roomsPool[newRoomTypeID], roomPosition, Quaternion.identity);
                                GameObject newMapRoom = GameObject.Instantiate(mapRoomsPool[newRoomTypeID], createMap);
                                newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                                roomsInMap.Add(newMapRoom);
                                currentRooms++;
                                RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                                script.SetDoors();
                                script.NullifyDoor(FOUR_DIRECTIONS.RIGHT);
                                script.roomInMap = newMapRoom;
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

                        Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + tileSize/*HallwaySize*/ + roomsNormalHeight / 2 * tileSize);
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
                            List<GameObject> jointedRooms = new();
                            List<GameObject> mapJoints = new();
                            BuildJointRoom(newRoomTypeID, roomCenter, FOUR_DIRECTIONS.DOWN, storeRooms, roomTreeIndex, 3, roomsPool, mapRoomsPool, ref jointedRooms, ref mapJoints); // left
                            StoreJointedRooms(jointedRooms, mapJoints);
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
                        Vector3 roomCenter = new Vector3(currentGridLocation.x, 0, currentGridLocation.z + tileSize + roomsNormalHeight * tileSize);
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
                        Vector3 roomCenter = new Vector3(currentGridLocation.x, 0, currentGridLocation.z + tileSize + roomsNormalHeight * tileSize);
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
                        Vector3 roomCenter = new Vector3(currentGridLocation.x, 0, currentGridLocation.z - tileSize - roomsNormalHeight * tileSize);
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
                        Vector3 roomCenter = new Vector3(currentGridLocation.x, 0, currentGridLocation.z - tileSize - roomsNormalHeight * tileSize);
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
                        Vector3 roomCenter = new Vector3(currentGridLocation.x + tileSize + roomsNormalWidth * tileSize, 0, currentGridLocation.z);
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
                        Vector3 roomCenter = new Vector3(currentGridLocation.x + tileSize + roomsNormalWidth * tileSize, 0, currentGridLocation.z);
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
                        Vector3 roomCenter = new Vector3(currentGridLocation.x - tileSize - roomsNormalWidth * tileSize, 0, currentGridLocation.z);
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
                        Vector3 roomCenter = new Vector3(currentGridLocation.x - tileSize - roomsNormalWidth * tileSize, 0, currentGridLocation.z);
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
            Collider[] colliding = Physics.OverlapBox(localRooms[i], new Vector3(roomsNormalWidth / 2 * tileSize, 5, roomsNormalHeight / 2 * tileSize));
            if (colliding.Length > 0) return true;
        }
        return false;
    }

    void BuildJointRoom(int currentRoomTypeID, Vector3 currentGridLocation, FOUR_DIRECTIONS nullifyDoor, List<Vector3> localRooms, int currentRoomTreeIndex, int currentRoomDirection, GameObject[] roomsPool, GameObject[] mapRoomsPool, ref List<GameObject> jointedRooms, ref List<GameObject> mapJoints)
    {
        if (localRooms.Contains(currentGridLocation)) return;
        else localRooms.Add(currentGridLocation);

        RoomInfo currentRoomInfo = roomsInfo.roomInfoList[currentRoomTypeID];

        Vector3 roomPosition = new Vector3(currentGridLocation.x, 0, currentGridLocation.z - roomsNormalHeight / 2 * tileSize);
        GameObject newRoom = GameObject.Instantiate(roomsPool[currentRoomTypeID], roomPosition, Quaternion.identity);
        jointedRooms.Add(newRoom);
        GameObject newMapRoom = GameObject.Instantiate(mapRoomsPool[currentRoomTypeID], createMap);
        newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
        roomsInMap.Add(newMapRoom);
        RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
        script.SetDoors();
        if (nullifyDoor != FOUR_DIRECTIONS.NONE) script.NullifyDoor(nullifyDoor);
        script.roomInMap = newMapRoom;
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
                        Vector3 roomCenter = new Vector3(currentGridLocation.x, 0, currentGridLocation.z + roomsNormalHeight * tileSize);
                        if (!localRooms.Contains(roomCenter))
                        {
                            roomPosition = new Vector3(currentGridLocation.x, 0, currentGridLocation.z + tileSize + roomsNormalHeight / 2 * tileSize);
                            GameObject.Instantiate(jointsPrefabs[currentRoomInfo.jointRoomTypeIdTop], roomPosition, Quaternion.identity);
                            newMapRoom = GameObject.Instantiate(mapJointsPrefabs[currentRoomInfo.jointRoomTypeIdTop], createMap);
                            newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                            mapJoints.Add(newMapRoom);
                        }
                        BuildJointRoom(jointInfo.tail.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms, roomsTree.Count - 1, 0, roomsPool, mapRoomsPool, ref jointedRooms, ref mapJoints); // top
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
                        Vector3 roomCenter = new Vector3(currentGridLocation.x, 0, currentGridLocation.z + roomsNormalHeight * tileSize);
                        if (!localRooms.Contains(roomCenter))
                        {
                            roomPosition = new Vector3(currentGridLocation.x, 0, currentGridLocation.z + tileSize + roomsNormalHeight / 2 * tileSize);
                            GameObject.Instantiate(jointsPrefabs[currentRoomInfo.jointRoomTypeIdTop], roomPosition, Quaternion.identity);
                            newMapRoom = GameObject.Instantiate(mapJointsPrefabs[currentRoomInfo.jointRoomTypeIdTop], createMap);
                            newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                            mapJoints.Add(newMapRoom);
                        }
                        BuildJointRoom(jointInfo.head.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms, roomsTree.Count - 1, 0, roomsPool, mapRoomsPool, ref jointedRooms, ref mapJoints); // top
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
                        Vector3 roomCenter = new Vector3(currentGridLocation.x, 0, currentGridLocation.z - tileSize - roomsNormalHeight * tileSize);
                        if (!localRooms.Contains(roomCenter))
                        {
                            roomPosition = new Vector3(currentGridLocation.x, 0, currentGridLocation.z - tileSize - roomsNormalHeight / 2 * tileSize);
                            GameObject.Instantiate(jointsPrefabs[currentRoomInfo.jointRoomTypeIdDown], roomPosition, Quaternion.identity);
                            newMapRoom = GameObject.Instantiate(mapJointsPrefabs[currentRoomInfo.jointRoomTypeIdDown], createMap);
                            newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                            mapJoints.Add(newMapRoom);

                        }
                        BuildJointRoom(jointInfo.tail.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms, roomsTree.Count - 1, 1, roomsPool, mapRoomsPool, ref jointedRooms, ref mapJoints); // down
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
                        Vector3 roomCenter = new Vector3(currentGridLocation.x, 0, currentGridLocation.z - tileSize - roomsNormalHeight * tileSize);
                        if (!localRooms.Contains(roomCenter))
                        {
                            roomPosition = new Vector3(currentGridLocation.x, 0, currentGridLocation.z - tileSize - roomsNormalHeight / 2 * tileSize);
                            GameObject.Instantiate(jointsPrefabs[currentRoomInfo.jointRoomTypeIdDown], roomPosition, Quaternion.identity);
                            newMapRoom = GameObject.Instantiate(mapJointsPrefabs[currentRoomInfo.jointRoomTypeIdDown], createMap);
                            newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                            mapJoints.Add(newMapRoom);
                        }
                        BuildJointRoom(jointInfo.head.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms, roomsTree.Count - 1, 1, roomsPool, mapRoomsPool, ref jointedRooms, ref mapJoints); // down
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
                        Vector3 roomCenter = new Vector3(currentGridLocation.x + tileSize + roomsNormalWidth * tileSize, 0, currentGridLocation.z);
                        if (!localRooms.Contains(roomCenter))
                        {
                            roomPosition = new Vector3(currentGridLocation.x + tileSize / 2 + roomsNormalWidth / 2 * tileSize, 0, currentGridLocation.z - roomsNormalHeight / 2 * tileSize);
                            GameObject.Instantiate(jointsPrefabs[currentRoomInfo.jointRoomTypeIdRight], roomPosition, Quaternion.identity);
                            newMapRoom = GameObject.Instantiate(mapJointsPrefabs[currentRoomInfo.jointRoomTypeIdRight], createMap);
                            newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                            mapJoints.Add(newMapRoom);
                        }
                        BuildJointRoom(jointInfo.tail.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms, roomsTree.Count - 1, 2, roomsPool, mapRoomsPool, ref jointedRooms, ref mapJoints); // right
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
                        Vector3 roomCenter = new Vector3(currentGridLocation.x + tileSize + roomsNormalWidth * tileSize, 0, currentGridLocation.z);
                        if (!localRooms.Contains(roomCenter))
                        {
                            roomPosition = new Vector3(currentGridLocation.x + tileSize / 2 + roomsNormalWidth / 2 * tileSize, 0, currentGridLocation.z - roomsNormalHeight / 2 * tileSize);
                            GameObject.Instantiate(jointsPrefabs[currentRoomInfo.jointRoomTypeIdRight], roomPosition, Quaternion.identity);
                            newMapRoom = GameObject.Instantiate(mapJointsPrefabs[currentRoomInfo.jointRoomTypeIdRight], createMap);
                            newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                            mapJoints.Add(newMapRoom);
                        }
                        BuildJointRoom(jointInfo.head.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms, roomsTree.Count - 1, 2, roomsPool, mapRoomsPool, ref jointedRooms, ref mapJoints); // right
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
                        Vector3 roomCenter = new Vector3(currentGridLocation.x - tileSize - roomsNormalWidth * tileSize, 0, currentGridLocation.z);
                        if (!localRooms.Contains(roomCenter))
                        {
                            roomPosition = new Vector3(currentGridLocation.x - tileSize / 2 - roomsNormalWidth / 2 * tileSize, 0, currentGridLocation.z - roomsNormalHeight / 2 * tileSize);
                            GameObject.Instantiate(jointsPrefabs[currentRoomInfo.jointRoomTypeIdLeft], roomPosition, Quaternion.identity);
                            newMapRoom = GameObject.Instantiate(mapJointsPrefabs[currentRoomInfo.jointRoomTypeIdLeft], createMap);
                            newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                            mapJoints.Add(newMapRoom);
                        }
                        BuildJointRoom(jointInfo.tail.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms, roomsTree.Count - 1, 3, roomsPool, mapRoomsPool, ref jointedRooms, ref mapJoints); // left
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
                        Vector3 roomCenter = new Vector3(currentGridLocation.x - tileSize - roomsNormalWidth * tileSize, 0, currentGridLocation.z);
                        if (!localRooms.Contains(roomCenter))
                        {
                            roomPosition = new Vector3(currentGridLocation.x - tileSize / 2 - roomsNormalWidth / 2 * tileSize, 0, currentGridLocation.z - roomsNormalHeight / 2 * tileSize);
                            GameObject.Instantiate(jointsPrefabs[currentRoomInfo.jointRoomTypeIdLeft], roomPosition, Quaternion.identity);
                            newMapRoom = GameObject.Instantiate(mapJointsPrefabs[currentRoomInfo.jointRoomTypeIdLeft], createMap);
                            newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                            mapJoints.Add(newMapRoom);
                        }
                        BuildJointRoom(jointInfo.head.objectTypeID, roomCenter, FOUR_DIRECTIONS.NONE, localRooms, roomsTree.Count - 1, 3, roomsPool, mapRoomsPool, ref jointedRooms, ref mapJoints); // left
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

    void StoreJointedRooms(List<GameObject> jointedRooms, List<GameObject> mapJoints)
    {
        for (int i = 0; i < jointedRooms.Count; i++)
        {
            RoomBehaviour script = jointedRooms[i].GetComponent<RoomBehaviour>();
            for (int j = 0; j < jointedRooms.Count; j++)
            {
                if (i == j) continue;
                script.joinedRooms.Add(jointedRooms[j]);
            }
            for (int j = 0; j < mapJoints.Count; j++)
            {
                script.mapJoints.Add(mapJoints[j]);
            }
        }
    }

    void CreateBossRoom(RoomBehaviour lastRoomScript)
    {
        GameObject bossRoomPrefab = bossRoomsPrefabs[Random.Range(0, bossRoomsPrefabs.Length)];
        GameObject mapBossRoomPrefab = mapBossRoomsPrefabs[Random.Range(0, mapBossRoomsPrefabs.Length)];
        BossRoomBehaviour bossScript = bossRoomPrefab.GetComponent<BossRoomBehaviour>();

        // find position
        if (!CreateBossRoomLoop(roomsTree[GetTreeIndex(lastRoomScript)], bossRoomPrefab, mapBossRoomPrefab, bossScript)) Debug.LogError("Boss room was imposible to be created");
    }

    bool CreateBossRoomLoop(TreeNode node, GameObject bossRoomPrefab, GameObject mapBossRoomPrefab, BossRoomBehaviour bossScript) // true --> created, false --> imposible to create boss room
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
                    colliding = Physics.OverlapBox(new Vector3(bossEntrance.position.x, 0, bossEntrance.position.z + tileSize + bossScript.height * tileSize / 2), new Vector3(bossScript.width * tileSize / 2, 5, bossScript.height * tileSize / 2), Quaternion.identity);
                    if (colliding.Length == 0)
                    {
                        Vector3 roomPosition = new Vector3(bossEntrance.position.x, 0, bossEntrance.position.z + tileSize);
                        bossRoom = GameObject.Instantiate(bossRoomPrefab, roomPosition, Quaternion.identity);
                        GameObject newMapRoom = GameObject.Instantiate(mapBossRoomPrefab, createMap);
                        newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                        newMapRoom.transform.localRotation = Quaternion.identity;
                        roomsInMap.Add(newMapRoom);
                        bossScript.roomInMap = newMapRoom;
                        bossEntrance.state = DOOR_STATE.BOSS;
                        return true;
                    }
                    else // no space
                    {
                        lastRoomDoors.Remove(bossEntrance);
                    }
                    break;
                case FOUR_DIRECTIONS.DOWN:
                    colliding = Physics.OverlapBox(new Vector3(bossEntrance.position.x, 0, bossEntrance.position.z - tileSize - bossScript.height * tileSize / 2), new Vector3(bossScript.width * tileSize / 2, 5, bossScript.height * tileSize / 2), Quaternion.AngleAxis(180, Vector3.up));
                    if (colliding.Length == 0)
                    {
                        Vector3 roomPosition = new Vector3(bossEntrance.position.x, 0, bossEntrance.position.z - tileSize);
                        bossRoom = GameObject.Instantiate(bossRoomPrefab, roomPosition, Quaternion.AngleAxis(180, Vector3.up));
                        GameObject newMapRoom = GameObject.Instantiate(mapBossRoomPrefab, createMap);
                        newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                        newMapRoom.transform.localRotation = Quaternion.AngleAxis(180, Vector3.forward);
                        roomsInMap.Add(newMapRoom);
                        bossScript.roomInMap = newMapRoom;
                        bossEntrance.state = DOOR_STATE.BOSS;
                        return true;
                    }
                    else // no space
                    {
                        lastRoomDoors.Remove(bossEntrance);
                    }
                    break;
                case FOUR_DIRECTIONS.RIGHT:
                    colliding = Physics.OverlapBox(new Vector3(bossEntrance.position.x + tileSize + bossScript.width * tileSize / 2, 0, bossEntrance.position.z), new Vector3(bossScript.width * tileSize / 2, 5, bossScript.height * tileSize / 2), Quaternion.AngleAxis(90, Vector3.up));
                    if (colliding.Length == 0)
                    {
                        Vector3 roomPosition = new Vector3(bossEntrance.position.x + tileSize, 0, bossEntrance.position.z);
                        bossRoom = GameObject.Instantiate(bossRoomPrefab, roomPosition, Quaternion.AngleAxis(90, Vector3.up));
                        GameObject newMapRoom = GameObject.Instantiate(mapBossRoomPrefab, createMap);
                        newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                        newMapRoom.transform.localRotation = Quaternion.AngleAxis(270, Vector3.forward);
                        roomsInMap.Add(newMapRoom);
                        bossScript.roomInMap = newMapRoom;
                        bossEntrance.state = DOOR_STATE.BOSS;
                        return true;
                    }
                    else // no space
                    {
                        lastRoomDoors.Remove(bossEntrance);
                    }
                    break;
                case FOUR_DIRECTIONS.LEFT:
                    colliding = Physics.OverlapBox(new Vector3(bossEntrance.position.x - tileSize - bossScript.width * tileSize / 2, 0, bossEntrance.position.z), new Vector3(bossScript.width * tileSize / 2, 5, bossScript.height * tileSize / 2), Quaternion.AngleAxis(270, Vector3.up));
                    if (colliding.Length == 0)
                    {
                        Vector3 roomPosition = new Vector3(bossEntrance.position.x - tileSize, 0, bossEntrance.position.z);
                        bossRoom = GameObject.Instantiate(bossRoomPrefab, roomPosition, Quaternion.AngleAxis(270, Vector3.up));
                        GameObject newMapRoom = GameObject.Instantiate(mapBossRoomPrefab, createMap);
                        newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                        newMapRoom.transform.localRotation = Quaternion.AngleAxis(90, Vector3.forward);
                        roomsInMap.Add(newMapRoom);
                        bossScript.roomInMap = newMapRoom;
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
        else return CreateBossRoomLoop(node.parent, bossRoomPrefab, mapBossRoomPrefab, bossScript);
    }

    void FillWithRooms()
    {
        RoomBehaviour auxScript;
        List<Door> forFillDoors = new();

        // Upgrades
        CheckIfUpgradesRemaining();
        
        while (currentRooms < maxRooms)
        {
            FindForFillDoors(roomsTree[1], ref forFillDoors); // 1 --> ignore start room
            if (forFillDoors.Count == 0) break; // no more rooms for fill
            Door randomDoor = forFillDoors[Random.Range(0, forFillDoors.Count)];

            if (Random.Range(0, 100) < breakDoorChance)
            {
                randomDoor.state = DOOR_STATE.DESTROYED;
                forFillDoors.Remove(randomDoor);
                continue;
            }

            bool upgradeSuccesfullyPlacedThisLoop = false;
            if (!upgradeRoomPlaced)
            {
                bool upgradeRoom = (Random.Range(0, 100) < 100);
                if (upgradeRoom)
                {
                    auxScript = CreateNextUpgradeRoom(randomDoor, GetTreeIndex(randomDoor.script), upgradeRoomsPrefabs, mapUpgradeRoomsPrefabs); // upgrade room
                    if (auxScript != null)
                    {
                        lastRoomCreated = auxScript.roomTypeID;
                        randomDoor.state = GetDoorColor();
                        forFillDoors.Clear();
                        upgradeRoomPlaced = true;
                        upgradeSuccesfullyPlacedThisLoop = true;
                    }
                }
            }

            if (!upgradeSuccesfullyPlacedThisLoop)
            {
                bool powerUpRoom = !(Random.Range(0, 100) < 80) && powerUpRoomsLeft > 0;
                if (powerUpRoom)
                {
                    auxScript = CreateNextPowerUpRoom(randomDoor, GetTreeIndex(randomDoor.script), powerUpRoomsPrefabs, mapPowerUpRoomsPrefabs); // power up room
                    if (auxScript != null)
                    {
                        lastRoomCreated = auxScript.roomTypeID;
                        randomDoor.state = GetDoorColor();
                        forFillDoors.Clear();
                        powerUpRoomsLeft--;
                    }
                    else powerUpRoom = false;
                }
                if (!powerUpRoom)
                {
                    auxScript = CreateNextRoom(randomDoor, GetTreeIndex(randomDoor.script), roomsPrefabs, mapRoomsPrefabs); // no power up room && no upgrade room
                    if (auxScript != null)
                    {
                        lastRoomCreated = auxScript.roomTypeID;
                        randomDoor.state = GetDoorColor();
                        forFillDoors.Clear();
                    }
                    else // door imposible to fill
                    {
                        forFillDoors.Remove(randomDoor);
                    }
                }
            }
        }

        // set rest of doors on hold state
        forFillDoors.Clear();
        FindForFillDoors(roomsTree[1], ref forFillDoors);
        for (int i = 0; i < forFillDoors.Count; i++)
        {
            forFillDoors[i].state = DOOR_STATE.HOLDED;
        }
    }

    void FindForFillDoors(TreeNode node, ref List<Door> forFillDoors)
    {
        if (node.script.roomTypeID == -1) return; // ending room
        RoomInfo roomInfo;
        switch (node.script.roomType)
        {
            case ROOM_TYPE.POWER_UP:
                roomInfo = roomsInfo.powerUpRoomInfoList[node.script.roomTypeID];
                break;
            case ROOM_TYPE.UPGRADE:
                roomInfo = roomsInfo.upgradeRoomInfoList[node.script.roomTypeID];
                break;
            case ROOM_TYPE.NORMAL:
            default:
                roomInfo = roomsInfo.roomInfoList[node.script.roomTypeID];
                break;
        }

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
                    case DOOR_STATE.BLUE:
                    case DOOR_STATE.RED:
                    case DOOR_STATE.PURPLE:
                    case DOOR_STATE.GREEN:
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
                    case DOOR_STATE.BLUE:
                    case DOOR_STATE.RED:
                    case DOOR_STATE.PURPLE:
                    case DOOR_STATE.GREEN:
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
                    case DOOR_STATE.BLUE:
                    case DOOR_STATE.RED:
                    case DOOR_STATE.PURPLE:
                    case DOOR_STATE.GREEN:
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
                    case DOOR_STATE.BLUE:
                    case DOOR_STATE.RED:
                    case DOOR_STATE.PURPLE:
                    case DOOR_STATE.GREEN:
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

    void CreateHallways()
    {
        List<Door> allDoors = new();
        FindAllDoors(roomsTree[0], ref allDoors); // 0 --> start room

        List<Gate> gates = new();
        while (allDoors.Count > 0)
        {
            Door neighborDoor = NeighborDoor(allDoors[0]);
            GATE_STATE state = GATE_STATE.NULL;
            switch (allDoors[0].state)
            {
                case DOOR_STATE.FOR_FILL:
                    Debug.LogError("Logic Error");
                    break;
                case DOOR_STATE.YELLOW:
                    state = GATE_STATE.YELLOW;
                    break;
                case DOOR_STATE.BLUE:
                    state = GATE_STATE.BLUE;
                    break;
                case DOOR_STATE.RED:
                    state = GATE_STATE.RED;
                    break;
                case DOOR_STATE.PURPLE:
                    state = GATE_STATE.PURPLE;
                    break;
                case DOOR_STATE.GREEN:
                    state = GATE_STATE.GREEN;
                    break;
                case DOOR_STATE.BOSS:
                    state = GATE_STATE.BOSS;
                    break;
                case DOOR_STATE.HOLDED:
                    if (neighborDoor != null) state = GATE_STATE.YELLOW;
                    else state = GATE_STATE.DESTROYED; // no conection
                    break;
                case DOOR_STATE.DESTROYED:
                    state = GATE_STATE.DESTROYED;
                    break;
                case DOOR_STATE.NULL:
                    Door aux = allDoors[0];
                    allDoors.Remove(allDoors[0]);
                    allDoors.Add(aux);
                    continue;
                default:
                    Debug.LogError("Logic Error");
                    break;
            }

            if (neighborDoor != null)
            {
                allDoors[0].otherScript = neighborDoor.script;
                neighborDoor.otherScript = allDoors[0].script;
                Gate gate1 = new Gate(allDoors[0].position, allDoors[0].direction, state, null, allDoors[0].script);
                Gate gate2 = new Gate(neighborDoor.position, neighborDoor.direction, state, gate1, neighborDoor.script);
                gate1.SetOther(gate2);

                PlaceHallway(gate1.position, gate1.direction, allDoors[0].script, neighborDoor.script);

                gates.Add(gate1);
                gates.Add(gate2);
                allDoors.Remove(allDoors[0]);
                allDoors.Remove(neighborDoor);
            }
            else if (state == GATE_STATE.BOSS)
            {
                BossRoomBehaviour bossScript = bossRoom.GetComponent<BossRoomBehaviour>();
                FOUR_DIRECTIONS bossGateDir;
                switch (allDoors[0].direction)
                {
                    case FOUR_DIRECTIONS.TOP: bossGateDir = FOUR_DIRECTIONS.DOWN; break;
                    case FOUR_DIRECTIONS.DOWN: bossGateDir = FOUR_DIRECTIONS.TOP; break;
                    case FOUR_DIRECTIONS.RIGHT: bossGateDir = FOUR_DIRECTIONS.LEFT; break;
                    case FOUR_DIRECTIONS.LEFT: bossGateDir = FOUR_DIRECTIONS.RIGHT; break;
                    case FOUR_DIRECTIONS.NONE:
                    default: bossGateDir = FOUR_DIRECTIONS.NONE; break;
                }
                allDoors[0].otherScript = bossScript;
                Gate gate1 = new Gate(allDoors[0].position, allDoors[0].direction, state, null, allDoors[0].script);
                Gate bossEnterGate = new Gate(bossScript.GetEnterDoorPosition(), bossGateDir, state, gate1, bossScript);
                gate1.SetOther(bossEnterGate);

                PlaceHallway(gate1.position, gate1.direction, allDoors[0].script, bossScript);

                gates.Add(gate1);
                gates.Add(bossEnterGate);
                allDoors.Remove(allDoors[0]);
            }
            else
            {
                allDoors[0].otherScript = null;
                Gate gate1 = new Gate(allDoors[0].position, allDoors[0].direction, state, null, allDoors[0].script);

                gates.Add(gate1);
                allDoors.Remove(allDoors[0]);
            }
        }
        draw = gates;
        placeGates.gates = gates;
        placeGates.PlaceAllGates();
    }

    void FindAllDoors(TreeNode node, ref List<Door> allDoors)
    {
        for (int i = 0; i < node.script.doors.Count; i++)
        {
            allDoors.Add(node.script.doors[i]);
        }

        if (node.children[0] != null) FindAllDoors(node.children[0], ref allDoors);
        if (node.children[1] != null) FindAllDoors(node.children[1], ref allDoors);
        if (node.children[2] != null) FindAllDoors(node.children[2], ref allDoors);
        if (node.children[3] != null) FindAllDoors(node.children[3], ref allDoors);
    }

    Door NeighborDoor(Door holdedDoor)
    {
        Collider[] colliding;
        switch (holdedDoor.direction)
        {
            case FOUR_DIRECTIONS.TOP:
                colliding = Physics.OverlapBox(new Vector3(holdedDoor.position.x, 0, holdedDoor.position.z + tileSize), new Vector3(1, 5, 1), Quaternion.identity, doorLayer);
                if (colliding.Length == 1)
                {
                    RoomBehaviour script = FindScriptInParent(colliding[0].transform);
                    return script.GetDoorByDirection(FOUR_DIRECTIONS.DOWN);
                }
                else if (colliding.Length > 0) Debug.LogError("Logic Error");
                return null; // no neighbor door
            case FOUR_DIRECTIONS.DOWN:
                colliding = Physics.OverlapBox(new Vector3(holdedDoor.position.x, 0, holdedDoor.position.z - tileSize), new Vector3(1, 5, 1), Quaternion.identity, doorLayer);
                if (colliding.Length == 1)
                {
                    RoomBehaviour script = FindScriptInParent(colliding[0].transform);
                    return script.GetDoorByDirection(FOUR_DIRECTIONS.TOP);
                }
                else if (colliding.Length > 0) Debug.LogError("Logic Error");
                return null; // no neighbor door
            case FOUR_DIRECTIONS.RIGHT:
                colliding = Physics.OverlapBox(new Vector3(holdedDoor.position.x + tileSize, 0, holdedDoor.position.z), new Vector3(1, 5, 1), Quaternion.identity, doorLayer);
                if (colliding.Length == 1)
                {
                    RoomBehaviour script = FindScriptInParent(colliding[0].transform);
                    return script.GetDoorByDirection(FOUR_DIRECTIONS.LEFT);
                }
                else if (colliding.Length > 0) Debug.LogError("Logic Error");
                return null; // no neighbor door
            case FOUR_DIRECTIONS.LEFT:
                colliding = Physics.OverlapBox(new Vector3(holdedDoor.position.x - tileSize, 0, holdedDoor.position.z), new Vector3(1, 5, 1), Quaternion.identity, doorLayer);
                if (colliding.Length == 1)
                {
                    RoomBehaviour script = FindScriptInParent(colliding[0].transform);
                    return script.GetDoorByDirection(FOUR_DIRECTIONS.RIGHT);
                }
                else if (colliding.Length > 0) Debug.LogError("Logic Error");
                return null; // no neighbor door
            case FOUR_DIRECTIONS.NONE:
            default:
                Debug.LogError("Logic Error");
                return null;
        }
    }

    RoomBehaviour FindScriptInParent(Transform child)
    {
        Transform auxTransform = child;
        while (auxTransform != null && auxTransform.GetComponent<RoomBehaviour>() == null)
        {
            auxTransform = auxTransform.parent;
        }

        if (auxTransform == null) return null;
        else return auxTransform.GetComponent<RoomBehaviour>();
    }

    void PlaceHallway(Vector3 doorPosition, FOUR_DIRECTIONS direction, RoomBehaviour headRoom, RoomBehaviour tailRoom)
    {
        Quaternion rotation;
        Quaternion mapRotation;
        switch (direction)
        {
            case FOUR_DIRECTIONS.TOP:
                rotation = Quaternion.identity;
                mapRotation = Quaternion.identity;
                break;
            case FOUR_DIRECTIONS.DOWN:
                rotation = Quaternion.AngleAxis(180, Vector3.up);
                mapRotation = Quaternion.AngleAxis(180, Vector3.forward);
                break;
            case FOUR_DIRECTIONS.RIGHT:
                rotation = Quaternion.AngleAxis(90, Vector3.up);
                mapRotation = Quaternion.AngleAxis(270, Vector3.forward);
                break;
            case FOUR_DIRECTIONS.LEFT:
                rotation = Quaternion.AngleAxis(270, Vector3.up);
                mapRotation = Quaternion.AngleAxis(90, Vector3.forward);
                break;
            case FOUR_DIRECTIONS.NONE:
            default:
                Debug.LogError("Logic Error");
                return;
        }
        GameObject newHallway = GameObject.Instantiate(hallwayPrefab, doorPosition, rotation);
        GameObject newMapHallway = GameObject.Instantiate(mapHallwayPrefab, createMap);
        newMapHallway.transform.localPosition = new Vector3(doorPosition.x / 3, doorPosition.z / 3, 0);
        newMapHallway.transform.localRotation = mapRotation;
        HallwayBehaviour script = newHallway.GetComponent<HallwayBehaviour>();
        script.hallwayInMap = newMapHallway;
        script.headRoom = headRoom;
        script.tailRoom = tailRoom;
        hallways.Add(script);
    }

    DOOR_STATE GetDoorColor()
    {
        int rand = Random.Range(0, 100);
        if (rand < 60) return DOOR_STATE.YELLOW;
        else if (rand < 75) return DOOR_STATE.BLUE;
        else if (rand < 90) return DOOR_STATE.RED;
        else if (rand < 98) return DOOR_STATE.PURPLE;
        else return DOOR_STATE.GREEN;
    }

    RoomBehaviour CreateNextPowerUpRoom(Door door, int roomTreeIndex, GameObject[] roomsPool, GameObject[] mapRoomsPool)
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
                    if (posibleRooms.Count == imposibleRooms.Count) return null;
                    do
                    {
                        newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    } while (imposibleRooms.Contains(newRoomTypeID));
                    RoomInfo roomInfo = roomsInfo.powerUpRoomInfoList[newRoomTypeID];

                    Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + tileSize/*HallwaySize*/ + roomsNormalHeight / 2 * tileSize);
                    Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth / 2 * tileSize, 5, roomsNormalHeight / 2 * tileSize));
                    if (colliding.Length == 0) // no room there
                    {
                        if (roomInfo.downDoor == 1) // if room has a door down
                        {
                            Vector3 roomPosition = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + tileSize);
                            GameObject newRoom = GameObject.Instantiate(roomsPool[newRoomTypeID], roomPosition, Quaternion.identity);
                            GameObject newMapRoom = GameObject.Instantiate(mapRoomsPool[newRoomTypeID], createMap);
                            newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                            roomsInMap.Add(newMapRoom);
                            currentRooms++;
                            RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                            script.SetDoors();
                            script.NullifyDoor(FOUR_DIRECTIONS.DOWN);
                            script.roomInMap = newMapRoom;
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
                        return null;
                    }
                }
                break;
            case FOUR_DIRECTIONS.DOWN:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == imposibleRooms.Count) return null;
                    do
                    {
                        newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    } while (imposibleRooms.Contains(newRoomTypeID));
                    RoomInfo roomInfo = roomsInfo.powerUpRoomInfoList[newRoomTypeID];

                    Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z - tileSize/*HallwaySize*/ - roomsNormalHeight / 2 * tileSize);
                    Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth / 2 * tileSize, 5, roomsNormalHeight / 2 * tileSize));
                    if (colliding.Length == 0) // no room there
                    {
                        if (roomInfo.topDoor == 1) // if room has a door at top
                        {
                            Vector3 roomPosition = new Vector3(door.position.x, door.position.y/*0*/, door.position.z - tileSize - roomsNormalHeight * tileSize);
                            GameObject newRoom = GameObject.Instantiate(roomsPool[newRoomTypeID], roomPosition, Quaternion.identity);
                            GameObject newMapRoom = GameObject.Instantiate(mapRoomsPool[newRoomTypeID], createMap);
                            newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                            roomsInMap.Add(newMapRoom);
                            currentRooms++;
                            RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                            script.SetDoors();
                            script.NullifyDoor(FOUR_DIRECTIONS.TOP);
                            script.roomInMap = newMapRoom;
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
                        return null;
                    }
                }
                break;
            case FOUR_DIRECTIONS.RIGHT:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == imposibleRooms.Count) return null;
                    do
                    {
                        newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    } while (imposibleRooms.Contains(newRoomTypeID));
                    RoomInfo roomInfo = roomsInfo.powerUpRoomInfoList[newRoomTypeID];

                    Vector3 roomCenter = new Vector3(door.position.x + tileSize/*HallwaySize*/ + roomsNormalWidth / 2 * tileSize, door.position.y/*0*/, door.position.z);
                    Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth / 2 * tileSize, 5, roomsNormalHeight / 2 * tileSize));
                    if (colliding.Length == 0) // no room there
                    {
                        if (roomInfo.leftDoor == 1) // if room has a door at left
                        {
                            Vector3 roomPosition = new Vector3(door.position.x + tileSize + roomsNormalWidth / 2 * tileSize, door.position.y/*0*/, door.position.z - roomsNormalHeight / 2 * tileSize);
                            GameObject newRoom = GameObject.Instantiate(roomsPool[newRoomTypeID], roomPosition, Quaternion.identity);
                            GameObject newMapRoom = GameObject.Instantiate(mapRoomsPool[newRoomTypeID], createMap);
                            newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                            roomsInMap.Add(newMapRoom);
                            currentRooms++;
                            RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                            script.SetDoors();
                            script.NullifyDoor(FOUR_DIRECTIONS.LEFT);
                            script.roomInMap = newMapRoom;
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
                        return null;
                    }
                }
                break;
            case FOUR_DIRECTIONS.LEFT:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == imposibleRooms.Count) return null;
                    do
                    {
                        newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    } while (imposibleRooms.Contains(newRoomTypeID));
                    RoomInfo roomInfo = roomsInfo.powerUpRoomInfoList[newRoomTypeID];

                    Vector3 roomCenter = new Vector3(door.position.x - 2/*HallwaySize*/ - roomsNormalWidth / 2 * tileSize, door.position.y/*0*/, door.position.z);
                    Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth / 2 * tileSize, 5, roomsNormalHeight / 2 * tileSize));
                    if (colliding.Length == 0) // no room there
                    {
                        if (roomInfo.rightDoor == 1) // if room has a door at right
                        {
                            Vector3 roomPosition = new Vector3(door.position.x - tileSize - roomsNormalWidth / 2 * tileSize, door.position.y/*0*/, door.position.z - roomsNormalHeight / 2 * tileSize);
                            GameObject newRoom = GameObject.Instantiate(roomsPool[newRoomTypeID], roomPosition, Quaternion.identity);
                            GameObject newMapRoom = GameObject.Instantiate(mapRoomsPool[newRoomTypeID], createMap);
                            newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                            roomsInMap.Add(newMapRoom);
                            currentRooms++;
                            RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                            script.SetDoors();
                            script.NullifyDoor(FOUR_DIRECTIONS.RIGHT);
                            script.roomInMap = newMapRoom;
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
                        return null;
                    }
                }
                break;
        }
        return null;
    }

    void PlacePowerUps()
    {
        List<GameObject> posiblePowerUps = new(powerUpPrefabs.ToList());
        GameObject[] powerUpGO = GameObject.FindGameObjectsWithTag("PowerUp");

        foreach (var go in powerUpGO) // each powe up spot
        {
            bool compatible = false;
            GameObject newPowerUp = null;
            while (!compatible)
            {
                newPowerUp = posiblePowerUps[Random.Range(0, posiblePowerUps.Count)];
                PowerUp script = newPowerUp.GetComponent<PowerUp>();
                compatible = IsCompatible(script.type);
                if (!compatible) posiblePowerUps.Remove(newPowerUp);
            }

            posiblePowerUps.Remove(newPowerUp);
            GameObject.Instantiate(newPowerUp, go.transform);
        }
    }

    bool IsCompatible(POWER_UP_TYPE type)
    {
        foreach(var powerUp in currentPowerUps)
        {
            foreach (var incompatibility in powerUp.incompatibilities)
            {
                if (incompatibility == type) return false;
            }
        }
        return true;
    }

    RoomBehaviour CreateNextUpgradeRoom(Door door, int roomTreeIndex, GameObject[] roomsPool, GameObject[] mapRoomsPool)
    {
        List<GameObject> posibleRooms = new(roomsPool.ToList());
        List<int> posibleUpgradeRoomsCount = new();
        PlayerSkills playerSkills = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSkills>();
        switch (upgradeLevel)
        {
            case 0:
                if (!playerSkills.CheckIfSkillIsOwned(SKILL_TYPE.DASH)) posibleUpgradeRoomsCount.Add(0);
                break;
            case 1:
                if (!playerSkills.CheckIfSkillIsOwned(SKILL_TYPE.BLUE)) posibleUpgradeRoomsCount.Add(1);
                if (!playerSkills.CheckIfSkillIsOwned(SKILL_TYPE.RED)) posibleUpgradeRoomsCount.Add(2);
                if (!playerSkills.CheckIfSkillIsOwned(SKILL_TYPE.BLOCKING_FIST)) posibleUpgradeRoomsCount.Add(3);
                break;
            case 2:
                if (!playerSkills.CheckIfSkillIsOwned(SKILL_TYPE.PURPLE)) posibleUpgradeRoomsCount.Add(4);
                if (!playerSkills.CheckIfSkillIsOwned(SKILL_TYPE.SUPER_JUMP)) posibleUpgradeRoomsCount.Add(5);
                break;
            case 3:
                if (!playerSkills.CheckIfSkillIsOwned(SKILL_TYPE.GREEN)) posibleUpgradeRoomsCount.Add(6);
                break;
            default:
                Debug.LogError("Logic Error");
                break;
        }
        List<int> imposibleRooms = new();
        if (lastRoomCreated != -1) imposibleRooms.Add(lastRoomCreated);
        int newRoomTypeID = -1;

        switch (door.direction)
        {
            case FOUR_DIRECTIONS.TOP:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == imposibleRooms.Count || posibleUpgradeRoomsCount.Count == imposibleRooms.Count) return null;
                    do
                    {
                        newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    } while (imposibleRooms.Contains(newRoomTypeID) || !posibleUpgradeRoomsCount.Contains(newRoomTypeID));
                    RoomInfo roomInfo = roomsInfo.upgradeRoomInfoList[newRoomTypeID];

                    Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + tileSize/*HallwaySize*/ + roomsNormalHeight / 2 * tileSize);
                    Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth / 2 * tileSize, 5, roomsNormalHeight / 2 * tileSize));
                    if (colliding.Length == 0) // no room there
                    {
                        if (roomInfo.downDoor == 1) // if room has a door down
                        {
                            Vector3 roomPosition = new Vector3(door.position.x, door.position.y/*0*/, door.position.z + tileSize);
                            GameObject newRoom = GameObject.Instantiate(roomsPool[newRoomTypeID], roomPosition, Quaternion.identity);
                            GameObject newMapRoom = GameObject.Instantiate(mapRoomsPool[newRoomTypeID], createMap);
                            newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                            roomsInMap.Add(newMapRoom);
                            currentRooms++;
                            RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                            script.SetDoors();
                            script.NullifyDoor(FOUR_DIRECTIONS.DOWN);
                            script.roomInMap = newMapRoom;
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
                        return null;
                    }
                }
                break;
            case FOUR_DIRECTIONS.DOWN:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == imposibleRooms.Count || posibleUpgradeRoomsCount.Count == imposibleRooms.Count) return null;
                    do
                    {
                        newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    } while (imposibleRooms.Contains(newRoomTypeID) || !posibleUpgradeRoomsCount.Contains(newRoomTypeID));
                    RoomInfo roomInfo = roomsInfo.upgradeRoomInfoList[newRoomTypeID];

                    Vector3 roomCenter = new Vector3(door.position.x, door.position.y/*0*/, door.position.z - tileSize/*HallwaySize*/ - roomsNormalHeight / 2 * tileSize);
                    Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth / 2 * tileSize, 5, roomsNormalHeight / 2 * tileSize));
                    if (colliding.Length == 0) // no room there
                    {
                        if (roomInfo.topDoor == 1) // if room has a door at top
                        {
                            Vector3 roomPosition = new Vector3(door.position.x, door.position.y/*0*/, door.position.z - tileSize - roomsNormalHeight * tileSize);
                            GameObject newRoom = GameObject.Instantiate(roomsPool[newRoomTypeID], roomPosition, Quaternion.identity);
                            GameObject newMapRoom = GameObject.Instantiate(mapRoomsPool[newRoomTypeID], createMap);
                            newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                            roomsInMap.Add(newMapRoom);
                            currentRooms++;
                            RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                            script.SetDoors();
                            script.NullifyDoor(FOUR_DIRECTIONS.TOP);
                            script.roomInMap = newMapRoom;
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
                        return null;
                    }
                }
                break;
            case FOUR_DIRECTIONS.RIGHT:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == imposibleRooms.Count || posibleUpgradeRoomsCount.Count == imposibleRooms.Count) return null;
                    do
                    {
                        newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    } while (imposibleRooms.Contains(newRoomTypeID) || !posibleUpgradeRoomsCount.Contains(newRoomTypeID));
                    RoomInfo roomInfo = roomsInfo.upgradeRoomInfoList[newRoomTypeID];

                    Vector3 roomCenter = new Vector3(door.position.x + tileSize/*HallwaySize*/ + roomsNormalWidth / 2 * tileSize, door.position.y/*0*/, door.position.z);
                    Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth / 2 * tileSize, 5, roomsNormalHeight / 2 * tileSize));
                    if (colliding.Length == 0) // no room there
                    {
                        if (roomInfo.leftDoor == 1) // if room has a door at left
                        {
                            Vector3 roomPosition = new Vector3(door.position.x + tileSize + roomsNormalWidth / 2 * tileSize, door.position.y/*0*/, door.position.z - roomsNormalHeight / 2 * tileSize);
                            GameObject newRoom = GameObject.Instantiate(roomsPool[newRoomTypeID], roomPosition, Quaternion.identity);
                            GameObject newMapRoom = GameObject.Instantiate(mapRoomsPool[newRoomTypeID], createMap);
                            newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                            roomsInMap.Add(newMapRoom);
                            currentRooms++;
                            RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                            script.SetDoors();
                            script.NullifyDoor(FOUR_DIRECTIONS.LEFT);
                            script.roomInMap = newMapRoom;
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
                        return null;
                    }
                }
                break;
            case FOUR_DIRECTIONS.LEFT:
                while (newRoomTypeID == -1)
                {
                    if (posibleRooms.Count == imposibleRooms.Count || posibleUpgradeRoomsCount.Count == imposibleRooms.Count) return null;
                    do
                    {
                        newRoomTypeID = Random.Range(0, posibleRooms.Count);
                    } while (imposibleRooms.Contains(newRoomTypeID) || !posibleUpgradeRoomsCount.Contains(newRoomTypeID));
                    RoomInfo roomInfo = roomsInfo.upgradeRoomInfoList[newRoomTypeID];

                    Vector3 roomCenter = new Vector3(door.position.x - 2/*HallwaySize*/ - roomsNormalWidth / 2 * tileSize, door.position.y/*0*/, door.position.z);
                    Collider[] colliding = Physics.OverlapBox(roomCenter, new Vector3(roomsNormalWidth / 2 * tileSize, 5, roomsNormalHeight / 2 * tileSize));
                    if (colliding.Length == 0) // no room there
                    {
                        if (roomInfo.rightDoor == 1) // if room has a door at right
                        {
                            Vector3 roomPosition = new Vector3(door.position.x - tileSize - roomsNormalWidth / 2 * tileSize, door.position.y/*0*/, door.position.z - roomsNormalHeight / 2 * tileSize);
                            GameObject newRoom = GameObject.Instantiate(roomsPool[newRoomTypeID], roomPosition, Quaternion.identity);
                            GameObject newMapRoom = GameObject.Instantiate(mapRoomsPool[newRoomTypeID], createMap);
                            newMapRoom.transform.localPosition = new Vector3(roomPosition.x / 3, roomPosition.z / 3, 0);
                            roomsInMap.Add(newMapRoom);
                            currentRooms++;
                            RoomBehaviour script = newRoom.GetComponent<RoomBehaviour>();
                            script.SetDoors();
                            script.NullifyDoor(FOUR_DIRECTIONS.RIGHT);
                            script.roomInMap = newMapRoom;
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
                        return null;
                    }
                }
                break;
        }
        return null;
    }

    void CheckIfUpgradesRemaining()
    {
        int level = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerSkills>().GetUpgradeLevel();

        if (level == -1) upgradeRoomPlaced = true;
        else upgradeLevel = level;
    }

    // Map
    void SetMapSize()
    {
        float maxX = -9999, maxY = -9999, minX = 9999, minY = 9999;

        for (int i = 0; i < roomsInMap.Count; i++)
        {
            if (roomsInMap[i].transform.localPosition.x > maxX) maxX = roomsInMap[i].transform.localPosition.x;
            if (roomsInMap[i].transform.localPosition.y > maxY) maxY = roomsInMap[i].transform.localPosition.y;
            if (roomsInMap[i].transform.localPosition.x < minX) minX = roomsInMap[i].transform.localPosition.x;
            if (roomsInMap[i].transform.localPosition.y < minY) minY = roomsInMap[i].transform.localPosition.y;
        }

        mapCenterPos.GetComponent<RectTransform>().sizeDelta = new Vector2(maxX - minX + 20, maxY - minY + 20);
        createMap.GetComponent<RectTransform>().localPosition = new Vector3(-(maxX + minX) / 2.0f, -(maxY + minY + 5) / 2.0f, 0);
    }

    // Enable and Disable Rooms
    public void UpdateRooms(RoomBehaviour currentRoom)
    {
        // rooms
        List<RoomBehaviour> newActiveRooms = new();
        newActiveRooms.Add(currentRoom);

        foreach (var door in currentRoom.doors)
        {
            if (door.otherScript)
            {
                newActiveRooms.Add(door.otherScript);
                foreach (var jointedRoom in door.otherScript.joinedRooms)
                {
                    newActiveRooms.Add(jointedRoom.GetComponent<RoomBehaviour>());
                }
            }
        }
        foreach (var jointedRoom in currentRoom.joinedRooms)
        {
            RoomBehaviour jointedRoomScript = jointedRoom.GetComponent<RoomBehaviour>();
            newActiveRooms.Add(jointedRoomScript);
            foreach (var door in jointedRoomScript.doors)
            {
                if (door.otherScript) newActiveRooms.Add(door.otherScript);
            }
        }

        foreach (var room in activeRooms)
        {
            if (!newActiveRooms.Contains(room)) room.gameObject.SetActive(false);
        }

        foreach (var room in newActiveRooms)
        {
            if (!room.gameObject.activeSelf) room.gameObject.SetActive(true);
        }

        activeRooms = newActiveRooms;

        List<HallwayBehaviour> newActiveHallways = new();

        // hallways
        foreach (var hallway in hallways)
        {
            foreach (var headRoom in newActiveRooms)
            {
                if (hallway.headRoom == headRoom)
                {
                    foreach (var tailRoom in newActiveRooms)
                    {
                        if (hallway.tailRoom == tailRoom) newActiveHallways.Add(hallway);
                    }
                }
            }
        }

        foreach (var halway in activeHallways)
        {
            if (!newActiveHallways.Contains(halway)) halway.gameObject.SetActive(false);
        }

        foreach (var halway in newActiveHallways)
        {
            if (!halway.gameObject.activeSelf) halway.gameObject.SetActive(true);
        }

        activeHallways = newActiveHallways;
    }
}

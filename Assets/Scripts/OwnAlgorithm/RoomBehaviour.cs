using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public enum DOOR_STATE
{
    FOR_FILL,
    YELLOW,
    BLUE,
    RED,
    PURPLE,
    GREEN,
    BOSS,
    HOLDED,
    DESTROYED,
    NULL // no tiene puerta
}

public class Door
{
    public Vector3 position;
    public FOUR_DIRECTIONS direction;
    public DOOR_STATE state;
    public RoomBehaviour script;

    public Door(Vector3 position, FOUR_DIRECTIONS direction, RoomBehaviour script)
    {
        this.position = position;
        this.direction = direction;
        this.state = DOOR_STATE.FOR_FILL;
        this.script = script;
    }
}

public class RoomBehaviour : MonoBehaviour
{
    [Header("Room")]
    public int roomTypeID;
    [HideInInspector] public OwnRoomGenarator manager;

    [NonEditable] bool doorsFilled = false;
    [SerializeField] Transform[] doorsTransform;
    [SerializeField] FOUR_DIRECTIONS[] doorsDirections;
    public List<Door> doors = new();
    [NonEditable][SerializeField] bool entered;

    [Header("Map Room")]
    [HideInInspector] public GameObject roomInMap;
    [HideInInspector] public List<GameObject> gatesInMap = new();
    public List<GameObject> joinedRooms = new();
    public List<GameObject> mapJoints = new();

    [Header("Enemies")]
    [SerializeField] GameObject[] enemies;

    void Start()
    {
        entered = false;
        //NavMeshSurface navMesh = GetComponent<NavMeshSurface>();
        //if (navMesh) navMesh.BuildNavMesh();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (entered) return;
        if (!other.CompareTag("Player")) return;

        EnteredInRoom();
        for (int i = 0; i < joinedRooms.Count; i++) joinedRooms[i].GetComponent<RoomBehaviour>().EnteredInRoom();
        for (int i = 0; i < mapJoints.Count; i++) mapJoints[i].SetActive(true);
    }

    public void SetDoors()
    {
        if (doorsTransform.Length != doorsDirections.Length) Debug.LogError("Diferent number of doorsTransform and doorsDirections");
        for (int i = 0; i < doorsTransform.Length; i++) doors.Add(new Door(doorsTransform[i].position, doorsDirections[i], this));
    }

    public void EnteredInRoom()
    {
        entered = true;
        roomInMap.SetActive(true);
        for (int i = 0; i < gatesInMap.Count; i++) gatesInMap[i].GetComponent<GateInMap>().ShowGate();

        InitEnemies();
        // close gates
    }

    public bool GetDoorsFilled()
    {
        for (int i = 0; i < doors.Count; i++)
        {
            if (doors[i].state == DOOR_STATE.FOR_FILL) return false;
        }
        return true;
    }

    public Door GetRandomUnfilledDoor()
    {
        if (doorsFilled) Debug.LogError("All doors are filled");

        int rand = -1;
        while (rand == -1 || doors[rand].state != DOOR_STATE.FOR_FILL)
        {
            rand = Random.Range(0, doors.Count);
        }
         
        return doors[rand];
    }

    public void NullifyDoor(FOUR_DIRECTIONS direction)
    {
        for (int i = 0; i < doors.Count; i++)
        {
            if (doors[i].direction == direction)
            {
                doors[i].state = DOOR_STATE.NULL;
                return;
            }
        }
    }

    public Door GetDoorByDirection(FOUR_DIRECTIONS direction)
    {
        for (int i = 0; i < doors.Count; i++)
        {
            if (doors[i].direction == direction)
            {
                return doors[i];
            }
        }

        return null;
    }

    void InitEnemies()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            switch (enemies[i].GetComponent<EnemyType>().type)
            {
                case ENEMY_TYPE.SPHERE_ROBOT:
                    enemies[i].GetComponent<SphereRobot>().enabled = true;
                    break;
                default:
                    break;
            }
        }
    }
}

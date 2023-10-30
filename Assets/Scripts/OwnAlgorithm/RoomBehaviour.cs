using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DOOR_STATE
{
    FOR_FILL,
    YELLOW,
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
    public int roomTypeID;
    [HideInInspector] public OwnRoomGenarator manager;

    [NonEditable] bool doorsFilled = false;
    [SerializeField] Transform[] doorsTransform;
    [SerializeField] FOUR_DIRECTIONS[] doorsDirections;
    public List<Door> doors = new();
    [HideInInspector] public GameObject roomInMap;

    public void SetDoors()
    {
        if (doorsTransform.Length != doorsDirections.Length) Debug.LogError("Diferent number of doorsTransform and doorsDirections");
        for (int i = 0; i < doorsTransform.Length; i++) doors.Add(new Door(doorsTransform[i].position, doorsDirections[i], this));
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
}

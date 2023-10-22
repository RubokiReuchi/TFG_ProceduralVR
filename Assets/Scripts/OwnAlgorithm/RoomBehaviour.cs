using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DOOR_STATE
{
    FOR_FILL,
    YELLOW,
    HOLDED,
    DESTROYED,
    NULL // no tiene puerta
}

public class Door
{
    public Vector3 position;
    public FOUR_DIRECTIONS direction;
    public DOOR_STATE state;

    public Door(Vector3 position, FOUR_DIRECTIONS direction)
    {
        this.position = position;
        this.direction = direction;
        this.state = DOOR_STATE.FOR_FILL;
    }
}

public class RoomBehaviour : MonoBehaviour
{
    public int roomTypeID;
    [HideInInspector] public OwnRoomGenarator manager;

    [NonEditable] bool doorsFilled = false;
    [SerializeField] Transform[] doorsTransform;
    [SerializeField] FOUR_DIRECTIONS[] doorsDirections;
    List<Door> doors = new();

    public void SetDoors()
    {
        if (doorsTransform.Length != doorsDirections.Length) Debug.LogError("Diferent number of doorsTransform and doorsDirections");
        for (int i = 0; i < doorsTransform.Length; i++) doors.Add(new Door(doorsTransform[i].position, doorsDirections[i]));
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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GATE_STATE
{
    YELLOW,
    BOSS,
    DESTROYED,
    NULL
}

public class Gate
{
    public Vector3 position;
    public FOUR_DIRECTIONS direction;
    public GATE_STATE state;
    public Gate other; // attached gate if this Gate is not null

    public Gate(Vector3 position, FOUR_DIRECTIONS direction, GATE_STATE state, Gate other)
    {
        this.position = position;
        this.direction = direction;
        this.state = state;
        this.other = other;
    }

    public void SetOther(Gate other)
    {
        this.other = other;
    }
}

public class PlaceGates : MonoBehaviour
{
    public List<Gate> gates;

    public void PlaceAllGates()
    {

    }
}

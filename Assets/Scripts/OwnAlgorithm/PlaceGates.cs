using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GATE_STATE
{
    YELLOW,
    BOSS,
    DESTROYED
}

public class Gate
{
    public Vector3 position;
    public FOUR_DIRECTIONS direction;
    public Gate other; // attached gate if this Gate is not null
}

public class PlaceGates : MonoBehaviour
{
    public List<Gate> gates;

    public void PlaceAllGates()
    {

    }
}

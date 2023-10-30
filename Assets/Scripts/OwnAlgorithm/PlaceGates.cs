using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GATE_STATE
{
    YELLOW,
    BLUE,
    RED,
    PURPLE,
    GREEN,
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
    public GameObject self;
    public RoomBehaviour roomBehaviour; // script of the room owner of the door

    public Gate(Vector3 position, FOUR_DIRECTIONS direction, GATE_STATE state, Gate other, RoomBehaviour roomBehaviour)
    {
        this.position = position;
        this.direction = direction;
        this.state = state;
        this.other = other;
        this.roomBehaviour = roomBehaviour;
    }

    public void SetOther(Gate other)
    {
        this.other = other;
    }
}

public class PlaceGates : MonoBehaviour
{
    [Header("Gates Prefabs")]
    [SerializeField] GameObject yellowGatePrefab;
    [SerializeField] GameObject blueGatePrefab;
    [SerializeField] GameObject redGatePrefab;
    [SerializeField] GameObject purpleGatePrefab;
    [SerializeField] GameObject greenGatePrefab;
    [SerializeField] GameObject bossGatePrefab;
    [SerializeField] GameObject destroyedGatePrefab;
    public List<Gate> gates;

    [Header("Map Gates")]
    [SerializeField] Transform createMap;
    [SerializeField] GameObject mapYellowGatePrefab;
    [SerializeField] GameObject mapBlueGatePrefab;
    [SerializeField] GameObject mapRedGatePrefab;
    [SerializeField] GameObject mapPurpleGatePrefab;
    [SerializeField] GameObject mapGreenGatePrefab;
    [SerializeField] GameObject mapBossGatePrefab;
    [SerializeField] GameObject mapDestroyedGatePrefab;

    public void PlaceAllGates()
    {
        for (int i = 0; i < gates.Count; i++)
        {
            GameObject gate = null;
            Quaternion rotation;
            if (gates[i].direction == FOUR_DIRECTIONS.TOP || gates[i].direction == FOUR_DIRECTIONS.DOWN) rotation = Quaternion.identity;
            else rotation = Quaternion.AngleAxis(90, Vector3.up);
            GameObject newMapGate;
            switch (gates[i].state)
            {
                case GATE_STATE.YELLOW:
                    gate = GameObject.Instantiate(yellowGatePrefab, gates[i].position, rotation);
                    gate.GetComponent<GateBehaviour>().other = gates[i].other;
                    newMapGate = GameObject.Instantiate(mapYellowGatePrefab, createMap);
                    newMapGate.transform.localPosition = new Vector3(gates[i].position.x / 3, gates[i].position.z / 3, 0);
                    gates[i].roomBehaviour.gatesInMap.Add(newMapGate);
                    break;
                case GATE_STATE.BLUE:
                    gate = GameObject.Instantiate(blueGatePrefab, gates[i].position, rotation);
                    gate.GetComponent<GateBehaviour>().other = gates[i].other;
                    newMapGate = GameObject.Instantiate(mapBlueGatePrefab, createMap);
                    newMapGate.transform.localPosition = new Vector3(gates[i].position.x / 3, gates[i].position.z / 3, 0);
                    gates[i].roomBehaviour.gatesInMap.Add(newMapGate);
                    break;
                case GATE_STATE.RED:
                    gate = GameObject.Instantiate(redGatePrefab, gates[i].position, rotation);
                    gate.GetComponent<GateBehaviour>().other = gates[i].other;
                    newMapGate = GameObject.Instantiate(mapRedGatePrefab, createMap);
                    newMapGate.transform.localPosition = new Vector3(gates[i].position.x / 3, gates[i].position.z / 3, 0);
                    gates[i].roomBehaviour.gatesInMap.Add(newMapGate);
                    break;
                case GATE_STATE.PURPLE:
                    gate = GameObject.Instantiate(purpleGatePrefab, gates[i].position, rotation);
                    gate.GetComponent<GateBehaviour>().other = gates[i].other;
                    newMapGate = GameObject.Instantiate(mapPurpleGatePrefab, createMap);
                    newMapGate.transform.localPosition = new Vector3(gates[i].position.x / 3, gates[i].position.z / 3, 0);
                    gates[i].roomBehaviour.gatesInMap.Add(newMapGate);
                    break;
                case GATE_STATE.GREEN:
                    gate = GameObject.Instantiate(greenGatePrefab, gates[i].position, rotation);
                    gate.GetComponent<GateBehaviour>().other = gates[i].other;
                    newMapGate = GameObject.Instantiate(mapGreenGatePrefab, createMap);
                    newMapGate.transform.localPosition = new Vector3(gates[i].position.x / 3, gates[i].position.z / 3, 0);
                    gates[i].roomBehaviour.gatesInMap.Add(newMapGate);
                    break;
                case GATE_STATE.BOSS:
                    gate = GameObject.Instantiate(bossGatePrefab, gates[i].position, rotation);
                    gate.GetComponent<GateBehaviour>().other = gates[i].other;
                    newMapGate = GameObject.Instantiate(mapBossGatePrefab, createMap);
                    newMapGate.transform.localPosition = new Vector3(gates[i].position.x / 3, gates[i].position.z / 3, 0);
                    gates[i].roomBehaviour.gatesInMap.Add(newMapGate);
                    break;
                case GATE_STATE.DESTROYED:
                    gate = GameObject.Instantiate(destroyedGatePrefab, gates[i].position, rotation);
                    newMapGate = GameObject.Instantiate(mapDestroyedGatePrefab, createMap);
                    newMapGate.transform.localPosition = new Vector3(gates[i].position.x / 3, gates[i].position.z / 3, 0);
                    gates[i].roomBehaviour.gatesInMap.Add(newMapGate);
                    break;
                case GATE_STATE.NULL:
                default:
                    Debug.LogError("Logic Error");
                    break;
            }
            gates[i].self = gate;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateRooms : MonoBehaviour
{
    public GameObject[] rooms;
    public int roomsNum;
    [Range(0.5f, 100.0f)] public float circleRadius;
    int tileSize = 2;

    List<GameObject> createdRooms = new();

    // Start is called before the first frame update
    void Start()
    {
        CreateRooms(roomsNum, circleRadius);
        StartCoroutine(SeparateRooms());
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space)) StartCoroutine(SeparateRooms());
    }

    void CreateRooms(int roomsNum, float circleRadius)
    {
        for (int i = 0; i < roomsNum; i++)
        {
            createdRooms.Add(GameObject.Instantiate(rooms[Random.Range(0, rooms.Length)], CalculatePosition(circleRadius), CalculateRotation()));
        }
    }

    Vector3 CalculatePosition(float circleRadius)
    {
        // Get Random Point In Circle
        Vector3 newPos;
        float rand = Random.Range(0, 100) / 100.0f;

        float t = 2 * Mathf.PI * rand;
        float u = rand + rand;
        float r;

        if (u > 1) r = 2 - u;
        else r = u;

        float x = circleRadius * r * Mathf.Cos(t);
        float z = circleRadius * r * Mathf.Sin(t);

        newPos = new Vector3(Mathf.Floor((x + tileSize + 1) / tileSize) * tileSize, 0, Mathf.Floor((z + tileSize + 1) / tileSize) * tileSize);
        return newPos;
    }

    Quaternion CalculateRotation()
    {
        Quaternion result = Quaternion.identity;

        int rand = Random.Range(0, 4);
        while (rand > 0)
        {
            result *= Quaternion.Euler(0, 90, 0);
            rand--;
        }

        return result;
    }

    IEnumerator SeparateRooms()
    {
        do
        {
            yield return new WaitForEndOfFrame();
            for (int current = 0; current < createdRooms.Count; current++)
            {
                for (int other = 0; other < createdRooms.Count; other++)
                {
                    if (current == other || !createdRooms[current].GetComponent<RoomOverlapping>().CheckOverlap(createdRooms[other].GetInstanceID())) continue;

                    Vector3 direction = (createdRooms[other].GetComponent<BoxCollider>().bounds.center - createdRooms[current].GetComponent<BoxCollider>().bounds.center).normalized;

                    // save check
                    if (direction == Vector3.zero) direction = GetRandomVector3();

                    createdRooms[current].transform.position = new Vector3(createdRooms[current].transform.position.x - Mathf.RoundToInt(direction.x) * tileSize, createdRooms[current].transform.position.y, createdRooms[current].transform.position.z - Mathf.RoundToInt(direction.z) * tileSize);
                    bool ret = createdRooms[current].GetComponent<RoomOverlapping>().CheckOverlap(createdRooms[other].GetInstanceID());
                }
            }
        }
        while (IsAnyRoomOverlapping());
    }

    bool IsAnyRoomOverlapping()
    {
        for (int i = 0; i < createdRooms.Count; i++)
        {
            if (createdRooms[i].GetComponent<RoomOverlapping>().CheckAnyOverlap()) return true;
        }

        return false;
    }

    Vector3 GetRandomVector3()
    {
        int rand = Random.Range(0, 4);
        if (rand == 0) return Vector3.forward;
        else if (rand == 1) return Vector3.left;
        else if (rand == 2) return Vector3.back;
        else return Vector3.right;
    }
}

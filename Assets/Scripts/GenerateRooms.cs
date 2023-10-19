using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateRooms : MonoBehaviour
{
    public GameObject[] rooms;
    public int roomsNum;
    public float circleRadius;
    int tileSize = 2;

    List<GameObject> createdRooms = new();

    // Start is called before the first frame update
    void Start()
    {
        CreateRooms(roomsNum, circleRadius);
        StartCoroutine(SeparateRooms());
    }

    void CreateRooms(int roomsNum, float circleRadius)
    {
        for (int i = 0; i < roomsNum; i++)
        {
            createdRooms.Add(GameObject.Instantiate(rooms[Random.Range(0, rooms.Length)], CalculatePosition(circleRadius), Quaternion.identity));
        }
    }

    Vector3 CalculatePosition(float circleRadius)
    {
        // Get Random Point In Circle
        Vector3 newPos;

        do
        {
            float rand = Random.Range(0, 100) / 100.0f;

            float t = 2 * Mathf.PI * rand;
            float u = rand + rand;
            float r;

            if (u > 1) r = 2 - u;
            else r = u;

            float x = circleRadius * r * Mathf.Cos(t);
            float z = circleRadius * r * Mathf.Sin(t);

            newPos = new Vector3(Mathf.Floor((x + tileSize + 1) / tileSize) * tileSize, 0, Mathf.Floor((z + tileSize + 1) / tileSize) * tileSize);
        } while (PositionAlreadyOccuped(newPos));
        
        

        return newPos;
    }

    bool PositionAlreadyOccuped(Vector3 pos)
    {
        for (int i = 0; i < createdRooms.Count; i++)
        {
            if (createdRooms[i].transform.position == pos) return true;
        }

        return false;
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

                    Vector3 direction = (createdRooms[other].transform.position - createdRooms[current].transform.position).normalized;

                    createdRooms[current].transform.position = new Vector3(createdRooms[current].transform.position.x - direction.x * tileSize, createdRooms[current].transform.position.y, createdRooms[current].transform.position.z - direction.z * tileSize);
                    createdRooms[other].transform.position = new Vector3(createdRooms[other].transform.position.x + direction.x * tileSize, createdRooms[other].transform.position.y, createdRooms[other].transform.position.z + direction.z * tileSize);
                }
            }
        }
        while (IsAnyRoomOverlapping());
    }

    bool IsAnyRoomOverlapping()
    {
        for (int i = 0; i < createdRooms.Count; i++)
        {
            if (createdRooms[i].GetComponent<RoomOverlapping>().overlapping) return true;
        }

        return false;
    }
}

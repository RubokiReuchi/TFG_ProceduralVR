using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateRooms : MonoBehaviour
{
    //public GameObject[] rooms;
    public GameObject floorTile;
    public GameObject emptyRoom;
    public GameObject floorPool;

    [Header("Note: High rooms number with small circle radius can cause undesired results (recomend 4/1 proporcion)")]
    public int roomsNum;
    [Range(0.5f, 100.0f)] public float circleRadius;
    public int minTiles;
    public int maxTiles;
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
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    for (int current = 0; current < createdRooms.Count; current++)
        //    {
        //        for (int other = 0; other < createdRooms.Count; other++)
        //        {
        //            if (current == other || !createdRooms[current].GetComponent<RoomOverlapping>().CheckOverlap(createdRooms[other].GetInstanceID())) continue;

        //            Vector3 direction = (createdRooms[other].GetComponent<BoxCollider>().bounds.center - createdRooms[current].GetComponent<BoxCollider>().bounds.center).normalized;

        //            // save check
        //            if (direction == Vector3.zero) direction = GetRandomVector3();

        //            createdRooms[current].transform.position = new Vector3(createdRooms[current].transform.position.x - Mathf.RoundToInt(direction.x) * tileSize, createdRooms[current].transform.position.y, createdRooms[current].transform.position.z - Mathf.RoundToInt(direction.z) * tileSize);
        //        }
        //    }
        //}
    }

    void CreateRooms(int roomsNum, float circleRadius)
    {
        for (int i = 0; i < roomsNum; i++)
        {
            GameObject newRoom = GameObject.Instantiate(emptyRoom, CalculatePosition(circleRadius), Quaternion.identity);
            Vector2 roomSize = CalculateSize();
            for (int j = 0; j < roomSize.x; j++)
            {
                for (int k = 0; k < roomSize.y; k++)
                {
                    if (floorPool.transform.childCount == 0) Debug.Log("No Children");
                    GameObject tile = floorPool.transform.GetChild(0).gameObject;
                    tile.transform.parent = newRoom.transform; //GameObject.Instantiate(floorTile, newRoom.transform);
                    tile.transform.localPosition = new Vector3(1 + 2 * j, 0, 1 + 2 * k);
                }
            }
            BoxCollider collider = newRoom.GetComponent<BoxCollider>();
            collider.center = new Vector3(roomSize.x, 0, roomSize.y);
            collider.size = new Vector3(roomSize.x * 2, 1, roomSize.y * 2);
            newRoom.GetComponent<RoomOverlapping>().roomID = i;
            newRoom.name = "Room" + i;
            createdRooms.Add(newRoom);
        }
    }

    Vector2 CalculateSize()
    {
        int width = Random.Range(minTiles, maxTiles);
        int height = Random.Range(minTiles, maxTiles);

        return new Vector2(width, height);
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

    void ChooseMainRooms()
    {

    }
}

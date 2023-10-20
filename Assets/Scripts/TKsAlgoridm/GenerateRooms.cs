using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using DelaunatorSharp;
using DelaunatorSharp.Unity.Extensions;
using UnityEditor.Rendering;
using UnityEditor.Experimental.GraphView;

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

    int countTotalWidth = 0;
    int countTotalHeight = 0;

    List<GameObject> createdRooms = new();
    List<GameObject> mainRooms = new();

    struct Edge
    {
        public Vector3 start;
        public Vector3 end;
        public int lenght;

        public Edge(Vector3 start, Vector3 end, int lenght)
        {
            this.start = start;
            this.end = end;
            this.lenght = lenght;
        }
    }

    Dictionary<int, Vector3> roomsCenter = new();
    List<Edge> delaunatorEdges = new();
    List<Edge> treeEdges = new();

    // Start is called before the first frame update
    void Start()
    {
        CreateRooms(roomsNum, circleRadius);
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

        if (delaunatorEdges.Count > 0)
        {
            for (int i = 0; i < delaunatorEdges.Count; i++)
            {
                Debug.DrawLine(delaunatorEdges[i].start, delaunatorEdges[i].end, Color.white);
            }
        }

        if (treeEdges.Count > 0)
        {
            for (int i = 0; i < treeEdges.Count; i++)
            {
                Debug.DrawLine(treeEdges[i].start, treeEdges[i].end, Color.red);
            }
        }
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
            RoomOverlapping script = newRoom.GetComponent<RoomOverlapping>();
            script.roomID = i;
            script.roomWidth = (int)roomSize.x;
            script.roomHeight = (int)roomSize.y;
            newRoom.name = "Room" + i;
            createdRooms.Add(newRoom);

            countTotalWidth += (int)roomSize.x;
            countTotalHeight += (int)roomSize.y;
        }

        StartCoroutine(SeparateRooms());
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

        ChooseMainRooms();
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
        int mediaWidth = countTotalWidth / roomsNum;
        int mediaHeight = countTotalHeight / roomsNum;

        for (int i = 0; i < createdRooms.Count; i++)
        {
            RoomOverlapping script = createdRooms[i].GetComponent<RoomOverlapping>();
            if (script.roomWidth >= mediaWidth * 1.25f && script.roomHeight >= mediaWidth * 1.25f)
            {
                mainRooms.Add(createdRooms[i]);
            }
        }

        if (mainRooms.Count > 2) CreateDelaunayGraph();
        else Debug.Log("Not enough main rooms");
    }

    void CreateDelaunayGraph()
    {
        Delaunator delaunator;
        List<IPoint> delaunatorPoints = new();

        for (int i = 0; i < mainRooms.Count; i++)
        {
            RoomOverlapping script = mainRooms[i].GetComponent<RoomOverlapping>();
            Vector3 center = script.GetCentralPoint();
            roomsCenter.Add(script.roomID, center);
            delaunatorPoints.Add(new Point(center.x, center.z));
        }

        delaunator = new Delaunator(delaunatorPoints.ToArray());
        delaunator.ForEachTriangleEdge(edge =>
        {
            Vector3 start = new Vector3(edge.P.ToVector2().x, 0, edge.P.ToVector2().y);
            Vector3 end = new Vector3(edge.Q.ToVector2().x, 0, edge.Q.ToVector2().y);
            delaunatorEdges.Add(new Edge(start, end, Mathf.RoundToInt(Vector3.Distance(end, start))));
        });

        // Bubble Sort
        for (int i = 1; i < delaunatorEdges.Count; i++)
        {
            for (int j = 0; j < delaunatorEdges.Count - 1; j++)
            {
                if (delaunatorEdges[j].lenght > delaunatorEdges[j + 1].lenght)
                {
                    Edge aux = delaunatorEdges[j];
                    delaunatorEdges[j] = delaunatorEdges[j + 1];
                    delaunatorEdges[j + 1] = aux;
                }
            }
        }

        CreateMST();
    }

    void CreateMST()
    {
        for (int i = 0; i < delaunatorEdges.Count; i++)
        {
            if (!Concidence(i)) treeEdges.Add(delaunatorEdges[i]);
        }
    }

    bool Concidence(int edge)
    {
        List<Vector3> storeStartCoincidence = new();
        List<Vector3> storeEndCoincidence = new();
        for (int j = 0; j < treeEdges.Count; j++)
        {
            if (edge == j) continue;

            if (delaunatorEdges[edge].start == treeEdges[j].start) storeStartCoincidence.Add(treeEdges[j].end);
            else if (delaunatorEdges[edge].start == treeEdges[j].end) storeStartCoincidence.Add(treeEdges[j].start);

            if (delaunatorEdges[edge].end == treeEdges[j].start) storeEndCoincidence.Add(treeEdges[j].end);
            else if (delaunatorEdges[edge].end == treeEdges[j].end) storeEndCoincidence.Add(treeEdges[j].start);
        }

        for (int j = 0; j < storeStartCoincidence.Count; j++)
        {
            for (int k = 0; k < storeEndCoincidence.Count; k++)
            {
                if (storeStartCoincidence[j] == storeEndCoincidence[k])
                {
                    return true;
                }
            }
        }

        return false;
    }
}

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
    // Create and Separe
    [SerializeField] GameObject floorTile;
    [SerializeField] GameObject emptyRoom;
    [SerializeField] GameObject floorPool;

    [Header("Note: High rooms number with small circle radius can cause undesired results (recomend 4/1 proporcion)")]
    [SerializeField] int roomsNum;
    [Range(0.5f, 100.0f)] public float circleRadius;
    [SerializeField] int minTiles;
    [SerializeField] int maxTiles;
    int tileSize = 2;

    int countTotalWidth = 0;
    int countTotalHeight = 0;

    List<GameObject> createdRooms = new();
    List<GameObject> mainRooms = new();
    [Header("Higher Threshold means a bigger map")]
    [Range(0.1f, 0.25f)] [SerializeField] float mainRoomsThreshold;
    [SerializeField] int minimumMainRooms;

    // Path
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

    struct Graph
    {
        public int node;
        public int e;
        public List<KeyEdge> edges;

        public Graph(int node, int e, List<KeyEdge> edges)
        {
            this.node = node;
            this.e = e;
            this.edges = edges;
        }
    }

    struct TreeMaintainanceSet
    {
        public int parent;
        public int rank;
    }

    struct KeyEdge
    {
        public int startID;
        public int endID;
        public int lenght;

        public KeyEdge(int startID, int endID, int lenght)
        {
            this.startID = startID;
            this.endID = endID;
            this.lenght = lenght;
        }
    }

    Dictionary<int, Vector3> roomsCenter = new();
    List<Edge> delaunatorEdges = new();
    Dictionary<int, int> mainRoomsKeys = new(); // key (mainRoomID), value (roomID)
    List<Edge> treeEdges = new();
    List<Edge> returningEdges = new();

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

        // DisplayEdges
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

        if (returningEdges.Count > 0)
        {
            for (int i = 0; i < returningEdges.Count; i++)
            {
                Debug.DrawLine(returningEdges[i].start, returningEdges[i].end, Color.blue);
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

        while (mainRooms.Count < minimumMainRooms)
        {
            mainRooms.Clear();
            for (int i = 0; i < createdRooms.Count; i++)
            {
                RoomOverlapping script = createdRooms[i].GetComponent<RoomOverlapping>();
                if (script.roomWidth >= mediaWidth * (1 + mainRoomsThreshold) && script.roomHeight >= mediaWidth * (1 + mainRoomsThreshold))
                {
                    mainRooms.Add(createdRooms[i]);
                }
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
        
        CreateMST(new Graph(mainRooms.Count, delaunatorEdges.Count, ConvertToKeyEdges(delaunatorEdges)));
    }

    List<KeyEdge> ConvertToKeyEdges(List<Edge> edges)
    {
        for (int i = 0; i < mainRooms.Count; i++)
        {
            // link a mainRoomID to the mainRooms roomID
            mainRoomsKeys.Add(i, mainRooms[i].GetComponent<RoomOverlapping>().roomID);
        }

        List<KeyEdge> result = new();
        for (int i = 0; i < edges.Count; i++)
        {
            // get roomID using room position
            int roomStartKey = roomsCenter.FirstOrDefault(x => x.Value == edges[i].start).Key;
            int roomEndKey = roomsCenter.FirstOrDefault(x => x.Value == edges[i].end).Key;

            // convert roomID into mainRoomID
            int mainRoomStartKey = mainRoomsKeys.FirstOrDefault(x => x.Value == roomStartKey).Key;
            int mainRoomEndKey = mainRoomsKeys.FirstOrDefault(x => x.Value == roomEndKey).Key;
            result.Add(new KeyEdge(mainRoomStartKey, mainRoomEndKey, edges[i].lenght));
        }
        return result;
    }

    List<Edge> ConvertFromKeyEdges(List<KeyEdge> edges)
    {
        List<Edge> result = new();
        for (int i = 0; i < edges.Count; i++)
        {
            // convert mainRoomID into roomID
            int roomsStartKey = mainRoomsKeys[edges[i].startID];
            int roomsEndKey = mainRoomsKeys[edges[i].endID];

            // get room position using roomID
            Vector3 start = roomsCenter[roomsStartKey];
            Vector3 end = roomsCenter[roomsEndKey];
            result.Add(new Edge(start, end, edges[i].lenght));
        }
        return result;
    }

    void CreateMST(Graph graph)
    {
        int node = graph.node;
        List<KeyEdge> result = new();
        int e = 0;
        int i = 0;
        TreeMaintainanceSet[] subsets = new TreeMaintainanceSet[node];

        for (int v = 0; v < node; v++)
        {
            subsets[v].parent = v;
            subsets[v].rank = 0;
        }

        while (e < node - 1 && i < graph.e)
        {
            KeyEdge nextEdge = graph.edges[i++];
            int x = FindDisjointSet(subsets, nextEdge.startID);
            int y = FindDisjointSet(subsets, nextEdge.endID);

            if (x != y)
            {
                result.Add(nextEdge);
                e++;
                UnionDisjointSet(subsets, x, y);
            }
        }

        treeEdges = ConvertFromKeyEdges(result);
        RecoverSomeEdges();
    }

    int FindDisjointSet(TreeMaintainanceSet[] subsets, int i)
    {
        if (subsets[i].parent != i)
        {
            subsets[i].parent = FindDisjointSet(subsets, subsets[i].parent);
        }
        return subsets[i].parent;
    }

    void UnionDisjointSet(TreeMaintainanceSet[] subsets, int x, int y)
    {
        int xRoot = FindDisjointSet(subsets, x);
        int yRoot = FindDisjointSet(subsets, y);

        if (subsets[xRoot].rank < subsets[yRoot].rank) subsets[xRoot].parent = yRoot;
        else if (subsets[xRoot].rank > subsets[yRoot].rank) subsets[yRoot].parent = xRoot;
        else
        {
            subsets[yRoot].parent = xRoot;
            subsets[xRoot].rank++;
        }
    }

    void RecoverSomeEdges()
    {
        for (int i = 0; i < delaunatorEdges.Count; i++)
        {
            if (!treeEdges.Contains(delaunatorEdges[i]))
            {
                int rand = Random.Range(0, 100);
                if (rand < 15) returningEdges.Add(delaunatorEdges[i]);
            }
        }
    }
}

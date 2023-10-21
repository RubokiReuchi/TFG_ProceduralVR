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
    List<GameObject> secondaryRooms = new();
    Dictionary<Vector3, GameObject> mainRoomsPosition = new();
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
    List<Edge> mapEdges = new();

    // Hallways
    struct LinearHallway
    {
        public Vector2 startPos;
        public Vector2 endPos;

        public LinearHallway(Vector2 startPos, Vector2 endPos)
        {
            this.startPos = startPos;
            this.endPos = endPos;
        }
    }
    struct CornerHallway
    {
        public Vector2 startPos;
        public Vector2 cornerPos;
        public Vector2 endPos;

        public CornerHallway(Vector2 startPos, Vector2 cornerPos, Vector2 endPos)
        {
            this.startPos = startPos;
            this.cornerPos = cornerPos;
            this.endPos = endPos;
        }
    }
    List<LinearHallway> linearHallways = new();
    List<CornerHallway> cornerHallways = new();
    List<Vector2> doorVerticalHallways = new();
    List<Vector2> doorHorizontalHallways = new();

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
        //if (delaunatorEdges.Count > 0)
        //{
        //    for (int i = 0; i < delaunatorEdges.Count; i++)
        //    {
        //        Debug.DrawLine(delaunatorEdges[i].start, delaunatorEdges[i].end, Color.white);
        //    }
        //}

        //if (treeEdges.Count > 0)
        //{
        //    for (int i = 0; i < treeEdges.Count; i++)
        //    {
        //        Debug.DrawLine(treeEdges[i].start, treeEdges[i].end, Color.red);
        //    }
        //}

        //if (returningEdges.Count > 0)
        //{
        //    for (int i = 0; i < returningEdges.Count; i++)
        //    {
        //        Debug.DrawLine(returningEdges[i].start, returningEdges[i].end, Color.blue);
        //    }
        //}

        //if (mapEdges.Count > 0)
        //{
        //    for (int i = 0; i < mapEdges.Count; i++)
        //    {
        //        Debug.DrawLine(mapEdges[i].start, mapEdges[i].end, Color.red);
        //    }
        //}

        if (linearHallways.Count > 0)
        {
            for (int i = 0; i < linearHallways.Count; i++)
            {
                Debug.DrawLine(new Vector3(linearHallways[i].startPos.x, 0, linearHallways[i].startPos.y), new Vector3(linearHallways[i].endPos.x, 0, linearHallways[i].endPos.y), Color.blue);
            }
        }

        if (cornerHallways.Count > 0)
        {
            for (int i = 0; i < cornerHallways.Count; i++)
            {
                Debug.DrawLine(new Vector3(cornerHallways[i].startPos.x, 0, cornerHallways[i].startPos.y), new Vector3(cornerHallways[i].cornerPos.x, 0, cornerHallways[i].cornerPos.y), Color.cyan);
                Debug.DrawLine(new Vector3(cornerHallways[i].cornerPos.x, 0, cornerHallways[i].cornerPos.y), new Vector3(cornerHallways[i].endPos.x, 0, cornerHallways[i].endPos.y), Color.cyan);
            }
        }

        if (doorVerticalHallways.Count > 0)
        {
            for (int i = 0; i < doorVerticalHallways.Count; i++)
            {
                Debug.DrawLine(new Vector3(doorVerticalHallways[i].x, 0, doorVerticalHallways[i].y), new Vector3(doorVerticalHallways[i].x, 1, doorVerticalHallways[i].y), Color.yellow);
            }
        }

        if (doorHorizontalHallways.Count > 0)
        {
            for (int i = 0; i < doorHorizontalHallways.Count; i++)
            {
                Debug.DrawLine(new Vector3(doorHorizontalHallways[i].x, 0, doorHorizontalHallways[i].y), new Vector3(doorHorizontalHallways[i].x, 1, doorHorizontalHallways[i].y), Color.yellow);
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
                    GameObject tile = GameObject.Instantiate(floorTile, newRoom.transform);
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

        if (minimumMainRooms > createdRooms.Count) mainRooms = createdRooms;
        else
        {
            while (mainRooms.Count < minimumMainRooms)
            {
                mainRooms.Clear();
                secondaryRooms.Clear();
                for (int i = 0; i < createdRooms.Count; i++)
                {
                    RoomOverlapping script = createdRooms[i].GetComponent<RoomOverlapping>();
                    if (script.roomWidth >= mediaWidth * (1 + mainRoomsThreshold) && script.roomHeight >= mediaHeight * (1 + mainRoomsThreshold))
                    {
                        mainRooms.Add(createdRooms[i]);
                    }
                    else
                    {
                        secondaryRooms.Add(createdRooms[i]);
                    }
                }
                mainRoomsThreshold -= 0.05f;
            }
        }

        // REMOVE
        for (int i = 0; i < secondaryRooms.Count; i++)
        {
            secondaryRooms[i].SetActive(false);
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
            mainRoomsPosition.Add(center, mainRooms[i]);
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

        for (int i = 0; i < treeEdges.Count; i++) mapEdges.Add(treeEdges[i]);
        for (int i = 0; i < returningEdges.Count; i++) mapEdges.Add(returningEdges[i]);

        CalculateHallways();
    }

    void CalculateHallways()
    {
        for (int i = 0; i < mapEdges.Count; i++)
        {
            GameObject startRoom = mainRoomsPosition[mapEdges[i].start];
            GameObject endRoom = mainRoomsPosition[mapEdges[i].end];
            RoomOverlapping startScript = startRoom.GetComponent<RoomOverlapping>();
            RoomOverlapping endScript = endRoom.GetComponent<RoomOverlapping>();

            int verticalDir = 0, horizontalDir = 0;
            bool oneLine = CanUseOnlyOneLine(startRoom, endRoom, ref verticalDir, ref horizontalDir);

            Vector3 startPoint1 = startRoom.transform.position;
            Vector3 endPoint1 = new Vector3(startRoom.transform.position.x + startScript.roomWidth * tileSize, 0, startRoom.transform.position.z + startScript.roomHeight * tileSize);
            Vector3 startPoint2 = endRoom.transform.position;
            Vector3 endPoint2 = new Vector3(endRoom.transform.position.x + endScript.roomWidth * tileSize, 0, endRoom.transform.position.z + endScript.roomHeight * tileSize);

            if (oneLine)
            {
                if (verticalDir == 0)
                {
                    if (horizontalDir == -1) // path to the right
                    {
                        int middleHeight = Mathf.RoundToInt(GetMiddlePoint((int)startPoint1.z + 1, (int)endPoint1.z - 1, (int)startPoint2.z + 1, (int)endPoint2.z - 1));
                        if (endPoint2.x == startPoint1.x) doorHorizontalHallways.Add(new Vector2(endPoint2.x, middleHeight));
                        else linearHallways.Add(new LinearHallway(new Vector2(endPoint2.x, middleHeight), new Vector2(startPoint1.x, middleHeight)));
                    }
                    else // path to the left
                    {
                        int middleHeight = Mathf.RoundToInt(GetMiddlePoint((int)startPoint1.z + 1, (int)endPoint1.z - 1, (int)startPoint2.z + 1, (int)endPoint2.z - 1));
                        if (endPoint1.x == startPoint2.x) doorHorizontalHallways.Add(new Vector2(endPoint1.x, middleHeight));
                        else linearHallways.Add(new LinearHallway(new Vector2(endPoint1.x, middleHeight), new Vector2(startPoint2.x, middleHeight)));
                    }
                }
                else
                {
                    if (verticalDir == -1) // path to down
                    {
                        int middleHeight = Mathf.RoundToInt(GetMiddlePoint((int)startPoint1.x + 1, (int)endPoint1.x - 1, (int)startPoint2.x + 1, (int)endPoint2.x - 1));
                        if (endPoint2.z == startPoint1.z) doorVerticalHallways.Add(new Vector2(middleHeight, endPoint2.z));
                        else linearHallways.Add(new LinearHallway(new Vector2(middleHeight, endPoint2.z), new Vector2(middleHeight, startPoint1.z)));
                    }
                    else // path to up
                    {
                        int middleHeight = Mathf.RoundToInt(GetMiddlePoint((int)startPoint1.x + 1, (int)endPoint1.x - 1, (int)startPoint2.x + 1, (int)endPoint2.x - 1));
                        if (endPoint1.z == startPoint2.z) doorHorizontalHallways.Add(new Vector2(middleHeight, endPoint1.z));
                        else linearHallways.Add(new LinearHallway(new Vector2(middleHeight, endPoint1.z), new Vector2(middleHeight, startPoint2.z)));
                    }
                }
            }
            else
            {
                Vector3 startPos, cornerPos, endPos;

                int startVertical = Random.Range(0, 2);

                if (startVertical == 1) // true
                {
                    // vertical path
                    if (verticalDir == -1) startPos = new Vector3(Random.Range((int)startPoint1.x + 1, (int)endPoint1.x - 1), 0, startPoint1.z);
                    else startPos = new Vector3(Random.Range((int)startPoint1.x + 1, (int)endPoint1.x - 1), 0, endPoint1.z);

                    // horizontal path
                    if (horizontalDir == -1) endPos = new Vector3(endPoint2.x, 0, Random.Range((int)startPoint2.z + 1, (int)endPoint2.z - 1));
                    else endPos = new Vector3(startPoint2.x, 0, Random.Range((int)startPoint2.z + 1, (int)endPoint2.z - 1));

                    // join paths
                    cornerPos = new Vector3(startPos.x, 0, endPos.z);
                }
                else // false
                {
                    // horizontal path
                    if (horizontalDir == -1) startPos = new Vector3(startPoint1.x, 0, Random.Range((int)startPoint1.z + 1, (int)endPoint1.z - 1));
                    else startPos = new Vector3(endPoint1.x, 0, Random.Range((int)startPoint1.z + 1, (int)endPoint1.z - 1));

                    // vertical path
                    if (verticalDir == -1) endPos = new Vector3(Random.Range((int)startPoint2.x + 1, (int)endPoint2.x - 1), 0, endPoint2.z);
                    else endPos = new Vector3(Random.Range((int)startPoint2.x + 1, (int)endPoint2.x - 1), 0, startPoint2.z);

                    // join paths
                    cornerPos = new Vector3(endPos.x, 0, startPos.z);
                }
                cornerHallways.Add(new CornerHallway(new Vector2(startPos.x, startPos.z), new Vector2(cornerPos.x, cornerPos.z), new Vector2(endPos.x, endPos.z)));
            }
        }
    }

    bool CanUseOnlyOneLine(GameObject startRoom, GameObject endRoom, ref int vertical, ref int horizontal) // -1 negative, 0 don't need axis correction, 1 positive
    {
        RoomOverlapping startScript = startRoom.GetComponent<RoomOverlapping>();
        RoomOverlapping endScript = endRoom.GetComponent<RoomOverlapping>();
        // working arround Start Room
        // check vertical
        int startPoint1 = (int)startRoom.transform.position.z + 1; // 1 --> path width
        int endPoint1 = (int)startRoom.transform.position.z + startScript.roomHeight * tileSize - 1;
        int startPoint2 = (int)endRoom.transform.position.z + 1;
        int endPoint2 = (int)endRoom.transform.position.z + endScript.roomHeight * tileSize - 1;
        if (startPoint1 == startPoint2 && endPoint1 == endPoint2)
        {
            vertical = 0; // don't need vertical hallway
        }
        else if (startPoint1 < startPoint2 && endPoint1 > startPoint2)
        {
            vertical = 0; // don't need vertical hallway
        }
        else if (startPoint1 < endPoint2 && endPoint1 > endPoint2)
        {
            vertical = 0; // don't need vertical hallway
        }
        else if (startPoint2 < startPoint1 && endPoint2 > startPoint1)
        {
            vertical = 0; // don't need vertical hallway
        }
        else if (startPoint2 < endPoint1 && endPoint2 > endPoint1)
        {
            vertical = 0; // don't need vertical hallway
        }
        else if (startPoint1 < startPoint2)
        {
            vertical = 1; // need up hallway
        }
        else
        {
            vertical = -1; // need down hallway
        }

        // check horizontal
        startPoint1 = (int)startRoom.transform.position.x + 1; // 1 --> path width
        endPoint1 = (int)startRoom.transform.position.x + startScript.roomWidth * tileSize - 1;
        startPoint2 = (int)endRoom.transform.position.x + 1;
        endPoint2= (int)endRoom.transform.position.x + endScript.roomWidth * tileSize - 1;
        if (startPoint1 == startPoint2 && endPoint1 == endPoint2)
        {
            horizontal = 0; // don't need horizontal hallway
        }
        else if (startPoint1 <= startPoint2 && endPoint1 >= startPoint2)
        {
            horizontal = 0; // don't need horizontal hallway
        }
        else if (startPoint1 <= endPoint2 && endPoint1 >= endPoint2)
        {
            horizontal = 0; // don't need horizontal hallway
        }
        else if (startPoint2 <= startPoint1 && endPoint2 >= startPoint1)
        {
            horizontal = 0; // don't need horizontal hallway
        }
        else if (startPoint2 <= endPoint1 && endPoint2 >= endPoint1)
        {
            horizontal = 0; // don't need horizontal hallway
        }
        else if (startPoint1 < startPoint2)
        {
            horizontal = 1; // need left hallway
        }
        else
        {
            horizontal = -1; // need right hallway
        }

        return (vertical == 0 ||  horizontal == 0);
    }

    int GetMiddlePoint(int startPoint1, int endPoint1, int startPoint2, int endPoint2)
    {
        int centerPoint1 = startPoint1 + (endPoint1 - startPoint1) /2;
        int centerPoint2 = startPoint2 + (endPoint2 - startPoint2) / 2;

        int middleCenter = centerPoint1 + (centerPoint2 - centerPoint1) / 2;
        if (middleCenter % 2 == 0) // si es par
        {
            int nearest = 99999;
            if (middleCenter - startPoint1 < nearest) nearest = startPoint1;
            if (middleCenter - endPoint1 < nearest) nearest = startPoint1;
            if (middleCenter - startPoint2 < nearest) nearest = startPoint1;
            if (middleCenter - endPoint2 < nearest) nearest = startPoint1;

            if (nearest > 0) middleCenter++;
            else middleCenter--;
        }

        if (startPoint1 <= middleCenter && endPoint1 >= middleCenter && startPoint2 <= middleCenter && endPoint2 >= middleCenter) return middleCenter;

        int direction;
        if (startPoint1 < startPoint2 && endPoint1 > startPoint2) direction = -tileSize;
        else direction = tileSize;

        while (true)
        {
            middleCenter += direction;

            if (startPoint1 <= middleCenter && endPoint1 >= middleCenter && startPoint2 <= middleCenter && endPoint2 >= middleCenter) return middleCenter;
        }
    }
}

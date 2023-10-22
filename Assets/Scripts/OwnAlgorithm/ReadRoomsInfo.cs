using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RoomInfo
{
    public int topDoor; // 0 --> no door, 1 --> door, 2 --> joint
    public int downDoor;
    public int leftDoor;
    public int rightDoor;
    public int jointRoomTypeIdTop; // jointID for top, -1 == no joint
    public int jointRoomTypeIdDown; // jointID for down, -1 == no joint
    public int jointRoomTypeIdRight; // jointID for right, -1 == no joint
    public int jointRoomTypeIdLeft; // jointID for left, -1 == no joint
    public int jointRoomNumber; // number of rooms that form it's joint room, 0 --> no joint

    public RoomInfo(int topDoor, int downDoor, int leftDoor, int rightDoor, int jointRoomTypeIdTop, int jointRoomTypeIdDown, int jointRoomTypeIdRight, int jointRoomTypeIdLeft, int jointRoomNumber)
    {
        this.topDoor = topDoor;
        this.downDoor = downDoor;
        this.leftDoor = leftDoor;
        this.rightDoor = rightDoor;
        this.jointRoomTypeIdTop = jointRoomTypeIdTop;
        this.jointRoomTypeIdDown = jointRoomTypeIdDown;
        this.jointRoomTypeIdRight = jointRoomTypeIdRight;
        this.jointRoomTypeIdLeft = jointRoomTypeIdLeft;
        this.jointRoomNumber = jointRoomNumber;
    }

    public bool IsJointRoom()
    {
        return (jointRoomTypeIdTop != -1 || jointRoomTypeIdDown != -1 || jointRoomTypeIdRight != -1 || jointRoomTypeIdLeft != -1);
    }
}

public enum OBJECT_TYPE
{
    ROOM,
    BOSS_ROOM,
    JOINT
}

public struct Joint
{
    public OBJECT_TYPE objectType; // 0 --> room, 1 --> boos room, 2 --> joint
    public int objectTypeID;
    public FOUR_DIRECTIONS direction; // 0 --> top, 1 --> down, 2 --> right, 3 --> left

    public Joint(OBJECT_TYPE objectType, int objectTypeID, FOUR_DIRECTIONS direction)
    {
        this.objectType = objectType;
        this.objectTypeID = objectTypeID;
        this.direction = direction;
    }
}

public struct JointInfo
{
    public Joint head;
    public Joint tail;

    public JointInfo(Joint head, Joint tail)
    {
        this.head = head;
        this.tail = tail;
    }
}

public class ReadRoomsInfo : MonoBehaviour
{
    public TextAsset pathRoomsInfoCSV;
    public TextAsset endingRoomsInfoCSV;
    public TextAsset jointsInfoCSV;

    public Dictionary<int, RoomInfo> roomInfoList = new();
    public Dictionary<int, JointInfo> jointInfoList = new();

    // Start is called before the first frame update
    void OnEnable()
    {
        ReadRoomsCSV();
        ReadJointsCSV();
    }

    void ReadRoomsCSV()
    {
        // path rooms
        string[] data = pathRoomsInfoCSV.text.Split(new string[] { ";", "\r\n" }, System.StringSplitOptions.None);

        int numOfColumns = 10;

        int i = numOfColumns;
        while (data[i] != "")
        {
            RoomInfo roomInfo = new RoomInfo(
                int.Parse(data[i + 1]),
                int.Parse(data[i + 2]),
                int.Parse(data[i + 3]),
                int.Parse(data[i + 4]),
                int.Parse(data[i + 5]),
                int.Parse(data[i + 6]),
                int.Parse(data[i + 7]),
                int.Parse(data[i + 8]),
                int.Parse(data[i + 9]));

            roomInfoList.Add(int.Parse(data[i]), roomInfo);
            i += numOfColumns;
        }

        int pathRoomsCount = roomInfoList.Count;

        // ending rooms
        data = endingRoomsInfoCSV.text.Split(new string[] { ";", "\r\n" }, System.StringSplitOptions.None);

        i = numOfColumns;
        while (data[i] != "")
        {
            RoomInfo roomInfo = new RoomInfo(
                int.Parse(data[i + 1]),
                int.Parse(data[i + 2]),
                int.Parse(data[i + 3]),
                int.Parse(data[i + 4]),
                int.Parse(data[i + 5]),
                int.Parse(data[i + 6]),
                int.Parse(data[i + 7]),
                int.Parse(data[i + 8]),
                int.Parse(data[i + 9]));

            roomInfoList.Add(int.Parse(data[i]) + pathRoomsCount, roomInfo);
            i += numOfColumns;
        }
    }

    void ReadJointsCSV()
    {
        string[] data = jointsInfoCSV.text.Split(new string[] { ";", "\r\n" }, System.StringSplitOptions.None);

        int numOfColumns = 7;

        int i = numOfColumns;
        while (data[i] != "")
        {
            Joint head = new Joint((OBJECT_TYPE)int.Parse(data[i + 1]), int.Parse(data[i + 2]), (FOUR_DIRECTIONS)int.Parse(data[i + 3]));
            Joint tail = new Joint((OBJECT_TYPE)int.Parse(data[i + 4]), int.Parse(data[i + 5]), (FOUR_DIRECTIONS)int.Parse(data[i + 6]));

            JointInfo jointInfo = new JointInfo(head, tail);

            jointInfoList.Add(int.Parse(data[i]), jointInfo);
            i += numOfColumns;
        }
    }
}

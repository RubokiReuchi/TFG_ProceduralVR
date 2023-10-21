using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RoomInfo
{
    public int roomTypeID;
    public int topDoor; // 0 --> no door, 1 --> door, 2 --> joint
    public int downDoor;
    public int leftDoor;
    public int rightDoor;
    public int jointRoomTypeID; // room that joints it, -1 == no room

    public RoomInfo(int roomTypeID, int topDoor, int downDoor, int leftDoor, int rightDoor, int jointRoomTypeID)
    {
        this.roomTypeID = roomTypeID;
        this.topDoor = topDoor;
        this.downDoor = downDoor;
        this.leftDoor = leftDoor;
        this.rightDoor = rightDoor;
        this.jointRoomTypeID = jointRoomTypeID;
    }
}

public class ReadRoomsInfo : MonoBehaviour
{
    public TextAsset roomsInfoCSV;
    public TextAsset bossRoomsInfoCSV;

    public List<RoomInfo> roomInfoList = new();
    public List<RoomInfo> bossRoomInfoList = new();

    // Start is called before the first frame update
    void Start()
    {
        ReadRoomsCSV();
    }

    void ReadRoomsCSV()
    {
        string[] data = roomsInfoCSV.text.Split(new string[] { ";", "\r\n" }, System.StringSplitOptions.None);

        int numOfColumns = 6;

        int i = numOfColumns;
        while (data[i] != "")
        {
            RoomInfo roomInfo = new RoomInfo(
                int.Parse(data[i]),
                int.Parse(data[i + 1]),
                int.Parse(data[i + 2]),
                int.Parse(data[i + 3]),
                int.Parse(data[i + 4]),
                int.Parse(data[i + 5]));

            roomInfoList.Add(roomInfo);
            i += numOfColumns;
        }
    }

    void ReadBossRoomsCSV()
    {
        string[] data = bossRoomsInfoCSV.text.Split(new string[] { ";", "\r\n" }, System.StringSplitOptions.None);

        int numOfColumns = 6;

        int i = numOfColumns;
        while (data[i] != "")
        {
            RoomInfo roomInfo = new RoomInfo(
                int.Parse(data[i]),
                int.Parse(data[i + 1]),
                int.Parse(data[i + 2]),
                int.Parse(data[i + 3]),
                int.Parse(data[i + 4]),
                int.Parse(data[i + 5]));

            roomInfoList.Add(roomInfo);
            i += numOfColumns;
        }
    }

    public RoomInfo GetRoomByType(int roomTypeID)
    {
        return roomInfoList[roomTypeID];
    }

    public RoomInfo GetBossRoomByType(int roomTypeID)
    {
        return bossRoomInfoList[roomTypeID];
    }
}

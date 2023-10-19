using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBehaviour : MonoBehaviour
{
    public GameObject[] walls; // 0 --> up, 1 --> down, 2 --> right, 3 --> left
    public GameObject[] doors;

    public bool[] testStatus;

    // Start is called before the first frame update
    void Start()
    {
        SetRoom(testStatus);
    }

    void SetRoom(bool[] status)
    {
        for (int i = 0; i < status.Length; i++)
        {
            walls[i].SetActive(!status[i]);
            doors[i].SetActive(status[i]);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorPool : MonoBehaviour
{
    public int floorNum;
    public GameObject floor;

    [ContextMenu("InatantiateFloors")]
    public void InatantiateFloors()
    {
        for (int i = 0; i < floorNum; i++)
        {
            GameObject.Instantiate(floor, transform);
        }
    }
}

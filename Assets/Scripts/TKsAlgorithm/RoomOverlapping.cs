using Pico.Platform;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomOverlapping : MonoBehaviour
{
    [NonEditable] public bool overlaping = false;
    [HideInInspector] public List<int> overlapGameobject = new();
    public LayerMask layerMask;

    BoxCollider bc;

    [NonEditable] public int roomID;
    [NonEditable] public int roomWidth;
    [NonEditable] public int roomHeight;

    // Start is called before the first frame update
    void Start()
    {
        bc = GetComponent<BoxCollider>();
    }

    public bool CheckOverlap(int otherInstanceID)
    {
        Collider[] colliding = Physics.OverlapBox(bc.bounds.center, bc.bounds.size / 2.0f - Vector3.one * 0.1f, Quaternion.identity, layerMask); // remove "Vector3.one * 0.1f" to space rooms
        for (int i = 0; i < colliding.Length; i++)
        {
            if (colliding[i].gameObject.GetInstanceID() == otherInstanceID) return true;
        }
        return false;
    }

    // return if is overlaping something
    public bool CheckAnyOverlap()
    {
        Collider[] colliding = Physics.OverlapBox(bc.bounds.center, bc.bounds.size / 2.0f - Vector3.one * 0.1f, Quaternion.identity, layerMask); // remove "Vector3.one * 0.1f" to space rooms

        overlaping = !(colliding.Length == 1); // the one is him self

        return overlaping;
    }

    public Vector3 GetCentralPoint()
    {
        return bc.bounds.center;
    }
}

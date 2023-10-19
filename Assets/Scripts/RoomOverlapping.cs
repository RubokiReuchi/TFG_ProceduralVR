using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomOverlapping : MonoBehaviour
{
    [NonEditable] public bool overlaping = false;
    [HideInInspector] public List<int> overlapGameobject = new();
    public LayerMask layerMask;

    BoxCollider bc;

    // Start is called before the first frame update
    void Start()
    {
        bc = GetComponent<BoxCollider>();
    }

    public bool CheckOverlap(int otherInstanceID)
    {
        Collider[] colliding = Physics.OverlapBox(bc.bounds.center, bc.bounds.size / 2.0f, Quaternion.identity, layerMask);
        for (int i = 0; i < colliding.Length; i++)
        {
            if (colliding[i].gameObject.GetInstanceID() == otherInstanceID) return true;
        }
        return false;
    }

    // return if is overlaping something
    public bool CheckAnyOverlap()
    {
        Collider[] colliding = Physics.OverlapBox(bc.bounds.center, bc.bounds.size / 2.0f, Quaternion.identity, layerMask);

        overlaping = !(colliding.Length == 1); // the one is him self

        return overlaping;
    }
}

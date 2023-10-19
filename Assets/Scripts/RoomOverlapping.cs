using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomOverlapping : MonoBehaviour
{
    [NonEditable] public bool overlapping = false;
    [HideInInspector] public List<int> overlapGameobject = new();

    public int num;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        num = overlapGameobject.Count;
        overlapping = !(overlapGameobject.Count == 0);
    }

    public bool CheckOverlap(int otherInstanceID)
    {
        return overlapGameobject.Contains(otherInstanceID);
    }

    private void OnTriggerEnter(Collider other)
    {
        int otherID = other.gameObject.GetInstanceID();
        if (other.gameObject.CompareTag("RoomBound") && !overlapGameobject.Contains(otherID)) overlapGameobject.Add(otherID);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("RoomBound")) overlapGameobject.Remove(other.gameObject.GetInstanceID());
    }
}

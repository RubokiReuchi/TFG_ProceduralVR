using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomOverlapping : MonoBehaviour
{
    [NonEditable] public bool overlapping = false;
    [HideInInspector] public List<int> overlapGameobject = new();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        overlapping = !(overlapGameobject.Count == 0);
    }

    public bool CheckOverlap(int otherInstanceID)
    {
        return overlapGameobject.Contains(otherInstanceID);
    }

    private void OnCollisionEnter(Collision collision)
    {
        overlapGameobject.Add(collision.gameObject.GetInstanceID());
    }

    private void OnCollisionExit(Collision collision)
    {
        overlapGameobject.Remove(collision.gameObject.GetInstanceID());
    }
}

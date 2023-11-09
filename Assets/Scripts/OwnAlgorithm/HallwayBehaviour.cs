using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HallwayBehaviour : MonoBehaviour
{
    [HideInInspector] public GameObject hallwayInMap;
    [HideInInspector] public RoomBehaviour headRoom;
    [HideInInspector] public RoomBehaviour tailRoom;
    bool entered;

    void Start()
    {
        entered = false;

        if (!RoomGenarator.instance.activeHallways.Contains(this)) gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (entered || !other.CompareTag("PlayerHead")) return;
        hallwayInMap.SetActive(true);
        entered = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HallwayBehaviour : MonoBehaviour
{
    [HideInInspector] public GameObject hallwayInMap;
    bool entered;

    void Start()
    {
        entered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (entered) return;
        hallwayInMap.SetActive(true);
        entered = true;
    }
}

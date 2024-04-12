using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TutorialHallwayBehaviour : MonoBehaviour
{
    [SerializeField] GameObject hallwayInMap;
    public TutorialRoomBehaviour headRoom;
    public TutorialRoomBehaviour tailRoom;
    bool entered;

    void Start()
    {
        entered = false;

        if (!TutorialRoomGenerator.instance.activeHallways.Contains(this)) gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (entered || !other.CompareTag("PlayerHead")) return;
        hallwayInMap.SetActive(true);
        entered = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;

public class TutorialRoomGenerator : MonoBehaviour
{
    public static TutorialRoomGenerator instance;

    [SerializeField] TutorialRoomBehaviour startRoom;
    [SerializeField] TutorialHallwayBehaviour[] hallways;
    [HideInInspector] public List<TutorialRoomBehaviour> activeRooms = new();
    [HideInInspector] public List<TutorialHallwayBehaviour> activeHallways = new();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        activeRooms.Add(startRoom);
    }

    // Enable and Disable Rooms
    public void UpdateRooms(TutorialRoomBehaviour currentRoom)
    {
        // rooms
        List<TutorialRoomBehaviour> newActiveRooms = new();
        newActiveRooms.Add(currentRoom);

        foreach (var gateBehaviour in currentRoom.gateBehaviours)
        {
            newActiveRooms.Add(gateBehaviour.otherGateBehaviour.roomBehaviour);
        }

        foreach (var room in activeRooms)
        {
            if (!newActiveRooms.Contains(room)) room.gameObject.SetActive(false);
        }

        foreach (var room in newActiveRooms)
        {
            if (!room.gameObject.activeSelf) room.gameObject.SetActive(true);
        }

        activeRooms = newActiveRooms;

        List<TutorialHallwayBehaviour> newActiveHallways = new();

        // hallways
        foreach (var hallway in hallways)
        {
            foreach (var headRoom in newActiveRooms)
            {
                if (hallway.headRoom == headRoom)
                {
                    foreach (var tailRoom in newActiveRooms)
                    {
                        if (hallway.tailRoom == tailRoom) newActiveHallways.Add(hallway);
                    }
                }
            }
        }

        foreach (var halway in activeHallways)
        {
            if (!newActiveHallways.Contains(halway)) halway.gameObject.SetActive(false);
        }

        foreach (var halway in newActiveHallways)
        {
            if (!halway.gameObject.activeSelf) halway.gameObject.SetActive(true);
        }

        activeHallways = newActiveHallways;
    }
}
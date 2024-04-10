using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialRoomBehaviour : MonoBehaviour
{
    [Header("Room")]
    public TutorialGateBehaviour[] gateBehaviours;
    [NonEditable][SerializeField] protected bool entered;


    [Header("Map Room")]
    [SerializeField] GameObject roomInMap;
    [HideInInspector] public List<GameObject> gatesInMap = new();

    [Header("Enemies")]
    [NonEditable][SerializeField] bool onCombat;
    [SerializeField] GameObject[] enemies;
    [SerializeField] GameObject[] blockedGates;

    void Start()
    {
        entered = false;
        onCombat = false;

        if (!TutorialRoomGenerator.instance.activeRooms.Contains(this)) gameObject.SetActive(false);
    }

    void Update()
    {
        if (!onCombat) return;

        foreach (GameObject enemy in enemies)
        {
            if (enemy) return;
        }
        onCombat = false;
        foreach (GameObject blockedGate in blockedGates)
        {
            blockedGate.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Open");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PlayerHead")) return;
        TutorialRoomGenerator.instance.UpdateRooms(this);

        if (entered) return;
        EnteredInRoom();
    }

    public void EnteredInRoom()
    {
        entered = true;
        if (roomInMap) roomInMap.SetActive(true);
        for (int i = 0; i < gatesInMap.Count; i++) gatesInMap[i].GetComponent<GateInMap>().ShowGate();

        if (enemies.Length == 0) return;
        onCombat = true;
        InitEnemies();
        foreach (GameObject blockedGate in blockedGates)
        {
            blockedGate.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Close");
        }
    }

    void InitEnemies()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            switch (enemies[i].GetComponent<EnemyType>().type)
            {
                case ENEMY_TYPE.SPHERE_ROBOT:
                    enemies[i].GetComponent<SphereRobot>().enabled = true;
                    break;
                case ENEMY_TYPE.MUTANT:
                    enemies[i].GetComponent<Mutant>().enabled = true;
                    break;
                default:
                    break;
            }
        }
    }
}

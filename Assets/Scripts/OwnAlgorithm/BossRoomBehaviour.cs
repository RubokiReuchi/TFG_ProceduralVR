using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossRoomBehaviour : RoomBehaviour
{
    [HideInInspector] public RoomGenarator manager;
    public int width; // in tiles
    public int height; // in tiles

    [SerializeField] GameObject boss;
    [SerializeField] Material fadeMaterial;

    // Start is called before the first frame update
    void Start()
    {
        entered = false;
        onCombat = false;

        if (!RoomGenarator.instance.activeRooms.Contains(this)) gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!onCombat) return;

        if (boss) return;

        onCombat = false;
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(3.0f);
        float opacity = 0.0f;
        while (opacity < 1.0f)
        {
            opacity += Time.deltaTime * 0.5f;
            if (opacity > 1.0f) opacity = 1.0f;
            fadeMaterial.SetFloat("_Opacity", opacity);
            yield return null;
        }
        GetComponent<AudioSource>().Play();
        DataPersistenceManager.instance.SaveGame();
        SceneManager.LoadScene(3); // lobby
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PlayerHead")) return;
        RoomGenarator.instance.UpdateRooms(this);

        if (entered) return;
        EnteredBossInRoom();
        for (int i = 0; i < joinedRooms.Count; i++) joinedRooms[i].GetComponent<RoomBehaviour>().EnteredInRoom();
        for (int i = 0; i < mapJoints.Count; i++) mapJoints[i].SetActive(true);
    }

    public void EnteredBossInRoom()
    {
        entered = true;
        if (roomInMap) roomInMap.SetActive(true);
        for (int i = 0; i < gatesInMap.Count; i++) gatesInMap[i].GetComponent<GateInMap>().ShowGate();

        onCombat = true;
        InitEnemies();
        foreach (GameObject blockedGate in blockedGates)
        {
            blockedGate.transform.GetChild(0).GetComponent<Animator>().SetTrigger("Close");
        }
    }

    public Vector3 GetEnterDoorPosition()
    {
        return doorsTransform[0].position;
    }

    void InitEnemies()
    {
        boss.GetComponent<Octopus>().enabled = true;
    }
}

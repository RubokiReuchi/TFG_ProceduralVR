using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossRoomBehaviour : RoomBehaviour
{
    [HideInInspector] public RoomGenarator manager;
    public int width; // in tiles
    public int height; // in tiles

    [SerializeField] Material fadeMaterial;

    // Start is called before the first frame update
    void Start()
    {
        if (!RoomGenarator.instance.activeRooms.Contains(this)) gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

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

        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        float opacity = 1.0f;
        while (opacity > 0)
        {
            opacity -= Time.deltaTime * 0.5f;
            if (opacity < 0) opacity = 0;
            fadeMaterial.SetFloat("_Opacity", opacity);
            yield return null;
        }
        DataPersistenceManager.instance.SaveGame();
        SceneManager.LoadScene(2); // lobby
    }

    public Vector3 GetEnterDoorPosition()
    {
        return doorsTransform[0].position;
    }
}

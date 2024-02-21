using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShieldOrientation : MonoBehaviour
{
    Transform player;
    [SerializeField] Enemy enemyScript;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!enemyScript.enabled) return;

        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }
}

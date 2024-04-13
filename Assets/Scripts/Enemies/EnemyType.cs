using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum ENEMY_TYPE
{
    SPHERE_ROBOT,
    TUTORIAL_SPHERE_ROBOT,
    MUTANT,
    MUTANT_ORB
}

public class EnemyType : MonoBehaviour
{
    public ENEMY_TYPE type;
    Enemy enemyScript;
    PlayerState playerState;
    bool xRayLayer = false;
    [SerializeField] GameObject[] xRayedGO;

    void Start()
    {
        enemyScript = GetComponent<Enemy>();
        playerState = PlayerState.instance;
    }

    private void Update()
    {
        if (enemyScript.firstEnable) return;

        if (!xRayLayer && playerState.xRayVisionActive)
        {
            foreach (var go in xRayedGO) go.layer = LayerMask.NameToLayer("XRayEnemy");
            xRayLayer = true;
        }
        else if (xRayLayer && !playerState.xRayVisionActive)
        {
            foreach (var go in xRayedGO) go.layer = LayerMask.NameToLayer("Enemy");
            xRayLayer = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum ENEMY_TYPE
{
    SPHERE_ROBOT
}

public class EnemyType : MonoBehaviour
{
    public ENEMY_TYPE type;
    Enemy enemyScript;
    Animator animator;
    PlayerState playerState;
    bool xRayLayer = false;
    [SerializeField] GameObject[] xRayedGO;

    void Start()
    {
        enemyScript = GetComponent<Enemy>();
        animator = transform.GetChild(0).GetComponent<Animator>();
        playerState = PlayerState.instance;
    }

    private void Update()
    {
        if (enemyScript.firstEnable) return;
        if (enemyScript.enabled == enemyScript.onCC)
        {
            if (enemyScript.onCC)
            {
                enemyScript.enabled = false;
                animator.speed = 0.0f;
            }
            else
            {
                enemyScript.enabled = true;
                animator.speed = 1.0f;
            }
        }

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

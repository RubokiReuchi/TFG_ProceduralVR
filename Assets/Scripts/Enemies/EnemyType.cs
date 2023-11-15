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

    void Start()
    {
        enemyScript = GetComponent<Enemy>();
        animator = transform.GetChild(0).GetComponent<Animator>();
    }

    private void Update()
    {
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
    }
}

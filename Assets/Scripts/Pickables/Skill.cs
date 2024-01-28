using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SKILL_TYPE
{
    // TIER 0
    DASH,
    // TIER 1
    BLUE,
    RED,
    BLOCKING_FIST,
    // TIER 2
    PURPLE,
    SUPER_JUMP,
    // TIER 3
    GREEN
}

public class Skill : MonoBehaviour
{
    public SKILL_TYPE type;

    [SerializeField] Puzzle puzzle;
    [SerializeField] Enemy[] enemies;

    bool ready = false;

    void Update()
    {
        if (ready) return;
        foreach (Enemy enemy in enemies)
        {
            if (enemy.alive)
            {
                return;
            }
        }

        ready = true;
        // set object visible
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!ready) return;
        if (!other.transform.root.CompareTag("Player")) return;

        puzzle.StartPuzzle();

        PlayerSkills.instance.UnlockSkill(type);
        Destroy(gameObject);
    }
}

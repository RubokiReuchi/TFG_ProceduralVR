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
    [SerializeField] GameObject[] meshes;
    [SerializeField] Enemy[] enemies;

    bool ready = false;

    void Start()
    {
        foreach (GameObject mesh in meshes)
        {
            mesh.SetActive(false);
        }
    }

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
        foreach (GameObject mesh in meshes)
        {
            mesh.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!ready) return;
        if (!other.transform.root.CompareTag("Player")) return;

        PlayerSkills.instance.UnlockSkill(type);

        if (puzzle) puzzle.StartPuzzle();

        Destroy(gameObject);
    }
}

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

    // map barriers
    [SerializeField] PowerChecker powerCheckerPuzzle;
    [SerializeField] IceSpikesPuzzle IceSpikesPuzzle;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        switch (type)
        {
            case SKILL_TYPE.BLUE:
                powerCheckerPuzzle.StartPuzzle();
                break;
            case SKILL_TYPE.RED:
                IceSpikesPuzzle.StartPuzzle();
                break;
            case SKILL_TYPE.PURPLE:
                break;
            case SKILL_TYPE.SUPER_JUMP:
                break;
            case SKILL_TYPE.GREEN:
                break;
            default:
                break;
        }
        PlayerSkills.instance.UnlockSkill(type);
        Destroy(gameObject);
    }
}

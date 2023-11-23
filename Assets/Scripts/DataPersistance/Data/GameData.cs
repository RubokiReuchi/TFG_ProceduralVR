using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    // tier 0
    public bool dashUnlocked;

    // tier 1
    public bool blueUnlocked;
    public bool redUnlocked;
    public bool blockingFistUnlocked;

    // tier 2
    public bool purpleUnlocked;
    public bool superJumpUnlocked;

    // tier 3
    public bool greenUnlocked;

    // new game values
    public GameData()
    {
        dashUnlocked = false;
        blueUnlocked = false;
        redUnlocked = false;
        blockingFistUnlocked = false;
        purpleUnlocked = false;
        superJumpUnlocked = false;
        greenUnlocked = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeNode
{
    public RoomBehaviour script;
    public TreeNode parent;
    public TreeNode[] children = new TreeNode[4]; // 0 --> top, 1 --> down, 2 --> right, 3 --> left

    public TreeNode(RoomBehaviour script, TreeNode parent)
    {
        this.script = script;
        this.parent = parent;
        for (int i = 0; i < 4; i++) children[i] = null;
    }
}
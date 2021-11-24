using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearOctree
{
    public static Dictionary<uint, LinearOctreeNode> Tree = new Dictionary<uint, LinearOctreeNode>();

    public int Depth;
    public Bounds OuterBounds;

    public LinearOctree(Bounds bounds, int depth)
    {
        this.Depth = depth;
        this.OuterBounds = bounds;

        /* Generate an Octree */
        GenerateOctree();
    }

    void GenerateOctree()
    {
        /* Generate a dummy buffer for calculating octrees */
        Collider[] colliders = new Collider[0];

        /* Generate a root node */
        LinearOctreeNode root = new LinearOctreeNode(Physics.OverlapBoxNonAlloc(OuterBounds.center, OuterBounds.size / 2.0f, colliders) == 0, OuterBounds, null);
    }
}

public class LinearOctreeNode
{
    public uint LocCode;
    public bool Empty;
    public Bounds bounds;

    public LinearOctreeNode(bool isEmpty, Bounds bounds, LinearOctreeNode parent, Octant octant = Octant.LDB)
    {
        /* Construct a new node */
        Empty = isEmpty;
        if (parent == null)
        {
            LocCode = 1;
        }
        else
        {
            LocCode = parent.LocCode << 3; //make room for a new octant
            LocCode = LocCode | octant.GetBitCode(); //append the new octant to the end
        }

        /* Add it to the hash tree */
        LinearOctree.Tree.Add(LocCode, this);
    }

    public LinearOctreeNode GetParent()
    {
        return LinearOctree.Tree[LocCode >> 3];
    }

    public LinearOctreeNode GetChild(Octant octant)
    {
        return LinearOctree.Tree[(LocCode << 3) | octant.GetBitCode()];
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearOctree
{
    public Dictionary<uint, LinearOctreeNode> Tree = new Dictionary<uint, LinearOctreeNode>();

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
        Collider[] colliders = new Collider[1];

        /* Generate a root node */
        LinearOctreeNode root = new LinearOctreeNode(this, Physics.OverlapBoxNonAlloc(OuterBounds.center, OuterBounds.extents, colliders) == 0, OuterBounds, null);
        if (root.Empty)
        {
            Debug.LogWarning("EMPTY OCTREE");
            return; //we can terminate early if the full space is empty
        }
        

        /* Generate a full octree */
        List<LinearOctreeNode> filledNodes = new List<LinearOctreeNode>();
        List<LinearOctreeNode> nextLevelNodes = new List<LinearOctreeNode>();
        filledNodes.Add(root);

        for(int level = 1; level < Depth; level++)
        {
            /* Generate the next level of nodes */
            foreach(var node in filledNodes)
            {
                /* For each non-empty node, split it */
                foreach(var octant in OctantExtensions.Octants)
                {
                    Bounds octantBounds = node.GetSubBounds(octant);
                    LinearOctreeNode sub = new LinearOctreeNode(this, Physics.OverlapBoxNonAlloc(octantBounds.center, octantBounds.extents, colliders) == 0, octantBounds, node, octant);
                    if (!sub.Empty) nextLevelNodes.Add(sub);
                }
            }

            /* We finished another level. Iterate deeper if we have more nodes to check */
            if(nextLevelNodes.Count > 0)
            {
                filledNodes = nextLevelNodes;
                nextLevelNodes = new List<LinearOctreeNode>();
            }
            else
            {
                break;
            }
        }
    }

    public List<LinearOctreeNode> GetLevel(int level)
    {
        List<LinearOctreeNode> ret = new List<LinearOctreeNode>();

        /* We just need all nodes with codes between a range */
        uint minRange = (uint)(1) << (level * 3);
        uint maxRange = (uint)(1) << (level * 3 + 1);

        for(uint locCode = minRange; locCode < maxRange; locCode += 1)
        {
            if (Tree.ContainsKey(locCode))
            {
                ret.Add(Tree[locCode]);
            }
        }

        return ret;
    }
}

public class LinearOctreeNode : System.IComparable<LinearOctreeNode>
{
    public uint LocCode;
    public bool Empty;
    public Bounds bounds;
    public LinearOctree tree;

    public LinearOctreeNode(LinearOctree tree, bool isEmpty, Bounds bounds, LinearOctreeNode parent, Octant octant = Octant.LDB)
    {
        /* Set Parameters */
        this.bounds = bounds;
        this.Empty = isEmpty;
        this.tree = tree;

        /* Construct a new node */
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
        tree.Tree.Add(LocCode, this);
    }

    public LinearOctreeNode GetParent()
    {
        return tree.Tree[LocCode >> 3];
    }

    public LinearOctreeNode GetChild(Octant octant)
    {
        if (!tree.Tree.ContainsKey((LocCode << 3) | octant.GetBitCode())) return null;
        return tree.Tree[(LocCode << 3) | octant.GetBitCode()];
    }

    public Bounds GetSubBounds(Octant octant)
    {
        Vector3 deltaCenter = bounds.extents / 2;
        deltaCenter.Scale(octant.GetDirection());
        return new Bounds(bounds.center + deltaCenter, bounds.extents);
    }

    public int CompareTo(LinearOctreeNode other)
    {
        if (this.LocCode < other.LocCode) return -1;
        return 1; //we will never have 2 equal nodes
    }

    public bool IsLeaf
    {
        get
        {
            if (Empty) return false;
            foreach(var octant in OctantExtensions.Octants)
            {
                if (GetChild(octant) != null) return false;
            }
            return true;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Octree
{
    public Bounds OuterBounds;
    public int Depth;

    public OctreeCell[][] tree; /* Level, Nodes */
    int[] subdivisions; /* Index: level, value: number of elements per dimension */

    public Octree(Bounds bounds, int depth)
    {
        this.OuterBounds = bounds;
        this.Depth = depth;
        subdivisions = new int[depth];

        /* Generate an empty tree */
        tree = new OctreeCell[depth][];
        for(int level = 0; level < depth; level++)
        {
            tree[level] = new OctreeCell[1 << level * 1 << level * 1 << level];
            subdivisions[level] = 1 << level;
        }

        /* Fill the tree */
        GenerateOctree();
    }

    void GenerateOctree()
    {
        /* Start at the lowest level and generate from sampling the world */
        for (int level = Depth - 1; level >= Depth - 1; level--)
        {
            /* Iterate through space in quantized blocks */
            Vector3 cellWidth = new Vector3(OuterBounds.size.x / subdivisions[level], OuterBounds.size.y / subdivisions[level], OuterBounds.size.z / subdivisions[level]);
            for (int z = 0; z < subdivisions[level]; z++)
            {
                for (int y = 0; y < subdivisions[level]; y++)
                {
                    for (int x = 0; x < subdivisions[level]; x++)
                    {
                        /* Get the two corner positions (for bounds) */
                        Vector3 startCorner = OuterBounds.min + new Vector3(cellWidth.x * x, cellWidth.y * y, cellWidth.z * z);
                        Vector3 endCorner = OuterBounds.min + new Vector3(cellWidth.x * (x+1), cellWidth.y * (y+1), cellWidth.z * (z+1));
                        Vector3 center = (startCorner + endCorner) / 2.0f;

                        /* Generate a new cell */
                        bool overlap = Physics.OverlapBox(center, cellWidth / 2.0f).Length != 0;
                        tree[level][x + y * subdivisions[level] + z * subdivisions[level] * subdivisions[level]] = new OctreeCell(new Bounds(center, cellWidth), !overlap);
                    }
                }
            }
        }

        /* Now, moving upward through layers, merge to generate upper layers */
        for (int level = Depth - 2; level >= 0; level--)
        {
            /* Iterate and merge */
            for (int z = 0; z < subdivisions[level]; z++)
            {
                for (int y = 0; y < subdivisions[level]; y++)
                {
                    for (int x = 0; x < subdivisions[level]; x++)
                    {
                        /* Each sublevel is 8 times subdivided from this one */
                        /* XXX = XYZ POS/NEG */
                        OctreeCell ppp = tree[level + 1][(2 * x + 1) + ((2 * y + 1) * subdivisions[level + 1]) + ((2 * z + 1) * subdivisions[level + 1] * subdivisions[level + 1])];
                        OctreeCell ppn = tree[level + 1][(2 * x + 1) + ((2 * y + 1) * subdivisions[level + 1]) + ((2 * z) * subdivisions[level + 1] * subdivisions[level + 1])];
                        OctreeCell pnp = tree[level + 1][(2 * x + 1) + ((2 * y) * subdivisions[level + 1]) + ((2 * z + 1) * subdivisions[level + 1] * subdivisions[level + 1])];
                        OctreeCell pnn = tree[level + 1][(2 * x + 1) + ((2 * y) * subdivisions[level + 1]) + ((2 * z) * subdivisions[level + 1] * subdivisions[level + 1])];
                        OctreeCell npp = tree[level + 1][(2 * x) + ((2 * y + 1) * subdivisions[level + 1]) + ((2 * z + 1) * subdivisions[level + 1] * subdivisions[level + 1])];
                        OctreeCell npn = tree[level + 1][(2 * x) + ((2 * y + 1) * subdivisions[level + 1]) + ((2 * z) * subdivisions[level + 1] * subdivisions[level + 1])];
                        OctreeCell nnp = tree[level + 1][(2 * x) + ((2 * y) * subdivisions[level + 1]) + ((2 * z + 1) * subdivisions[level + 1] * subdivisions[level + 1])];
                        OctreeCell nnn = tree[level + 1][(2 * x) + ((2 * y) * subdivisions[level + 1]) + ((2 * z) * subdivisions[level + 1] * subdivisions[level + 1])];
                        bool IsOpen = ppp.IsOpen && ppn.IsOpen && pnp.IsOpen && pnn.IsOpen && npp.IsOpen && npn.IsOpen && nnp.IsOpen && nnn.IsOpen;

                        Bounds total = ppp.bounds;
                        total.Encapsulate(nnn.bounds.min);
                        tree[level][x + y * subdivisions[level] + z * subdivisions[level] * subdivisions[level]] = new OctreeCell(total, IsOpen);
                    }
                }
            }
        }
    }
}

public class OctreeCell
{
    public Bounds bounds;

    /* Children are packed into an array based on their position */
    /* (+++), (++-), (+-+), (+--), (-++), (-+-), (--+), (---)  for (xyz axes) */
    public OctreeCell[] children;
    public bool IsOpen;

    public OctreeCell(Bounds bounds, bool IsOpen)
    {
        children = new OctreeCell[8];

        this.bounds = bounds;
        this.IsOpen = IsOpen;
    }
}
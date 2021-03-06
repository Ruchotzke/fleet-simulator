using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LinearOctree
{
    public class Octree
    {
        public Bounds OuterBounds;
        public int Depth;

        public OctreeCell[][] tree; /* Level, Nodes */
        public OctreeCell root;
        int[] subdivisions; /* Index: level, value: number of elements per dimension */

        public Octree(Bounds bounds, int depth)
        {
            this.OuterBounds = bounds;
            this.Depth = depth;
            subdivisions = new int[depth];

            /* Generate an empty tree */
            tree = new OctreeCell[depth][];
            for (int level = 0; level < depth; level++)
            {
                tree[level] = new OctreeCell[1 << level * 1 << level * 1 << level];
                subdivisions[level] = 1 << level;
            }

            /* Fill the tree */
            GenerateOctree();

            /* Set the alias for the root node */
            root = tree[0][0];
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
                            Vector3 endCorner = OuterBounds.min + new Vector3(cellWidth.x * (x + 1), cellWidth.y * (y + 1), cellWidth.z * (z + 1));
                            Vector3 center = (startCorner + endCorner) / 2.0f;

                            /* Generate a new cell */
                            bool overlap = Physics.OverlapBox(center, cellWidth / 2.0f).Length != 0;
                            OctreeCell newCell = new OctreeCell(new Bounds(center, cellWidth), !overlap);
                            tree[level][x + y * subdivisions[level] + z * subdivisions[level] * subdivisions[level]] = newCell;

                            /* Each cell is responsible for adding its neighbors if they already exist */
                            /* Cells behind us (-x, -y, or -z) already exist. Our future positive neighbors will add us */
                            /* Neighbors are only for open cells. If a cell is closed it has no neighbors */
                            if (!overlap)
                            {
                                if (x > 0)
                                {
                                    OctreeCell other = tree[level][(x - 1) + y * subdivisions[level] + z * subdivisions[level] * subdivisions[level]];
                                    if (other.IsOpen)
                                    {
                                        other.neighbors.Add(newCell);
                                        newCell.neighbors.Add(other);
                                    }
                                }
                                if (y > 0)
                                {
                                    OctreeCell other = tree[level][x + (y - 1) * subdivisions[level] + z * subdivisions[level] * subdivisions[level]];
                                    if (other.IsOpen)
                                    {
                                        other.neighbors.Add(newCell);
                                        newCell.neighbors.Add(other);
                                    }
                                }
                                if (z > 0)
                                {
                                    OctreeCell other = tree[level][x + y * subdivisions[level] + (z - 1) * subdivisions[level] * subdivisions[level]];
                                    if (other.IsOpen)
                                    {
                                        other.neighbors.Add(newCell);
                                        newCell.neighbors.Add(other);
                                    }
                                }
                            }
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
                            OctreeCell[] children = new OctreeCell[] {
                            tree[level + 1][(2 * x + 1) + ((2 * y + 1) * subdivisions[level + 1]) + ((2 * z + 1) * subdivisions[level + 1] * subdivisions[level + 1])], //ppp
                            tree[level + 1][(2 * x + 1) + ((2 * y + 1) * subdivisions[level + 1]) + ((2 * z) * subdivisions[level + 1] * subdivisions[level + 1])],
                            tree[level + 1][(2 * x + 1) + ((2 * y) * subdivisions[level + 1]) + ((2 * z + 1) * subdivisions[level + 1] * subdivisions[level + 1])],
                            tree[level + 1][(2 * x + 1) + ((2 * y) * subdivisions[level + 1]) + ((2 * z) * subdivisions[level + 1] * subdivisions[level + 1])],
                            tree[level + 1][(2 * x) + ((2 * y + 1) * subdivisions[level + 1]) + ((2 * z + 1) * subdivisions[level + 1] * subdivisions[level + 1])],
                            tree[level + 1][(2 * x) + ((2 * y + 1) * subdivisions[level + 1]) + ((2 * z) * subdivisions[level + 1] * subdivisions[level + 1])],
                            tree[level + 1][(2 * x) + ((2 * y) * subdivisions[level + 1]) + ((2 * z + 1) * subdivisions[level + 1] * subdivisions[level + 1])],
                            tree[level + 1][(2 * x) + ((2 * y) * subdivisions[level + 1]) + ((2 * z) * subdivisions[level + 1] * subdivisions[level + 1])]              //nnn
                        };
                            bool IsOpen = true;
                            foreach (var child in children)
                            {
                                if (!child.IsOpen)
                                {
                                    IsOpen = false;
                                    break;
                                }
                            }

                            /* Generate a new cell */
                            Bounds total = children[0].bounds;
                            total.Encapsulate(children[7].bounds.min);
                            OctreeCell newCell = new OctreeCell(total, IsOpen);
                            newCell.children = children;
                            tree[level][x + y * subdivisions[level] + z * subdivisions[level] * subdivisions[level]] = newCell;

                            /* Refactor neighbors if necessary */
                            if (newCell.IsOpen)
                            {
                                List<OctreeCell> lowerNeighbors = new List<OctreeCell>();
                                foreach (var cell in children)
                                {
                                    foreach (var neighbor in cell.neighbors)
                                    {
                                        if (children[0] != neighbor && children[1] != neighbor && children[2] != neighbor && children[3] != neighbor &&
                                            children[4] != neighbor && children[5] != neighbor && children[6] != neighbor && children[7] != neighbor)
                                        {
                                            if (!lowerNeighbors.Contains(neighbor))
                                            {
                                                lowerNeighbors.Add(neighbor);
                                            }
                                        }
                                    }
                                }
                                newCell.neighbors = lowerNeighbors;
                            }
                        }
                    }
                }
            }
        }

        public OctreeCell[] FindPoint(Vector3 point)
        {
            OctreeCell[] ret = new OctreeCell[Depth];

            if (OuterBounds.Contains(point))
            {
                ret[0] = root;
            }
            else
            {
                Debug.Log("Supplied Point is not in bounds of octree.");
                return null;
            }

            for (int level = 1; level < Depth; level++)
            {
                foreach (var child in ret[level - 1].children)
                {
                    if (child.bounds.Contains(point))
                    {
                        ret[level] = child;
                        break;
                    }
                }
            }

            return ret;
        }
    }

    public class OctreeCell
    {
        public Bounds bounds;

        /* Children are packed into an array based on their position */
        /* (+++), (++-), (+-+), (+--), (-++), (-+-), (--+), (---)  for (xyz axes) */
        public OctreeCell[] children;
        public List<OctreeCell> neighbors;
        public bool IsOpen;

        public OctreeCell(Bounds bounds, bool IsOpen)
        {
            this.bounds = bounds;
            this.IsOpen = IsOpen;
            neighbors = new List<OctreeCell>();
        }
    }
}
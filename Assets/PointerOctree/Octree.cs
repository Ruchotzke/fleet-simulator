using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PointerOctree
{
    public class Octree
    {
        public Bounds area;
        public int depth;
        public OctreeNode root;
        public OctreeNode[,,] baseNodes;

        public Octree(Bounds area, int depth)
        {
            this.area = area;
            this.depth = depth;

            /* Generate the octree */
            GenerateOctree();
        }

        void GenerateOctree()
        {
            /* Generate the bottommost layer of nodes */
            int nodes = (int)Mathf.Pow(2, depth);
            baseNodes = new OctreeNode[nodes, nodes, nodes];
            Vector3 nodeSize = area.size / nodes;
            Collider[] resultContainer = new Collider[1]; //for use with non-alloc overlap box
            for (int x = 0; x < nodes; x++)
            {
                for(int y = 0; y < nodes; y++)
                {
                    for(int z = 0; z < nodes; z++)
                    {
                        Bounds cellArea = new Bounds(area.min + nodeSize / 2.0f + new Vector3(nodeSize.x * x, nodeSize.y * y, nodeSize.z * z) , nodeSize / 2.0f);
                        baseNodes[x, y, z] = new OctreeNode(cellArea, Physics.OverlapBoxNonAlloc(cellArea.center, cellArea.size, resultContainer) != 0);
                    }
                }
            }

            /* Iteratively connect neighbors */
            for (int x = 0; x < nodes - 1; x++)
            {
                for (int y = 0; y < nodes - 1; y++)
                {
                    for (int z = 0; z < nodes - 1; z++)
                    {
                        /* Each node will have 6 face neighbors, 12 edge neighbors, and 8 vertex neighbors */
                        /* We are moving bottom to top, so just connect the top half of each */
                        OctreeNode curr = baseNodes[x, y, z];

                        /* 3 Face Neighbors */
                        curr.FaceNeighbors.Add(baseNodes[x + 1, y, z]);
                        curr.FaceNeighbors.Add(baseNodes[x, y + 1, z]);
                        curr.FaceNeighbors.Add(baseNodes[x, y, z + 1]);
                        baseNodes[x + 1, y, z].FaceNeighbors.Add(curr);
                        baseNodes[x, y + 1, z].FaceNeighbors.Add(curr);
                        baseNodes[x, y, z + 1].FaceNeighbors.Add(curr);

                        /* 6 Edge Neighbors */
                        curr.EdgeNeighbors.Add(baseNodes[x + 1, y, z]);
                        curr.EdgeNeighbors.Add(baseNodes[x, y + 1, z]);
                        curr.EdgeNeighbors.Add(baseNodes[x, y, z + 1]);
                        curr.EdgeNeighbors.Add(baseNodes[x + 1, y, z]);
                        curr.EdgeNeighbors.Add(baseNodes[x, y + 1, z]);
                        curr.EdgeNeighbors.Add(baseNodes[x, y, z + 1]);

                        /* 4 Vertex Neighbors */

                    }
                }
            }

            /* Join the nodes iteratively to generate an octree structure and maintain neighbors */

            /* Prune off unnecessary nodes (completely filled nodes) to reduce memory footprint */
        }
    }

    /// <summary>
    /// A class to contain a single node in the octree.
    /// </summary>
    public class OctreeNode
    {
        public Bounds Area;
        public bool IsFilled;
        public OctreeNode Parent;
        public OctreeNode[] Children;

        public List<OctreeNode> FaceNeighbors = new List<OctreeNode>();
        public List<OctreeNode> EdgeNeighbors = new List<OctreeNode>();
        public List<OctreeNode> CornerNeighbors = new List<OctreeNode>();

        public OctreeNode(Bounds area, bool isFilled)
        {
            Area = area;
            IsFilled = isFilled;
        }
    }
}

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

        public OctreeNode(Bounds area, bool isFilled)
        {
            Area = area;
            IsFilled = isFilled;
        }
    }
}

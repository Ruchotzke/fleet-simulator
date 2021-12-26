using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PointerOctree
{
    public class OctreeContainer : MonoBehaviour
    {
        public Octree octree;
        public int depth;
        public Bounds area;

        private void Awake()
        {
            /* Generate the octree */
            octree = new Octree(area, depth);
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                ///* Draw the area */
                //Gizmos.color = new Color(0.0f, 0.0f, 1.0f, 0.3f);
                //Gizmos.DrawCube(octree.area.center, octree.area.size);

                /* Draw the inner cubes */
                foreach (var node in octree.baseNodes)
                {
                    if (!node.IsFilled) continue;
                    Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.3f);
                    Gizmos.DrawCube(node.Area.center, node.Area.size * 1.9f);
                }
            }
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class OctreeMono : MonoBehaviour
{
    LinearOctree octree;

    [SerializeField] Bounds bounds;
    [SerializeField] int levels;
    [SerializeField, Range(0, 6)] int viewLevel = 0;

    // Start is called before the first frame update
    void Start()
    {
        /* Generate an octree */
        Stopwatch watch = new Stopwatch();
        watch.Start();
        octree = new LinearOctree(bounds, levels);
        watch.Stop();
        Debug.Log("Elapsed Time: " + watch.Elapsed.Milliseconds);
    }

    private void Update()
    {
        octree = new LinearOctree(bounds, levels);
    }

    private void OnDrawGizmos()
    {
        /* Highlight all filled voxels for a level */
        if(octree != null)
        {
            Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
            //foreach(var node in octree.GetLevel(viewLevel))
            //{
            //    Gizmos.DrawCube(node.bounds.center, node.bounds.size);
            //}

            foreach (var key in octree.Tree.Keys)
            {
                if (!octree.Tree[key].Empty && octree.Tree[key].IsLeaf)
                {
                    Gizmos.DrawCube(octree.Tree[key].bounds.center, octree.Tree[key].bounds.size);
                }
            }

            //foreach (var key in octree.Tree.Keys)
            //{
            //    if (octree.Tree[key].Empty)
            //    {
            //        Gizmos.DrawCube(octree.Tree[key].bounds.center, octree.Tree[key].bounds.size);
            //    }
            //}
        }
    }
}

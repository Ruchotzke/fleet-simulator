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

        for(int i = 0; i < levels; i++)
        {
            Debug.Log("LEVEL " + i + ": " + octree.GetLevel(i).Count);
        }
    }

    private void OnDrawGizmos()
    {
        /* Highlight all filled voxels */
        if(octree != null)
        {
            Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
            foreach(var node in octree.GetLevel(viewLevel))
            {
                Gizmos.DrawCube(node.bounds.center, node.bounds.size);
            }
        }
    }
}

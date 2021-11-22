using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class OctreeMono : MonoBehaviour
{
    Octree octree;

    [SerializeField] Bounds bounds;
    [SerializeField] int levels;
    [SerializeField, Range(0, 6)] int viewLevel = 0;

    // Start is called before the first frame update
    void Start()
    {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        octree = new Octree(bounds, levels);
        watch.Stop();
        Debug.Log("Elapsed Time: " + watch.Elapsed.Milliseconds);
    }

    private void OnDrawGizmos()
    {
        /* Highlight all filled voxels */
        if(octree != null)
        {
            Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.2f);
            foreach (var voxel in octree.tree[viewLevel])
            {
                if (voxel.IsOpen) continue;
                Gizmos.DrawCube(voxel.bounds.center, voxel.bounds.size);
            }
        }
    }
}

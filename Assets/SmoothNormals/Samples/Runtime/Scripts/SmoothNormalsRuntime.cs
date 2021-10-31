using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AquaSys.Tools.Samples
{
    public class SmoothNormalsRuntime : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshRenderers != null)
            {
                foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
                {
                    var uvs = new List<Vector3>();
                    skinnedMeshRenderer.sharedMesh.GetUVs(7, uvs);
                    if (uvs.Count == 0)
                    {
                        skinnedMeshRenderer.sharedMesh.SetUVs(7, SmoothNormals.ComputeSmoothedNormals(skinnedMeshRenderer.sharedMesh));
                    }
                }
            }
            else
            {
                var meshFilters = GetComponentsInChildren<MeshFilter>();
                if (meshFilters != null)
                {
                    foreach (var meshFilter in meshFilters)
                    {
                        var uvs = new List<Vector3>();
                        meshFilter.sharedMesh.GetUVs(7, uvs);
                        if (uvs.Count == 0)
                        {
                            meshFilter.sharedMesh.SetUVs(7, SmoothNormals.ComputeSmoothedNormals(meshFilter.sharedMesh));
                        }
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
//#define SMOOTH_ALL_MODEL

using UnityEngine;
using UnityEditor;
using AquaSys.SmoothNormals.Editor;

namespace AquaSys.SmoothNormals.Samples.Editor
{
    public class SmoothNormalsMeshImporter : AssetPostprocessor
    {
        void OnPostprocessModel(GameObject root)
        {
            if (SmoothNormalsEditor.CheckLabel(assetImporter))
            {
                if (SmoothNormalsEditor.toClearLabel)
                {
                    SmoothNormalsEditor.RemoveLabel(assetImporter);
                }
                else
                {
                    var meshes = Utils.GetMesh(root);

                    foreach (var mesh in meshes)
                    {
                        if (mesh.uv8 == null || mesh.uv8.Length == 0)
                            mesh.SetUVs(7, AquaSmoothNormals.ComputeSmoothedNormals(mesh));
                    }
                }
                   
            }

            else
            {
#if !SMOOTH_ALL_MODEL
                if (SmoothNormalsEditor.toAddLabel)
#endif
                SmoothNormalsEditor.AddLabel(assetImporter);
            }

        }
    }
}
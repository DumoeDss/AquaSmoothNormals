using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AquaSys.Tools.Samples.Editor
{
    public class SmoothNormalsEditor : EditorWindow
    {
        [MenuItem("Tools/SmoothNormals")]
        static SmoothNormalsEditor OpenWindow()
        {
            var window = GetWindow<SmoothNormalsEditor>(true, "SmoothNormals", true);
            window.minSize = new Vector2(600f, 400f);
            window.maxSize = new Vector2(600f, 1000f);
            return window;
        }
        GameObject root;
        Mesh mesh;
        private void OnGUI()
        {
            root = (GameObject)EditorGUILayout.ObjectField("Model To Smooth", root, typeof(GameObject), true);
            mesh = (Mesh)EditorGUILayout.ObjectField("Mesh To Smooth", mesh, typeof(Mesh), true);
            if (GUILayout.Button("SmoothNormals"))
            {
                SmoothNormal();
            }
            if (GUILayout.Button("ClearSmoothedNormals"))
            {
                SmoothNormal(true);
            }
        }

        void SmoothNormal(bool clear= false)
        {
            if (root != null)
            {
                var meshes = Utils.GetMesh(root);
                foreach (var mesh in meshes)
                {
                    if (clear)
                    {
                        mesh.uv8 = null;
                    }
                    else
                    {
                        mesh.SetUVs(7, SmoothNormals.ComputeSmoothedNormals(mesh));
                    }
                }
                EditorUtility.SetDirty(root);
            }

            if (mesh != null)
            {
                if (clear)
                {
                    mesh.uv8 = null;
                }
                else
                {
                    mesh.SetUVs(7, SmoothNormals.ComputeSmoothedNormals(mesh));
                }
                EditorUtility.SetDirty(mesh);
            }
        }
    }

    public class Utils
    {
       public static List<Mesh> GetMesh(GameObject go)
        {
            List<Mesh> meshes = new List<Mesh>();
            var meshFilters = go.GetComponentsInChildren<MeshFilter>();
            foreach (var item in meshFilters)
                meshes.Add(item.sharedMesh);
            var skinnedMeshRenderers = go.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var item in skinnedMeshRenderers)
                meshes.Add(item.sharedMesh);
            return meshes;
        }
    }

}

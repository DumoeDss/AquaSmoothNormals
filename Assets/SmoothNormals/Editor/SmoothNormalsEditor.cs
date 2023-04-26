using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace AquaSys.SmoothNormals.Editor
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
            string modelPath = "";
            if(root != null)
            {

                if (PrefabUtility.IsPartOfPrefabAsset(root))
                {
                 modelPath=   AssetDatabase.GetAssetPath(root);
                }else if (PrefabUtility.IsPartOfPrefabInstance(root))
                {
                    var prefabAsset = PrefabUtility.GetCorrespondingObjectFromOriginalSource(root);
                    modelPath = AssetDatabase.GetAssetPath(prefabAsset);
                }
            }
           
            if (GUILayout.Button("SmoothNormals"))
            {
                if (!string.IsNullOrEmpty(modelPath))
                {
                    toAddLabel = true;
                    AssetDatabase.ImportAsset(modelPath);
                    toAddLabel = false;
                }
                else
                {
                    SmoothNormal();
                }
            }
            if (GUILayout.Button("ClearSmoothedNormals"))
            {
                if (!string.IsNullOrEmpty(modelPath))
                {
                    toClearLabel = true;
                    AssetDatabase.ImportAsset(modelPath);
                    toClearLabel = false;
                }
                else
                {
                    SmoothNormal(true);
                }
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
                        mesh.SetUVs(7, AquaSmoothNormals.ComputeSmoothedNormals(mesh));
                    }
                }
                EditorUtility.SetDirty(root);
                AssetDatabase.Refresh();
            }

            if (mesh != null)
            {
                if (clear)
                {
                    mesh.uv8 = null;
                }
                else
                {
                    mesh.SetUVs(7, AquaSmoothNormals.ComputeSmoothedNormals(mesh));
                }
                EditorUtility.SetDirty(mesh);
                AssetDatabase.Refresh();
            }
        }

        static readonly string FlagLabel = "AquaOutlineBakedNormals";

        public static bool toAddLabel;
        public static bool toClearLabel;

        public static bool CheckLabel(AssetImporter assetImporter)
        {
            foreach (var label in AssetDatabase.GetLabels(assetImporter))
            {
                if (label.Contains(FlagLabel))
                {
                    return true;
                }
            }
            return false;
        }

        public static void AddLabel(AssetImporter assetImporter)
        {
            var labels = new List<string>(AssetDatabase.GetLabels(assetImporter));
            if (!labels.Contains(FlagLabel))
            {
                labels.Add(FlagLabel);
            }
            else
            {
                Debug.LogWarning("Lable has beed added!");
            }

            AssetDatabase.SetLabels(assetImporter, labels.ToArray());
        }

        public static void RemoveLabel(AssetImporter assetImporter)
        {
            var labels = new List<string>(AssetDatabase.GetLabels(assetImporter));
            if (labels.Contains(FlagLabel))
            {
                labels.Remove(FlagLabel);
            }

            AssetDatabase.SetLabels(assetImporter, labels.ToArray());
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

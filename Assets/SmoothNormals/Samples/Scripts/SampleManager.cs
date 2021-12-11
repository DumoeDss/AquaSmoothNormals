using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace AquaSys.SmoothNormals.Samples
{
    public class SampleManager : MonoBehaviour
    {
        public List<GameObject> sampleObjs;
        List<SkinnedMeshRenderer> skinnedMeshRenderers;
        List<MeshFilter> meshFilters;
        List<Material> materials;

        public Button GenerateSmoothNormalsToUv8Btn;
        public Button ClearBakedNoramlsInUv8Btn;
        public Button UseSmoothNormalsBtn;
        public Button UseOriginalNormalsBtn;

        private void Start()
        {
            Init();
        }

        void Init()
        {
            if (skinnedMeshRenderers == null)
            {
                skinnedMeshRenderers = new List<SkinnedMeshRenderer>();
                foreach (var item in sampleObjs)
                {
                    skinnedMeshRenderers.AddRange(item.GetComponentsInChildren<SkinnedMeshRenderer>());
                }
            }

            if (meshFilters == null)
            {
                meshFilters = new List<MeshFilter>();
                foreach (var item in sampleObjs)
                {
                    meshFilters.AddRange(item.GetComponentsInChildren<MeshFilter>());
                }
            }

            if (materials == null)
            {
                materials = new List<Material>();
                foreach (var item in skinnedMeshRenderers)
                {
                    materials.AddRange(item.sharedMaterials);
                }
                foreach (var item in meshFilters)
                {
                    materials.AddRange(item.gameObject.GetComponent<MeshRenderer>().sharedMaterials);
                }
            }
        }

        private void OnEnable()
        {
            GenerateSmoothNormalsToUv8Btn.onClick.AddListener(GenerateSmoothNormalsToUv8);
            ClearBakedNoramlsInUv8Btn.onClick.AddListener(ClearBakedNoramlsInUv8);
            UseSmoothNormalsBtn.onClick.AddListener(UseSmoothNormals);
            UseOriginalNormalsBtn.onClick.AddListener(UseOriginalNormals);
        }

        private void OnDisable()
        {
            GenerateSmoothNormalsToUv8Btn.onClick.RemoveListener(GenerateSmoothNormalsToUv8);
            ClearBakedNoramlsInUv8Btn.onClick.RemoveListener(ClearBakedNoramlsInUv8);
            UseSmoothNormalsBtn.onClick.RemoveListener(UseSmoothNormals);
            UseOriginalNormalsBtn.onClick.RemoveListener(UseOriginalNormals);
        }

        private void UseOriginalNormals()
        {
            ChangeNormals(false);
        }

        private void UseSmoothNormals()
        {
            ChangeNormals(true);
        }

        void ChangeNormals(bool smooth)
        {
            if (materials != null)
            {
                foreach (var item in materials)
                {
                    item.SetFloat("_Is_BakedNormal", smooth?1:0);
                }
            }
        }

        private void ClearBakedNoramlsInUv8()
        {
            SetMeshNormals(true);
        }

        private void GenerateSmoothNormalsToUv8()
        {
            SetMeshNormals();
        }


        public void SetMeshNormals(bool clear =false)
        {
            if (skinnedMeshRenderers != null)
            {
                foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
                {
                    if (clear)
                    {
                        skinnedMeshRenderer.sharedMesh.uv8 = null;
                    }
                    else
                    {
                        skinnedMeshRenderer.sharedMesh.SetUVs(7, SmoothNormals.ComputeSmoothedNormals(skinnedMeshRenderer.sharedMesh));
                    }
                }
            }

            if (meshFilters != null)
            {
                foreach (var meshFilter in meshFilters)
                {
                    if (clear)
                    {
                        meshFilter.sharedMesh.uv8 = null;
                    }
                    else
                    {
                        meshFilter.sharedMesh.SetUVs(7, SmoothNormals.ComputeSmoothedNormals(meshFilter.sharedMesh));
                    }
                }
            }
        }
    }
}
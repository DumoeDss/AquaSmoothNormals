using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace AquaSys.SmoothNormals
{
    public class SmoothNormals
    {
        public static Vector2[] ComputeSmoothedNormals(Mesh mesh)
        {
            Vector3[] verts = mesh.vertices;
            Vector3[] nors = mesh.normals;
            Vector4[] tans = mesh.tangents;

            int vertexCount = verts.Length;

            int[] indices = mesh.triangles;
            int indicesCount = indices.Length;

            //Vector3[] recalcNors = new Vector3[vertexCount];
            //float[] angles = new float[vertexCount];

            //for (int i = 0; i < indicesCount - 3; i += 3)
            //{
            //    int idx0 = indices[i], idx1 = indices[i + 1], idx2 = indices[i + 2];

            //    recalcNors[idx0] = CalcNormal(verts[idx0], verts[idx1], verts[idx2], 0, ref angles[idx0]);
            //    recalcNors[idx1] = CalcNormal(verts[idx0], verts[idx1], verts[idx2], 1, ref angles[idx1]);
            //    recalcNors[idx2] = CalcNormal(verts[idx0], verts[idx1], verts[idx2], 2, ref angles[idx2]);
            //}

            if (tans.Length == 0)
            {
                Debug.LogError($"{mesh.name} don't have tangents.");
                return null;
            }

            //CalcSmoothedNormals
            UnsafeHashMap<Vector3, Vector3> smoothedNormalsMap = new UnsafeHashMap<Vector3, Vector3>(vertexCount, Allocator.Persistent);
            for (int i = 0; i < vertexCount; i++)
            {
                if (smoothedNormalsMap.ContainsKey(verts[i]))
                {
                    smoothedNormalsMap[verts[i]] = smoothedNormalsMap[verts[i]] +
                    nors[i];
                    //recalcNors[i] * angles[i];
                }
                else
                {
                    smoothedNormalsMap.Add(verts[i],
                     nors[i]);
                     //recalcNors[i]  * angles[i]);
                }
            }

            //BakeSmoothedNormals
            NativeArray<Vector3> normalsNative = new NativeArray<Vector3>(nors, Allocator.Persistent);
            NativeArray<Vector3> vertrxNative = new NativeArray<Vector3>(verts, Allocator.Persistent);
            NativeArray<Vector4> tangents = new NativeArray<Vector4>(tans, Allocator.Persistent);
            NativeArray<Vector2> bakedNormals = new NativeArray<Vector2>(vertexCount, Allocator.Persistent);

            BakeNormalJob bakeNormalJob = new BakeNormalJob(vertrxNative, normalsNative, tangents, smoothedNormalsMap, bakedNormals);
            bakeNormalJob.Schedule(vertexCount, 100).Complete();

            var bakedSmoothedNormals = new Vector2[vertexCount];
            bakedNormals.CopyTo(bakedSmoothedNormals);

            smoothedNormalsMap.Dispose();
            normalsNative.Dispose();
            vertrxNative.Dispose();
            tangents.Dispose();
            bakedNormals.Dispose();
            return bakedSmoothedNormals;
        }

        //static Vector3 CalcNormal(Vector3 p0, Vector3 p1, Vector3 p2, int num, ref float angle)
        //{
        //    Vector3 v1, v2;

        //    switch (num)
        //    {
        //        case 1:
        //            v1 = (p1 - p0).normalized;
        //            v2 = (p2 - p1).normalized;
        //            //angle = Mathf.Acos(Vector3.Dot(v1, v2));
        //            break;

        //        case 2:
        //            v1 = (p2 - p1).normalized;
        //            v2 = (p2 - p0).normalized;
        //            //angle = Mathf.Acos(Vector3.Dot(v1, v2));

        //            break;

        //        default:
        //            v1 = (p1 - p0).normalized;
        //            v2 = (p2 - p0).normalized;
        //            //angle = Mathf.Acos(Vector3.Dot(v1, -v2));

        //            break;
        //    }
        //    angle = Mathf.Acos(Vector3.Dot(v1, v2));

        //    return Vector3.Cross(v1, v2).normalized;
        //}

        struct BakeNormalJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector3> vertrx, normals;
            [ReadOnly] public NativeArray<Vector4> tangants;
            [NativeDisableContainerSafetyRestriction]
            [ReadOnly] public UnsafeHashMap<Vector3, Vector3> smoothedNormals;
            [WriteOnly] public NativeArray<Vector2> bakedNormals;

            public BakeNormalJob(NativeArray<Vector3> vertrx,
                NativeArray<Vector3> normals,
                NativeArray<Vector4> tangents,
                UnsafeHashMap<Vector3, Vector3> smoothedNormals,
                NativeArray<Vector2> bakedNormals)
            {
                this.vertrx = vertrx;
                this.normals = normals;
                this.tangants = tangents;
                this.smoothedNormals = smoothedNormals;
                this.bakedNormals = bakedNormals;
            }

            void IJobParallelFor.Execute(int index)
            {
                Vector3 smoothedNormal = smoothedNormals[vertrx[index]];

                var normalOS = normals[index].normalized;
                Vector3 tangantOS = tangants[index];
                tangantOS = tangantOS.normalized;
                var bitangentOS = (Vector3.Cross(normalOS, tangantOS) * tangants[index].w).normalized;

                var tbn = new Matrix4x4(tangantOS, bitangentOS, normalOS, Vector3.zero);

                tbn = tbn.transpose;

                var bakedNormal = 
                    OctahedronNormal(
                    tbn.MultiplyVector(smoothedNormal).normalized
                    )
                    ;

                bakedNormals[index] = bakedNormal;
            }

            Vector2 OctahedronNormal(Vector3 ResultNormal)
            {
                Vector3 absVec = new Vector3(Mathf.Abs(ResultNormal.x), Mathf.Abs(ResultNormal.y), Mathf.Abs(ResultNormal.z));
                Vector2 OctNormal = (Vector2)ResultNormal / Vector3.Dot(Vector3.one, absVec);
                if (ResultNormal.z <= 0)
                {
                    float absY = Mathf.Abs(OctNormal.y);
                    float value = (1 - absY) * (OctNormal.y >= 0 ? 1 : -1);
                    OctNormal = new Vector2(value, value);
                }
                return OctNormal;
            }
           
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.General
{
    public class StairSmoother : SingletonMonobehaviour
    {
        [SerializeField]
        private MeshFilter meshFilter;
        [SerializeField]
        private MeshCollider meshCollider;

        public void Awake()
        {
            var mesh = GetSmoothingMeshFromNavmesh();
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = meshFilter.mesh;
        }

        private Mesh GetSmoothingMeshFromNavmesh()
        {
            var navmeshTriangulation = NavMesh.CalculateTriangulation();

            var mesh = new Mesh();

            mesh.vertices = navmeshTriangulation.vertices.Select(vert => transform.InverseTransformPoint(vert)).ToArray();

            mesh.triangles = navmeshTriangulation.indices;

            mesh.normals = mesh.vertices.Select(x => Vector3.up).ToArray();

            mesh.uv = mesh.vertices.Select(x => Vector2.zero).ToArray();

            return mesh;
        }
    }
}

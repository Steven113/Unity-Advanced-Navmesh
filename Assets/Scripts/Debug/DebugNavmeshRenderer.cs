using Assets.Scripts.Demo;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Debugging
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class DebugNavmeshRenderer : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private AdvancedNavmesh advancedNavmesh;
        // Start is called before the first frame update
        void Start()
        {
            _meshFilter = FindObjectOfType<MeshFilter>();
            _meshRenderer = FindObjectOfType<MeshRenderer>();
            advancedNavmesh = FindObjectOfType<AdvancedNavmesh>();
            advancedNavmesh.OnAdvancedPathfindingReady.RegisterAcion(BuildMeshFromNavmesh);
        }

        void BuildMeshFromNavmesh()
        {
            var debugColorArray = new[] { Color.red, Color.blue, Color.green };

            var triangles = advancedNavmesh.AllTriangles;

            var transformedVertices = triangles.Select(tri => new[] { transform.InverseTransformPoint(tri.Corner1), transform.InverseTransformPoint(tri.Corner2), transform.InverseTransformPoint(tri.Corner3) });

            var distinctVertices = transformedVertices.SelectMany(arr => arr)
                .Distinct().ToArray();

            var indexMap = new Dictionary<Vector3, int>();

            for (var i = 0; i < distinctVertices.Length; ++i)
            {
                indexMap[distinctVertices[i]] = i;
            }

            var mesh = new Mesh();
            _meshFilter.mesh = mesh;
            mesh.vertices = distinctVertices;
            mesh.uv = distinctVertices.Select(x => new Vector2(x.x, x.z)).ToArray();
            mesh.triangles = transformedVertices.SelectMany(arr => arr).Select(item => indexMap[item]).ToArray();
            mesh.colors = mesh.vertices.Select(x => indexMap[x]).Select(i => debugColorArray[i % debugColorArray.Length]).ToArray();
        }
    }
}

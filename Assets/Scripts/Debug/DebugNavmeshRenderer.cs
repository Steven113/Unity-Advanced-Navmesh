using Assets.Scripts.AI;
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
            advancedNavmesh.OnAdvancedPathfindingReady.RegisterAction(() => StartCoroutine(BuildMeshFromNavmesh()));
        }

        IEnumerator BuildMeshFromNavmesh()
        {
            while (true)
            {
                var scoreLookup = new Dictionary<int, Color>
                {
                    [0] = Color.red,
                    [1] = Color.yellow,
                    [2] = new Color(1.0f, 0.64f, 0.0f),
                    [3] = Color.green,
                };

                var triangles = advancedNavmesh.AllTriangles;

                var transformedVertices = triangles.Select(tri => new[] { transform.InverseTransformPoint(tri.Corner1), transform.InverseTransformPoint(tri.Corner2), transform.InverseTransformPoint(tri.Corner3) });

                var transformedVerticesFlattened = transformedVertices.SelectMany(arr => arr).ToArray();


                var mesh = new Mesh();
                _meshFilter.mesh = mesh;
                mesh.vertices = transformedVerticesFlattened;
                mesh.uv = transformedVerticesFlattened.Select(x => new Vector2(x.x, x.z)).ToArray();
                mesh.triangles = Enumerable.Range(0, transformedVerticesFlattened.Count()).ToArray();
                mesh.colors = triangles.SelectMany(tri => Enumerable.Repeat(scoreLookup[tri.MetaData.Score],3)).ToArray();

                yield return null;
            }
        }
    }
}

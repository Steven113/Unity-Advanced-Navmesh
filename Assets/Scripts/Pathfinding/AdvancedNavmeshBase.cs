using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using Octree;
using System.Collections.ObjectModel;
using Utils;
using UnityEditor;
using Assets.Scripts.Controllers;
using Assets.Scripts.Pathfinding;

namespace Assets.Scripts.AI
{
    /// <summary>
    /// Provides a API to do things with the Navmesh that the Unity provided Navmesh doesn't, such as querying triangles
    /// <see cref="TMetaData"/> is a type you will use to store "special" data about triangles which you will then update later
    /// This data could be whether the triangle has cover, whether there is a enemy standing in it, whatever you want.
    /// 
    /// Since Unity doesn't support generic MonoBehaviours, you will need to implement this MonoBehaviour to use it in your game
    /// </summary>
    /// <typeparam name="TMetaData"></typeparam>
    public abstract class AdvancedNavmeshBase<TMetaData> : MonoBehaviour where TMetaData : struct
    {
        [SerializeField]
        private float neighbourCornerTolerance = 0.03f;
        [SerializeField]
        private float playerNavmeshFillIntervalSeconds = 1f;
        [SerializeField]
        private float minNodeWidth = 0.01f;
        [SerializeField]
        private float minTriangleSideSize = 0.0001f;
        private Octree<NavmeshTriangle<TMetaData>> navMeshTriTree;
        internal IEnumerable<NavmeshTriangle<TMetaData>> AllTriangles => navMeshTriTree.GetAllContents();
        public ReadyCallback OnAdvancedPathfindingReady { get; } = new ReadyCallback();

        public void Start()
        {
            StartCoroutine(PathfindingCoroutine());
        }

        private class FunnelSearch : IComparable<FunnelSearch>
        {
            public NavmeshTriangle<TMetaData> TriangleToMoveTo { get; }

            public FunnelSearch(NavmeshTriangle<TMetaData> triangleToMoveTo)
            {
                TriangleToMoveTo = triangleToMoveTo;
            }

            public int CompareTo(FunnelSearch other)
            {
                return ((IComparable<NavmeshTriangle<TMetaData>>)TriangleToMoveTo).CompareTo(other.TriangleToMoveTo);
            }
        }

        private IEnumerator PathfindingCoroutine()
        {
            #region Get Navmesh Data

            var navmeshTriangulation = NavMesh.CalculateTriangulation();

            var triangles = new List<NavmeshTriangle<TMetaData>>();

            for (int index = 0; index < navmeshTriangulation.indices.Length; index += 3)
            {
                var corner1 = navmeshTriangulation.vertices[navmeshTriangulation.indices[index]];
                var corner2 = navmeshTriangulation.vertices[navmeshTriangulation.indices[index + 1]];
                var corner3 = navmeshTriangulation.vertices[navmeshTriangulation.indices[index + 2]];

                var allCorners = new[] {corner1, corner2, corner3 };

                //if (allCorners.Any(cornerA => allCorners.Count(cornerB => cornerB.Equals(cornerA)) == 2)) continue;

                //if (Vector3.Distance(corner1, new Vector3(-45.543f, 5.757813f, -46.3784f)) < 0.01f)
                //{
                //    Debug.Break();
                //}

                var skipAddingTriangle = false;

                foreach (var i in Enumerable.Range(0, 3))
                {
                    if (Vector3.Distance(allCorners[i], allCorners[(i+1)%3]) < minTriangleSideSize)
                    {
                        skipAddingTriangle = true;
                        continue;
                    }
                }

                var area = navmeshTriangulation.areas[navmeshTriangulation.indices[index] / 3];

                if (!skipAddingTriangle) triangles.Add(new NavmeshTriangle<TMetaData>(corner1, corner2, corner3, area));
            }

            //triangles.Sort((triA, triB) => triA.AABB.MinX.CompareTo(triA.AABB.MinX));

            var boundingSet = AABB.Create(navmeshTriangulation.vertices, minNodeWidth);

            boundingSet.DrawAABB(Color.white);

            navMeshTriTree = new Octree<NavmeshTriangle<TMetaData>>(minNodeWidth, boundingSet);

            Utils.Utils.StopWatchTimedActivity(() =>
            {
                foreach (var tri in triangles)
                {

                    Debug.Assert(navMeshTriTree.Insert(tri));
                }
            }, "Inserting triangles into octree");

            #region Find neighbours for each triangle

            Utils.Utils.StopWatchTimedActivity(() =>
            {
                for (int a = 0; a < triangles.Count; ++a)
                {
                    var triA = triangles[a];

                    navMeshTriTree.GetOverlappingItems(triA.AABB, out Collection<NavmeshTriangle<TMetaData>> possibleNeighbours);

                    foreach (var possibleNeighbour in possibleNeighbours)
                    {
                        if (possibleNeighbour.Equals(triA)) continue;

                        NavmeshTriangle<TMetaData>.TryMakeIntoNeighbours(triA, possibleNeighbour, neighbourCornerTolerance);
                    }
                }
            }, "Find neighbouring triangles");

            #endregion
            #endregion

            //foreach (var tri in triangles)
            //{
            //    tri.DebugDraw(Color.red, Color.blue, 30f);
            //}

            OnAdvancedPathfindingReady.MarkReady();

            var navMeshDataProviderBase = FindObjectOfType<NavmeshMetaDataProviderBase<TMetaData>>();

            if (navMeshDataProviderBase != null)
            {
                yield return StartCoroutine(navMeshDataProviderBase.UpdateNavmeshMetaData(navMeshTriTree));
            } else
            {
                yield return null;
            }

        }

        private bool TryFindNearestTriangleToPoint(Vector3 point, float agentRadius, float agentHeight, out NavmeshTriangle<TMetaData> navmeshTriangle, out Vector3? closestPointOnTrianglePlane, float debugDrawTime = 1f)
        {
            var playerScanBounds = new AABB(point, new Vector3(agentRadius, agentHeight, agentRadius));

            playerScanBounds.DrawAABB(Color.red, debugDrawTime);

            navMeshTriTree.GetOverlappingItems(playerScanBounds, out Collection<NavmeshTriangle<TMetaData>> playerOverlappingTriangles);

            //foreach (var playerOverlappingTri in playerOverlappingTriangles) playerOverlappingTri.DebugDraw(Color.yellow, Color.green, playerNavmeshFillIntervalSeconds);

            var trisThatPlayerMayBeStandingOn = playerOverlappingTriangles.Where(tri => tri.ContainsPoint(point) || tri.PointWithinDistanceToSide(point: point, distance: agentRadius));

            if (trisThatPlayerMayBeStandingOn.Any())
            {
                foreach (var triThatPlayerMayBeStandingOn in trisThatPlayerMayBeStandingOn) triThatPlayerMayBeStandingOn.DebugDraw(Color.blue, Color.green, playerNavmeshFillIntervalSeconds);

                navmeshTriangle = trisThatPlayerMayBeStandingOn.OrderBy(tri => tri.DistanceFromTriangle(point)).First();
                closestPointOnTrianglePlane = navmeshTriangle.Plane.ClosestPointOnPlane(point);

                //Debug.DrawLine(point, navmeshTriangle.Plane.ClosestPointOnPlane(point), Color.red, debugDrawTime);

                navmeshTriangle.DebugDraw(Color.magenta, Color.blue, playerNavmeshFillIntervalSeconds);

                return true;
            }

            navmeshTriangle = null;
            closestPointOnTrianglePlane = null;
            return false;
        }

        /// <summary>
        /// If you take a shared corner - triangle centre vector and cross it with mid point of shared corners - triangle centre, the dot of that cross product and this vector should be 1 for the shared corner to be on the rhs
        /// </summary>
        internal static readonly Vector3 PathGenerationCrossProductReference = Vector3.down;

        #region Funnel algorithm for getting path http://digestingduck.blogspot.com/2010/03/simple-stupid-funnel-algorithm.html
        float triarea2(Vector3 a, Vector3 b, Vector3 c)
        {
            float ax = b[0] - a[0];
            float ay = b[2] - a[2];
            float bx = c[0] - a[0];
            float by = c[2] - a[2];
            return bx * ay - ax * by;
        }

        /// <summary>
        /// Transforms a series of visited Navmesh triangles into a path.
        /// </summary>
        /// <param name="portals">The neighbour data for the links between your triangles"</param>
        /// <param name="agentPosition">Where the agent is standing at the start of the path</param>
        /// <param name="path">The series of vertices your agent must visit</param>
        internal void GetPathThroughTrianglesUsingFunnelAlgorithm(NavmeshTriangle<TMetaData>.NeighbourInfo[] portals, Vector3 agentPosition,
                 out List<Vector3> path)
        {
            path = new List<Vector3>();

            // Find straight path.
            // Init scan state
            int apexIndex = 0, leftIndex = 0, rightIndex = 0;
            var portalApex = agentPosition;
            var portalLeft = portals[0].LHS;
            var portalRight = portals[0].RHS;

            // Add start point.
            path.Add(portalApex);

            var nportals = portals.Length;

            Vector3 left, right;

            for (int i = 1; i < nportals; ++i)
            {
                left = portals[i].LHS;
                right = portals[i].RHS;

                // Update right vertex.
                if (triarea2(portalApex, portalRight, right) <= 0.0f)
                {
                    if (portalApex.Equals(portalRight) || triarea2(portalApex, portalLeft, right) > 0.0f)
                    {
                        // Tighten the funnel.
                        portalRight = right;
                        rightIndex = i;
                    }
                    else
                    {
                        // Right over left, insert left to path and restart scan from portal left point.
                        path.Add(portalLeft);
                        // Make current left the new apex.
                        portalApex = portalLeft;
                        apexIndex = leftIndex;
                        // Reset portal
                        portalLeft = portalApex;
                        portalRight = portalApex;
                        leftIndex = apexIndex;
                        rightIndex = apexIndex;
                        // Restart scan
                        i = apexIndex;
                        continue;
                    }
                }

                // Update left vertex.
                if (triarea2(portalApex, portalLeft, left) >= 0.0f)
                {
                    if (portalApex.Equals(portalLeft) || triarea2(portalApex, portalRight, left) < 0.0f)
                    {
                        // Tighten the funnel.
                        portalLeft = left;
                        leftIndex = i;
                    }
                    else
                    {
                        // Left over right, insert right to path and restart scan from portal right point.
                        path.Add(portalRight);
                        // Make current right the new apex.
                        portalApex = portalRight;
                        apexIndex = rightIndex;
                        // Reset portal
                        portalLeft = portalApex;
                        portalRight = portalApex;
                        leftIndex = apexIndex;
                        rightIndex = apexIndex;
                        // Restart scan
                        i = apexIndex;
                        continue;
                    }
                }
            }
            //// Append last point to path.
            //if (npts < maxPts)
            //{
            //    vcpy(&pts[npts * 2], &portals[(nportals - 1) * 4 + 0]);
            //    npts++;
            //}
        }

        #endregion
    }
}

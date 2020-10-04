using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Octree;

namespace Assets.Scripts.AI
{
    /// <summary>
    /// Represents a single triangle in the navigation mesh, with a given <see cref="TMetaData"/> type storing extra info the developer wants to keep
    /// such as whether the triangle is visible to the player
    /// </summary>
    /// <typeparam name="TMetaData"></typeparam>
    internal class NavmeshTriangle<TMetaData> : IComparable<NavmeshTriangle<TMetaData>>, IAABBBoundedObject where TMetaData : struct
    {
        public Vector3 Corner1 { get; }
        public Vector3 Corner2 { get; }
        public Vector3 Corner3 { get; }
        private Vector3[] AllCorners { get; }
        public int Area { get; }
        private List<NeighbourInfo> neighbours { get; set; } = new List<NeighbourInfo>();
        public IEnumerable<NeighbourInfo> Neighbours => neighbours;

        public AABB AABB { get; }
        public Plane Plane { get; }
        #region Fields for A* pathfinding
        public float Cost { get; set; }
        /// <summary>
        /// This field allows for custom pathfinding implementations to know whether a triangle has been visited
        /// For example with A* instead of a open or closed list you just check CostVersion compared to which no. pathfinding search you are doing
        /// If I am doing search #20 and CostVersion is 18, I've never visited this node before
        /// </summary>
        public int CostVersion { get; set; }
        #endregion
        public TMetaData MetaData { get; set; }
        /// <summary>
        /// A helper field in case you want to check if you've already updated the Metadata. 
        /// For example if this is the 20th time I'm updating the meta data and the version of a triangle is 20, I've already updated it
        /// This can be useful for flood-fill algorithms
        /// </summary>
        public int MetaDataVersion { get; set; }
        public Vector3 Centre { get; }

        public NavmeshTriangle(Vector3 corner1, Vector3 corner2, Vector3 corner3, int area)
        {
            Corner1 = corner1;
            Corner2 = corner2;
            Corner3 = corner3;

            AllCorners = new Vector3[] { Corner1, Corner2, Corner3 };

            AABB = AABB.Create(AllCorners);

            //foreach (var cornerA in AllCorners)
            //    foreach (var cornerB in AllCorners)
            //        Debug.DrawLine(cornerA, cornerB, Color.red, 30f);

            Plane = new Plane(Corner1, Corner2, Corner3);

            Centre = Utils.Utils.GetCentre(AllCorners);

            Area = area;
        }

        public void AddNeighbour(NeighbourInfo neighbour)
        {
            neighbours.Add(neighbour);
        }

        private static bool AreNeighbours(NavmeshTriangle<TMetaData> triA, NavmeshTriangle<TMetaData> triB, float neighbourCornerTolerance, out Vector3[] sharedCorners)
        {
            sharedCorners = null;

            if (!triA.AABB.Overlaps(triB.AABB)) return false;
            if (!triB.AABB.Overlaps(triA.AABB)) return false;
            if ((triA.Area & triB.Area) == 0) return false;

            sharedCorners = triA.AllCorners.Where(cornerA => triB.AllCorners.Any(cornerB => Vector3.Distance(cornerA, cornerB) < neighbourCornerTolerance)).ToArray();

            return sharedCorners.Length == 1 || sharedCorners.Length == 2;
        }

        internal static void TryMakeIntoNeighbours(NavmeshTriangle<TMetaData> triA, NavmeshTriangle<TMetaData> triB, float neighbourCornerTolerance)
        {
            if (AreNeighbours(triA, triB, neighbourCornerTolerance, out Vector3[] sharedCorners))
            {
                if (sharedCorners.Length == 2)
                {
                    triA.AddNeighbour(new NeighbourInfo(triA, triB, sharedCorners[0], sharedCorners[1]));
                    triB.AddNeighbour(new NeighbourInfo(triB, triA, sharedCorners[0], sharedCorners[1]));
                }
                //else if (sharedCorners.Length == 1)
                //{
                //    triA.AddNeighbour(new NeighbourInfoOneSharedCorner(triA, triB, sharedCorners[0]));
                //    triB.AddNeighbour(new NeighbourInfoOneSharedCorner(triB, triA, sharedCorners[0]));
                //}
                //Debug.DrawLine(triA.AABB.center, triB.AABB.center, Color.blue, 30f);
            }
        }

        public bool ContainsPoint(Vector3 point)
        {
            for (int i = 0; i < AllCorners.Length; ++i)
            {
                var end = AllCorners[(i + 1) % AllCorners.Length];
                var start = AllCorners[i];

                var reference = Vector3.Cross(end - start, Centre - start);

                var check = Vector3.Cross(end - start, point - start);

                if (Vector3.Dot(reference, check) < 0) return false;
            }

            return true;
        }

        public float DistanceFromTriangle(Vector3 point)
        {
            return Vector3.Distance(point, Centre);
        }

        public void DebugDraw(Color sideColor, Color neighbourLineColor, float time)
        {
            for (int i = 0; i < AllCorners.Length; ++i)
            {
                Debug.DrawLine(AllCorners[i], AllCorners[(i + 1) % AllCorners.Length], sideColor, time);
            }
            foreach (var neighbourInfo in neighbours)
            {
                Debug.DrawLine(Utils.Utils.GetCentre(neighbourInfo.Neighbour.AllCorners), Utils.Utils.GetCentre(AllCorners), neighbourLineColor, time);
            }

            Debug.DrawRay(Centre, Plane.normal, Color.yellow, time);

            //AABB.DrawAABB(Color.green);

            //foreach (var dir in new [] { -0.5f, 0.5f })
            //{
            //    Debug.DrawRay(AABB.center, new Vector3(0, 0, AABB.extents.z * dir), Color.green, time);
            //    Debug.DrawRay(AABB.center, new Vector3(AABB.extents.x * dir, 0, 0), Color.green, time);
            //    Debug.DrawRay(AABB.center, new Vector3(0, AABB.extents.y * dir, 0), Color.green, time);
            //}
        }

        internal void DebugDrawCost(Color costColor, Color cheaperNeighbourColor, float renderTime)
        {
            Debug.DrawRay(Centre, Vector3.up * Cost * 0.1f, costColor, renderTime);

            foreach (var neighbour in Neighbours.Where(neighbourInfo => neighbourInfo.Neighbour.Cost < Cost && neighbourInfo.Neighbour.CostVersion == CostVersion))
            {
                Debug.DrawRay(Centre, (neighbour.Neighbour.Centre - Centre) * 0.9f, cheaperNeighbourColor, renderTime);
            }
        }



        int IComparable<NavmeshTriangle<TMetaData>>.CompareTo(NavmeshTriangle<TMetaData> other)
        {
            return Cost.CompareTo(other.Cost);
        }

        internal bool PointWithinDistanceToSide(Vector3 point, float distance)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (Utils.Utils.DistanceToLineSegment(AllCorners[i], AllCorners[(i + 1) % 3], point) < distance)
                {
                    return true;
                }
            }

            return false;
        }

        internal bool PointSideOfPlane(Vector3 point)
        {
            return Vector3.Dot(point - Plane.ClosestPointOnPlane(point), AdvancedNavmeshBase<TMetaData>.PathGenerationCrossProductReference) < 0;
        }

        public override bool Equals(object obj)
        {
            return obj is NavmeshTriangle<TMetaData> triangle &&
                   Corner1.Equals(triangle.Corner1) &&
                   Corner2.Equals(triangle.Corner2) &&
                   Corner3.Equals(triangle.Corner3);
        }

        public override int GetHashCode()
        {
            var hashCode = -1113428400;
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector3>.Default.GetHashCode(Corner1);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector3>.Default.GetHashCode(Corner2);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector3>.Default.GetHashCode(Corner3);
            return hashCode;
        }

        internal class NeighbourInfo
        {

            public float DistanceToNeighbour { get; }
            public NavmeshTriangle<TMetaData> Neighbour { get; }
            public Vector3 RHS { get; }
            public Vector3 LHS { get; }
            public Vector3 SharedSideNormal { get; }

            public NeighbourInfo(NavmeshTriangle<TMetaData> triangleRecievingNeighbour, NavmeshTriangle<TMetaData> neighbour, Vector3 sharedSidePoint1, Vector3 sharedSidePoint2)
            {
                this.Neighbour = neighbour;

                var midPointBetweenNodes = (sharedSidePoint1 + sharedSidePoint2) / 2;
                DistanceToNeighbour = triangleRecievingNeighbour.AllCorners.Sum(corner => Vector3.Distance(midPointBetweenNodes, corner)) / triangleRecievingNeighbour.AllCorners.Length
                + neighbour.AllCorners.Sum(corner => Vector3.Distance(midPointBetweenNodes, corner)) / neighbour.AllCorners.Length;

                RHS = IsRhs(triangleRecievingNeighbour.Centre, sharedSidePoint1, midPointBetweenNodes) ? sharedSidePoint1 : sharedSidePoint2;
                LHS = !IsRhs(triangleRecievingNeighbour.Centre, sharedSidePoint2, midPointBetweenNodes) ? sharedSidePoint2 : sharedSidePoint1;

                SharedSideNormal = ((RHS + LHS) / 2 - triangleRecievingNeighbour.Centre).normalized;
            }

            /// <summary>
            /// Helper to calculate for a given end of the edge joining two navmesh triangles, if is on the RHS
            /// </summary>
            /// <param name="triangleCenter"></param>
            /// <param name="candidateRHSPoint"></param>
            /// <param name="midPointOfEdge"></param>
            /// <returns></returns>
            private static bool IsRhs(Vector3 triangleCenter, Vector3 candidateRHSPoint, Vector3 midPointOfEdge)
            {
                return Vector3.Dot(Vector3.Cross(candidateRHSPoint - triangleCenter, midPointOfEdge - triangleCenter), AdvancedNavmeshBase<TMetaData>.PathGenerationCrossProductReference) > 0;
            }
        }
    }
}
using Assets.Scripts.AI;
using Assets.Scripts.Pathfinding;
using Octree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Demo
{
    public class NavmeshCoverDataGenerator : NavmeshMetaDataProviderBase<bool>
    {
        [SerializeField]
        private Transform playerLookStart;
        [SerializeField]
        private Transform playerLookEnd;
        [SerializeField]
        private float playerFOV = 60;

        internal override IEnumerator UpdateNavmeshMetaData(IOctreeReadonly<NavmeshTriangle<bool>> triangles)
        {
            var allTriangles = triangles.GetAllContents();

            while (true)
            {
                var playerLookDirection = playerLookEnd.position - playerLookStart.position;

                foreach (var currentTri in allTriangles)
                {
                    currentTri.MetaData = true;

                    var closestPointOnTriPlane = currentTri.Plane.ClosestPointOnPlane(playerLookStart.position);

                    if (currentTri.ContainsPoint(closestPointOnTriPlane) && Vector3.Angle(playerLookDirection, closestPointOnTriPlane - playerLookStart.position) < playerFOV)
                    {
                        currentTri.MetaData = false;
                    } else
                    {
                        var corners = new[] { currentTri.Corner1, currentTri.Corner2, currentTri.Corner3 };

                        for (var i = 0; i < 3; ++i)
                        {
                            var closestPointOnSide = Utils.Utils.ClosestPointOnLineSegment(corners[i], corners[(i + 1) % 3], playerLookStart.position);

                            if (Vector3.Angle(playerLookDirection, closestPointOnSide - playerLookStart.position) < playerFOV)
                            {
                                currentTri.MetaData = false;
                                break;
                            }
                        }
                    }
                }

                yield return null;
            }
        }
    }
}

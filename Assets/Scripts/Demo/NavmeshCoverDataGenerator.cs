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
        [SerializeField]
        private float maxPlayerRange = 100;

        internal override IEnumerator UpdateNavmeshMetaData(IOctreeReadonly<NavmeshTriangle<bool>> triangles)
        {
            var allTriangles = triangles.GetAllContents();

            while (true)
            {
                var playerLookDir = playerLookEnd.position - playerLookStart.position;

                foreach (var currentTri in allTriangles)
                {
                    currentTri.MetaData = false;

                    var corners = new[] { currentTri.Corner1, currentTri.Corner2, currentTri.Corner3 };

                    Vector3? coverCheckPoint = null;

                    for (var i = 0; i < 3; ++i)
                    {
                        var closestPointOnSide = (corners[i] + corners[(i + 1) % 3]) /2f;

                        //if (Vector3.Angle(closestPointOnSide - playerLookStart.position, playerLookDir) > playerFOV)
                        //{
                        //    continue;
                        //}
                        //else if (Vector3.Distance(closestPointOnSide, playerLookStart.position) > maxPlayerRange)
                        //{
                        //    continue;
                        //}

                        if (!coverCheckPoint.HasValue || Vector3.Distance(coverCheckPoint.Value, playerLookStart.position) > Vector3.Distance(closestPointOnSide, playerLookStart.position))
                        {
                            coverCheckPoint = closestPointOnSide;
                        }
                    }

                    if (Vector3.Angle(coverCheckPoint.Value - playerLookStart.position, playerLookDir) > playerFOV)
                    {
                        currentTri.MetaData = true;
                    } else if (Vector3.Distance(coverCheckPoint.Value, playerLookStart.position) > maxPlayerRange)
                    {
                        currentTri.MetaData = true;
                    } else
                    {
                        if (Physics.Raycast(playerLookStart.position, coverCheckPoint.Value - playerLookStart.position, out var hitInfo, maxPlayerRange)
                            && Vector3.Distance(hitInfo.point, playerLookStart.position) < Vector3.Distance(coverCheckPoint.Value, playerLookStart.position))
                        {
                            currentTri.MetaData = true;
                        }
                    }
                }

                yield return null;
            }
        }
    }
}

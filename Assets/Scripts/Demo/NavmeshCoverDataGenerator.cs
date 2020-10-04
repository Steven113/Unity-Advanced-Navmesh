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
    public class NavmeshCoverDataGenerator : NavmeshMetaDataProviderBase<CoverScore>
    {
        [SerializeField]
        private Transform playerLookStart;
        [SerializeField]
        private Transform playerLookEnd;
        [SerializeField]
        private float playerFOV = 60;
        [SerializeField]
        private float maxPlayerRange = 100;
        [SerializeField]
        private int agentAreaId = 1;

        internal override IEnumerator UpdateNavmeshMetaData(IOctreeReadonly<NavmeshTriangle<CoverScore>> triangles)
        {
            var allTriangles = triangles.GetAllContents();

            while (true)
            {
                var playerLookDir = playerLookEnd.position - playerLookStart.position;

                foreach (var currentTri in allTriangles.Where(tri => tri.Area == agentAreaId))
                {
                    var corners = new[] { currentTri.Corner1, currentTri.Corner2, currentTri.Corner3 };

                    var protectedSides = 0;

                    for (var i = 0; i < 3; ++i)
                    {
                        var closestPointOnSide = (corners[i] + corners[(i + 1) % 3]) /2f;

                        if (Vector3.Angle(closestPointOnSide - playerLookStart.position, playerLookDir) > playerFOV)
                        {
                            ++protectedSides;
                        }
                        else if (Vector3.Distance(closestPointOnSide, playerLookStart.position) > maxPlayerRange)
                        {
                            ++protectedSides;
                        }
                        else
                        {
                            if (Physics.Raycast(playerLookStart.position, closestPointOnSide - playerLookStart.position, out var hitInfo, maxPlayerRange)
                                && Vector3.Distance(hitInfo.point, playerLookStart.position) < Vector3.Distance(closestPointOnSide, playerLookStart.position))
                            {
                                ++protectedSides;
                            }
                        }
                    }

                    currentTri.MetaData = new CoverScore(protectedSides);
                }

                yield return null;
            }
        }
    }
}

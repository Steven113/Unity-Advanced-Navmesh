using Assets.Scripts.AI;
using Assets.Scripts.Pathfinding;
using Octree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Demo
{
    public class NavmeshCoverDataGenerator : NavmeshMetaDataProviderBase<bool>
    {
        internal override IEnumerator UpdateNavmeshMetaData(IOctreeReadonly<NavmeshTriangle<bool>> triangles)
        {
            var allTriangles = triangles.GetAllContents();

            for (var i = 0; i < allTriangles.Count; ++i)
            {
                allTriangles[i].MetaData = i % 2 == 0;
            }

            yield return null;
        }
    }
}

using Assets.Scripts.AI;
using Octree;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Pathfinding
{
    public abstract class NavmeshMetaDataProviderBase<TMetaData> : MonoBehaviour where TMetaData : struct
    {
        /// <summary>
        /// A coroutine which iterates over the triangles in the mesh and sets their TMetaData.
        /// The <see cref="AdvancedNavmeshBase{TMetaData}"/> instance will start this coroutine when the Navmesh data is populated,
        /// and will run it to completion
        /// 
        /// If the navmesh data changes, this coroutine will be immediately terminated
        /// </summary>
        /// <param name="triangles"></param>
        /// <returns></returns>
        internal abstract IEnumerator UpdateNavmeshMetaData(IOctreeReadonly<NavmeshTriangle<TMetaData>> triangles);
    }
}

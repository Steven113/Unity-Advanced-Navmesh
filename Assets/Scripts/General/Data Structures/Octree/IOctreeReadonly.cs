﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Octree
{
    public interface IOctreeReadonly<T> where T : IAABBBoundedObject
    {
        List<T> GetAllContents();
        void GetOverlappingItems(AABB itemAABB, out Collection<T> items);
    }
}
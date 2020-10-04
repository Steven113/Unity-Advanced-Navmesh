//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Octree
{
	[Serializable]
	public class OctreeNode<T> where T : IAABBBoundedObject
    {
        public AABB Bounds { get; }
		public List<OctreeNode<T>> Children { get; }  = new List<OctreeNode<T>> ();
		public List<T> ObjectsInNode { get; } = new List<T>();
		public OctreeNode<T> Parent { get; }
        public float MinDimensionOfNode { get; }
        public bool ShouldHaveChildren { get; }

		//mins and maxes are global, position is global. Minx and maxes adjusted based on centre
		public OctreeNode (float minDimensionOfNode,AABB nodeBounds)
		{
            //this.paren;
            Bounds = nodeBounds;

            MinDimensionOfNode = minDimensionOfNode;

            ShouldHaveChildren = true;

            foreach (var i in Enumerable.Range(0, 3))
            {
                if (Bounds.extents[i] * 2 < minDimensionOfNode)
                {
                    ShouldHaveChildren = false;
                    break;
                }
            }
        }
        
        public bool SubdivisionCanContainItem(T item)
        {
            return Enumerable.Range(0, 3).All(i => item.AABB.extents[i] < Bounds.extents[i] / 2);
        }

        public bool Insert(T item, bool debugRender = false){
			AABB itemAABB = item.AABB;
			if (Bounds.Encloses (itemAABB)) {
				if (!ShouldHaveChildren || !SubdivisionCanContainItem(item))
                {
                    ObjectsInNode.Add(item);
                    return true;
                } else
                {
                    if (Children.Count == 0)
                    {
                        foreach (var extent in Bounds.EnumerateExtents())
                        {
                             var newCentre = Bounds.center + extent / 2;

                            //Debug.DrawLine(newCentre, Bounds.center, Color.white, 30f);

                            Children.Add(new OctreeNode<T>(MinDimensionOfNode, new AABB(newCentre, Bounds.extents / 2)));
                        }
                    }

                    foreach (var child in Children)
                    {
                        if (child.Insert(item, debugRender))
                            return true;
                    }

                    ObjectsInNode.Add(item);
                    return true;

                }
			} else if (debugRender)
            {
                Bounds.DrawAABB(Color.red);
                item.AABB.DrawAABB(Color.green);
            }
			return false;
		}

		public void getAllContents(ref Collection<T> items){
			
			for (int i = 0; i<ObjectsInNode.Count; ++i) {
				items.Add(ObjectsInNode[i]);
			}
			
			for (int i = 0; i<Children.Count; ++i) {
				Children[i].getAllContents(ref items);
			}
		}

		public void GetOverlappingItems(AABB itemBounds, ref Collection<T> items){
			//bounds.DrawAABB ();
			if (Bounds.ContainsPoint(itemBounds.center) || Bounds.Overlaps(itemBounds)){

				foreach (var itemInNode in ObjectsInNode){
					items.Add(itemInNode);
				}

				for (int i = 0; i<Children.Count; ++i){
					Children[i].GetOverlappingItems(itemBounds,ref items);

				}
			}
		}

        public void GetOverlappingItems(Vector3 pos, ref Collection<T> items)
        {
            //bounds.DrawAABB ();
            if (Bounds.ContainsPoint(pos))
            {

                int count = ObjectsInNode.Count;
                for (int i = 0; i < count; ++i)
                {
                    items.Add(ObjectsInNode[i]);
                    //Debug.DrawRay(bounds.center.FlattenVector()+new Vector3Custom(0,5,0),(getAABBFunc(objectsInNode[i]).center-bounds.center).FlattenVector(),Color.red,3f);
                }

                for (int i = 0; i < Children.Count; ++i)
                {
                    //Debug.DrawRay(bounds.center.FlattenVector()+new Vector3Custom(0,5,0),(nodeList[children[i]].bounds.center-bounds.center).FlattenVector(),Color.green,3f);
                    Children[i].GetOverlappingItems(pos, ref items);

                }
            }
        }
    }
}


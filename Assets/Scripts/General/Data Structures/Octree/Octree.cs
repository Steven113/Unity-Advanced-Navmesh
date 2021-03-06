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

namespace Octree
{


	//only supports static insertion
	[Serializable]
	public class Octree<T> : IOctreeReadonly<T> where T : IAABBBoundedObject
	{
		public List<OctreeNode<T>> nodes = new List<OctreeNode<T>>();
		public OctreeNode<T> root;
		public float minDimensionOfNode = 1;

		public Octree(float minDimensionOfNode, AABB treeBounds)
		{
			root = new OctreeNode<T>(minDimensionOfNode, treeBounds);
			this.minDimensionOfNode = minDimensionOfNode;
			//this.anchor = anchor;
		}

		public bool Insert(T item, bool debugRender = false)
		{
			//item.AABB.DrawAABB(Color.blue);

			return root.Insert(item, debugRender);
		}

		//recursively get all objects in tree
		public List<T> GetAllContents()
		{
			Collection<T> items = new Collection<T>();

			for (int i = 0; i < root.ObjectsInNode.Count; ++i)
			{
				items.Add(root.ObjectsInNode[i]);
			}

			for (int i = 0; i < root.Children.Count; ++i)
			{
				root.Children[i].getAllContents(ref items);
			}

			List<T> result = new List<T>(items.Count);
			while (items.Count > 0)
			{
				result.Add(items[0]);
				items.RemoveAt(0); //we remove the first item so that accessing the next item will still be O(1)
			}
			return result;
		}

		public void GetOverlappingItems(AABB itemAABB, out Collection<T> items)
		{

			items = new Collection<T>();

			root.GetOverlappingItems(itemAABB, ref items);
			//itemAABB.DrawAABB(Color.red);

		}

		//public void GetOverlappingItems(Vector3 pos, out Collection<T> items)
		//{
		//    items = new Collection<T>();

		//    root.GetOverlappingItems(pos, ref items);
		//}
	}
}


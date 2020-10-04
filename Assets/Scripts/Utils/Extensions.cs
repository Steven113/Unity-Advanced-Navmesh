using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public static class Extensions
    {
        public static Vector3 Flatten(this Vector3 vector3)
        {
            return new Vector3(vector3.x, 0, vector3.z);
        }

        public static bool TryGetComponent<T>(this GameObject gameObject, out T component)
        {
            component = gameObject.GetComponent<T>();
            return component != null;
        }

        public static void DisableAndStopAllCoroutines(this MonoBehaviour monoBehaviour)
        {
            monoBehaviour.enabled = false;
            monoBehaviour.StopAllCoroutines();
        }
    }

    public static class IEnumerableExtensions
    {
        public interface IEnumerablePredicateSplit<T>
        {
            IEnumerable<T> Items { get; }
            T PredicateMatch { get; }
        }

        public class EnumerablePredicateSplit<T> : IEnumerablePredicateSplit<T>
        {
            public IEnumerable<T> Items { get; }

            public T PredicateMatch { get; }

            public EnumerablePredicateSplit(IEnumerable<T> items, T predicateMatch)
            {
                Items = items;
                PredicateMatch = predicateMatch;
            }
        }

        /// <summary>
        /// Returns sublists of the given list, where each sublist ends with a item that returns true for the predicate, unless no items in the list match the predicate, in which case the input list is returned
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerablePredicateSplit<T>> SplitByPredicate<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            var currentSublist = new List<T>();

            foreach (var item in items)
            {
                if (predicate(item))
                {
                    yield return new EnumerablePredicateSplit<T>(new List<T>(currentSublist), item);

                    currentSublist.Clear();
                }
                else
                {
                    currentSublist.Add(item);
                }
            }

            if (currentSublist.Count > 0) yield return new EnumerablePredicateSplit<T>(new List<T>(currentSublist), default(T));
        }

        public static IEnumerable<T> DistinctPreserveOrder<T>(this IEnumerable<T> items)
        {
            var hashset = new HashSet<T>();

            foreach (var item in items)
            {
                if (hashset.Add(item)) yield return item;
            }
        }
    }
}

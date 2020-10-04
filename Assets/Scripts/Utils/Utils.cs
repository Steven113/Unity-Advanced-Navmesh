using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utils
{
    public static class Utils
    {
        public static T[] Shuffle<T>(T[] input)
        {
            T[] result = input.ToArray();

            return RandomlyReorderInPlace(input);
        }

        private static T[] RandomlyReorderInPlace<T>(T [] input)
        {
            for (var i = 0; i < input.Length; ++i)
            {
                var swapIndex = Random.Range(0, input.Length);
                var toSwap = input[swapIndex];
                input[swapIndex] = input[i];
                input[i] = toSwap;
            }

            return input;
        }

        public static bool BoundsIntersect(Bounds a, Bounds b)
        {
            return b.Intersects(a) || a.Intersects(b);
        }

        public static bool EitherBoundsContainsOtherBounds(Bounds a, Bounds b)
        {
            return a.Contains(b) || b.Contains(a);
        }

        private static bool Contains(this Bounds bounds, Bounds other) =>
            bounds.max.x >= other.max.x && bounds.min.x <= other.max.x
            && bounds.max.y >= other.max.y && bounds.min.y <= other.max.y
            && bounds.max.z >= other.max.z && bounds.min.z <= other.max.z;



        public static float OrthoDistance(this Vector3 vector3, Vector3 other) => Math.Abs(vector3.x - other.x) + Math.Abs(vector3.y - other.y) + Math.Abs(vector3.z - other.z);

        public static Vector3 GetCentre(IEnumerable<Vector3> vectors) => vectors.Aggregate((vec1, vec2) => vec1 + vec2)/vectors.Count();

        public static Vector3 ClosestPointOnLineSegment(Vector3 segmentStart, Vector3 segmentEnd, Vector3 point)
        {
            // Shift the problem to the origin to simplify the math.    
            var wander = point - segmentStart;
            var span = segmentEnd - segmentStart;

            // Compute how far along the line is the closest approach to our point.
            float t = Vector3.Dot(wander, span) / span.sqrMagnitude;

            // Restrict this point to within the line segment from start to end.
            t = Mathf.Clamp01(t);

            // Return this point.
            return segmentStart + t * span;
        }

        public static float DistanceToLineSegment(Vector3 segmentStart, Vector3 segmentEnd, Vector3 point)
        {
            return Vector3.Distance(ClosestPointOnLineSegment(segmentStart, segmentEnd, point), point);
        }

        public static void StopWatchTimedActivity(Action activity, string activityName)
        {
            var stopWatch = Stopwatch.StartNew();

            activity();

            stopWatch.Stop();

            UnityEngine.Debug.Log($"{activityName} took {stopWatch.ElapsedMilliseconds} ms");
        }

        public static IEnumerable<T> InfiniteRandomlyShuffledEnumerator<T>(T[] items)
        {
            while (true)
            {
                foreach (var item in RandomlyReorderInPlace(items))
                    yield return item;
            }
        }

        public static bool TryGetInstance<T>(this GameObject gameObject, out T instance) where T : MonoBehaviour
        {
            instance = gameObject.GetComponent<T>();

            return instance != null;
        }
    }

    public class ReadyCallback
    {
        private Action OnReady { get; set; }

        private bool _ready;

        public void RegisterAction(Action action)
        {
            if (_ready) action();
            else OnReady += action;
        }

        public void MarkReady()
        {
            if (!_ready) OnReady?.Invoke();
            _ready = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.General
{
    public class SingletonMonobehaviour : MonoBehaviour
    {
        private static Dictionary<Type, SingletonMonobehaviour> InitializedSingletonTypes = new Dictionary<Type, SingletonMonobehaviour>();

        public void Awake()
        {
            if (InitializedSingletonTypes.ContainsKey(GetType()))
            {
                throw new InvalidOperationException($"The type {GetType()} is a singleton type. Only one monobehaviour in the scene may be of this type at a given time. The current instance is attached to {InitializedSingletonTypes[GetType()].name}");
            }

            InitializedSingletonTypes[GetType()] = this;
        }

        public static T GetInstance<T>() where T : SingletonMonobehaviour
        {
            if (InitializedSingletonTypes.TryGetValue(typeof(T), out SingletonMonobehaviour singleton) && singleton is T singletonCast)
            {
                return singletonCast;
            }

            throw new InvalidOperationException($"Singleton of type {typeof(T)} does not have a instance in the scene");
        }

        void OnDestroy()
        {
            InitializedSingletonTypes.Remove(GetType());
        }
    }
}

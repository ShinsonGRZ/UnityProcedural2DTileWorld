﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PoolingSystem.Factories;
using PoolingSystem.GarbageCollectors;

namespace PoolingSystem
{
    public class PoolManager : MonoBehaviour
    {
        private static PoolManager _instance;

        /// <summary>
        /// The singleton instance of pool manager in this scene.
        /// </summary>
        public static PoolManager Instance
        {
            get
            {
                if (!_instance)
                    _instance = new GameObject().AddComponent<PoolManager>();
                return _instance;
            }
        }

        /// <summary>
        /// The singleton instance of pool manager in this scene.
        /// </summary>
        public static PoolManager Pools
        {
            get { return Instance; }
        }

        [SerializeField] [Tooltip("Predefined pools.")] private PredefinedObjectPool[] _predefinedPools;
        private readonly Dictionary<GameObject, ObjectPool> _pools = new Dictionary<GameObject, ObjectPool>();
        [SerializeField] private PoolingSetting poolingSetting;

        /// <summary>
        /// Access to an specified pool.
        /// </summary>
        /// <param name="prefab">The prefab that defines the pool.</param>
        /// <returns>The pool asociated with the prefab.</returns>
        public ObjectPool this[GameObject prefab]
        {
            get { return _pools[prefab]; }
            protected set { _pools[prefab] = value; }
        }

        /// <summary>
        /// Load all the pools defined in the inspector.
        /// </summary>
        private void LoadPredefined()
        {
            foreach (var poolManager in FindObjectsOfType<PoolManager>())
            {
                if (poolManager._predefinedPools == null)
                    continue;
                foreach (var predefinedPool in poolManager._predefinedPools)
                {
                    var pool = ObjectPool.FromPredefined(predefinedPool);
                    pool.transform.parent = transform;
                    this[pool.Prefab] = pool;
                }
            }
        }

        // Awake is called when the script instance is being loaded
        public void Awake()
        {
            if (!_instance)
                _instance = this;
            else if (this != _instance)
                return;

            name = "Pool Manager";
            LoadPredefined();
        }

        // Start is called just before any of the Update methods is called the first time
        public void Start()
        {
            if (this != _instance)
                Destroy(this);
        }

        // This function is called when the MonoBehaviour will be destroyed
        public void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }

    [Serializable]
    public struct PoolingSetting
    {
        [SerializeField] public FactoryProviders Factory;
        [SerializeField] public GarbageCollectorProviders GarbageCollector;
    }

    [Serializable]
    public struct PredefinedObjectPool
    {
        [SerializeField] [Tooltip("The base prefab of this pool.")] public GameObject Prefab;
        [SerializeField] [Tooltip("The minimum number of objects to keep.")] public uint Min;
        [SerializeField] [Tooltip("The maximum number of objects to keep.\n(0 = No limit.).")] public uint Max;
        [SerializeField] [Range(0.1f, 1.0f)] [Tooltip("The target usage ratio.")] public float UsageRatio;
    }

    public interface IObjectPool
    {
        /// <summary>
        /// The prefab that defines the pool.
        /// </summary>
        GameObject Prefab { get; }

        /// <summary>
        /// The ammount of elements currently allocated.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// The ammount of elements currently active.
        /// </summary>
        int ActiveCount { get; }

        /// <summary>
        /// The ammount of elements currently inactive.
        /// </summary>
        int InactiveCount { get; }

        /// <summary>
        /// Retrieves or instantiates a new one if the pool is empty and the limit is not exceeded.
        /// Internally called by the other Spawn() methods.
        /// </summary>
        /// <returns>A prefab instance ready to use.</returns>
        GameObject Spawn();

        /// <summary>
        /// Retrieves or instantiates a new one if the pool is empty and the limit is not exceeded.
        /// </summary>
        /// <param name="transform">The transform with position and rotation to set.</param>
        /// <returns>A prefab instance ready to use</returns>
        GameObject Spawn(Transform transform);

        /// <summary>
        /// Retrieves or instantiates a new one if the pool is empty and the limit is not exceeded.
        /// </summary>
        /// <param name="transform">The transform with position and rotation to set.</param>
        /// <param name="parent">The parent transform in the scene to set.</param>
        /// <returns>A prefab instance ready to use</returns>
        GameObject Spawn(Transform transform, Transform parent);

        /// <summary>
        /// Retrieves or instantiates a new one if the pool is empty and the limit is not exceeded.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <param name="rotation">The rotation to set.</param>
        /// <returns>A prefab instance ready to use</returns>
        GameObject Spawn(Vector3 position, Quaternion rotation);

        /// <summary>
        /// Retrieves or instantiates a new one if the pool is empty and the limit is not exceeded.
        /// </summary>
        /// <param name="position">The position to set.</param>
        /// <param name="rotation">The rotation to set.</param>
        /// <param name="parent">The parent transform in the scene to set.</param>
        /// <returns>A prefab instance ready to use</returns>
        GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent);

        /// <summary>
        /// Forcedly despawn an active instance.
        /// </summary>
        void Despawn();

        /// <summary>
        /// Despawn the selected instance.
        /// </summary>
        /// <param name="instance">The instance to despawn</param>
        void Despawn(GameObject instance);
    }
}
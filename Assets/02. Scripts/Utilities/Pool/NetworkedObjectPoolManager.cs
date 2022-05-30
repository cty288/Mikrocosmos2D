using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Pool;
using MikroFramework.ResKit;
using MikroFramework.Singletons;
using MikroFramework.Utilities;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class NetworkedObjectPoolManager : NetworkBehaviour, IGameObjectPoolManager<NetworkedGameObjectPool>, ISingleton{
        public static NetworkedObjectPoolManager Singleton {
            get {
                return SingletonProperty<NetworkedObjectPoolManager>.Singleton;
            }
        }

        private Dictionary<string, NetworkedGameObjectPool> pools;
        public Dictionary<string, NetworkedGameObjectPool> GameObjectPools
        {
            get { return pools; }
        }

        private ResLoader resLoader;
        /// <summary>
        /// If the pooled gameobject does not contain a component that inherits from PoolableGameObject class,
        /// will the manager auto add a default PoolableGameObject component to it? If not, an error will be
        /// thrown if the pooled gameobject does not have the component. (Default: false)
        /// </summary>
        public static bool AutoAddDefaultPoolableGameObject = false;

        /// <summary>
        /// When requesting the manager to allocate a prefab, if the Manager does not contain a
        /// GameObjectPool for that prefab, will the Manager auto create one for it? If not, an error will be
        /// thrown if the Manager does not contain it. (Default: false)
        /// </summary>
        public static bool AutoCreatePoolWhenAllocating = false;

        private void Awake()
        {
            pools = new Dictionary<string, NetworkedGameObjectPool>();
            resLoader = new ResLoader();
        }

        public NetworkedGameObjectPool GetOrCreatePool(GameObject prefab)
        {
            return CreatePool(prefab, 10, 50);
        }

        public NetworkedGameObjectPool GetOrCreatePoolFromAB(string prefabAssetName, string ownerBundleName,
            out GameObject prefab)
        {
            return CreatePoolFromAB(prefabAssetName, ownerBundleName, 10, 50, out prefab);
        }

        [SerializeField] private GameObject networkedGameObjectPoolPrefab;
        public NetworkedGameObjectPool CreatePool(GameObject prefab, int initialCount, int maxCount) {
            if (NetworkServer.active) {
                string prefabName = prefab.name;

                if (CheckPoolExists(prefab, out NetworkedGameObjectPool existingPool))
                {
                    return existingPool;
                }

                if (AutoAddDefaultPoolableGameObject)
                {
                    if (prefab.GetComponent<PoolableGameObject>() == null)
                    {
                        prefab.AddComponent<DefaultPoolableGameObject>();
                    }
                }
                
                NetworkedGameObjectPool pool = Instantiate(networkedGameObjectPoolPrefab)
                    .GetComponent<NetworkedGameObjectPool>();
                pool.PooledPrefab = prefab;
                pool.Init(initialCount, maxCount);
                NetworkServer.Spawn(pool.gameObject);
                GameObjectPools.Add(prefabName, pool);
                return pool;
            }

            return null;

        }

        public NetworkedGameObjectPool CreatePoolFromAB(string prefabAssetName, string ownerBundleName,
            int initialCount, int maxCount, out GameObject prefab) {
            prefab = resLoader.LoadSync<GameObject>(ownerBundleName, prefabAssetName);
            return CreatePool(prefab, initialCount, maxCount);
        }

        private bool CheckPoolExists(GameObject prefab, out NetworkedGameObjectPool pool)
        {
            string prefabName = prefab.name;
            if (!pools.ContainsKey(prefabName))
            {
                GameObject existingPoolGo = GameObject.Find($"Object Pool: {prefabName}"); //;.GetComponent<SafeGameObjectPool>();
                if (existingPoolGo)
                {
                    if (existingPoolGo.TryGetComponent<NetworkedGameObjectPool>(out NetworkedGameObjectPool existingPool))
                    {
                        pools.Add(prefabName, existingPool);
                        pool = existingPool;
                        return true;
                    }
                }

                pool = null;
                return false;
            }

            pool = pools[prefabName];
            return true;
        }


        public GameObject Allocate(GameObject prefab)
        {
            if (prefab)
            {
                NetworkedGameObjectPool pool;
                string prefabName = prefab.name;

                if (!CheckPoolExists(prefab, out pool))
                {
                    if (AutoCreatePoolWhenAllocating)
                    {
                        pool = GetOrCreatePool(prefab);
                    }
                    else
                    {
                        Debug.LogError(
                            $"The pool does not exist for {prefabName}! Use CreatePool() to create its pool!");
                        return null;
                    }
                }


                return pool.Allocate();

            }

            Debug.LogWarning("A Prefab is missing but the SafeGameObjectPool is still trying to access it!");
            return null;
        }



        public bool Recycle(GameObject recycledObject)
        {
            if (recycledObject)
            {
                string prefabName = CommonUtility.DeleteCloneName(recycledObject);
                recycledObject.name = prefabName;
                NetworkedGameObjectPool pool;

                if (!CheckPoolExists(recycledObject, out pool))
                {
                    if (AutoCreatePoolWhenAllocating)
                    {
                        pool = GetOrCreatePool(recycledObject);
                        pool.Recycle(recycledObject);
                        return true;
                    }
                    else {
                        Debug.LogError($"The pool does not exist for {prefabName}! Use CreatePool() to create its pool!");
                        return false;
                    }
                   
                }
                else
                {
                    return pool.Recycle(recycledObject);
                }
            }

            return false;
        }

        public bool AddNewPool(NetworkedGameObjectPool newPool)
        {
            if (newPool != null && newPool.PooledPrefab)
            {
                if (!CheckPoolExists(newPool.PooledPrefab, out NetworkedGameObjectPool pool))
                {
                    pools.Add(newPool.PooledPrefab.name, newPool);
                    return true;
                }
            }

            return false;
        }

        private void OnDestroy() {
            resLoader.ReleaseAllAssets();
        }

        public void OnSingletonInit() {
            
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using MikroFramework.Event;
using MikroFramework.Factory;
using MikroFramework.Pool;
using Mirror;
using UnityEngine;

namespace Mikrocosmos
{
    public class NetworkedGameObjectPool : GameObjectPool
    {
        [SerializeField]
        protected int initCount = 0;
        [SerializeField]
        protected int maxCount = 50;

        protected Queue<GameObject> destroyedObjectInQueue;

        private int numHiddenObjectCreating = 0;



        /// <summary>
        /// Number of Object Instantitated to the Object pool per frame at the initialization process of the object pool
        /// </summary>
        public float NumObjInitPerFrame = 10;

        /// <summary>
        /// Number of Object Destroyed from the Object pool per frame
        /// </summary>
        public float NumObjDestroyPerFrame = 2;

        public int MaxCount
        {
            get
            {
                return maxCount;
            }
            set
            {
                CheckInited();
                if (value < 0)
                {
                    Debug.LogError("MaxCount must be greater or equal to 0");
                }
                maxCount = value;

                if (maxCount < CurrentCount)
                {
                    int removedCount = CurrentCount - maxCount;
                    for (int i = 0; i < removedCount; i++)
                    {
                        GameObject popedObj = cachedStack.Pop();

                        if (!popedObj.activeInHierarchy)
                        {
                            destroyedObjectInQueue.Enqueue(popedObj);
                            destroyingObjs.Value = true;
                        }
                    }
                }
            }
        }

        private void Awake()
        {
            creatingObjs.RegisterOnValueChaned(value => {
                if (value)
                {
                    StartCoroutine(InitializeObjectsToGame());
                }
            }).UnRegisterWhenGameObjectDestroyed(this.gameObject);

            destroyingObjs.RegisterOnValueChaned(value => {
                if (value)
                {
                    StartCoroutine(DestoryObjectsInQueue());
                }
            }).UnRegisterWhenGameObjectDestroyed(this.gameObject);
        }

        /// <summary>
        /// Recycle and disactive the gameobject. 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Recycle(GameObject obj)
        {
            if (NetworkServer.active) {
                CheckInited();

                if (obj && pooledPrefab)
                {
                    if (obj.name != pooledPrefab.name)
                    {
                        Debug.LogError($"{obj.name} recycled to the wrong ObjectPool!");
                    }

                    if (obj == null || !obj.activeInHierarchy)
                    {
                        return false;
                    }

                    if (CurrentCount < maxCount)
                    {
                        obj.transform.position = Vector3.zero;
                        obj.transform.SetParent(transform);
                        cachedStack.Push(obj);
                        obj.SetActive(false);
                        obj.GetComponent<PoolableGameObject>().OnRecycled();
                        NetworkServer.UnSpawn(obj);
                        
                      
                       
                        return true;
                    }
                    else
                    {
                        Debug.Log($"The SafeGameObjectPool {pooledPrefab.name} is full! The Object will not return to the pool");
                        NetworkServer.UnSpawn(obj);
                        obj.SetActive(false);
                        destroyedObjectInQueue.Enqueue(obj);
                        destroyingObjs.Value = true;
                        obj.GetComponent<PoolableGameObject>().OnRecycled();
                        return false;
                    }
                }
                else
                {
                    Debug.LogWarning("A Prefab is missing but the SafeGameObjectPool is still trying to access it!");
                    return false;
                }
            }

            return false;

        }

        /// <summary>
        /// Allocate a gameobject from the pool and set active
        /// </summary>
        /// <returns></returns>
        public override GameObject Allocate()
        {
            if (NetworkServer.active) {
                CheckInited();
                GameObject createdObj = base.Allocate();
               
                if (createdObj) {
                   // createdObj.transform.SetParent(this.transform);
                    createdObj.SetActive(true);
                    createdObj.GetComponent<PoolableGameObject>().Pool = this;
                    createdObj.transform.parent = null;
                    //NetworkServer.Spawn(createdObj);
                    createdObj.GetComponent<PoolableGameObject>().OnInit();
                }
                else {
                    Debug.LogWarning("A Prefab is missing but the SafeGameObjectPool is still trying to access it!");
                }

                return createdObj;
            }

            return null;
        }

        protected override void Init(GameObject pooledPrefab)
        {
            destroyedObjectInQueue = new Queue<GameObject>(maxCount);
            Init(pooledPrefab, 0, 50);
        }


        /// <summary>
        /// Initialize the initial count and max count of the pool. 
        /// </summary>
        /// <param name="initCount"></param>
        /// <param name="maxCount"></param>
        public NetworkedGameObjectPool Init(int initCount = 0, int maxCount = 50)
        {
            destroyedObjectInQueue = new Queue<GameObject>(maxCount);
            Init(pooledPrefab, initCount, maxCount);
            return this;
        }

        

        /// <summary>
        /// Create a new SafeGameObject Pool.
        /// </summary>
        /// <param name="pooledPrefab"></param>
        /// <returns></returns>
        public static NetworkedGameObjectPool Create(GameObject pooledPrefab)
        {
            if (NetworkServer.active) {
                NetworkedGameObjectPool pool = Create<NetworkedGameObjectPool>(pooledPrefab);
                //NetworkServer.Spawn(pool.gameObject);
                return pool;
            }

            return null;

        }

        protected void Init(GameObject pooledPrefab, int initCount = 0, int maxCount = 50)
        {
            cachedStack.Clear();
            destroyedObjectInQueue.Clear();
            numHiddenObjectCreating = 0;


            if (pooledPrefab.GetComponent<PoolableGameObject>() == null)
            {
                Debug.LogError("Pooled Prefab must have a component that inherited from PoolableGameObject!");
            }

            this.gameObject.name = $"Object Pool: {pooledPrefab.name}";
            this.pooledPrefab = pooledPrefab;
            this.initCount = initCount;
            this.maxCount = maxCount;

            prefabFactory = new DefaultGameObjectFactory(pooledPrefab);

            if (initCount > maxCount)
            {
                Debug.LogError("Initial count of the SafeObjectPool can't be bigger than the maxCount");
            }

            if (initCount < 0 || maxCount < 0)
            {
                Debug.LogError("Initial count or Max Count must be greater or equal to 0");
            }

            this.maxCount = maxCount;

            if (CurrentCount < initCount)
            {
                numHiddenObjectCreating += initCount - CurrentCount;
                poolState = GameObjectPoolState.Initializing;
                creatingObjs.Value = true;
            }
            else
            {
                poolState = GameObjectPoolState.Inited;
            }


        }


        private BindableProperty<bool> creatingObjs = new BindableProperty<bool>() { Value = false };
        private BindableProperty<bool> destroyingObjs = new BindableProperty<bool>() { Value = false };


        IEnumerator DestoryObjectsInQueue()
        {
            while (destroyedObjectInQueue.Count > 0)
            {
                for (int i = 0; i < NumObjDestroyPerFrame; i++)
                {
                    if (destroyedObjectInQueue.Count > 0)
                    {
                        GameObject obj = destroyedObjectInQueue.Dequeue().gameObject;
                        NetworkServer.Destroy(obj);
                    }
                }

                yield return null;
            }

            destroyingObjs.Value = false;
        }

        IEnumerator InitializeObjectsToGame()
        {
            while (numHiddenObjectCreating > 0)
            {
                for (int i = 0; i < NumObjInitPerFrame; i++)
                {
                    if (numHiddenObjectCreating > 0)
                    {
                        GameObject createdObject = base.Allocate();
                        createdObject.SetActive(false);
                        createdObject.transform.SetParent(this.transform);
                        cachedStack.Push(createdObject);
                        numHiddenObjectCreating--;
                     //   NetworkServer.Spawn(createdObject);
                        createdObject.GetComponent<PoolableGameObject>().OnInit();
                    }
                }

                yield return null;
            }

            creatingObjs.Value = false;
            poolState = GameObjectPoolState.Inited;
        }

        private void CheckInited()
        {
            if (poolState == GameObjectPoolState.NotInited)
            {
                Debug.LogError("The SafeGameObject Pool hasn't been inited yet. Use Init() before" +
                               "calling any functions");
            }
        }


        //merge sort an array of integers
        private void MergeSort(int[] array, int left, int right)
        {
            if (left < right)
            {
                int middle = (left + right) / 2;

                MergeSort(array, left, middle);
                MergeSort(array, middle + 1, right);

                Merge(array, left, middle, right);
            }
        }

        private void Merge(int[] array, int left, int middle, int right) {
            int[] temp = new int[array.Length];
            int i, left_end, num_elements, tmp_pos;

            left_end = middle;
            tmp_pos = left;
            num_elements = right - left + 1;

            while ((left <= left_end) && (middle <= right))
            {
                if (array[left] <= array[middle])
                    temp[tmp_pos++] = array[left++];
                else
                    temp[tmp_pos++] = array[middle++];
            }

            while (left <= left_end)
                temp[tmp_pos++] = array[left++];

            while (middle <= right)
                temp[tmp_pos++] = array[middle++];

            for (i = 0; i < num_elements; i++)
            {
                array[right] = temp[right];
                right--;
            }
        }
    }
}

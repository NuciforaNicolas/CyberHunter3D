using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager instance;

        [SerializeField] int maxTentatives;

        private void Awake()
        {
            instance = this;
        }

        public Pool GenerateObjectPool(string name, GameObject prefab, int size)
        {
            Pool newPool = new Pool();
            newPool.mName = name;
            newPool.mPrefab = prefab;
            newPool.mSize = size;
            newPool.mLastIndex = 0;
            newPool.mList = new List<GameObject>();
            for(int i = 0; i < size; i++)
            {
                GameObject go = Instantiate<GameObject>(prefab, Vector3.zero, Quaternion.identity);
                go.SetActive(false);
                newPool.mList.Add(go);
            }
            return newPool;
        }

        public GameObject GetObjectFromPool(Pool pool)
        {
            var objectFound = false;
            var index = 0;
            while(!objectFound)
            {
                for (index = pool.mLastIndex; index < pool.mList.Count; index++)
                {
                    if (!pool.mList[index].activeInHierarchy)
                    {
                        pool.mLastIndex = index + 1;
                        objectFound = true;
                        break;
                    }
                    if(index == (pool.mList.Count - 1))
                    {
                        pool.mLastIndex = 0;
                    }
                }
                if(pool.mLastIndex == pool.mList.Count)
                {
                    pool.mLastIndex = 0;
                }
            }
            return pool.mList[index];
        }
    }
}
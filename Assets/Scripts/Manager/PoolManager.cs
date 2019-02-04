using UnityEngine;
using System.Collections.Generic;

public class PoolObject : MonoBehaviour
{
    public Object prefab;

    private string _className = null;
    public string className
    {
        get
        {
            if (string.IsNullOrEmpty(_className))
                _className = this.GetType().Name;
            return _className;
        }
    }

    private GameObject _mObject = null;
    public GameObject mObject
    {
        get
        {
            if (_mObject == null)
                _mObject = gameObject;
            return _mObject;
        }
    }

    private Transform _mTrans = null;
    public Transform mTrans
    {
        get
        {
            if (_mTrans == null)
                _mTrans = transform;
            return _mTrans;
        }
    }

    public T GetObject<T>() where T : PoolObject
    {
        return this as T;
    }
}

public class PoolManager : Singleton<PoolManager>
{
    [SerializeField]
    private List<PoolObject> poolList;
    private Dictionary<string, Stack<PoolObject>> inactivePool;

    private Transform inactiveObjectsRoot;

    protected override void Awake()
    {
        if (instance == null)
        {
            instance = this;
            instance.Init();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected override void Init()
    {
        base.Init();

        inactivePool = new Dictionary<string, Stack<PoolObject>>();

        GameObject child = new GameObject("InactiveObjects");
        child.SetActive(false);
        inactiveObjectsRoot = child.transform;
        inactiveObjectsRoot.parent = transform;
    }

    protected override void OnEnable()
    {
    }

    public static T GetObject<T>() where T : PoolObject
    {
        return GetInstance().GetPoolObject<T>();
    }

    private T GetPoolObject<T>() where T : PoolObject
    {
        string name = typeof(T).Name;
        T returnT = null;

        if (inactivePool.ContainsKey(name) && inactivePool[name].Count > 0)
        {
            returnT = inactivePool[name].Pop().GetObject<T>();
            returnT.mObject.SetActive(true);
            return returnT;
        }

        Object prefab = null;
        for (int i = 0, max = poolList == null ? 0 : poolList.Count; i < max; i++)
        {
            if (poolList[i].className.Equals(name))
                prefab = poolList[i].prefab;
        }

        if (prefab != null)
        {
            GameObject obj = Instantiate(prefab) as GameObject;
            obj.name = prefab.name;
            obj.SetActive(true);
            return obj.GetComponent<T>();
        }
        return null;
    }

    public static void ReturnObject<T>(T obj) where T : PoolObject
    {
        GetInstance().ReturnPoolObject<T>(obj);
    }

    private void ReturnPoolObject<T>(T obj) where T : PoolObject
    {
        if (obj == null)
            return;

        obj.mObject.SetActive(false);

        string name = typeof(T).Name;

        obj.mTrans.parent = inactiveObjectsRoot;
        obj.mTrans.localPosition = Vector3.zero;
        obj.mTrans.localScale = Vector3.one;

        if (!inactivePool.ContainsKey(name))
        {
            inactivePool.Add(name, new Stack<PoolObject>());
        }
        inactivePool[name].Push(obj);
    }

    public static int GetPoolObjectCount<T>() where T : PoolObject
    {
        string name = typeof(T).Name;
        if (GetInstance().inactivePool.ContainsKey(name))
        {
            return GetInstance().inactivePool[name].Count;
        }
        return 0;
    }

    public static void WarmUp<T>(int count) where T : PoolObject
    {
        GetInstance().WarmUpObjects<T>(count);
    }

    private void WarmUpObjects<T>(int count) where T : PoolObject
    {
        string name = typeof(T).Name;
        int nowCount = GetPoolObjectCount<T>();
        Object prefab = null;

        if (!inactivePool.ContainsKey(name))
        {
            inactivePool.Add(name, new Stack<PoolObject>());
        }

        for (int i = 0, max = poolList == null ? 0 : poolList.Count; i < max; i++)
        {
            if (poolList[i].className.Equals(name))
                prefab = poolList[i].prefab;
        }

        for (int i = 0, max = count - nowCount; i < max; i++)
        {
            GameObject body = Instantiate(prefab) as GameObject;
            body.name = prefab.name;
            body.SetActive(true);

            T obj = body.GetComponent<T>();
            obj.mTrans.parent = inactiveObjectsRoot;
            obj.mTrans.localPosition = Vector3.zero;
            obj.mTrans.localScale = Vector3.one;

            inactivePool[name].Push(obj);
        }
    }
}

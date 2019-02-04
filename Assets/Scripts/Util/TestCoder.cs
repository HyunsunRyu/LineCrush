using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestCoder : MonoBehaviour
{
    [System.Serializable]
    public class TestCodeData
    {
        public int keyValue;
        public KeyCode key;
        public System.Action callback;
    }
    [SerializeField] private List<TestCodeData> testCodeCallbackList;
    private Dictionary<int, TestCodeData> callbackDic = new Dictionary<int, TestCodeData>();

    private static TestCoder instance = null;
    private static TestCoder Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TestCoder>();
                if(instance != null)
                    instance.Init();
            }   
            if (instance == null)
            {
                GameObject obj = new GameObject("TestCoder");
                instance = obj.AddComponent<TestCoder>();
                instance.Init();
            }
            return instance;
        }
    }

    private void Init()
    {
        callbackDic.Clear();
#if !UNITY_EDITOR
        return;
#else
        DontDestroyOnLoad(gameObject);
        testCodeCallbackList = new List<TestCodeData>();
        StartCoroutine(CheckInput());
#endif
    }

    public static void SetTestCode(KeyCode key, System.Action callback)
    {
#if !UNITY_EDITOR
        return;
#else
        int keyValue = callback.GetHashCode();

        if (!Instance.callbackDic.ContainsKey(keyValue))
        {
            TestCodeData data = new TestCodeData();
            data.keyValue = callback.GetHashCode();
            data.key = key;
            data.callback = callback;

            Instance.callbackDic.Add(keyValue, data);
            Instance.testCodeCallbackList.Add(data);
        }
        else
        {
            Instance.callbackDic[keyValue].key = key;
            instance.callbackDic[keyValue].callback = callback;
        }
#endif
    }

    public static void RemoveTestCode(System.Action method)
    {
#if !UNITY_EDITOR
        return;
#else
        int keyValue = method.GetHashCode();
        if (Instance.callbackDic.ContainsKey(keyValue))
        {
            Instance.testCodeCallbackList.Remove(Instance.callbackDic[keyValue]);
            Instance.callbackDic.Remove(keyValue);
        }
        else
        {
            Debug.Log("ah?");
        }
#endif
    }

    public static bool IsAddedTestCode(System.Action method)
    {
        int key = method.GetHashCode();
        return Instance.callbackDic.ContainsKey(key);
    }

    private IEnumerator CheckInput()
    {
        while (true)
        {
            for (int i = 0, max = testCodeCallbackList.Count; i < max; i++)
            {
                if (Input.GetKeyDown(testCodeCallbackList[i].key) && testCodeCallbackList[i].callback != null)
                {
                    testCodeCallbackList[i].callback();
                }
            }
            yield return null;
        }
    }
}

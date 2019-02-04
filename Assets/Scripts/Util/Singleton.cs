using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    protected static T instance = null;

    public static T GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<T>();
            if (instance == null)
            {
                GameObject singleton = new GameObject(typeof(T).Name);
                instance = singleton.AddComponent<T>();
            }
            else
                instance.Init();
        }
        return instance;
    }
    
    protected abstract void Awake();
    protected abstract void OnEnable();

    protected virtual void Init()
    {
        DontDestroyOnLoad(gameObject);
    }
}

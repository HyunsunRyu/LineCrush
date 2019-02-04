using UnityEngine;
using System.Collections;

public class ScreenSizeGetter : MonoBehaviour
{
    private static ScreenSizeGetter instance = null;

    [SerializeField] private Vector2 screenBaseSize = new Vector2(1080f, 1920f);
    
    private int screenWidth, screenHeight;
    private float screenHalfWidth, screenHalfHeight;
    private float screenRate, invScreenRate;

    private static void MakeInstance()
    {
        instance = FindObjectOfType(typeof(ScreenSizeGetter)) as ScreenSizeGetter;
        if (instance == null)
        {
            GameObject obj = new GameObject("ScreenSizeGetter");
            instance = obj.AddComponent<ScreenSizeGetter>();
        }
        instance.Init();
    }

    public static int width
    {
        get
        {
            if (instance == null)
                MakeInstance();
            return instance.screenWidth;
        }
    }

    public static int height
    {
        get
        {
            if (instance == null)
                MakeInstance();
            return instance.screenHeight;
        }
    }

    public static float halfWidth
    {
        get
        {
            if (instance == null)
                MakeInstance();
            return instance.screenHalfWidth;
        }
    }

    public static float halfHeight
    {
        get
        {
            if (instance == null)
                MakeInstance();
            return instance.screenHalfHeight;
        }
    }

    public static float rate
    {
        get
        {
            if (instance == null)
                MakeInstance();
            return instance.screenRate;
        }
    }

    public static float invRate
    {
        get
        {
            if (instance == null)
                MakeInstance();
            return instance.invScreenRate;
        }
    }

    public static Vector2 GetBaseScreenSize()
    {
        if (instance == null)
            MakeInstance();
        return instance.screenBaseSize;
    }

    private void Awake()
    {
        if (instance == null)
            Init();
        else if (instance == this)
            return;
        else
        {
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        instance = this;

        float rateValue = (screenBaseSize.x * (float)Screen.height) / (screenBaseSize.y * (float)Screen.width);
        
        if (rateValue > 1f)  //비율이 screenBaseSize보다 세로로 더 김. fix width. //
        {
            screenWidth = Mathf.RoundToInt(screenBaseSize.x);
            screenHeight = Mathf.RoundToInt((float)Screen.height * screenWidth / (float)Screen.width);
            screenRate = (float)Screen.width / screenWidth;
        }
        else if (rateValue < 1f) //비율이 screenBaseSize보다 가로로 더 김. fix height//
        {
            screenHeight = Mathf.RoundToInt(screenBaseSize.y);
            screenWidth = Mathf.RoundToInt((float)Screen.width * screenHeight / (float)Screen.height);
            screenRate = (float)Screen.height / screenHeight;
        }
        else
        {
            screenWidth = Mathf.RoundToInt(screenBaseSize.x);
            screenHeight = Mathf.RoundToInt(screenBaseSize.y);
            screenRate = (float)Screen.width / screenWidth;
        }

        invScreenRate = 1f / screenRate;
        screenHalfWidth = screenWidth * 0.5f;
        screenHalfHeight = screenHeight * 0.5f;
        DontDestroyOnLoad(gameObject);
    }
}

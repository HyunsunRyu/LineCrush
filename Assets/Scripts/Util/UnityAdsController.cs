using UnityEngine;
using UnityEngine.Advertisements;
using CodeStage.AntiCheat.ObscuredTypes;

public class UnityAdsController : MonoBehaviour
{
    private static UnityAdsController instance = null;
    private ObscuredInt gameID;

    private static System.Action _handleFinished;
    private static System.Action _handleSkipped;
    private static System.Action _handleFailed;
    private static System.Action _onContinue;

    private static UnityAdsController GetInstance()
    {
        if (instance == null)
            instance = FindObjectOfType<UnityAdsController>();
        if (instance == null)
        {
            GameObject obj = new GameObject(typeof(UnityAdsController).Name);
            instance = obj.AddComponent<UnityAdsController>();
        }
        return instance;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            Init();
        }
        else
            Destroy(gameObject);
    }

    private void Init()
    {
#if UNITY_ANDROID
        gameID = DataManager.GetDesignValue(Define.GameDesign.AndroidAdsID);
#else
        gameID = DataManager.GetDesignValue(Define.GameDesign.IOSAdsID);
#endif
        if (Advertisement.isSupported && !Advertisement.isInitialized)
            Advertisement.Initialize(gameID.ToString(), false);
    }

    public static bool isShowing { get { return Advertisement.isShowing; } }
    public static bool isSupported { get { return Advertisement.isSupported; } }
    public static bool isInitialized { get { return Advertisement.isInitialized; } }

    public static bool IsReady()
    {
        return IsReady(null);
    }
    public static bool IsReady(string zoneID)
    {
        if (string.IsNullOrEmpty(zoneID)) zoneID = null;

        return Advertisement.IsReady(zoneID);
    }

    public static void ShowAd()
    {
        ShowAd(null, null, null, null, null);
    }

    public static void ShowAd(string zoneID)
    {
        ShowAd(zoneID, null, null, null, null);
    }

    public static void ShowAd(string zoneID, System.Action handleFinished)
    {
        ShowAd(zoneID, handleFinished, null, null, null);
    }

    public static void ShowAd(string zoneID, System.Action handleFinished, System.Action handleSkipped)
    {
        ShowAd(zoneID, handleFinished, handleSkipped, null, null);
    }

    public static void ShowAd(string zoneID, System.Action handleFinished, System.Action handleSkipped, System.Action handleFailed)
    {
        ShowAd(zoneID, handleFinished, handleSkipped, handleFailed, null);
    }

    public static void ShowAd(string zoneID, System.Action handleFinished, System.Action handleSkipped, System.Action handleFailed, System.Action onContinue)
    {
        if (string.IsNullOrEmpty(zoneID))
            zoneID = null;

        _handleFinished = handleFinished;
        _handleSkipped = handleSkipped;
        _handleFailed = handleFailed;
        _onContinue = onContinue;

        if (Advertisement.IsReady(zoneID))
        {
            Debug.Log("Showing ad now...");

            ShowOptions options = new ShowOptions();
            options.resultCallback = HandleShowResult;
            //options.pause = true;

            Advertisement.Show(zoneID, options);
        }
        else
        {
            Debug.LogWarning(string.Format("Unable to show ad. The ad placement zone {0} is not ready.",
                                           object.ReferenceEquals(zoneID, null) ? "default" : zoneID));
        }
    }

    private static void HandleShowResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                Debug.Log("The ad was successfully shown.");
                if (!object.ReferenceEquals(_handleFinished, null)) _handleFinished();
                break;
            case ShowResult.Skipped:
                Debug.LogWarning("The ad was skipped before reaching the end.");
                if (!object.ReferenceEquals(_handleSkipped, null)) _handleSkipped();
                break;
            case ShowResult.Failed:
                Debug.LogError("The ad failed to be shown.");
                if (!object.ReferenceEquals(_handleFailed, null)) _handleFailed();
                break;
        }

        if (!object.ReferenceEquals(_onContinue, null)) _onContinue();
    }
}

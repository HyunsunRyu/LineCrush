using UnityEngine;
using System.Collections;

public class TipController : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private UILabel tipText;
    [SerializeField] private UILabel loadingText;
    [SerializeField] private UISprite characterImage;

    private static TipController instance = null;

    private const float minShowTime = 2f;

    private float showTime = 0f;
    private System.Action hideCallback = null;

    public static bool isShowing { get; private set; }

    private static TipController GetInstance()
    {
        if (instance == null)
            instance = FindObjectOfType<TipController>();
        return instance;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            InitData();
        }
        else
            Destroy(gameObject);
    }

    private void InitData()
    {
        Hide();
    }

    public static void ShowRandomTip()
    {
        if (GetInstance() == null)
            return;

        GetInstance().hideCallback = null;
        GetInstance().showTime = minShowTime;
        GetInstance().StartCoroutine(GetInstance().CheckTime());
        GetInstance().Show();
    }

    public static void HideTip(System.Action afterHideCallback)
    {
        if (GetInstance() == null)
            afterHideCallback();

        afterHideCallback += GetInstance().Hide;

        if (GetInstance().showTime > 0f)
            GetInstance().hideCallback = afterHideCallback;
        else
            afterHideCallback();
    }

    private void Show()
    {
        root.SetActive(true);
        SetRandomTip();
        isShowing = true;
    }

    private void Hide()
    {
        root.SetActive(false);
        isShowing = false;
    }

    private void SetRandomTip()
    {
        TipTable tip = DataManager.GetInstance().GetRandomTipTable();
        tipText.text = tip.GetTipText(); 
        loadingText.text = DataManager.GetText(TextTable.loadingKey);
    }

    private IEnumerator CheckTime()
    {
        bool bWork = true;
        while (bWork)
        {
            showTime -= Time.deltaTime;
            if (showTime <= 0f)
            {
                showTime = 0f;
                bWork = false;

                if (hideCallback != null)
                    hideCallback();
            }
            yield return null;
        }
    }
}

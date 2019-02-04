using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : Singleton<TutorialManager>
{
    [System.Serializable]
    private class TutorialUI
    {
        public Define.TutorialType tutorial = Define.TutorialType.None;
        public string method = "";
        public GameObject icon = null;
    }

    [SerializeField] private List<TutorialBase> tutorialList;
    [SerializeField] private GameObject root = null;
    [SerializeField] private TutorialMessageBoxController messageBox;
    [SerializeField] private TutorialBlocksController blocker;
    [SerializeField] private TutorialEmphasisController emphasis;
    [SerializeField] private TouchFingerController finger;
    [SerializeField] private List<TutorialUI> tutorialUI;

    private const float delayTime = 1f;

    private int nowCount = 0;
    private bool bReaction = false;
    private GameObject nowTutorialUI = null;
    private System.Action afterEndTutorialCallback = null;

    private Dictionary<Define.TutorialType, TutorialBase> tutorialDic;
    private Dictionary<Define.TutorialType, bool> clearTutorial;

    public Define.TutorialType nowTutorial { get; private set; }
    public int tutorialValue { get; private set; }

    public readonly int tutorialCount = System.Enum.GetValues(typeof(Define.TutorialType)).Length - 1;

    private const float topHeight = 300f;
    private const float bottomHeight = -630f;

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

        root.SetActive(false);

        tutorialDic = new Dictionary<Define.TutorialType, TutorialBase>();
        clearTutorial = new Dictionary<Define.TutorialType, bool>();

        for (int i = 0, max = tutorialList.Count; i < max; i++)
        {
            Define.TutorialType type = tutorialList[i].GetTutorialType();
            if (!tutorialDic.ContainsKey(type))
            {
                tutorialDic.Add(type, tutorialList[i]);
                tutorialList[i].Init();
            }
        }

        for (int i = 0, max = tutorialUI.Count; i < max; i++)
            tutorialUI[i].icon.SetActive(false);

        messageBox.Init();
        emphasis.Init();
        blocker.Init();
        finger.Init();
    }

    public void SetTutorialClearData(Define.TutorialType type, bool bClear)
    {
        if (!clearTutorial.ContainsKey(type))
            clearTutorial.Add(type, bClear);
        else
            clearTutorial[type] = bClear;
    }

    public bool IsCleared(Define.TutorialType type)
    {
        if (clearTutorial.ContainsKey(type))
            return clearTutorial[type];
        return false;
    }

    private TutorialBase GetTutorial(Define.TutorialType type)
    {
        if (tutorialDic.ContainsKey(type))
            return tutorialDic[type];
        return null;
    }

    public void SetTutorialValue(int value)
    {
        tutorialValue = value;
    }

    protected override void OnEnable()
    {
        nowTutorial = Define.TutorialType.None;
        nowTutorialUI = null;
    }

    public void StartTutorial(Define.TutorialType type, int nowCount, System.Action afterEndTutorial)
    {
        afterEndTutorialCallback = afterEndTutorial;

        this.nowCount = nowCount;
        nowTutorial = type;
        bReaction = false;
        root.SetActive(true);

        blocker.SetAllBlock(true);
        emphasis.SetAllBlind(true);
        NextTutorial();

        SetTutorialClearData(nowTutorial, true);
        DataManager.GetInstance().SaveAllData();
    }

    public void StartTutorial(Define.TutorialType type, System.Action afterEndTutorial)
    {
        StartTutorial(type, 0, afterEndTutorial);
    }

    public void NextTutorial()
    {
        HideTutorialUI();

        TutorialTable table = DataManager.GetInstance().GetTutorial(nowTutorial, nowCount);
        if (table == null)
        {
            messageBox.HideMessageBox(() =>
            {
                EndTutorial();
            });
        }
        else
        {
            messageBox.SetMessageBox(table.textIdx, table.targetPos, () =>
            {
                blocker.SetAllBlock(true);
                switch (table.emphasis)
                {
                    case Define.TutorialEmphasis.NoEmphasis:
                        emphasis.SetAllBlind(true);
                        break;
                    case Define.TutorialEmphasis.JustEmphasis:
                        emphasis.SetEmphasis(table.targetPos, table.targetSize);
                        break;
                    case Define.TutorialEmphasis.EmphasisTarget:
                        emphasis.SetEmphasis(table.targetPos, table.targetSize);
                        break;
                }

                TutorialBase tutorial = GetTutorial(table.tutorialType);
                if (tutorial != null)
                    tutorial.CallMethod(table.method);

                StartCoroutine(DelayFunc(delayTime, () =>
                {
                    Vector3 pos = new Vector3(-370f, 0f, 0f);
                    if (table.targetPos.y >= 0f)
                        pos.y = bottomHeight;
                    else
                        pos.y = topHeight;

                    switch (table.emphasis)
                    {
                        case Define.TutorialEmphasis.NoEmphasis:
                            blocker.SetTouchTarget();
                            break;
                        case Define.TutorialEmphasis.JustEmphasis:
                            blocker.SetTouchTarget();
                            break;
                        case Define.TutorialEmphasis.EmphasisTarget:
                            blocker.SetEmphasis(table.blockPos, table.blockSize);
                            pos = table.blockPos;
                            break;
                    }

                    nowCount++;
                    SetReaction(pos);
                }));
            });
        }
    }

    private IEnumerator DelayFunc(float time, System.Action callback)
    {
        yield return new WaitForSeconds(time);
        if (callback != null)
            callback();
    }

    private void EndTutorial()
    {
        nowTutorial = Define.TutorialType.None;
        nowCount = 0;
        bReaction = false;
        root.SetActive(false);

        emphasis.SetAllBlind(false);
        blocker.SetAllBlock(false);
        
        if (afterEndTutorialCallback != null)
            afterEndTutorialCallback();

        DataManager.GetInstance().SaveAllData();
    }

    private void SetReaction(Vector3 pos)
    {
        finger.Active(pos);
        bReaction = true;
    }

    public void CallReaction()
    {
        if (!bReaction)
            return;

        finger.Inactive();
        bReaction = false;
        NextTutorial();
    }

    public void ShowTutorialUI(Define.TutorialType tutorial, string method)
    {
        HideTutorialUI();

        if (string.IsNullOrEmpty(method))
            return;

        for (int i = 0, max = tutorialUI.Count; i < max; i++)
        {
            if (tutorialUI[i].tutorial == tutorial && string.Equals(tutorialUI[i].method, method))
            {
                tutorialUI[i].icon.SetActive(true);
                nowTutorialUI = tutorialUI[i].icon;
                return;
            }
        }
    }

    public GameObject GetTutorialUI(Define.TutorialType tutorial, string methodName)
    {
        for (int i = 0, max = tutorialUI.Count; i < max; i++)
        {
            if (tutorialUI[i].tutorial == tutorial && string.Equals(tutorialUI[i].method, methodName))
            {
                return tutorialUI[i].icon;
            }
        }
        return null;
    }

    public void HideTutorialUI()
    {
        if (nowTutorialUI != null)
            nowTutorialUI.SetActive(false);
        nowTutorialUI = null;
    }
}

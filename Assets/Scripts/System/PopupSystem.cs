using UnityEngine;
using System.Collections.Generic;

public class PopupSystem : ISystem
{
    [SerializeField] private GameObject popupBlinder;
    [SerializeField] private AnimationCurve openCurve;
    [SerializeField] private AnimationCurve closeCurve;
    [SerializeField] protected List<IPopup> popupList;

    protected Dictionary<Define.PopupType, IPopup> popupDic = new Dictionary<Define.PopupType, IPopup>();

    private static PopupSystem instance;
    private static Stack<Define.PopupType> openPopupStack = new Stack<Define.PopupType>();
    private static Define.PopupType nowOpenPopup = Define.PopupType.None;

    public override void InitSystem()
    {
        openPopupStack.Clear();
        openPopupStack.Push(Define.PopupType.None);
        nowOpenPopup = Define.PopupType.None;
        popupBlinder.SetActive(false);

        instance = this;

        popupDic.Clear();
        for (int i = 0, max = popupList.Count; i < max; i++)
        {
            Define.PopupType type = popupList[i].GetPopupType();
            if (!popupDic.ContainsKey(type))
            {
                popupDic.Add(type, popupList[i]);
                popupList[i].Init();
            }
            else
                Debug.Log("something is wrong");
        }
    }

    public override void ClearSystem()
    {
    }

    public override void InitData()
    {
    }

    public static void OpenPopup(Define.PopupType type, System.Action callback = null)
    {
        if (instance == null)
            return;
        if (instance.popupDic.ContainsKey(type))
        {
            if (nowOpenPopup != Define.PopupType.None)
            {
                instance.JustClosePopup(nowOpenPopup, () =>
                {
                    instance.JustOpenPopup(type, () =>
                    {
                        if (callback != null)
                            callback();
                        openPopupStack.Push(type);
                        nowOpenPopup = type;
                    });
                });
            }
            else
            {
                instance.JustOpenPopup(type, () =>
                {
                    if (callback != null)
                        callback();
                    openPopupStack.Push(type);
                    nowOpenPopup = type;
                });
            }
        }
    }

    private void JustOpenPopup(Define.PopupType popupType, System.Action callback)
    {
        if (popupDic.ContainsKey(popupType))
        {
            popupBlinder.SetActive(true);
            popupDic[popupType].OpenPopup(() =>
            {
                if (callback != null)
                    callback();
            });
        }
    }

    public static void ClosePopup(Define.PopupType type, System.Action callback = null)
    {
        if (instance == null)
            return;
        if (instance.popupDic.ContainsKey(type))
        {
            instance.JustClosePopup(type, () =>
            {
                openPopupStack.Pop();
                nowOpenPopup = openPopupStack.Peek();

                if (nowOpenPopup != Define.PopupType.None)
                    instance.JustOpenPopup(nowOpenPopup, null);
                else
                    instance.popupBlinder.SetActive(false);

                if (callback != null)
                    callback();
            });
        }
    }

    private void JustClosePopup(Define.PopupType popupType, System.Action callback)
    {
        if (popupDic.ContainsKey(popupType))
        {
            popupDic[popupType].ClosePopup(() =>
            {
                if (callback != null)
                    callback();
            });
        }
    }

    public static void CloseAllPopup(System.Action callback = null)
    {
        if (instance == null)
            return;
        if (instance.popupDic.ContainsKey(nowOpenPopup) && nowOpenPopup != Define.PopupType.None)
        {
            instance.JustClosePopup(nowOpenPopup, () =>
            {
                openPopupStack.Clear();
                openPopupStack.Push(Define.PopupType.None);

                instance.popupBlinder.SetActive(false);

                if (callback != null)
                    callback();
            });
        }
    }

    public static void ClearPopupStack()
    {
        openPopupStack.Clear();
    }

    public static T GetPopup<T>(Define.PopupType type) where T : IPopup
    {
        if (instance == null)
            return null;
        if (instance.popupDic.ContainsKey(type))
        {
            return instance.popupDic[type] as T;
        }
        return null;
    }

    public static bool TryGetCharacterImageOnPopup(ref string image, ref string outline, int idx)
    {
        if (instance == null)
            return false;
        PetData data = DataManager.GetInstance().GetPetDataWithUnqIdx(idx);
        if (data != null)
        {
            image = data.GetOnGameImage();
            outline = data.GetPopupOutline();
            return true;
        }
        return false;
    }

    public static AnimationCurve GetOpenCurve()
    {
        if (instance == null)
            return null;
        return instance.openCurve;
    }

    public static AnimationCurve GetCloseCurve()
    {
        if (instance == null)
            return null;
        return instance.closeCurve;
    }

    public static bool IsOpened(Define.PopupType popup)
    {
        return nowOpenPopup == popup;
    }
}

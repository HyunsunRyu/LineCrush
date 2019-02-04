using UnityEngine;
using System.Collections;

public abstract class IPopup : IWindow
{
    [SerializeField] private Define.PopupType type;
    
    private const float animTime = 0.2f;
    
    public void OpenPopup(System.Action callback)
    {
        Open(callback);
    }

    public void ClosePopup(System.Action callback)
    {
        Close(callback);
    }

    public Define.PopupType GetPopupType() { return type; }

    protected override void UIOpenAnimation(System.Action callback)
    {
        Vector3 start = new Vector3(0f, 0f, 1f);
        Vector3 end = Vector3.one;

        AnimCurveController.Scale(PopupSystem.GetOpenCurve(), start, end, animTime, moveTarget, () =>
        {
            if (callback != null)
                callback();
        });
    }

    protected override void UICloseAnimation(System.Action callback)
    {
        Vector3 start = Vector3.one;
        Vector3 end = new Vector3(0f, 0f, 1f);

        AnimCurveController.Scale(PopupSystem.GetCloseCurve(), start, end, animTime, moveTarget, () =>
        {
            if (callback != null)
                callback();
        });
    }
}

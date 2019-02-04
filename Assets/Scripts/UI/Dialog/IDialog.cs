using UnityEngine;
using System.Collections;

public class IDialog : IWindow
{
    [SerializeField] private Define.DialogType type;
    
    protected const float dialogOpenMovingTime = 0.3f;
    protected const float dialogCloseMovingTime = 0.2f;

    public sealed override void Init()
    {
        base.Init();
    }

    public void OpenDialog(System.Action callback)
    {
        Open(callback);
    }

    public void CloseDialog(System.Action callback)
    {
        Close(callback);
    }

    public Define.DialogType GetDialogType() { return type; }

    protected override void UIOpenAnimation(System.Action callback)
    {
        Vector3 start = new Vector3(ScreenSizeGetter.width * -0.5f, 0f, 0f);
        Vector3 end = Vector3.zero;

        AnimCurveController.Move(UISystem.GetOpenDialogCurve(), start, end, dialogOpenMovingTime, moveTarget, ()=>
        {
            if (callback != null)
                callback();
        });
    }

    protected override void UICloseAnimation(System.Action callback)
    {
        Vector3 start = Vector3.zero;
        Vector3 end = new Vector3(ScreenSizeGetter.width * 0.5f, 0f, 0f);

        AnimCurveController.Move(UISystem.GetCloseDialogCurve(), start, end, dialogCloseMovingTime, moveTarget, () =>
        {
            if (callback != null)
                callback();
        });
    }

    public virtual bool HasUpdateNews()
    {
        return false;
    }
}

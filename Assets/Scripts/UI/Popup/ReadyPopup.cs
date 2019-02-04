using UnityEngine;
using System.Collections;

public class ReadyPopup : IPopup
{
    [SerializeField] private AnimationCurve scaleCurve;
    [SerializeField] private AnimationCurve rotationCurve;
    [SerializeField] private UILabel label;

    private readonly Vector3 startScale = Vector3.zero;
    private readonly Vector3 endScale = Vector3.one;
    private const float animTime = 1f;

    private const float startAngle = 20f;
    private const float endAngle = -20f;

    private System.Action callback = null;

    public override void BeforeOpen()
    {
        SoundManager.GetInstance().PlaySound(Define.SoundType.ReadyVoice);

        label.text = DataManager.GetText(TextTable.readyKey);
    }

    public void SetCallback(System.Action callback)
    {
        this.callback = callback;
    }
    
    protected override void UIOpenAnimation(System.Action callback)
    {
        AnimCurveController.Scale(scaleCurve, startScale, endScale, animTime, moveTarget, () =>
        {
            if (callback != null)
                callback();

            PopupSystem.ClosePopup(Define.PopupType.Ready, ()=>
            {
                GoPopup popup = PopupSystem.GetPopup<GoPopup>(Define.PopupType.Go);
                popup.SetData(this.callback);
                PopupSystem.OpenPopup(Define.PopupType.Go);
            });
        });

        AnimCurveController.Rotation(rotationCurve, Vector3.forward, startAngle, endAngle, animTime, moveTarget, null);
    }

    protected override void UICloseAnimation(System.Action callback)
    {
        if (callback != null)
            callback();
    }
}

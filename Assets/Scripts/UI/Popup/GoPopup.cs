using UnityEngine;
using System.Collections;

public class GoPopup : IPopup
{
    [SerializeField] private AnimationCurve scaleCurve;
    [SerializeField] private AnimationCurve rotationCurve;
    [SerializeField] private UILabel label;

    private Vector3 startScale = Vector3.zero;
    private Vector3 endScale = Vector3.one;
    private float animTime = 1f;

    private const float startAngle = 0f;
    private const float endAngle = 20f;

    private System.Action callback;

    public override void BeforeOpen()
    {
        if (Random.Range(0, 10) != 0)
            SoundManager.GetInstance().PlaySound(Define.SoundType.GoVoice);
        else
            SoundManager.GetInstance().PlaySound(Define.SoundType.GoFunny);

        label.text = DataManager.GetText(TextTable.goKey);
    }

    public void SetData(System.Action callback)
    {
        this.callback = callback;
    }

    protected override void UIOpenAnimation(System.Action callback)
    {
        AnimCurveController.Scale(scaleCurve, startScale, endScale, animTime, moveTarget, () =>
        {
            if (callback != null)
                callback();

            PopupSystem.ClosePopup(Define.PopupType.Go, this.callback);
        });

        AnimCurveController.Rotation(rotationCurve, Vector3.forward, startAngle, endAngle, animTime, moveTarget, null);
    }

    protected override void UICloseAnimation(System.Action callback)
    {
        if (callback != null)
            callback();
    }
}

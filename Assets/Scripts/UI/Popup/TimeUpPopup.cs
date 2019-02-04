using UnityEngine;

public class TimeUpPopup : IPopup
{
    [SerializeField] private AnimationCurve openMoveCurve;
    [SerializeField] private AnimationCurve closeMoveCurve;
    [SerializeField] private AnimationCurve openRotationCurve;
    [SerializeField] private UILabel label;

    private Vector3 openStartMove = Vector3.zero;
    private Vector3 openEndMove = Vector3.zero;
    private Vector3 closeStartMove = Vector3.zero;
    private Vector3 closeEndMove = Vector3.zero;

    private float openAnimTime = 1.5f;
    private float closeAnimTime = 0.5f;

    private const float openStartAngle = 0f;
    private const float openEndAngle = 15f;

    private System.Action closeCallback;

    private bool timeUp = false;

    public void SetData(bool bTimeUp, System.Action callback)
    {
        timeUp = bTimeUp;
        
        closeCallback = callback;
    }

    public override void BeforeOpen()
    {
        if (timeUp)
        {
            SoundManager.GetInstance().PlaySound(Define.SoundType.TimeUpVoice);
            label.text = DataManager.GetText(TextTable.timeupKey);
        }
        else
        {
            SoundManager.GetInstance().PlaySound(Define.SoundType.GameOver);
            label.text = DataManager.GetText(TextTable.gameoverKey);
        }
    }

    protected override void UIOpenAnimation(System.Action callback)
    {
        openStartMove.y = ScreenSizeGetter.height * 0.5f + 200f;
        AnimCurveController.Move(openMoveCurve, openStartMove, openEndMove, openAnimTime, moveTarget, () =>
        {
            if (callback != null)
                callback();

            PopupSystem.ClosePopup(Define.PopupType.TimeUp);
        });

        AnimCurveController.Rotation(openRotationCurve, Vector3.forward, openStartAngle, openEndAngle, openAnimTime, moveTarget, null);
    }

    protected override void UICloseAnimation(System.Action callback)
    {
        closeEndMove.x = -ScreenSizeGetter.width;

        AnimCurveController.Move(closeMoveCurve, closeStartMove, closeEndMove, closeAnimTime, moveTarget, () =>
        {
            if (callback != null)
                callback();

            if (closeCallback != null)
                closeCallback();
        });
    }
}

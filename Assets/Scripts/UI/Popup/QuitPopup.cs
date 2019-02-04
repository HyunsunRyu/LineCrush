using UnityEngine;
using System.Collections;

public class QuitPopup : IPopup
{
    [SerializeField] private UILabel message;

    private System.Action yesCallback, noCallback;

    public void SetCallback(System.Action yesCallback, System.Action noCallback)
    {
        this.yesCallback = yesCallback;
        this.noCallback = noCallback;
    }

    public override void BeforeOpen()
    {
        message.text = DataManager.GetText(TextTable.quitMsgKey);
    }

    public void OnYes()
    {
        yesCallback();
    }

    public void OnNo()
    {
        SoundManager.GetInstance().PlaySound(Define.SoundType.Click);
        PopupSystem.ClosePopup(Define.PopupType.Quit, noCallback);
    }
}

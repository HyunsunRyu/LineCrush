using UnityEngine;

public class BasicPopup : IPopup
{
    [SerializeField] private UILabel mainLabel;

    private System.Action callback;

    public void SetData(string message, System.Action callback = null)
    {
        mainLabel.text = message;

        this.callback = callback;
    }

    public void OnCloseBtn()
    {
        SoundManager.GetInstance().PlaySound(Define.SoundType.Click);
        PopupSystem.ClosePopup(Define.PopupType.Basic, callback);
    }
}

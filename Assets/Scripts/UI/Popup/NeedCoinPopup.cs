using UnityEngine;
using System.Collections;

public class NeedCoinPopup : IPopup
{
    [SerializeField] private UILabel mainLabel;
    [SerializeField] private UILabel coinLabel;
    [SerializeField] private UISprite coinIcon;
    [SerializeField] private Transform coinLabelBody;
    [SerializeField] private Transform coinIconBody;

    private System.Action yesCallback = null;

    public void SetData(int needCoin, System.Action yesCallback)
    {
        mainLabel.text = DataManager.GetText(TextTable.goShopPopupMsgKey);
        coinLabel.text = needCoin.ToString();

        float halfDis = coinLabel.width * 0.5f + 5;
        Vector3 pos = coinLabelBody.localPosition;
        pos.x = halfDis;
        coinLabelBody.localPosition = pos;

        pos = coinIconBody.localPosition;
        pos.x = -halfDis - (coinIcon.width * 0.5f);
        coinIconBody.localPosition = pos;

        this.yesCallback = yesCallback;
    }

    public void OnYes()
    {
        SoundManager.GetInstance().PlaySound(Define.SoundType.Click);
        PopupSystem.ClosePopup(Define.PopupType.NeedCoin, yesCallback);
    }

    public void OnNo()
    {
        SoundManager.GetInstance().PlaySound(Define.SoundType.Click);
        PopupSystem.ClosePopup(Define.PopupType.NeedCoin);
    }
}

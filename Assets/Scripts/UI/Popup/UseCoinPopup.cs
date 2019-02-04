using UnityEngine;
using System.Collections;

public class UseCoinPopup : IPopup
{
    [SerializeField] private UILabel mainLabel;
    [SerializeField] private UILabel coinLabel;
    [SerializeField] private UISprite characterImage;
    [SerializeField] private UISprite characterOutline;
    [SerializeField] private UISprite coinIcon;
    [SerializeField] private Transform coinLabelBody;
    [SerializeField] private Transform coinIconBody;

    private System.Action buyCallback = null;
    private int coin = 0;
    private const int baseCharacterIdx = 3;

    public void SetData(string msg, int coin, System.Action buyCallback)
    {
        SetData(msg, coin, baseCharacterIdx, buyCallback);
    }

    public void SetData(string msg, int coin, int petIdx, System.Action buyCallback)
    {
        this.coin = coin;
        this.buyCallback = buyCallback;

        mainLabel.text = msg;
        coinLabel.text = coin.ToString();
        Vector3 pos = coinLabelBody.localPosition;
        float halfDis = coinLabel.width * 0.5f + 5;
        pos.x = halfDis;
        coinLabelBody.localPosition = pos;

        pos = coinIconBody.localPosition;
        pos.x = -halfDis - (coinIcon.width * 0.5f);
        coinIconBody.localPosition = pos;

        string characterImageStr = "";
        string characterImageOutline = "";
        if (PopupSystem.TryGetCharacterImageOnPopup(ref characterImageStr, ref characterImageOutline, petIdx))
        {
            characterImage.spriteName = characterImageStr;
            characterOutline.spriteName = characterImageOutline;

            characterImage.MakePixelPerfect();
            characterOutline.MakePixelPerfect();
        }
    }

    public void OnBuy()
    {
        int needCoin = 0;
        if (DataManager.GetInstance().IsEnoughCoin(coin, ref needCoin))
        {
            DataManager.GetInstance().UseCoin(coin);

            SoundManager.GetInstance().PlaySound(Define.SoundType.Click);
            PopupSystem.ClosePopup(Define.PopupType.UseCoin);

            if (LobbyManager.activeAdoptGuideTutorial)
                TutorialManager.GetInstance().CallReaction();

            if (buyCallback != null)
                buyCallback();

            DataManager.GetInstance().SaveAllData();
        }
    }

    public void OnNo()
    {
        SoundManager.GetInstance().PlaySound(Define.SoundType.Click);
        PopupSystem.ClosePopup(Define.PopupType.UseCoin);
    }
}
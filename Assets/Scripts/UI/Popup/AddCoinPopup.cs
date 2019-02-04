using UnityEngine;
using System.Collections;

public class AddCoinPopup : IPopup
{
    [SerializeField] private UILabel mainLabel;
    [SerializeField] private UILabel coinLabel;
    [SerializeField] private Transform coinLabelBody;
    [SerializeField] private Transform coinIconBody;

    private const int baseCharacterIdx = 3;

    public void SetData(int coin, System.Action callback = null)
    {
        if (callback != null)
            callback();

        string msg = (coin == 1) ? DataManager.GetText(TextTable.earnCoinMsgSingularKey) :
            DataManager.GetText(TextTable.earnCoinMsgPluralKey);

        msg = string.Format(msg, coin);

        mainLabel.text = msg;
        coinLabel.text = coin.ToString();

        Vector3 pos = coinLabelBody.localPosition;
        pos.x = 22f - coinLabel.width;
        coinLabelBody.localPosition = pos;
        
        pos = coinIconBody.localPosition;
        pos.x = 18f - coinLabel.width;
        coinIconBody.localPosition = pos;

        DataManager.GetInstance().AddCoin(coin);
        DataManager.GetInstance().SaveAllData();

        UISystem.UpdateNews();
    }

    public void OnClosePopup()
    {
        SoundManager.GetInstance().PlaySound(Define.SoundType.Click);
        PopupSystem.ClosePopup(Define.PopupType.AddCoin);
    }
}

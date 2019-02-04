using UnityEngine;
using System.Collections.Generic;

public class ShopDialog : IDialog
{
    [System.Serializable]
    private class ShopItemPack
    {
        public Transform body = null;
        public UILabel coinLabel = null;
        public UILabel buttonLabel = null;
    }

    [SerializeField] private UILabel titleLabel;
    [SerializeField] private UILabel warningLabel;
    [SerializeField] private UILabel talkboxLabel;
    [SerializeField] private ShopItemPack[] shopItems;
    [SerializeField] private ShopItemPack freeItem;
    [SerializeField] private UISprite background;
    [SerializeField] private Transform title;
    [SerializeField] private Transform warningBody;

    public override void BeforeOpen()
    {
        Vector3 pos;

        titleLabel.text = DataManager.GetText(TextTable.shopTextKey);
        warningLabel.text = DataManager.GetText(TextTable.warningBuyKey);

        bool bFreeItem = DataManager.GetInstance().CanShowFreeCoinVideo();
        if(bFreeItem)
            bFreeItem = UnityAdsController.IsReady(Define.freeCoinId);

        for (int i = 0; i < 3; i++)
        {
            ShopItemPack item = shopItems[i];

            int value = 0;
            if (DataManager.GetInstance().TryGetProductValue(i, ref value))
            {
                item.coinLabel.text = string.Format(DataManager.GetText(TextTable.luckyCountKey), value);
                item.buttonLabel.text = DataManager.GetText(TextTable.buyTextKey);
            }

            pos = item.body.localPosition;
            pos.y = bFreeItem ? -350f : -270f;
            item.body.localPosition = pos;
        }

        freeItem.coinLabel.text = string.Format(DataManager.GetText(TextTable.freeCoinKey), DataManager.GetDesignValue(Define.GameDesign.FreeCoin));
        freeItem.body.gameObject.SetActive(bFreeItem);

        pos = title.localPosition;
        pos.y = bFreeItem ? 366f : 286f;
        title.localPosition = pos;

        background.height = bFreeItem ? 880 : 720;

        pos = warningBody.localPosition;
        pos.y = bFreeItem ? -400f : -320f;
        warningBody.localPosition = pos;

        int needCoin = DataManager.GetInstance().needCoinCount;
        if (needCoin > 0)
            talkboxLabel.text = string.Format(DataManager.GetText(TextTable.notEnoughCoinKey), needCoin);
        else if (bFreeItem)
            talkboxLabel.text = DataManager.GetText(TextTable.hasFreeCoinKey);
        else
            talkboxLabel.text = DataManager.GetText(TextTable.wellcomeShopKey);
    }

    public void OnCloseButton()
    {
        UISystem.CloseDialog(Define.DialogType.ShopDialog);
    }

    public void OnClickBuyButton1() { OnClickBuyButton(0); }
    public void OnClickBuyButton2() { OnClickBuyButton(1); }
    public void OnClickBuyButton3() { OnClickBuyButton(2); }

    private void OnClickBuyButton(int idx)
    {
        string productKey = "";
        if (DataManager.GetInstance().TryGetProductKey(idx, ref productKey))
        {
            InAppPurchaser.BuyProduct(productKey, (string key) =>
            {
                int value = 0;
                if (DataManager.GetInstance().TryGetProductValue(key, ref value))
                {
                    UISystem.UpdateNews();

                    AddCoinPopup popup = PopupSystem.GetPopup<AddCoinPopup>(Define.PopupType.AddCoin);
                    popup.SetData(value);
                    PopupSystem.OpenPopup(Define.PopupType.AddCoin, OnCloseButton);

                    UnityEngine.Analytics.Analytics.CustomEvent("buy product", new Dictionary<string, object>
                    {
                        {productKey, key}
                    });
                }
                else
                {
                    BasicPopup popup = PopupSystem.GetPopup<BasicPopup>(Define.PopupType.Basic);
                    popup.SetData(DataManager.GetText(TextTable.errorBuyProductKey));
                    PopupSystem.OpenPopup(Define.PopupType.Basic);

                    UnityEngine.Analytics.Analytics.CustomEvent("product error", new Dictionary<string, object>
                    {
                        {productKey, key}
                    });
                }
            });
        }
    }

    public void OnClickFreeVideo()
    {
        if (UnityAdsController.IsReady(Define.freeCoinId))
            UnityAdsController.ShowAd(Define.freeCoinId, CallbackSuccess, CallbackSkip, CallbackSkip);
        else
        {
            BasicPopup popup = PopupSystem.GetPopup<BasicPopup>(Define.PopupType.Basic);
            popup.SetData(DataManager.GetText(TextTable.errorVideKey));
            PopupSystem.OpenPopup(Define.PopupType.Basic);
        }
    }

    private void CallbackSuccess()
    {
        AddCoinPopup popup = PopupSystem.GetPopup<AddCoinPopup>(Define.PopupType.AddCoin);
        popup.SetData(DataManager.GetDesignValue(Define.GameDesign.FreeCoin), ()=>
        {
            DataManager.GetInstance().AfterShowFreeCoinVideo();
        });
        PopupSystem.OpenPopup(Define.PopupType.AddCoin, OnCloseButton);
    }

    private void CallbackFailed()
    {
        BasicPopup popup = PopupSystem.GetPopup<BasicPopup>(Define.PopupType.Basic);
        popup.SetData(DataManager.GetText(TextTable.failedVideoKey));
        PopupSystem.OpenPopup(Define.PopupType.Basic);
    }

    private void CallbackSkip()
    {
        BasicPopup popup = PopupSystem.GetPopup<BasicPopup>(Define.PopupType.Basic);
        popup.SetData(DataManager.GetText(TextTable.skipVideoKey));
        PopupSystem.OpenPopup(Define.PopupType.Basic);
    }
}

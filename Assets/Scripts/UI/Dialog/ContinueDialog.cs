using UnityEngine;
using System.Collections;

public class ContinueDialog : IDialog
{
    [SerializeField] private UISprite gage;
    [SerializeField] private UILabel gageLabel;
    [SerializeField] private UILabel option1;
    [SerializeField] private UILabel option2;
    [SerializeField] private UILabel titleLabel;
    [SerializeField] private UILabel warningLabel;
    [SerializeField] private Transform option1Body;
    [SerializeField] private Transform option2Body;

    private float continueCheckTime = 0f;
    private bool bCheckTime = false;

    private static bool bTimeUp = false;

    public static void SetData(bool timeUp)
    {
        bTimeUp = timeUp;
    }

    public override void BeforeOpen()
    {
        continueCheckTime = DataManager.GetDesignValue(Define.GameDesign.ContinueCheckTime);

        gageLabel.text = continueCheckTime.ToString();
        gage.fillAmount = 1f;

        option1.text = DataManager.GetText(TextTable.continueOption1Key);
        int addTime = DataManager.GetDesignValue(Define.GameDesign.ContinueAddTime);

        option2.text = string.Format(DataManager.GetText(TextTable.continueOption2Key), addTime);

        int checkTime = Mathf.RoundToInt(DataManager.GetDesignValue(Define.GameDesign.ContinueCheckTime));
        warningLabel.text = string.Format(DataManager.GetText(TextTable.warningContinueKey), checkTime);

        if (bTimeUp)
        {
            option1Body.gameObject.SetActive(false);
            option2Body.gameObject.SetActive(true);

            option2Body.localPosition = new Vector3(-66, 0, 0);
        }
        else
        {
            option1Body.gameObject.SetActive(true);
            option2Body.gameObject.SetActive(true);

            option1Body.localPosition = new Vector3(-66, 46, 0);
            option2Body.localPosition = new Vector3(-66, -46, 0);
        }
    }

    public override void AfterOpen()
    {
        bCheckTime = true;
        StartCoroutine(CheckTimer());
    }

    private IEnumerator CheckTimer()
    {
        int intTime = (int)continueCheckTime;
        while (bCheckTime)
        {
            continueCheckTime -= Time.deltaTime;
            if (continueCheckTime <= 0f)
            {
                continueCheckTime = 0f;
                bCheckTime = false;
            }

            if (continueCheckTime < intTime)
            {
                intTime--;
                if (intTime < 0)
                    intTime = 0;
                else
                    SoundManager.GetInstance().PlaySound(Define.SoundType.Tick);

                gageLabel.text = intTime.ToString();
            }
            yield return null;
        }

        if (!bCheckTime && continueCheckTime <= 0f)
            OnClickClose();
    }

    public void OnClickKeepPlay()
    {
        if (!bCheckTime)
            return;

        bCheckTime = false;
        StopCoroutine(CheckTimer());

        UnityAdsController.ShowAd(Define.continueId, CallbackSuccess, OnClickClose, OnClickClose);
    }

    private void CallbackSuccess()
    {
        DataManager.GetInstance().AfterShowContinueVideo();
        UISystem.CloseDialog(Define.DialogType.ContinueDialog, () =>
        {
            GameManager.GameContinue(bTimeUp);
        });
    }

    public void OnClickClose()
    {
        UISystem.CloseDialog(Define.DialogType.ContinueDialog, ()=>
        {
            UISystem.OpenDialog(Define.DialogType.ResultDialog);
        });
    }
}

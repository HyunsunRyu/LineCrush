using UnityEngine;

public class RewardItemController : UIDragListItem
{
    [SerializeField] private UILabel infoLabel;
    [SerializeField] private UILabel stateLabel;
    [SerializeField] private UILabel coinLabel;
    [SerializeField] private UILabel allClearLabel;
    [SerializeField] private Transform coinIcon;
    [SerializeField] private Transform coinLabelBody;
    [SerializeField] private GameObject rewardButton;
    [SerializeField] private GameObject stateBody;
    [SerializeField] private GameObject rewardInfoBody;
    [SerializeField] private GameObject allClearBody;

    public override void InitItem(int idx)
    {
        this.idx = idx;
        UpdateUI();
    }

    public override void ClickedItem()
    {
    }

    public override void ClearData()
    {
        base.ClearData();
    }

    public override void UpdateUI()
    {
        RewardBase reward = RewardManager.GetInstance().GetNowReward((Define.RewardType)idx);

        infoLabel.text = reward.GetInfoText();

        coinLabel.text = reward.GetRewardCoin().ToString();
        Vector3 pos = coinLabelBody.localPosition;
        pos.x = 34f - coinLabel.width;
        coinLabelBody.localPosition = pos;
        pos.x -= 4f;
        coinIcon.localPosition = pos;

        if (reward.IsMaxLevel())
        {
            rewardInfoBody.SetActive(false);
            rewardButton.SetActive(false);
            stateBody.SetActive(false);
            allClearBody.SetActive(true);
            allClearLabel.text = DataManager.GetText(TextTable.allClearKey);
        }
        else if (reward.IsCompleted())
        {
            rewardInfoBody.SetActive(true);
            rewardButton.SetActive(true);
            stateBody.SetActive(false);
            allClearBody.SetActive(false);
            stateLabel.text = "";
        }
        else
        {
            rewardInfoBody.SetActive(true);
            rewardButton.SetActive(false);
            stateBody.SetActive(true);
            allClearBody.SetActive(false);
            stateLabel.text = RewardManager.GetInstance().GetRewardStateText(reward);
        }
    }

    public void OnClickRewardButton()
    {
        SoundManager.GetInstance().PlaySound(Define.SoundType.Click);
        RewardManager.GetInstance().GetReward((Define.RewardType)idx);
        UpdateUI();

        UISystem.UpdateNews();
    }
}

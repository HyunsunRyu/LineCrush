using UnityEngine;
using System.Collections.Generic;

public class RewardDialog : IDialog
{
    [SerializeField] private UILabel titleLabel;
    [SerializeField] private UIListDragController uiDrag;

    private Dictionary<string, string> btnDic;

    public override void BeforeOpen()
    {
        RewardManager.GetInstance().CheckAllReward();
        titleLabel.text = DataManager.GetText(TextTable.rewardKey);
        if (btnDic == null)
        {
            btnDic = new Dictionary<string, string>();
            btnDic.Add("RewardButton", "OnClickRewardButton");
        }
        uiDrag.StartList(GetListItem, RewardManager.GetInstance().rewardCount, btnDic);
    }

    public void CloseRewardDialog()
    {
        UISystem.CloseDialog(Define.DialogType.RewardDialog);
    }

    private RewardItemController GetListItem()
    {
        RewardItemController item = PoolManager.GetObject<RewardItemController>();
        return item;
    }

    private void DeleteItem(UIDragListItem item)
    {
        PoolManager.ReturnObject(item as RewardItemController);
    }

    public override void AfterClose()
    {
        uiDrag.DeleteList(DeleteItem);
    }

    public override bool HasUpdateNews()
    {
        for (int i = 0, max = RewardManager.GetInstance().rewardCount; i < max; i++)
        {
            RewardBase reward = RewardManager.GetInstance().GetNowReward((Define.RewardType)i);
            if (!reward.IsMaxLevel() && reward.IsCompleted())
                return true;
        }
        return false;
    }
}

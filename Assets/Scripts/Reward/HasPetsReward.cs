using UnityEngine;
using System.Collections;

public class HasPetsReward : RewardBase
{
    public override string GetInfoText()
    {
        RewardTable table = GetRewardTable(nowLevel);
        if (table == null)
            return "";
        return string.Format(DataManager.GetText(table.infoIdx), table.maxValue);
    }

    public override void CheckState()
    {
        RewardTable table = GetRewardTable(nowLevel);
        if (table != null)
        {
            nowValue = DataManager.GetInstance().GetHasPetListCount();
        }
    }
}

using UnityEngine;
using System.Collections;

public class BestScoreReward : RewardBase
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
            if ((double)int.MaxValue < DataManager.GetInstance().bestScore)
                nowValue = int.MaxValue;
            else
                nowValue = (int)DataManager.GetInstance().bestScore;
        }
    }
}

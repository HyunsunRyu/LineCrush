using UnityEngine;
using System.Collections;
using System;

public class UseSkillReward : RewardBase
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
            nowValue = DataManager.GetInstance().useSkillCount;
        }
    }
}

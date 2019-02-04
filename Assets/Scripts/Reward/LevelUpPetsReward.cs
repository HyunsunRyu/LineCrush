using UnityEngine;
using System.Collections;

public class LevelUpPetsReward : RewardBase
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
            int petLevel = 0;
            for (int i = 0, max = DataManager.GetInstance().GetHasPetListCount(); i < max; i++)
            {
                PetData data = DataManager.GetInstance().GetHasPetDataWithNumber(i);
                petLevel += data.level;
            }
            nowValue = petLevel;
        }
    }
}

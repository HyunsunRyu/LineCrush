using UnityEngine;
using System.Collections;

public abstract class RewardBase : MonoBehaviour
{
    [SerializeField] private Define.RewardType type;

    public bool isCompleted { get; protected set; }
    public int nowLevel { get; protected set; }
    public int nowValue { get; protected set; }

    public void Init()
    {
        nowLevel = 0;
        nowValue = 0;
        isCompleted = false;
    }

    public Define.RewardType GetRewardType() { return type; }

    public RewardTable GetRewardTable(int level) { return DataManager.GetInstance().GetRewardTable(type, level); }

    public void SetNowLevel(int level) { nowLevel = level; }
    public void SetNowValue(int value) { nowValue = value; }
    public void SetCompleteData(bool isCompleted) { this.isCompleted = isCompleted; }

    public int GetMaxLevel() { return DataManager.GetInstance().GetRewardMaxLevel(type); }

    public void Complete() { isCompleted = true; }

    public bool IsMaxLevel() { return nowLevel >= DataManager.GetInstance().GetRewardMaxLevel(type); }

    public bool TryGetReward(ref int rewardCoin)
    {
        if (!isCompleted)
            return false;

        RewardTable data = GetRewardTable(nowLevel);
        if (data == null)
            return false;

        rewardCoin = data.reward;
        if (nowLevel < GetMaxLevel())
            nowLevel++;

        isCompleted = false;
        nowValue = 0;

        RewardManager.GetInstance().CheckReward(type);
        return true;
    }

    public int GetRewardCoin()
    {
        RewardTable table = GetRewardTable(nowLevel);
        if (table == null)
            return 0;
        return table.reward;
    }

    public bool IsCompleted()
    {
        if (isCompleted)
            return true;

        CheckState();
        RewardTable table = GetRewardTable(nowLevel);
        return nowValue >= table.maxValue;
    }

    public abstract void CheckState();
    public abstract string GetInfoText();
}

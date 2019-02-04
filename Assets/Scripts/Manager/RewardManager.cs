using UnityEngine;
using System.Collections.Generic;

public class RewardManager : Singleton<RewardManager>
{
    [SerializeField] private List<RewardBase> rewardList;

    private Dictionary<Define.RewardType, RewardBase> rewardDic;
    public readonly int rewardCount = System.Enum.GetValues(typeof(Define.RewardType)).Length;

    protected override void Awake()
    {
        if (instance == null)
        {
            instance = this;
            instance.Init();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected override void Init()
    {
        base.Init();

        rewardDic = new Dictionary<Define.RewardType, RewardBase>();

        for (int i = 0, max = rewardList.Count; i < max; i++)
        {
            Define.RewardType type = rewardList[i].GetRewardType();

            if (!rewardDic.ContainsKey(type))
            {
                rewardList[i].Init();
                rewardDic.Add(type, rewardList[i]);
            }
        }
    }

    protected override void OnEnable()
    {
    }

    public void SetSavedRewardData(Define.RewardType rewardType, int level, int value, bool isCompleted)
    {
        if (rewardDic.ContainsKey(rewardType))
        {
            rewardDic[rewardType].SetNowLevel(level);
            rewardDic[rewardType].SetNowValue(value);
            rewardDic[rewardType].SetCompleteData(isCompleted);
        }
    }

    public RewardBase GetNowReward(Define.RewardType rewardType)
    {
        if (rewardDic.ContainsKey(rewardType))
        {
            return rewardDic[rewardType];
        }
        return null;
    }

    public string GetRewardStateText(RewardBase reward)
    {
        RewardTable data = DataManager.GetInstance().GetRewardTable(reward.GetRewardType(), reward.nowLevel);
        if (data != null)
            return reward.nowValue.ToString() + "/" + data.maxValue.ToString();
        Debug.Log("something is wrong");
        return null;
    }

    public bool IsComplete(RewardBase reward)
    {
        RewardTable data = DataManager.GetInstance().GetRewardTable(reward.GetRewardType(), reward.nowLevel);
        if (data != null)
            return reward.nowValue >= data.maxValue;
        Debug.Log("something is wrong");
        return false;
    }

    public void CheckAllReward()
    {
        for (int i = 0; i < rewardCount; i++)
        {
            Define.RewardType type = (Define.RewardType)i;
            CheckReward(type);
        }
    }

    public void CheckReward(Define.RewardType type)
    {
        if (rewardDic.ContainsKey(type))
        {
            if (rewardDic[type].IsCompleted())
            {
                rewardDic[type].Complete();
            }
        }
    }

    public void GetReward(Define.RewardType rewardType)
    {
        if (rewardDic.ContainsKey(rewardType))
        {
            int rewardCoin = 0;
            if (rewardDic[rewardType].TryGetReward(ref rewardCoin))
            {
                AddCoinPopup popup = PopupSystem.GetPopup<AddCoinPopup>(Define.PopupType.AddCoin);
                popup.SetData(rewardCoin);
                PopupSystem.OpenPopup(Define.PopupType.AddCoin);
            }
        }
    }
}

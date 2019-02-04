using UnityEngine;
using System.Collections.Generic;

public class MissionManager : Singleton<MissionManager>
{
    private Dictionary<int, Mission> nowMissions = new Dictionary<int, Mission>();

    [SerializeField] private List<MissionBase> missionList;
    private Dictionary<Define.MissionType, MissionBase> missionDic;
    
    //the data for missions. set them from other managers and systems. //
    public int nowMissionLevel { get; private set; }
    public int lastClearedLineCount { get; private set; }
    public int useSkillCount { get; private set; }
    public int lastCombo { get; private set; }
    public int arrangeCount { get; private set; }
    public double nowAddedScore { get; private set; }

    private System.Action clearMissionCallback = null;

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

        missionDic = new Dictionary<Define.MissionType, MissionBase>();

        for (int i = 0, max = missionList.Count; i < max; i++)
        {
            Define.MissionType type = missionList[i].type;
            if (missionDic.ContainsKey(type))
                Debug.Log("something is wrong");
            else
                missionDic.Add(type, missionList[i]);
        }
    }

    protected override void OnEnable()
    {
        nowMissionLevel = 0;
        //저장된 미션이 없어서 로드된 미션이 아무것도 없는 경우. //
        if (nowMissions.Count <= 0)
            MakeRandomMissions();
        InitData();
    }

    public void SetMission(int level, Define.MissionType type)
    {
        if (!nowMissions.ContainsKey(level))
            nowMissions.Add(level, new Mission());

        nowMissions[level].SetNewMission(type, level);
    }

    public void MakeRandomMissions()
    {
        nowMissionLevel = 0;

        nowMissions.Clear();
        for (int i = 0; i < Define.missionLevelCount; i++)
        {
            nowMissions.Add(i, new Mission());
            Define.MissionType type = Define.MissionType.ClearLines;
            if (DataManager.GetInstance().TryGetRandomMissionType(i, ref type))
            {
                nowMissions[i].SetNewMission(type, i);
            }
        }
        InitData();
    }

    public void ResetMission()
    {
        nowMissionLevel = 0;
        for (int i = 0; i < Define.missionLevelCount; i++)
        {
            nowMissions[i].ResetMission();
        }

        InitData();
    }

    public void SetClearMissionCallback(System.Action callback)
    {
        clearMissionCallback = callback;
    }

    public void CheckClearedMission()
    {
        if (nowMissionLevel < 0 || nowMissionLevel >= Define.missionLevelCount || nowMissions[nowMissionLevel].isCleared)
            return;

        bool bCleared = false;
        int level = nowMissionLevel;
        for (int i = nowMissionLevel; i < Define.missionLevelCount; i++)
        {
            level = i;

            if (nowMissions[level].isCleared)
                break;

            if (missionDic.ContainsKey(nowMissions[level].missionType))
            {
                if (missionDic[nowMissions[level].missionType].IsCleared(nowMissions[level].missionLevel))
                {
                    bCleared = true;
                    nowMissions[level].ClearMission();
                }
                else
                    break;
            }
            else
                break;
        }

        if (bCleared)
            nowMissionLevel = level;

        if (clearMissionCallback != null)
            clearMissionCallback();
    }

    private void InitData()
    {
        lastClearedLineCount = 0;
        useSkillCount = 0;
        nowAddedScore = 0;
        lastCombo = 0;
        arrangeCount = 0;
    }

    public string GetMissionText(int level)
    {
        if (missionDic.ContainsKey(nowMissions[level].missionType))
        {
            return missionDic[nowMissions[level].missionType].GetMissionText(nowMissions[level].missionLevel);
        }
        Debug.Log("something is wrong");
        return null;
    }

    public string GetMissionText()
    {
        return GetMissionText(nowMissionLevel);
    }

    public int GetMissionReward(int level)
    {
        if (missionDic.ContainsKey(nowMissions[level].missionType))
        {
            return missionDic[nowMissions[level].missionType].GetMissionReward(level);
        }
        Debug.Log("something is wrong");
        return Define.nullValue;
    }

    public string GetMissionType(int level)
    {
        return nowMissions[level].missionType.ToString();
    }

    public int GetMissionReward()
    {
        return GetMissionReward(nowMissionLevel);
    }

    public float GetMissionState()
    {
        return GetMissionState(nowMissionLevel);
    }

    public float GetMissionState(int level)
    {
        if (missionDic.ContainsKey(nowMissions[level].missionType))
        {
            float rate = missionDic[nowMissions[level].missionType].GetMissionState(level);
            return Mathf.RoundToInt(rate * 100f);
        }
        return 0f;
    }

    public bool IsClearedMission(int level)
    {
        if (level < 0 || level >= Define.missionLevelCount)
            return false;
        return nowMissions[level].isCleared;
    }

    public bool IsClearedMission()
    {
        return IsClearedMission(nowMissionLevel);
    }

    public void AddUseSkillCount()
    {
        useSkillCount++;
    }
    
    public void AddScore(int score)
    {
        nowAddedScore += score;
    }
    
    public void AddArrangeCount()
    {
        arrangeCount++;
    }

    public void SetClearedLineCount(int count)
    {
        if (lastClearedLineCount < count)
            lastClearedLineCount = count;
    }

    public void SetComboCount(int count)
    {
        if (lastCombo < count)
        {
            lastCombo = count;
        }
    }

    public Mission GetNowMission(int level)
    {
        if (nowMissions.ContainsKey(level))
            return nowMissions[level];

        return null;
    }
    
    public void ClearMission(int idx)
    {
#if UNITY_EDITOR
        nowMissions[idx].ClearMission();
#endif
    }
}

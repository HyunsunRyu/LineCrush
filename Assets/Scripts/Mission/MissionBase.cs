using UnityEngine;
using System.Collections.Generic;

public abstract class MissionBase : MonoBehaviour
{
    public Define.MissionType type;
    
    public abstract string GetMissionText(int level);
    public abstract bool IsCleared(int level);
    public abstract float GetMissionState(int level);

    public int GetMissionReward(int level)
    {
        MissionTable table = DataManager.GetInstance().GetMissionTable(type, level);
        if (table != null)
        {
            return table.reward;
        }
        return Define.nullValue;
    }
}

public class Mission
{
    public Define.MissionType missionType;
    public int missionLevel;
    public bool isCleared;

    public void SetNewMission(Define.MissionType type, int lv)
    {
        missionType = type;
        missionLevel = lv;
        isCleared = false;
    }

    public void ClearMission()
    {
        isCleared = true;
    }

    public void ResetMission()
    {
        isCleared = false;
    }
}

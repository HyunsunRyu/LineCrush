using UnityEngine;
using System.Collections;
using System;

public class GetScoreMission : MissionBase
{
    public override string GetMissionText(int level)
    {
        MissionTable table = DataManager.GetInstance().GetMissionTable(type, level);
        if (table != null)
        {
            return string.Format(DataManager.GetText(table.textIdx), table.value);
        }
        Debug.Log("something is wrong");
        return null;
    }

    public override bool IsCleared(int level)
    {
        MissionTable table = DataManager.GetInstance().GetMissionTable(type, level);
        if (table != null)
        {
            return (MissionManager.GetInstance().nowAddedScore >= table.value);
        }
        return false;
    }

    public override float GetMissionState(int level)
    {
        MissionTable table = DataManager.GetInstance().GetMissionTable(type, level);
        if (table != null)
        {
            float rate = (float)((double)MissionManager.GetInstance().nowAddedScore / (double)table.value);
            return Mathf.Min(1f, rate);
        }
        return 0f;
    }
}

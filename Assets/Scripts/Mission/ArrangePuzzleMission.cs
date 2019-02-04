using UnityEngine;

public class ArrangePuzzleMission : MissionBase
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
            return (MissionManager.GetInstance().arrangeCount >= table.value);
        }
        return false;
    }

    public override float GetMissionState(int level)
    {
        MissionTable table = DataManager.GetInstance().GetMissionTable(type, level);
        if (table != null)
        {
            float rate = (float)MissionManager.GetInstance().arrangeCount / (float)table.value;
            return Mathf.Min(1f, rate);
        }
        return 0f;
    }
}

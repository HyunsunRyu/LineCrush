using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

public class MissionTable : DataController.MixedBase
{
    public ObscuredInt level { get; protected set; }
    public Define.MissionType missionType { get; protected set; }
    public int textIdx { get; protected set; }
    public ObscuredInt value { get; protected set; }
    public ObscuredInt reward { get; protected set; }

    public override bool TryLoadTable(string[] str, out int key)
    {
        int arrayIdx = 0;
        try
        {
            key = int.Parse(str[arrayIdx++]);
            level = key;
            missionType = (Define.MissionType)int.Parse(str[arrayIdx++]);
            textIdx = int.Parse(str[arrayIdx++]);
            value = int.Parse(str[arrayIdx++]);
            reward = int.Parse(str[arrayIdx++]);
            arrayIdx++; //note. //

            if (str.Length != arrayIdx)
                Debug.Log(str.Length + " : " + arrayIdx);
        }
        catch (System.Exception e)
        {
            key = -1;
            Debug.Log(arrayIdx);
            Debug.LogError(e.ToString());
            return false;
        }
        return true;
    }
}

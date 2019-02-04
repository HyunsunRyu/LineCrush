using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

public class LevelTable : DataController.MixedBase
{
    public ObscuredInt level { get; private set; }
    public ObscuredInt maxExp { get; private set; }

    public override bool TryLoadTable(string[] str, out int key)
    {
        int arrayIdx = 0;
        try
        {
            key = int.Parse(str[arrayIdx++]);
            level = int.Parse(str[arrayIdx++]);
            maxExp = int.Parse(str[arrayIdx++]);

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

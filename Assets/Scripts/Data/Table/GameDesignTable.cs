using UnityEngine;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;

public class GameDesignTable : DataController.DicBase
{
    public ObscuredInt value { get; private set; }

    public override bool TryLoadTable(string[] str, out int key)
    {
        int arrayIdx = 0;
        try
        {
            key = int.Parse(str[arrayIdx++]);
            value = int.Parse(str[arrayIdx++]);
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

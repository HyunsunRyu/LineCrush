using UnityEngine;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;

public class ProductTable : DataController.ListBase
{
    public int idx { get; private set; }
    public string productKey { get; private set; }
    public ObscuredInt value { get; private set; }

    public override bool TryLoadTable(string[] str)
    {
        int arrayIdx = 0;
        try
        {
            idx = int.Parse(str[arrayIdx++]);
            productKey = str[arrayIdx++];
            value = int.Parse(str[arrayIdx++]);
        }
        catch (System.Exception e)
        {
            Debug.Log(arrayIdx);
            if (e != null)
            {
                Debug.LogError(e);
            }
            return false;
        }
        return true;
    }
}

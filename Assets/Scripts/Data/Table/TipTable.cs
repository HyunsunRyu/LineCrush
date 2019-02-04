using UnityEngine;
using System.Collections;

public class TipTable : DataController.ListBase
{
    public int idx { get; private set; }
    private int textIdx;

    public override bool TryLoadTable(string[] str)
    {
        int arrayIdx = 0;
        try
        {
            idx = int.Parse(str[arrayIdx++]);
            textIdx = int.Parse(str[arrayIdx++]);

            if (str.Length != arrayIdx)
                Debug.Log(str.Length + " : " + arrayIdx);
        }
        catch (System.Exception e)
        {
            Debug.Log(arrayIdx);
            Debug.LogError(e.ToString());
            return false;
        }
        return true;
    }

    public string GetTipText()
    {
        return DataManager.GetText(textIdx);
    }
}

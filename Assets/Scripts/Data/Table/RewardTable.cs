using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

public class RewardTable : DataController.MixedBase
{
    //type,level,titleIdx,infoIdx,value,reward,isToday(0:false, 1:true),note
    public Define.RewardType type { get; private set; }
    public ObscuredInt level { get; private set; }
    public int infoIdx { get; private set; }
    public ObscuredInt maxValue { get; private set; }
    public ObscuredInt reward { get; private set; }

    public override bool TryLoadTable(string[] str, out int key)
    {
        int arrayIdx = 0;
        try
        {
            key = int.Parse(str[arrayIdx++]);

            type = (Define.RewardType)key;
            level = int.Parse(str[arrayIdx++]);
            infoIdx = int.Parse(str[arrayIdx++]);
            maxValue = int.Parse(str[arrayIdx++]);
            reward = int.Parse(str[arrayIdx++]);
            arrayIdx++; //note.

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

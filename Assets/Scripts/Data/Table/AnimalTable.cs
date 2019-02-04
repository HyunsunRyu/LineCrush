using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

public class AnimalTable : DataController.ListBase
{
    public int unqIdx { get; private set; }

    //unique data. loaded from table. //
    public int nameIdx { get; private set; }
    public int levelType { get; private set; }
    public Define.SkillType passiveSkillType { get; private set; }
    public Define.SkillType activeSkillType { get; private set; }
    public ObscuredInt cost { get; private set; }
    
    public override bool TryLoadTable(string[] str)
    {
        int arrayIdx = 0;
        try
        {
            unqIdx = int.Parse(str[arrayIdx++]);
            nameIdx = int.Parse(str[arrayIdx++]);
            levelType = int.Parse(str[arrayIdx++]);
            passiveSkillType = (Define.SkillType)int.Parse(str[arrayIdx++]);
            activeSkillType = (Define.SkillType)int.Parse(str[arrayIdx++]);
            cost = int.Parse(str[arrayIdx++]);

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
}
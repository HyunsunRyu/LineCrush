using UnityEngine;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;

public class SkillTable : DataController.ListBase
{
    private class SkillData
    {
        public ObscuredInt coolTime;
        public ObscuredInt value;
        public ObscuredInt cost;

        public SkillData(int coolTime, int value, int cost)
        {
            this.coolTime = coolTime;
            this.value = value;
            this.cost = cost;
        }
    }
    
    public Define.SkillType skillType { get; private set; }
    public bool bActiveSkill { get; private set; }
    public int skillTextIdx { get; private set; }
    public string skillIcon { get; private set; }

    private int maxLevel;

    private Dictionary<int, SkillData> skillData;

    public override bool TryLoadTable(string[] str)
    {
        int arrayIdx = 0;
        try
        {
            int length = str.Length;
            
            skillType = (Define.SkillType)(int.Parse(str[arrayIdx++]));
            bActiveSkill = (int.Parse(str[arrayIdx++]) == 1) ? true : false;
            skillTextIdx = int.Parse(str[arrayIdx++]);
            skillIcon = str[arrayIdx++];

            skillData = new Dictionary<int, SkillData>();
            while (true)
            {
                int level = int.Parse(str[arrayIdx++]);
                int coolTime = int.Parse(str[arrayIdx++]);
                int value = int.Parse(str[arrayIdx++]);
                int cost = int.Parse(str[arrayIdx++]);

                SkillData data = new SkillData(coolTime, value, cost);
                skillData.Add(level, data);

                maxLevel = level;

                if (length == arrayIdx)
                    break;
            }

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

    public bool IsMaxLevel(int level)
    {
        return level >= maxLevel;
    }

    public int GetSkillCoolTime(int level)
    {
        if (!skillData.ContainsKey(level))
        {
            Debug.Log("something is wrong");
            return 100;
        }
        return skillData[level].coolTime;
    }

    public int GetSkillValue(int level)
    {
        if (!skillData.ContainsKey(level))
        {
            Debug.Log("something is wrong");
            return 0;
        }
        return skillData[level].value;
    }

    public int GetSkillCost(int level)
    {
        if (!skillData.ContainsKey(level))
        {
            Debug.Log("something is wrong");
            return 99999;
        }
        return skillData[level].cost;
    }
}

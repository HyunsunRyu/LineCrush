using UnityEngine;
using System.Collections.Generic;
using System;

public class SkillManager : Singleton<SkillManager>
{
    [SerializeField] private List<SkillBase> skillList = new List<SkillBase>();

    private Dictionary<Define.SkillType, SkillBase> skillDic = new Dictionary<Define.SkillType, SkillBase>();


    protected override void Awake()
    {
        if (instance == null)
        {
            instance = this;
            instance.Init();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected override void Init()
    {
        base.Init();

        for (int i = 0, max = skillList.Count; i < max; i++)
        {
            if (!skillDic.ContainsKey(skillList[i].GetSkillType()))
            {
                skillDic.Add(skillList[i].GetSkillType(), skillList[i]);
            }
        }
    }

    protected override void OnEnable()
    {
        foreach (KeyValuePair<Define.SkillType, SkillBase> skill in skillDic)
        {
            skill.Value.Init();
        }
    }
    
    public string GetSkillInfo(Define.SkillType skillType, int skillLevel)
    {
        SkillTable skill = DataManager.GetInstance().GetSkillTable(skillType);
        if (skill != null && skillDic.ContainsKey(skill.skillType))
        {
            return skillDic[skill.skillType].GetSkillInfoText(skillLevel);
        }
        Debug.Log("NOSKILL" + skillType.ToString());
        return "";
    }
}

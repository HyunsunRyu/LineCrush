using UnityEngine;
using System.Collections;

public abstract class SkillBase : MonoBehaviour
{
    [SerializeField] private Define.SkillType skillType;

    protected SkillTable skillTable = null;

    public abstract string GetSkillInfoText(int skillLevel);

    public Define.SkillType GetSkillType() { return skillType; }

    public void Init()
    {
        skillTable = DataManager.GetInstance().GetSkillTable(skillType);
    }
}
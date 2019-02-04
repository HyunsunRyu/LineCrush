using UnityEngine;
using System.Collections;

public class FillLinesSkill : SkillBase
{
    public override string GetSkillInfoText(int skillLevel)
    {
        return DataManager.GetText(skillTable.skillTextIdx);
    }
}

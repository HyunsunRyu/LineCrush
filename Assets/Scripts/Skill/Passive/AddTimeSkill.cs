using UnityEngine;
using System.Collections;

public class AddTimeSkill : SkillBase
{
    public override string GetSkillInfoText(int skillLevel)
    {
        return string.Format(DataManager.GetText(skillTable.skillTextIdx), skillTable.GetSkillValue(skillLevel));
    }
}

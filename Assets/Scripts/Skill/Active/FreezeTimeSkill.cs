using UnityEngine;
using System.Collections;
using System;

public class FreezeTimeSkill : SkillBase
{
    public override string GetSkillInfoText(int skillLevel)
    {
        return string.Format(DataManager.GetText(skillTable.skillTextIdx), skillTable.GetSkillValue(skillLevel));
    }
}

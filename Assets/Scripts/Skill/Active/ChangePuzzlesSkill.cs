using UnityEngine;
using System.Collections;

public class ChangePuzzlesSkill : SkillBase
{
    public override string GetSkillInfoText(int skillLevel)
    {
        return DataManager.GetText(skillTable.skillTextIdx);
    }
}

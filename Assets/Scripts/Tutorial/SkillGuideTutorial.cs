using UnityEngine;
using System.Collections;
using System;

public class SkillGuideTutorial : TutorialBase
{
    public override void Init()
    {
    }

    public override void CallMethod(string methodName)
    {
        GameObject icon = null;
        switch (methodName)
        {
            case "EmphasisSkill":
                icon = TutorialManager.GetInstance().GetTutorialUI(GetTutorialType(), methodName);
                if (icon != null)
                {
                    float x = (TutorialManager.GetInstance().tutorialValue - 1) * Define.readyWidth;
                    icon.transform.localPosition = new Vector3(x, -640f, 0f);
                }
                break;
            case "TouchIcon":
                icon = TutorialManager.GetInstance().GetTutorialUI(GetTutorialType(), methodName);
                if (icon != null)
                {
                    float x = (TutorialManager.GetInstance().tutorialValue - 1) * Define.readyWidth;
                    icon.transform.localPosition = new Vector3(x, -562f, 0f);
                }
                break;
        }
        TutorialManager.GetInstance().ShowTutorialUI(GetTutorialType(), methodName);
    }
}

using UnityEngine;
using System.Collections;

public class RewardGuideTutorial : TutorialBase
{
    public override void Init()
    {
    }

    public override void CallMethod(string methodName)
    {
        TutorialManager.GetInstance().ShowTutorialUI(GetTutorialType(), methodName);
    }
}

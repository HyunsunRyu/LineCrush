using UnityEngine;
using System.Collections;
using System;

public class GameGuideTutorial : TutorialBase
{
    public override void Init()
    {
    }

    public override void CallMethod(string methodName)
    {
        TutorialManager.GetInstance().ShowTutorialUI(GetTutorialType(), methodName);
    }
}

using UnityEngine;
using System.Collections;
using System;

public class LobbyManager : SceneObject
{
    [SerializeField] private LobbyUISystem lobbyUISystem;
    [SerializeField] private PopupSystem popupSystem;

    public static Transform uiRoot { get; private set; }

    public static bool activeAdoptGuideTutorial { get; private set; }
    public static bool activeRewardGuideTutorial { get; private set; }

    public override void ClearManager()
    {
        SoundManager.GetInstance().FadeOutAndStopBGM(Define.SoundType.LobbyBGM, 2f);

        TestCoder.RemoveTestCode(TestCode);
    }

    protected override void Awake()
    {
        ScenesManager.AddScene(this);
        ScenesManager.SetEscapeMethod(ScenesManager.ExitGame);

        uiRoot = lobbyUISystem.GetUIRoot();

        lobbyUISystem.InitSystem();
        popupSystem.InitSystem();
    }

    protected override void OnEnable()
    {
        activeAdoptGuideTutorial = !TutorialManager.GetInstance().IsCleared(Define.TutorialType.AdoptGuide);
        activeRewardGuideTutorial = !TutorialManager.GetInstance().IsCleared(Define.TutorialType.RewardGuide);

        lobbyUISystem.InitData();
        popupSystem.InitData();

        if (TipController.isShowing)
        {
            TipController.HideTip(() =>
            {
                StartLobby();
            });
        }
        else
            StartLobby();

        TestCoder.SetTestCode(KeyCode.Space, TestCode);
    }

    private void TestCode()
    {
        DataManager.GetInstance().ResetData();
    }

    private void StartLobby()
    {
        RewardManager.GetInstance().CheckAllReward();
        SoundManager.GetInstance().PlayBGM(Define.SoundType.LobbyBGM);
        CheckTutorial();
    }

    private void CheckTutorial()
    {
        if (activeAdoptGuideTutorial && DataManager.GetInstance().GetHasPetListCount() <= 0)
        {
            TutorialManager.GetInstance().StartTutorial(Define.TutorialType.AdoptGuide, () =>
            {
                activeAdoptGuideTutorial = false;
            });
        }
        else if (activeRewardGuideTutorial && lobbyUISystem.HasRewardNews())
        {
            TutorialManager.GetInstance().StartTutorial(Define.TutorialType.RewardGuide, () =>
            {
                activeRewardGuideTutorial = false;
            });
        }
    }
}

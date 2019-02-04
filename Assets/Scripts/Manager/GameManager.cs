using UnityEngine;
using System.Collections;

public class GameManager : SceneObject
{
    private static GameManager instance = null;

    public static Transform tileRoot { get; private set; }
    public static Transform animalRoot { get; private set; }

    private const float delayTime = 0.5f;
    private const int warmUpCount = 40;

    [SerializeField] private TileSystem tileSystem;
    [SerializeField] private GameUISystem uiSystem;
    [SerializeField] private PopupSystem popupSystem;
    [SerializeField] private Transform tileRootTrans;
    [SerializeField] private Transform animalRootTrans;

    private ScoreAssistance scoreAssistance;
    private GameTimeAssistance gameTimeAssistance;
    private PetAssistance petAssistance;

    private bool bPlaying;

    protected override void Awake()
    {
        instance = this;

        ScenesManager.AddScene(this);
        
        tileRoot = tileRootTrans;
        animalRoot = animalRootTrans;

        Init();

        uiSystem.InitSystem();
        popupSystem.InitSystem();
        tileSystem.InitSystem();

        TestCoder.SetTestCode(KeyCode.Space, TestCode);

        scoreAssistance = Assistance.GetAssistance(typeof(ScoreAssistance).Name) as ScoreAssistance;
        gameTimeAssistance = Assistance.GetAssistance(typeof(GameTimeAssistance).Name) as GameTimeAssistance;
        petAssistance = Assistance.GetAssistance(typeof(PetAssistance).Name) as PetAssistance;
    }

    private void TestCode()
    {
        gameTimeAssistance.SetTime(1f);
    }

    private void Init()
    {
        PoolManager.WarmUp<ScoreController>(warmUpCount);
        PoolManager.WarmUp<SkillPointEffect>(warmUpCount);
        PoolManager.WarmUp<ClearEffectController>(warmUpCount);
    }

	public override void ClearManager()
	{
        tileSystem.ClearSystem();
        uiSystem.ClearSystem();
        popupSystem.ClearSystem();

        SoundManager.GetInstance().StopAllBGM();

        TestCoder.RemoveTestCode(TestCode);

        instance = null;
    }

    public IEnumerator DelayFunc(float delayTime, System.Action callback)
    {
        yield return new WaitForSeconds(delayTime);
        if (callback != null)
            callback();
    }

	protected override void OnEnable()
	{
        ScenesManager.SetEscapeMethod(null);

        SoundManager.GetInstance().StopBGM(Define.SoundType.GameBGM);

        scoreAssistance.SetScoreCallback(uiSystem.ScoreUpdate);
        gameTimeAssistance.SetGameTimeCallback(uiSystem.GameTimeUpdate);
        gameTimeAssistance.SetWarningTimeCallback(uiSystem.WarningTime);
        petAssistance.SetSkillCountCallback(uiSystem.UpdateSkillCount);
        tileSystem.SetComboCountCallback(uiSystem.UpdateComboCount);
        tileSystem.SetAddTimeCallback(uiSystem.AddTime);

        uiSystem.InitData();
        tileSystem.InitData();

        if (TipController.isShowing)
        {
            TipController.HideTip(() =>
            {
                StartCoroutine(DelayFunc(delayTime, GameStartOpening));
            });
        }
        else
            StartCoroutine(DelayFunc(delayTime, GameStartOpening));
    }

    public void GameStartOpening()
    {
        tileSystem.GameStartOpening();
        uiSystem.GameStartOpening();

        ReadyPopup popup = PopupSystem.GetPopup<ReadyPopup>(Define.PopupType.Ready);
        popup.SetCallback(GameStart);
        PopupSystem.OpenPopup(Define.PopupType.Ready);
    }

    public static void GameStart()
	{
        if (instance == null)
            return;

        ScenesManager.SetEscapeMethod(() =>
        {
            UISystem.OpenDialog(Define.DialogType.PauseDialog);
        });

        SoundManager.GetInstance().PlayBGM(Define.SoundType.GameBGM);

        instance.bPlaying = true;
        instance.tileSystem.GameStart();
        instance.uiSystem.GameStart();
	}

    public static void GameOver(bool bTimeUp)
	{
        if (instance == null)
            return;

        instance.bPlaying = false;
        instance.tileSystem.GameOver();

        ScenesManager.SetEscapeMethod(null);

        SoundManager.GetInstance().FadeOutAndStopBGM(Define.SoundType.TickTock, 0.5f);
        SoundManager.GetInstance().FadeOutAndStopBGM(Define.SoundType.TickTockBack, 0.5f);
        SoundManager.GetInstance().FadeOutAndStopBGM(Define.SoundType.GameBGM, 0.5f);

        TimeUpPopup popup = PopupSystem.GetPopup<TimeUpPopup>(Define.PopupType.TimeUp);
        popup.SetData(bTimeUp, ()=>
        {
            bool bShowContinue = DataManager.GetInstance().CanShowContinueVideo();
            if (bShowContinue && UnityAdsController.IsReady(Define.continueId))
            {
                ContinueDialog.SetData(bTimeUp);
                UISystem.OpenDialog(Define.DialogType.ContinueDialog);
            }   
            else
                UISystem.OpenDialog(Define.DialogType.ResultDialog);
        });
        PopupSystem.OpenPopup(Define.PopupType.TimeUp);

        instance.uiSystem.GameOver();
	}

    public static void GameContinue(bool bTimeUp)
    {
        if (instance == null)
            return;

        ScenesManager.SetEscapeMethod(() =>
        {
            UISystem.OpenDialog(Define.DialogType.PauseDialog);
        });

        SoundManager.GetInstance().PlayBGM(Define.SoundType.GameBGM);

        if (!bTimeUp)
            instance.tileSystem.ClearMap();
        
        instance.bPlaying = true;
        instance.tileSystem.GameContinue();
        instance.uiSystem.GameContinue();
    }

	public static void GameReset()
	{
        if (instance == null)
            return;

        MissionManager.GetInstance().ResetMission();
        instance.OnEnable();
	}

	public static bool IsPlaying()
	{
        if (instance == null)
            return false;
        return instance.bPlaying;
	}
}

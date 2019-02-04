using UnityEngine;

public class PauseDialog : IDialog
{
    [System.Serializable]
    private class SubMissionPack
    {
        public UILabel missionLabel = null;
        public GameObject clearIcon = null;
        public UILabel rewardLabel = null;
    }

    private enum AnimType { Open = 0, Close, None }

    [SerializeField] private UILabel titleLabel;
    [SerializeField] private UILabel bestScoreLabel;
    [SerializeField] private UILabel nowScoreLabel;
    [SerializeField] private SubMissionPack[] mission;

    private float lastTime = 0f;
    private float delta = 0f;
    private float saveDelta = 0f;
    private AnimationCurve curve = null;
    private Vector3 start, end;
    private AnimType animType = AnimType.None;
    private float invTime = 1f;
    private System.Action finishCallback = null;

    public override void BeforeOpen()
    {
        GameTimeAssistance.Pause();
        
        ScenesManager.SetEscapeMethod(null);

        titleLabel.text = DataManager.GetText(TextTable.pauseTitleKey);
        
        bestScoreLabel.text = DataManager.GetInstance().bestScore.ToString();
        nowScoreLabel.text = ScoreAssistance.nowScore.ToString();

        for (int i = 0; i < Define.missionLevelCount; i++)
        {
            mission[i].missionLabel.text = MissionManager.GetInstance().GetMissionText(i);
            mission[i].clearIcon.SetActive(MissionManager.GetInstance().IsClearedMission(i));
            mission[i].rewardLabel.text = MissionManager.GetInstance().GetMissionReward(i).ToString();
        }

        lastTime = Time.realtimeSinceStartup;
        delta = 0f;
        saveDelta = 0f;
        invTime = 1f;

        SoundManager.GetInstance().PauseAllBGM();
    }

    public override void AfterOpen()
    {
        ScenesManager.SetEscapeMethod(ClosePauseDialog);
    }

    public void ClosePauseDialog()
    {
        UISystem.CloseDialog(Define.DialogType.PauseDialog);
    }

    public override void AfterClose()
    {
        ScenesManager.SetEscapeMethod(() =>
        {
            UISystem.OpenDialog(Define.DialogType.PauseDialog);
        });
        GameTimeAssistance.Continue();

        SoundManager.GetInstance().ContinueAllBGM();
    }

    private void SetSoundCallback(bool result)
    {
        SoundManager.GetInstance().SetSoundVolume(result ? 1f : 0f);
    }

    private void SetBGMCallback(bool result)
    {
        SoundManager.GetInstance().SetBGMVolume(result ? 1f : 0f);
    }

    protected override void UIOpenAnimation(System.Action callback)
    {
        animType = AnimType.Open;

        delta = 0f;
        saveDelta = 0f;
        invTime = 1f / dialogOpenMovingTime;
        start = new Vector3(ScreenSizeGetter.width * -0.5f, 0f, 0f);
        end = Vector3.zero;
        finishCallback = callback;
        curve = UISystem.GetOpenDialogCurve();
    }

    protected override void UICloseAnimation(System.Action callback)
    {
        animType = AnimType.Close;

        delta = 0f;
        saveDelta = 0f;
        invTime = 1f / dialogCloseMovingTime;
        start = Vector3.zero;
        end = new Vector3(ScreenSizeGetter.width * 0.5f, 0f, 0f);
        finishCallback = callback;
        curve = UISystem.GetCloseDialogCurve();
    }

    private void Update()
    {
        delta = Time.realtimeSinceStartup - lastTime;
        lastTime = Time.realtimeSinceStartup;
        
        if (animType == AnimType.None)
            return;

        bool bWork = true;

        saveDelta += delta * invTime;
        if (saveDelta >= 1f)
        {
            saveDelta = 1f;
            bWork = false;
        }

        Vector3 pos = AnimCurveController.GetValue(curve, start, end, saveDelta);
        moveTarget.localPosition = pos;
        if (!bWork)
        {
            if (finishCallback != null)
                finishCallback();

            animType = AnimType.None;
        }
    }

    public void OnHomeButton()
    {
        TipController.ShowRandomTip();
        GameTimeAssistance.Continue();
        ScenesManager.ChangeScene("LobbyScene");
    }

    public void OnReplayButton()
    {
        UISystem.CloseDialog(Define.DialogType.PauseDialog, () =>
        {
            GameManager.GameReset();
        });
    }

    public void OnKeepPlayButton()
    {
        ClosePauseDialog();
    }
}
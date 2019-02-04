using UnityEngine;
using System.Collections;

public class GameTimeAssistance : Assistance
{
    private static bool bCheckingTime, bPause;
    
    private System.Action finishedFreezing = null;
    public float freezingTime { get; private set; }
    public float remainTime { get; private set; }
    public bool warningMode { get; private set; }

    private float maxRemainTime = 0f;
    private ScoreAssistance scoreAssistance;

    private System.Action<float> gameTimeUpdateCallback = null;
    private System.Action<bool> warningUICallback = null;

    public void SetGameTimeCallback(System.Action<float> callback)
    {
        gameTimeUpdateCallback = callback;
    }

    public void SetWarningTimeCallback(System.Action<bool> callback)
    {
        warningUICallback = callback;
    }

    public override void Init(TileSystem tileSystem)
    {
        base.Init(tileSystem);

        scoreAssistance = GetAssistance(typeof(ScoreAssistance).Name) as ScoreAssistance;
    }

    public override void Enable()
    {
        bCheckingTime = false;
        bPause = false;
        warningMode = false;
        freezingTime = 0f;
        remainTime = DataManager.GetDesignValue(Define.GameDesign.BasicGameTime);
        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            PetData pet = DataManager.GetInstance().GetSelectedPetData(i);
            if (pet != null && pet.passiveSkillType == Define.SkillType.AddTime)
            {
                remainTime += DataManager.GetInstance().GetSkillTable(pet.passiveSkillType).GetSkillValue(pet.pSkillLv);
            }
        }
        maxRemainTime = remainTime;
        if (gameTimeUpdateCallback != null)
            gameTimeUpdateCallback(remainTime);
        if (warningUICallback != null)
            warningUICallback(warningMode);
    }

    public void GameOver()
    {
        bCheckingTime = false;
    }

    public void GameContinue()
    {
        AddTime(DataManager.GetDesignValue(Define.GameDesign.ContinueAddTime));
        warningMode = false;
        StartCheckGameTime();
    }

    public void StartCheckGameTime()
    {
        if (!bCheckingTime)
        {
            bCheckingTime = true;
            StartCoroutine(CheckTime());

            if (gameTimeUpdateCallback != null)
                gameTimeUpdateCallback(remainTime);
        }
    }

    public static void Pause()
    {
        bPause = true;
        Time.timeScale = 0f;
    }

    public static void Continue()
    {
        bPause = false;
        Time.timeScale = 1f;
    }

    public void PauseTimeChecking(bool pause)
    {
        bPause = pause;
    }

    public void FreezeTime(float time)
    {
        bool bStarted = freezingTime != 0f;
        freezingTime += time;

        if (!bStarted)
        {
            StartCoroutine(MeltingFreezed());
        }
    }

    public void AddTime(float time)
    {
        remainTime += time;

        if (remainTime >= maxRemainTime)
            remainTime = maxRemainTime;
        else if (remainTime <= 0f)
            remainTime = 0f;

        if (gameTimeUpdateCallback != null)
            gameTimeUpdateCallback(remainTime);
    }

    public void SetTime(float time)
    {
#if UNITY_EDITOR
        remainTime = time;
#endif
    }

    private IEnumerator MeltingFreezed()
    {
        while (freezingTime > 0f)
        {
            yield return null;

            freezingTime -= Time.deltaTime;
            if (freezingTime <= 0f)
            {
                freezingTime = 0f;

                if (finishedFreezing != null)
                    finishedFreezing();
            }
        }
    }

    private IEnumerator CheckTime()
    {
        while (bCheckingTime)
        {
            yield return null;

            //일시정지 중, 또는 타일이 터지거나 스킬 사용 중에는 시간이 흐르지 않는다. //
            if (TileSystem.CanClickTile() && !bPause && freezingTime == 0f)
            {
                remainTime -= Time.deltaTime;
                
                if (remainTime <= 0f)
                {
                    remainTime = 0f;
                    bCheckingTime = false;
                    GameManager.GameOver(true);
                }

                if (gameTimeUpdateCallback != null)
                    gameTimeUpdateCallback(remainTime);
            }

            if (bCheckingTime)
            {
                if (remainTime <= Define.warningTime && !warningMode)
                {
                    warningMode = true;
                    scoreAssistance.SetWarningBonus();
                    SoundManager.GetInstance().FadeInAndPlayBGM(Define.SoundType.TickTock);
                    SoundManager.GetInstance().FadeInAndPlayBGM(Define.SoundType.TickTockBack);
                    SoundManager.GetInstance().FadeOutAndStopBGM(Define.SoundType.GameBGM);

                    if (warningUICallback != null)
                        warningUICallback(warningMode);
                }
            }
        }
    }
}

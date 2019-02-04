using UnityEngine;
using System.Collections;

public class ScoreAssistance : Assistance
{
    private enum PointMode { Normal, Skill }

    private int clearedLineCount;
    private PointMode pointMode;
    private int multiBonusValue;
    private int warningBonusValue;
    private int doubleBonusCount;

    public static double nowScore { get; private set; }

    private System.Action<double> scoreCallback;

    public void SetScoreCallback(System.Action<double> callback)
    {
        scoreCallback = callback;
    }
    
    public override void Init(TileSystem tileSystem)
    {
        base.Init(tileSystem);
    }

    public override void Enable()
    {
        clearedLineCount = 0;
        pointMode = PointMode.Normal;
        multiBonusValue = 1;
        warningBonusValue = 1;
        nowScore = 0;
        doubleBonusCount = 0;

        UpdateScore();
    }

    public void SetNormalPointMode(int clearedLineCount)
    {
        pointMode = PointMode.Normal;
        this.clearedLineCount = clearedLineCount;
    }

    public void SetSkillPointMode()
    {
        pointMode = PointMode.Skill;
    }

    public void SetMultiBonus(int value, int count)
    {
        multiBonusValue = value;
        doubleBonusCount += count;
    }

    public void SubDoubleBonusCount()
    {
        doubleBonusCount--;
        if (doubleBonusCount < 0)
            doubleBonusCount = 0;
    }

    public void SetWarningBonus()
    {
        warningBonusValue = Define.warningBonus;
    }

    public void ClearWarningBonus()
    {
        warningBonusValue = 1;
    }
    
    private int GetScore(Define.ScoreType scoreType)
    {
        int score = DataManager.GetDesignValue(Define.GameDesign.DefaultScore);
        if (pointMode == PointMode.Normal)
        {
            switch (scoreType)
            {
                case Define.ScoreType.Basic:
                    //do nothing
                    break;
                case Define.ScoreType.Double:
                    score *= 2;
                    break;
                case Define.ScoreType.Triple:
                    score *= 3;
                    break;
                case Define.ScoreType.Quad:
                    score *= 5;
                    break;
                default:
                    score *= 10;
                    break;
            }
        }

        score *= warningBonusValue;

        if (doubleBonusCount > 0)
            score *= multiBonusValue;

        return score;
    }

    private Define.ScoreType GetScoreType(int lineCount)
    {
        switch (lineCount)
        {
            case 0:
            case 1:
                return Define.ScoreType.Basic;
            case 2:
                return Define.ScoreType.Double;
            case 3:
                return Define.ScoreType.Triple;
            case 4:
                return Define.ScoreType.Quad;
            default:
                return Define.ScoreType.Amazing;
        }
    }

    private string GetScoreLabel(int score, Define.ScoreType scoreType)
    {
        string text = score.ToString();
        switch (scoreType)
        {
            default:
            case Define.ScoreType.Basic:
                return text;
            case Define.ScoreType.Double:
                return text;
            case Define.ScoreType.Triple:
                return text;
            case Define.ScoreType.Quad:
                return text;
            case Define.ScoreType.Amazing:
                return text;
        }
    }

    public int AddScore(Vector3 pos)
    {
        ScoreController scoreObject = PoolManager.GetObject<ScoreController>();
        scoreObject.mTrans.parent = GameManager.tileRoot;
        scoreObject.mTrans.localScale = Vector3.one;
        scoreObject.mTrans.localPosition = pos;

        Define.ScoreType scoreType = GetScoreType(clearedLineCount);

        int addScore = GetScore(scoreType);
        nowScore += addScore;

        UpdateScore();

        scoreObject.ShowScore(GetScoreLabel(addScore, scoreType), scoreType, () =>
        {   
            PoolManager.ReturnObject(scoreObject);
        });
        return addScore;
    }

    public void UpdateScore()
    {
        if(scoreCallback != null)
            scoreCallback(nowScore);
    }

    public void SetScore(double score)
    {
        nowScore = score;
        UpdateScore();
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResultDialog : IDialog
{
    [System.Serializable]
    private class SubMissionPack
    {
        public UILabel missionLabel = null;
        public Transform clearIcon = null;
        public UILabel rewardLabel = null;
        public Transform bigRewardIcon = null;
    }

    [System.Serializable]
    private class CharacterExpPack
    {
        public GameObject noCharacterPanel = null;
        public GameObject characterPanel = null;
        public UISprite characterImage = null;
        public UISprite expGage = null;
        public LevelUpEffect levelUpEffect = null;
        public UILabel levelLabel = null;
    }

    private class MethodPack
    {
        public IEnumerator coroutine;
        public System.Action method;

        public MethodPack(IEnumerator coroutine, System.Action method)
        {
            this.coroutine = coroutine;
            this.method = method;
        }
    }

    private class ExpDataPack
    {
        public int level;
        public int exp;
        public int addExp;
        public bool isLevelUp;
    }

    [SerializeField] private UILabel scoreLabel;
    [SerializeField] Transform bestScoreIcon;
    [SerializeField] GameObject bestScoreIconBody;
    [SerializeField] private SubMissionPack[] subMissions;
    [SerializeField] private CharacterExpPack[] characterInfos;
    [SerializeField] private BoxCollider skipBlocker;

    private ExpDataPack[] expData = new ExpDataPack[Define.selectedPetsCount];
    private bool[] isClearMission = new bool[Define.missionLevelCount];
    private bool isBestScore = false;

    private Queue<MethodPack> methodQueue = new Queue<MethodPack>();
    private MethodPack nowMethod = null;
    private bool bSkipWork = false;

    //다이얼로그가 열릴 때 데이터를 셋팅하고, 해당 데이터를 기반으로 UI표시. //
    //중간에 강제종료 되더라도 다이얼로그가 뜬 시점에 이미 데이터는 저장되도록. //
    public override void BeforeOpen()
    {
        scoreLabel.text = "0";

        bestScoreIconBody.SetActive(false);
        
        for (int i = 0; i < Define.missionLevelCount; i++)
        {
            subMissions[i].missionLabel.text = MissionManager.GetInstance().GetMissionText(i);
            subMissions[i].clearIcon.gameObject.SetActive(false);
            subMissions[i].rewardLabel.text = MissionManager.GetInstance().GetMissionReward(i).ToString();
            subMissions[i].bigRewardIcon.gameObject.SetActive(false);

            isClearMission[i] = MissionManager.GetInstance().IsClearedMission(i);
        }
        
        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            PetData data = DataManager.GetInstance().GetSelectedPetData(i);

            bool bNoCharacter = data == null;

            characterInfos[i].noCharacterPanel.SetActive(bNoCharacter);
            characterInfos[i].characterPanel.SetActive(!bNoCharacter);

            if (!bNoCharacter)
            {
                characterInfos[i].characterImage.spriteName = data.GetOnGameImage();
                characterInfos[i].levelUpEffect.Hide();
                characterInfos[i].expGage.fillAmount = (float)data.exp / (float)data.GetMaxExp();
                characterInfos[i].levelLabel.text = GetLevelText(data.level);

                if (expData[i] == null)
                    expData[i] = new ExpDataPack();

                expData[i].exp = data.exp;
                expData[i].level = data.level;
                expData[i].addExp = PetAssistance.GetExp(i);

                //pet exp. //
                expData[i].isLevelUp = data.AddExp(PetAssistance.GetExp(i));
            }
        }

        //reward mission. //
        int rewardValue = 0;
        for (int i = 0; i < Define.missionLevelCount; i++)
        {
            if (MissionManager.GetInstance().IsClearedMission(i))
            {
                rewardValue = MissionManager.GetInstance().GetMissionReward(i);
            }
        }
        if (rewardValue != 0)
            DataManager.GetInstance().AddCoin(rewardValue);

        //change missions. //
        MissionManager.GetInstance().MakeRandomMissions();
        
        //best score. //
        if (DataManager.GetInstance().IsBestScore(ScoreAssistance.nowScore))
        {
            isBestScore = true;
            DataManager.GetInstance().SetBestScore(ScoreAssistance.nowScore);
        }

        //play count. //
        DataManager.GetInstance().AddPlayCount();

        DataManager.GetInstance().SaveAllData();

        skipBlocker.enabled = true;
        bSkipWork = false;

        SoundManager.GetInstance().PlayBGM(Define.SoundType.Ending, false);
    }

    public override void AfterOpen()
    {
        bSkipWork = true;
        
        methodQueue.Clear();

        methodQueue.Enqueue(new MethodPack(ScoreAnim(), ScoreUpdateImm));
        methodQueue.Enqueue(new MethodPack(RewardAnim(), RewardUpdateImm));
        methodQueue.Enqueue(new MethodPack(CharacterExpAnim(), CharacterExpImm));
        
        StartCoroutine(DoMethodInCoroutinQueue());

        ScenesManager.SetEscapeMethod(Skip);
    }

    public override void BeforeClose()
    {
        StopAllCoroutines();

        SoundManager.GetInstance().StopBGM(Define.SoundType.Ending);
    }

    private IEnumerator DoMethodInCoroutinQueue()
    {
        if (methodQueue.Count > 0)
        {
            nowMethod = methodQueue.Dequeue();
            yield return StartCoroutine(nowMethod.coroutine);
            StartCoroutine(DoMethodInCoroutinQueue());
        }
        else
            skipBlocker.enabled = false;
    }

    public void Skip()
    {
        if (!bSkipWork)
            return;

        if (nowMethod != null)
        {
            StopAllCoroutines();
            nowMethod.method();
        }

        if (methodQueue.Count <= 0)
        {
            skipBlocker.enabled = false;
            ScenesManager.SetEscapeMethod(OnHomeButton);
        }
        else
            StartCoroutine(DoMethodInCoroutinQueue());
    }

    private IEnumerator WhileCoroutineInTime(float totalTime, System.Action<float> callback)
    {
        float delta = 0f;
        float invTotalTime = 1f / totalTime;
        bool bWork = true;
        while (bWork)
        {
            delta += Time.deltaTime;
            if (delta >= totalTime)
            {
                delta = totalTime;
                bWork = false;
            }
            if (callback != null)
                callback(delta * invTotalTime);
            yield return null;
        }
    }

    private IEnumerator WhileCoroutineInSpeed(float start, float end, float speed, System.Action<float> callback)
    {
        float delta = start;
        bool bWork = true;
        while (bWork)
        {
            delta += Time.deltaTime * speed;
            if (delta >= end)
            {
                delta = end;
                bWork = false;
            }
            if (callback != null)
                callback(delta);
            yield return null;
        }
    }

    private IEnumerator ScoreAnim()
    {
        yield return StartCoroutine(WhileCoroutineInTime(1f, (float value) =>
        {
            double score = System.Math.Round(ScoreAssistance.nowScore * value);
            scoreLabel.text = score.ToString();
        }));
        
        if (isBestScore)
        {
            yield return new WaitForSeconds(0.5f);
            bestScoreIconBody.SetActive(true);
            Vector3 pos = bestScoreIcon.localPosition;
            pos.x = (float)scoreLabel.width * -0.5f - 70f;
            bestScoreIcon.localPosition = pos;

            Vector3 startScale = new Vector3(2f, 2f, 2f);
            Vector3 endScale = Vector3.one;
            Vector3 dir = endScale - startScale;
            yield return StartCoroutine(WhileCoroutineInTime(0.5f, (float value) =>
            {
                bestScoreIcon.localScale = startScale + (dir * value);
            }));
        }
    }

    private void ScoreUpdateImm()
    {
        scoreLabel.text = ScoreAssistance.nowScore.ToString();
        
        bestScoreIconBody.SetActive(isBestScore);
        Vector3 pos = bestScoreIcon.localPosition;
        pos.x = (float)scoreLabel.width * -0.5f - 70f;
        bestScoreIcon.localPosition = pos;
    }

    private IEnumerator RewardAnim()
    {
        for (int i = 0; i < Define.missionLevelCount; i++)
        {
            bool bClear = isClearMission[i];
            if (bClear)
            {
                subMissions[i].clearIcon.gameObject.SetActive(true);
                subMissions[i].bigRewardIcon.gameObject.SetActive(true);

                Vector3 clearIconStartScale = new Vector3(2f, 2f, 2f);
                Vector3 clearIconDir = new Vector3(-1f, -1f, -1f);

                Vector3 bigIconStartScale = Vector3.zero;
                Vector3 bigIconDir = Vector3.one;

                yield return StartCoroutine(WhileCoroutineInTime(0.3f, (float value) =>
                {
                    subMissions[i].clearIcon.localScale = clearIconStartScale + (clearIconDir * value);
                    subMissions[i].bigRewardIcon.localScale = bigIconStartScale + (bigIconDir * value);
                }));
            }
        }
    }

    private void RewardUpdateImm()
    {
        for (int i = 0; i < Define.missionLevelCount; i++)
        {
            bool bClear = isClearMission[i];
            if (bClear)
            {
                subMissions[i].clearIcon.gameObject.SetActive(true);
                subMissions[i].bigRewardIcon.gameObject.SetActive(true);

                subMissions[i].clearIcon.localScale = Vector3.one;
                subMissions[i].bigRewardIcon.localScale = Vector3.one;
            }
        }
    }

    private IEnumerator CharacterExpAnim()
    {
        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            PetData data = DataManager.GetInstance().GetSelectedPetData(i);

            if (data != null)
            {
                StartCoroutine(AddExpEffect(i));
                yield return new WaitForSeconds(0.3f);
            }
        }
        yield return null;
    }

    private void CharacterExpImm()
    {
        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            PetData data = DataManager.GetInstance().GetSelectedPetData(i);

            if (data != null)
            {
                if (data.IsMaxLevel())
                    characterInfos[i].expGage.fillAmount = 1f;
                else
                    characterInfos[i].expGage.fillAmount = (float)data.exp / (float)data.GetMaxExp();
                characterInfos[i].levelLabel.text = GetLevelText(data.level);
                if (expData[i].isLevelUp)
                    characterInfos[i].levelUpEffect.LevelUp();
            }
        }
    }

    private IEnumerator AddExpEffect(int idx)
    {
        bool bLevelUp = false;
        bool bWork = true;
        PetData data = DataManager.GetInstance().GetSelectedPetData(idx);
        float speed = 1f;
        while (bWork)
        {
            if (data == null || data.IsMaxLevel(expData[idx].level))
            {
                bWork = false;
                break;
            }

            int maxExp = data.GetMaxExp(expData[idx].level);
            if (expData[idx].exp + expData[idx].addExp > maxExp)
            {
                float start = (float)expData[idx].exp / (float)maxExp;
                float end = 1f;
                yield return StartCoroutine(WhileCoroutineInSpeed(start, end, speed,(float value)=>
                {
                    characterInfos[idx].expGage.fillAmount = value;
                }));

                bLevelUp = true;
                expData[idx].addExp -= (maxExp - expData[idx].exp);
                expData[idx].exp = 0;
                expData[idx].level++;
                characterInfos[idx].levelLabel.text = GetLevelText(expData[idx].level);
            }
            else
            {
                float start = (float)expData[idx].exp / (float)maxExp;
                float end = (float)(expData[idx].exp + expData[idx].addExp) / (float)maxExp;
                yield return StartCoroutine(WhileCoroutineInSpeed(start, end, 1f, (float value) =>
                {
                    characterInfos[idx].expGage.fillAmount = value;
                }));

                bWork = false;
                expData[idx].exp += expData[idx].addExp;
                expData[idx].addExp = 0;
            }
        }

        if(bLevelUp)
            characterInfos[idx].levelUpEffect.LevelUp();
    }

    private string GetLevelText(int level)
    {
        return "LV" + (level + 1).ToString();
    }

    public void OnHomeButton()
    {
        StopAllCoroutines();
        TipController.ShowRandomTip();
        ScenesManager.ChangeScene("LobbyScene");
    }

    public void OnReplayButton()
    {
        StopAllCoroutines();

        UISystem.CloseDialog(Define.DialogType.ResultDialog, ()=>
        {
            GameManager.GameReset();
        });
    }
}
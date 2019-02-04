using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameUISystem : UISystem
{
    [System.Serializable]
    private class MissionPack
    {
        public UILabel missionText = null;
        public UILabel missionLevel = null;
        public Transform missionLevelBody = null;
        public UILabel missionStateLabel = null;
        public GameObject missionStateBody = null;
        public GameObject missionComplete = null;
        public UISprite missionLevelBack = null;
    }

    [System.Serializable]
    private class SkillPack
    {
        public UILabel skillCountLabel = null;
        public UISprite skillGage = null;
        public GameObject activeSkill = null;
        public GameObject inactiveSkill = null;
    }

    [SerializeField] private List<IDialog> dialogList;
    [SerializeField] private UISprite gameTimeGage;
    [SerializeField] private GameObject gameTimeGageTail;
    [SerializeField] private UILabel gameTimeLabel;
    [SerializeField] private UILabel scoreLabel;
    [SerializeField] private UILabel comboLabel;
    [SerializeField] private MissionPack mission;
    [SerializeField] private SkillPack[] skillHUD;
    [SerializeField] private BoxCollider pauseButtonCollider;
    [SerializeField] private UIWidget[] warningUI;

    private float invMaxTime = 0f;
    private int intRemainTime = 0;
    private bool warning = false;

    public override void InitSystem()
    {
        base.InitSystem();

        for (int i = 0, max = dialogList.Count; i < max; i++)
        {
            Define.DialogType type = dialogList[i].GetDialogType();
            if (!dialogDic.ContainsKey(type))
            {
                dialogDic.Add(type, dialogList[i]);
                dialogList[i].Init();
            }
            else
                Debug.Log("something is wrong");
        }
    }

    public override void ClearSystem()
    {
        nowOpenDialog = Define.DialogType.None;
    }

    public void ClosePauseDialog() { CloseDialog(Define.DialogType.PauseDialog); }

    public override void InitData()
    {
        MissionManager.GetInstance().SetClearMissionCallback(UpdateNowMission);
        intRemainTime = 0;
        float maxTime = DataManager.GetDesignValue(Define.GameDesign.BasicGameTime);
        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            PetData pet = DataManager.GetInstance().GetSelectedPetData(i);
            if (pet != null && pet.passiveSkillType == Define.SkillType.AddTime)
            {
                maxTime += DataManager.GetInstance().GetSkillTable(pet.passiveSkillType).GetSkillValue(pet.pSkillLv);
            }
        }
        invMaxTime = 1f / maxTime;
        pauseButtonCollider.enabled = false;

        UpdateNowMission();
        UpdateSkillState();

        InitWarningUI();
    }

    public void GameStartOpening()
    {
    }

    public void GameStart()
    {
        pauseButtonCollider.enabled = true;
    }

    public void GameContinue()
    {
        InitWarningUI();
        GameStart();
    }

    public void GameOver()
    {
        WarningTime(false);
    }

    public void UpdateNowMission()
    {
        if (MissionManager.GetInstance().IsClearedMission())
        {
            mission.missionText.text = DataManager.GetText(TextTable.completeAllMissionKey);
            mission.missionComplete.SetActive(true);
            mission.missionStateBody.SetActive(false);
        }
        else
        {
            mission.missionText.text = MissionManager.GetInstance().GetMissionText();
            mission.missionComplete.SetActive(false);
            mission.missionStateBody.SetActive(true);
            mission.missionStateLabel.text = MissionManager.GetInstance().GetMissionState().ToString();
        }
        mission.missionLevel.text = (MissionManager.GetInstance().nowMissionLevel + 1).ToString();
        switch (MissionManager.GetInstance().nowMissionLevel)
        {
            case 0:
                mission.missionLevelBack.color = Util.GetColor(204, 142, 80);
                mission.missionLevel.color = Util.GetColor(249, 228, 178);
                mission.missionLevelBody.localPosition = new Vector3(-449f, -62f, 0);
                break;
            case 1:
                mission.missionLevelBack.color = Util.GetColor(192, 192, 192);
                mission.missionLevel.color = Util.GetColor(255, 255, 255);
                mission.missionLevelBody.localPosition = new Vector3(-451f, -62f, 0);
                break;
            case 2:
            default:
                mission.missionLevelBack.color = Util.GetColor(255, 255, 50);
                mission.missionLevel.color = Util.GetColor(148, 148, 73);
                mission.missionLevelBody.localPosition = new Vector3(-448f, -62f, 0);
                break;
        }
    }

    public void UpdateSkillState()
    {
        for(int i=0;i<Define.selectedPetsCount;i++)
        {
            PetData data = DataManager.GetInstance().GetSelectedPetData(i);
            if (data == null)
            {
                skillHUD[i].activeSkill.SetActive(false);
                skillHUD[i].inactiveSkill.SetActive(true);
            }
            else
            {
                skillHUD[i].activeSkill.SetActive(true);
                skillHUD[i].inactiveSkill.SetActive(false);
            }
        }
    }

    public void OnPause()
    {
        if (!GameManager.IsPlaying())
            return;

        OpenDialog(Define.DialogType.PauseDialog);
    }

    public void ScoreUpdate(double score)
    {
        scoreLabel.text = score.ToString();
    }

    public void GameTimeUpdate(float remainTime)
    {
        gameTimeGage.fillAmount = remainTime * invMaxTime;
        if ((remainTime == 0f) == gameTimeGageTail.activeSelf)
            gameTimeGageTail.SetActive(!gameTimeGageTail.activeSelf);
        
        if (remainTime >= Define.warningTime)
        {
            int time = Mathf.FloorToInt(remainTime);
            if (time != intRemainTime)
            {
                intRemainTime = time;
                gameTimeLabel.text = intRemainTime.ToString();
            }
        }
        else
        {
            if (remainTime <= 0f)
                remainTime = 0f;

            gameTimeLabel.text = string.Format("{0:f2}", remainTime);
        }
    }

    public void InitWarningUI()
    {
        warning = false;
        for (int i = 0, max = warningUI.Length; i < max; i++)
        {
            warningUI[i].color = Color.white;
        }
    }

    public void WarningTime(bool onOff)
    {
        if (!onOff)
        {
            warning = false;
            StopCoroutine(WarningAnim());
        }
        else
        {
            if (!warning)
            {
                warning = true;
                StartCoroutine(WarningAnim());
            }
        }
    }

    private IEnumerator WarningAnim()
    {
        float time = 0f;
        bool direction = true;
        Color color = Color.white;
        int count = warningUI.Length;

        while (warning)
        {
            time += Time.deltaTime;
            if (time > 1f)
            {
                time = 0f;
                direction = !direction;
            }

            color.g = direction ? 1f - time : time;
            color.b = color.g;
            for (int i = 0; i < count; i++)
                warningUI[i].color = color;
            yield return null;
        }
    }

    public void GoToLobbyScene()
    {
        CloseDialog(Define.DialogType.ResultDialog, () =>
        {
            ScenesManager.ChangeScene("LobbyScene");
        });
    }

    public void UpdateSkillCount(int idx, int cooltime, int maxCooltime)
    {
        if (Define.selectedPetsCount > idx && idx >= 0)
        {
            int count = maxCooltime - cooltime;
            if (count <= 0)
            {
                skillHUD[idx].skillCountLabel.text = DataManager.GetText(TextTable.readySkillKey);
                skillHUD[idx].skillGage.fillAmount = 1f;
            }
            else
            {
                skillHUD[idx].skillCountLabel.text = count.ToString();
                skillHUD[idx].skillGage.fillAmount = (float)cooltime / (float)maxCooltime;
            }
        }
    }

    public void UpdateComboCount(int count)
    {
        if (count <= 0)
            comboLabel.text = "";
        else
            comboLabel.text = string.Format(DataManager.GetText(TextTable.comboTextKey), count);
    }

    public void AddTime(float time)
    {
        AddTimeController addTime = PoolManager.GetObject<AddTimeController>();
        addTime.SetAddTime(time);
        addTime.mTrans.parent = GameManager.tileRoot;
        addTime.mTrans.localPosition = new Vector3(376f, -246f, 0f);
        addTime.mTrans.localScale = Vector3.one;
    }
}
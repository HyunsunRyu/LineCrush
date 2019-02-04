using UnityEngine;
using System.Collections;

public class SkillInfoPopup : IPopup
{
    [System.Serializable]
    private class SkillPack
    {
        public Transform skillLevelObject = null;
        public UILabel skillLevelLabel = null;
        public Transform valueObject = null;
        public UILabel valueLabel = null;
        public Transform cooltimeObject = null;
        public UILabel cooltimeLabel = null;
        public UISprite background = null;
    }

    [SerializeField] private UISprite characterOutline;
    [SerializeField] private UISprite characterImage;
    [SerializeField] private UISprite skillIcon;
    [SerializeField] private UILabel skillInfoLable;
    [SerializeField] private SkillPack nowSkill;
    [SerializeField] private SkillPack nextSkill;
    [SerializeField] private SkillPack maxSkill;
    [SerializeField] private GameObject maxLevelPanel;
    [SerializeField] private GameObject notMaxLevelPanel;
    [SerializeField] private GameObject upgradeBtn;
    [SerializeField] private GameObject notUpgradePanel;
    [SerializeField] private UILabel upgradeLabel;
    [SerializeField] private UILabel coinLabel;
    [SerializeField] private UILabel notUpgradeLabel;
    [SerializeField] private UILabel bottomGuideLabel;
    [SerializeField] private Transform coinLabelBody;
    [SerializeField] private Transform coinIcon;

    private PetData petData = null;
    private SkillTable skillData = null;

    private const float doubleSkillHeight = 28f;
    private const float singleSkillHeight = 0f;
    private const int doubleBackHeight = 130;
    private const int singleBackHeight = 90;
    private const float doubleLevelHeight = 98f;
    private const float singleLevelHeight = 84f;

    private System.Action upgradeCallback = null;

    public void SetData(PetData pet, SkillTable skill, System.Action upgradeCallback)
    {
        petData = pet;
        skillData = skill;
        this.upgradeCallback = upgradeCallback;
    }

    public override void BeforeOpen()
    {
        characterImage.spriteName = petData.GetOnGameImage();
        characterOutline.spriteName = petData.GetPopupOutline();

        characterImage.MakePixelPerfect();
        characterOutline.MakePixelPerfect();
        
        bool bActiveSkill = (petData.activeSkillType == skillData.skillType);
        Define.SkillType skillType = bActiveSkill ? petData.activeSkillType : petData.passiveSkillType;
        int skillLv = bActiveSkill ? petData.aSkillLv : petData.pSkillLv;

        bool noCooltime = !bActiveSkill;
        bool noValue = bActiveSkill ? petData.GetActiveSkill().GetSkillValue(skillLv) == Define.nullValue : 
            petData.GetPassiveSkill().GetSkillValue(skillLv) == Define.nullValue;

        skillIcon.spriteName = skillData.skillIcon;
        skillInfoLable.text = SkillManager.GetInstance().GetSkillInfo(skillType, skillLv);

        //구매하기 전의 동물의 스킬. //
        if (!DataManager.GetInstance().IsInHasPetDataList(petData.unqIdx))
        {
            maxLevelPanel.SetActive(true);
            notMaxLevelPanel.SetActive(false);
            bottomGuideLabel.text = DataManager.GetText(TextTable.adoptLevelKey);
            SettingUI(maxSkill, noCooltime, noValue, skillLv);
        }
        //최대 레벨. //
        else if (skillData.IsMaxLevel(skillLv))
        {
            maxLevelPanel.SetActive(true);
            notMaxLevelPanel.SetActive(false);
            bottomGuideLabel.text = DataManager.GetText(TextTable.maxLevelKey);
            SettingUI(maxSkill, noCooltime, noValue, skillLv);
        }
        //스킬 업그레이드 불가. //
        else if (skillLv >= petData.level)
        {
            maxLevelPanel.SetActive(false);
            notMaxLevelPanel.SetActive(true);

            SettingUI(nowSkill, noCooltime, noValue, skillLv);
            SettingUI(nextSkill, noCooltime, noValue, skillLv + 1);
            
            notUpgradePanel.SetActive(true);
            upgradeBtn.SetActive(false);
            notUpgradeLabel.text = string.Format(DataManager.GetText(TextTable.cantUpgradeKey), petData.GetName(), skillLv + 2);

            Vector3 pos = coinLabelBody.localPosition;
            pos.x = 32 - coinLabel.width;
            coinLabelBody.localPosition = pos;

            pos = coinIcon.localPosition;
            pos.x = 24 - coinLabel.width;
            coinIcon.localPosition = pos;
        }
        //스킬 업그레이드 가능. //
        else
        {
            maxLevelPanel.SetActive(false);
            notMaxLevelPanel.SetActive(true);

            SettingUI(nowSkill, noCooltime, noValue, skillLv);
            SettingUI(nextSkill, noCooltime, noValue, skillLv + 1);

            notUpgradePanel.SetActive(false);
            upgradeBtn.SetActive(true);
            upgradeLabel.text = DataManager.GetText(TextTable.upgradeBtnKey);
            coinLabel.text = skillData.GetSkillCost(skillLv).ToString();

            Vector3 pos = coinLabelBody.localPosition;
            pos.x = 32 - coinLabel.width;
            coinLabelBody.localPosition = pos;

            pos = coinIcon.localPosition;
            pos.x = 24 - coinLabel.width;
            coinIcon.localPosition = pos;
        }
    }

    private void SettingUI(SkillPack skillPack, bool noCooltime, bool noValue, int skillLv)
    {
        skillPack.skillLevelLabel.text = GetSkillLevel(skillLv);

        if (!noCooltime && !noValue)
        {
            skillPack.skillLevelObject.localPosition = new Vector3(0f, doubleLevelHeight, 0f);
            skillPack.cooltimeObject.gameObject.SetActive(true);
            skillPack.cooltimeObject.localPosition = new Vector3(0f, -doubleSkillHeight, 0f);
            skillPack.valueObject.gameObject.SetActive(true);
            skillPack.valueObject.localPosition = new Vector3(0f, doubleSkillHeight, 0f);
            skillPack.cooltimeLabel.text = skillData.GetSkillCoolTime(skillLv).ToString();
            skillPack.valueLabel.text = skillData.GetSkillValue(skillLv).ToString();
            skillPack.background.height = doubleBackHeight;
        }
        else if (noCooltime)
        {
            skillPack.skillLevelObject.localPosition = new Vector3(0f, singleLevelHeight, 0f);
            skillPack.cooltimeObject.gameObject.SetActive(false);
            skillPack.valueObject.gameObject.SetActive(true);
            skillPack.valueObject.localPosition = new Vector3(0f, singleSkillHeight, 0f);
            skillPack.valueLabel.text = skillData.GetSkillValue(skillLv).ToString();
            skillPack.background.height = singleBackHeight;
        }
        else if (noValue)
        {
            skillPack.skillLevelObject.localPosition = new Vector3(0f, singleLevelHeight, 0f);
            skillPack.valueObject.gameObject.SetActive(false);
            skillPack.cooltimeObject.gameObject.SetActive(true);
            skillPack.cooltimeObject.localPosition = new Vector3(0f, singleSkillHeight, 0f);
            skillPack.cooltimeLabel.text = skillData.GetSkillCoolTime(skillLv).ToString();
            skillPack.background.height = singleBackHeight;
        }
        else
            Debug.Log("something is wrong");
    }

    private string GetSkillLevel(int level) { return "Level " + (level + 1).ToString(); }

    public void OnUpgradeBtn()
    {
        bool bActiveSkill = petData.activeSkillType == skillData.skillType;
        int skillLv = bActiveSkill ? petData.aSkillLv : petData.pSkillLv;
        int needCoin = 0;

        SoundManager.GetInstance().PlaySound(Define.SoundType.Click);

        if (DataManager.GetInstance().IsEnoughCoin(skillData.GetSkillCost(skillLv), ref needCoin))
        {
            if (upgradeCallback != null)
                upgradeCallback();

            PopupSystem.ClosePopup(Define.PopupType.SkillInfo);
        }
        else
        {
            //NeedCoinPopup popup = PopupSystem.GetPopup<NeedCoinPopup>(Define.PopupType.NeedCoin);
            //popup.SetData(needCoin, () =>
            //{
            //    PopupSystem.ClearPopupStack();
            //    UISystem.OpenDialog(Define.DialogType.ShopDialog);
            //});
            //PopupSystem.OpenPopup(Define.PopupType.NeedCoin);
            DataManager.GetInstance().SetNeedCoin(needCoin);
            UISystem.OpenDialog(Define.DialogType.ShopDialog);
        }
    }

    public void OnCloseBtn()
    {
        SoundManager.GetInstance().PlaySound(Define.SoundType.Click);
        PopupSystem.ClosePopup(Define.PopupType.SkillInfo);
    }
}

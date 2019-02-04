using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class InvenDialog : IDialog
{
    [System.Serializable]
    public class SkillUI
    {
        public UISprite icon;
        public GameObject iconBody;
        public UISprite levelBack;
        public GameObject levelBackObj;
        public UILabel skillLabel;
        public GameObject newsIcon;
    }

    [SerializeField] private UILabel titleLabel;
    [SerializeField] private UIListDragController uiDrag;
    [SerializeField] private Transform itemStore;
    [SerializeField] private UILabel characterNameLabel;
    [SerializeField] private UISprite characterImage;
    [SerializeField] private GameObject characterBody;
    [SerializeField] private UILabel noCharacterLabel;
    [SerializeField] private UILabel adoptLabel;
    [SerializeField] private UISprite adoptIcon;
    [SerializeField] private GameObject noneIcon;
    [SerializeField] private GameObject adoptButton;
    [SerializeField] private GameObject levelInfo;
    [SerializeField] private GameObject playmateFlag;
    [SerializeField] private UISprite playmateImage;
    [SerializeField] private UILabel playmateLabel;
    [SerializeField] private UILabel nowLvLabel;
    [SerializeField] private UILabel nowExpLabel;
    [SerializeField] private UISprite expGage;
    [SerializeField] private SkillUI[] skills;
    [SerializeField] private UISprite[] listPanelUIs;

    private PetData petData = null;
    private SkillTable skillData = null;
    private bool bAdopt = false;

    private List<PetData> petList = new List<PetData>();

    public override void BeforeOpen()
    {
        for (int i = 0, max = listPanelUIs.Length; i < max; i++)
        {
            listPanelUIs[i].RemoveFromPanel();
            listPanelUIs[i].gameObject.SetActive(true);
        }

        petData = null;

        //선택된 펫이 없는 경우 : 구매 또는 예외. //
        if (DataManager.GetInstance().nowSelectedPetData == null)
        {
            DataManager.GetInstance().ResetAdoptAnimals();
            DataManager.GetInstance().TryGetAdoptPetData(ref petData, 0);
            DataManager.GetInstance().SetNowSelectedPetData(petData);
        }

        UpdateAll();
        UpdateList();

        TestCoder.SetTestCode(KeyCode.Space, TestCode);
    }

    private void TestCode()
    {
        //DataManager.GetInstance().AddCoin(5000);

        //if (petData != null)
        //{
        //    petData.LevelUp();
        //    //petData.ActiveSkillLevelUp();
        //    //petData.PassiveSkillLevelUp();
        //}
        //UpdateAll();
    }

    public override void AfterClose()
    {
        uiDrag.DeleteList(DeleteItem);

        for (int i = 0, max = listPanelUIs.Length; i < max; i++)
        {
            listPanelUIs[i].RemoveFromPanel();
            listPanelUIs[i].gameObject.SetActive(false);
        }

        TestCoder.RemoveTestCode(TestCode);
    }

    public void UpdateAll()
    {
        //copy the pet data. //
        petData = DataManager.GetInstance().nowSelectedPetData;

        bAdopt = IsAdoptMode();

        UpdateCharacterData();
        UpdateSkillData();
        UpdateUIType();
        uiDrag.UpdateAllItems();
    }

    private void UpdateCharacterData()
    {
        characterBody.SetActive(petData != null);

        if (petData != null)
        {
            string image = "";
            if (petData.TryGetImage(0, ref image))
            {
                characterImage.spriteName = image;
            }
        }
        
        characterNameLabel.text = (petData != null) ? petData.GetName() : "";

        noCharacterLabel.text = (petData != null) ? "" : DataManager.GetText(TextTable.selectCharacterMsg);

        int selectedIdx = 0;
        if (!bAdopt && petData != null && DataManager.GetInstance().IsSelectedAnimal(ref selectedIdx, petData.unqIdx))
        {
            playmateFlag.SetActive(true);
            playmateLabel.text = DataManager.GetText(TextTable.playmateKey);
            switch (selectedIdx)
            {
                case 0:
                    playmateImage.spriteName = Define.playmate_red;
                    break;
                case 1:
                    playmateImage.spriteName = Define.playmate_blue;
                    break;
                default:
                case 2:
                    playmateImage.spriteName = Define.playmate_green;
                    break;
            }
        }
        else
        {
            playmateFlag.SetActive(false);
        }
    }

    private void UpdateSkillData()
    {
        for (int i = 0; i < 2; i++)
        {
            skills[i].iconBody.SetActive(petData != null);

            if (petData != null)
            {
                Define.SkillType skillType = (i == 0) ? petData.activeSkillType : petData.passiveSkillType;
                skills[i].icon.spriteName = DataManager.GetInstance().GetSkillIcon(skillType);
                skills[i].levelBackObj.SetActive(!bAdopt);
                if (!bAdopt)
                {
                    skills[i].levelBack.spriteName = Define.skillLevelBack[i];
                    skills[i].skillLabel.text = (petData.GetSkillLevel(i) + 1).ToString();

                    if (i == 0)
                        skills[i].newsIcon.SetActive(petData.CanActiveSkillUpgrade());
                    else
                        skills[i].newsIcon.SetActive(petData.CanPassiveSkillUpgrade());
                }
                else
                    skills[i].newsIcon.SetActive(false);
            }
        }
    }

    private string GetLevelStr(int level){ return "LV" + (level + 1).ToString(); }

    private void UpdateUIType()
    {
        int titleTextIdx = bAdopt ? TextTable.adoptTitleKey : TextTable.invenTitleKey;
        titleLabel.text = DataManager.GetText(titleTextIdx);
        
        if (!bAdopt)
        {
            adoptButton.SetActive(false);

            if (petData.IsMaxLevel())
            {
                levelInfo.SetActive(true);
                nowLvLabel.text = GetLevelStr(Define.maxCharacterLevel);
                nowExpLabel.text = "∞";
                nowExpLabel.fontSize = 30;
                expGage.fillAmount = 1f;
            }
            else
            {
                int maxExp = petData.GetMaxExp();
                int nowExp = petData.exp;
                levelInfo.SetActive(nowExp < maxExp);

                if (nowExp < maxExp)
                {
                    nowLvLabel.text = GetLevelStr(petData.level);
                    nowExpLabel.text = nowExp.ToString() + "/" + maxExp.ToString();
                    nowExpLabel.fontSize = 16;
                    if (maxExp > 0)
                        expGage.fillAmount = (float)nowExp / (float)maxExp;
                    else
                    {
                        Debug.Log("something is wrong");
                        expGage.fillAmount = 0f;
                    }
                }
            }
        }
        else
        {
            if (petData != null)
            {
                float halfDis = 0;
                adoptLabel.text = petData.cost.ToString();
                Vector3 pos = adoptLabel.transform.localPosition;
                halfDis = adoptLabel.width * 0.5f + 5f;
                pos.x = halfDis;
                adoptLabel.transform.localPosition = pos;

                pos = adoptIcon.transform.localPosition;
                pos.x = -halfDis - (adoptIcon.width * 0.5f);
                adoptIcon.transform.localPosition = pos;
            }
            adoptButton.SetActive(petData != null);
            levelInfo.SetActive(false);
        }
    }

    public PetData GetPetDataOnList(int idx)
    {
        PetData data = null;
        if (bAdopt)
        {
            DataManager.GetInstance().TryGetAdoptPetData(ref data, idx);
            return data;
        }

        DataManager.GetInstance().TryGetInvenPetData(ref data, idx);
        return data;
    }
    
    private void UpdateList()
    {
        petList.Clear();
        int petCount = 0;

        if (bAdopt)
        {
            DataManager.GetInstance().ResetAdoptAnimals();
            petCount = DataManager.GetInstance().GetAdoptPetListCount();
        }
        else
        {
            DataManager.GetInstance().ResetInvenAnimals();
            petCount = DataManager.GetInstance().GetInvenPetListCount();
        }

        uiDrag.StartList(GetListItem, petCount);
        noneIcon.SetActive(petCount == 0);

        if (petData != null)
        {
            int idx = 0;
            if (DataManager.GetInstance().TryGetInvenPetIndex(petData.unqIdx, ref idx))
            {
                uiDrag.SetFocusItem(idx);
            }
        }
    }

    private InvenItemController GetListItem()
    {
        InvenItemController item = PoolManager.GetObject<InvenItemController>();
        item.SetDialog(this);
        item.SetAdoptMode(bAdopt);
        return item;
    }
    
    private void DeleteItem(UIDragListItem item)
    {
        InvenItemController con = item as InvenItemController;
        con.RemoveFromPanel();
        PoolManager.ReturnObject(item as InvenItemController);
    }

    private bool IsAdoptMode()
    {
        return (petData == null || !DataManager.GetInstance().IsInHasPetDataList(petData.unqIdx));
    }

    public void OnClickSkill1(){ OnClickSkillIcon(true); }
    public void OnClickSkill2(){ OnClickSkillIcon(false); }

    private void OnClickSkillIcon(bool bActiveSkill)
    {
        if (petData == null)
            return;
        skillData = bActiveSkill ? petData.GetActiveSkill() : petData.GetPassiveSkill();
        if (skillData == null)
            return;
        
        SoundManager.GetInstance().PlaySound(Define.SoundType.Click);

        SkillInfoPopup popup = PopupSystem.GetPopup<SkillInfoPopup>(Define.PopupType.SkillInfo);
        popup.SetData(petData, skillData, UpgradeSkill);
        PopupSystem.OpenPopup(Define.PopupType.SkillInfo);
    }

    public void OnClickAdoptButton()
    {
        if (petData == null)
            return;

        SoundManager.GetInstance().PlaySound(Define.SoundType.Click);

        int needCoin = 0;
        if (DataManager.GetInstance().IsEnoughCoin(petData.cost, ref needCoin))
        {
            string msg = string.Format(DataManager.GetText(TextTable.adoptPetPopupMsgKey), petData.GetName());

            UseCoinPopup popup = PopupSystem.GetPopup<UseCoinPopup>(Define.PopupType.UseCoin);
            popup.SetData(msg, petData.cost, petData.unqIdx, AdoptPet);
            PopupSystem.OpenPopup(Define.PopupType.UseCoin);
        }
        else
        {
            //need coin. //
            //NeedCoinPopup popup = PopupSystem.GetPopup<NeedCoinPopup>(Define.PopupType.NeedCoin);
            //popup.SetData(needCoin, () =>
            //{
            //    UISystem.OpenDialog(Define.DialogType.ShopDialog);
            //});
            //PopupSystem.OpenPopup(Define.PopupType.NeedCoin);
            DataManager.GetInstance().SetNeedCoin(needCoin);
            UISystem.OpenDialog(Define.DialogType.ShopDialog);
        }

        if (LobbyManager.activeAdoptGuideTutorial)
            TutorialManager.GetInstance().CallReaction();
    }

    private void AdoptPet()
    {
        if (petData != null)
        {
            if (DataManager.GetInstance().TryAdoptAnimal(petData))
            {
                UISystem.CloseDialog(Define.DialogType.InvenDialog);

                UISystem.UpdateNews();
            }
        }
    }
    
    public void CloseInvenDialog()
    {
        UISystem.CloseDialog(Define.DialogType.InvenDialog);
    }

    private void UpgradeSkill()
    {
        int needCoin = 0;
        if (petData.activeSkillType == skillData.skillType)
        {
            if (DataManager.GetInstance().IsEnoughCoin(skillData.GetSkillCost(petData.aSkillLv), ref needCoin))
            {
                DataManager.GetInstance().UseCoin(skillData.GetSkillCost(petData.aSkillLv));
                petData.ActiveSkillLevelUp();
            }
        }
        else
        {
            if (DataManager.GetInstance().IsEnoughCoin(skillData.GetSkillCost(petData.pSkillLv), ref needCoin))
            {
                DataManager.GetInstance().UseCoin(skillData.GetSkillCost(petData.pSkillLv));
                petData.PassiveSkillLevelUp();
            }
        }
        UpdateAll();
        UISystem.UpdateNews();

        DataManager.GetInstance().SaveAllData();
    }

    public override bool HasUpdateNews()
    {
        bool hasNews = false;
        DataManager.GetInstance().ResetAdoptAnimals();
        for (int i = 0, max = DataManager.GetInstance().GetAdoptPetListCount(); i < max; i++)
        {
            int needCoin = 0;
            PetData pet = null;
            if (DataManager.GetInstance().TryGetAdoptPetData(ref pet, i))
            {
                if (DataManager.GetInstance().IsEnoughCoin(pet.cost, ref needCoin))
                {
                    hasNews = true;
                    break;
                }
            }
        }
        return hasNews;
    }
}
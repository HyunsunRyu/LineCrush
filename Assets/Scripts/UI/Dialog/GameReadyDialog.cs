using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameReadyDialog : IDialog
{
    [System.Serializable]
    private class SelectedPetUI
    {
        public GameObject characterObj = null;
        public UISprite characterImage = null;
        public UILabel addMessage = null;
        public UILabel characterName = null;
        public GameObject changeButton = null;
        public UISprite changeButtonImage = null;
        public UISprite characterBack = null;
        public UISprite outline = null;
        public GameObject newIcon = null;
    }

    [System.Serializable]
    private class MissionUI
    {
        public UILabel missionLabel = null;
        public UILabel rewardLabel = null;
    }

    private enum ListState { Hide = 0, Show, Showing, Hiding }

    [SerializeField] private AnimationCurve listShowCurve;
    [SerializeField] private AnimationCurve listHideCurve;
    [SerializeField] private UILabel titleLabel;
    [SerializeField] private UILabel missionLabel;
    [SerializeField] private UILabel playBtnLabel;
    [SerializeField] private SelectedPetUI[] petUIs;
    [SerializeField] private MissionUI[] missionUIs;
    [SerializeField] private UIListDragController uiDrag;
    [SerializeField] private Transform movingPanel;
    [SerializeField] private List<BoxCollider> inactiveColOnMoving;
    [SerializeField] private List<BoxCollider> activeColOnMoving;

    private int selectedIdx = Define.nullValue;

    private bool bListOn = false;
    private bool bListMoving = false;
    private const float showListPosX = 0f;
    private const float hideListPosX = 940f;
    private const float movingTime = 0.5f;

    public override void BeforeOpen()
    {
        UpdateCommonUI();
        UpdateMissionData();
        UpdatePetsData();
        HideListImm();
    }

    private void UpdateCommonUI()
    {
        titleLabel.text = DataManager.GetText(TextTable.playTitleKey);
        missionLabel.text = DataManager.GetText(TextTable.missionKey);
        playBtnLabel.text = DataManager.GetText(TextTable.playBtnKey);
    }

    private void UpdateMissionData()
    {
        for (int i = 0; i < Define.missionLevelCount; i++)
        {
            missionUIs[i].missionLabel.text = MissionManager.GetInstance().GetMissionText(i);
            missionUIs[i].rewardLabel.text = MissionManager.GetInstance().GetMissionReward(i).ToString();
        }
    }

    private void UpdatePetsData()
    {
        DataManager.GetInstance().ResetPlaymateAnimals(-1);
        int playmateAnimalCount = DataManager.GetInstance().GetPlaymatePetListCount();

        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            PetData data = DataManager.GetInstance().GetSelectedPetData(i);
            if (data != null)
            {
                petUIs[i].characterObj.SetActive(true);
                petUIs[i].characterImage.spriteName = data.GetProfile();
                petUIs[i].addMessage.text = "";
                petUIs[i].characterName.text = data.GetName();
                petUIs[i].newIcon.SetActive(false);

                switch (i)
                {
                    case 0:
                        petUIs[i].characterBack.spriteName = Define.profile_back_red;
                        petUIs[i].outline.spriteName = Define.profile_outline_red;
                        break;
                    case 1:
                        petUIs[i].characterBack.spriteName = Define.profile_back_blue;
                        petUIs[i].outline.spriteName = Define.profile_outline_blue;
                        break;
                    case 2:
                    default:
                        petUIs[i].characterBack.spriteName = Define.profile_back_green;
                        petUIs[i].outline.spriteName = Define.profile_outline_green;
                        break;
                }
            }
            else
            {
                petUIs[i].characterObj.SetActive(false);
                petUIs[i].characterName.text = "";
                petUIs[i].characterBack.spriteName = Define.profile_back;
                petUIs[i].addMessage.text = DataManager.GetText(TextTable.addPetPlaymateKey);
                petUIs[i].outline.spriteName = Define.profile_outline;

                petUIs[i].newIcon.SetActive(playmateAnimalCount > 0);
            }
        }
    }

    public override void AfterClose()
    {
        uiDrag.DeleteList(DeleteItem);
    }

    public void SelectCharacter1() { SelectCharacter(0); }
    public void SelectCharacter2() { SelectCharacter(1); }
    public void SelectCharacter3() { SelectCharacter(2); }

    private void SelectCharacter(int idx)
    {
        if (idx < 0 && idx >= Define.selectedPetsCount)
        {
            Debug.Log("something is wrong");
            return;
        }
        
        selectedIdx = idx;
        UpdateChangeButtonUI();
        ShowList();

        if (LobbyManager.activeAdoptGuideTutorial)
            TutorialManager.GetInstance().CallReaction();
    }

    private void UpdateChangeButtonUI()
    {
        for (int i = 0; i < Define.selectedPetsCount; i++)
        {
            if (i == selectedIdx)
                petUIs[i].changeButtonImage.spriteName = Define.unselect_change_button;
            else
                petUIs[i].changeButtonImage.spriteName = Define.select_change_button;
        }
    }

    private PlaymateItemController GetListItem()
    {
        PlaymateItemController item = PoolManager.GetObject<PlaymateItemController>();
        item.SetClickCallback(OnClickAnimalItem);
        return item;
    }

    private void DeleteItem(UIDragListItem item)
    {
        PoolManager.ReturnObject(item as PlaymateItemController);
    }

    public void OnClickAnimalItem(PetData data)
    {
        SoundManager.GetInstance().PlaySound(Define.SoundType.Click);

        if (data == null)
        {
            //선택 해제. //
            DataManager.GetInstance().SetSelectedPetData(null, selectedIdx);

            UpdatePetsData();
            HideList();
        }
        else
        {
            if (selectedIdx != Define.nullValue)
            {
                //이미 선택된 동물이 아닌 경우에만 교체. //
                int idx = 0;
                if (!DataManager.GetInstance().IsSelectedAnimal(ref idx, data.unqIdx))
                {
                    DataManager.GetInstance().SetSelectedPetData(data, selectedIdx);

                    UpdatePetsData();
                    HideList();

                    if (LobbyManager.activeAdoptGuideTutorial)
                        TutorialManager.GetInstance().CallReaction();
                }
            }
        }

        DataManager.GetInstance().SaveAllData();
    }

    private void SetScroll(int idx)
    {
        DataManager.GetInstance().ResetPlaymateAnimals(idx);
        int playmateAnimalCount = DataManager.GetInstance().GetPlaymatePetListCount();
        uiDrag.StartList(GetListItem, playmateAnimalCount);
    }

    public void OnCloseListButton()
    {
        HideList();
    }

    public void OnClickChangePetButton1()
    {
        SelectCharacter(0);
    }

    public void OnClickChangePetButton2()
    {
        SelectCharacter(1);
    }

    public void OnClickChangePetButton3()
    {
        SelectCharacter(2);
    }

    private void SettingAfterMoveList()
    {
        for (int i = 0, max = inactiveColOnMoving.Count; i < max; i++)
            inactiveColOnMoving[i].enabled = !bListMoving;

        for (int i = 0, max = activeColOnMoving.Count; i < max; i++)
            activeColOnMoving[i].enabled = bListMoving && !bListOn;

        for (int i = 0; i < Define.selectedPetsCount; i++)
            petUIs[i].changeButton.SetActive(bListOn);
    }

    private void HideListImm()
    {
        bListOn = false;
        bListMoving = false;

        movingPanel.localPosition = new Vector3(hideListPosX, 0f, 0f);

        SettingAfterMoveList();
    }

    private void ShowList()
    {
        if (bListMoving)
            return;

        uiDrag.DeleteList(DeleteItem);
        SetScroll(selectedIdx);

        if (!bListOn)
        {
            bListOn = true;
            bListMoving = true;
            SettingAfterMoveList();
            UpdateChangeButtonUI();

            Vector3 from = new Vector3(hideListPosX, 0f, 0f);
            Vector3 to = new Vector3(showListPosX, 0f, 0f);

            SoundManager.GetInstance().PlaySound(Define.SoundType.ShowDialog);

            AnimCurveController.Move(listShowCurve, from, to, movingTime, movingPanel, () =>
            {
                bListMoving = false;
                SettingAfterMoveList();
            });
        }
        else
            SoundManager.GetInstance().PlaySound(Define.SoundType.Click);
    }

    private void HideList()
    {
        if (!bListOn || bListMoving)
            return;
        bListOn = false;
        bListMoving = true;
        SettingAfterMoveList();

        selectedIdx = Define.nullValue;

        SoundManager.GetInstance().PlaySound(Define.SoundType.HideDialog);

        Vector3 from = new Vector3(showListPosX, 0f, 0f);
        Vector3 to = new Vector3(hideListPosX, 0f, 0f);

        AnimCurveController.Move(listHideCurve, from, to, movingTime, movingPanel, () =>
        {
            bListMoving = false;
            SettingAfterMoveList();

            uiDrag.DeleteList(DeleteItem);
        });
    }

    public void OnPlayButton()
    {
        SoundManager.GetInstance().PlaySound(Define.SoundType.Click);

        int selectedPetCount = DataManager.GetInstance().GetTotalSelectedPetsCount();
        if (selectedPetCount <= 0)
        {
            PopupSystem.GetPopup<BasicPopup>(Define.PopupType.Basic).SetData(DataManager.GetText(TextTable.cantPlayGameKey));
            PopupSystem.OpenPopup(Define.PopupType.Basic);
        }
        else
        {
            UISystem.CloseDialog(Define.DialogType.GameReadyDialog, () =>
            {
                TipController.ShowRandomTip();
                ScenesManager.ChangeScene("GameScene");
            });
        }
    }

    public void OnCloseGameReadyDialog()
    {
        UISystem.CloseDialog(Define.DialogType.GameReadyDialog);
    }
}

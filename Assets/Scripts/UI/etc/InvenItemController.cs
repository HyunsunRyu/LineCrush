using UnityEngine;
using System.Collections.Generic;

public class InvenItemController : UIDragListItem
{
    [SerializeField] private UISprite characterImage;
    [SerializeField] private UISprite background;
    [SerializeField] private UISprite outline;
    [SerializeField] private UILabel playmateLabel;
    [SerializeField] private GameObject playmateObj;
    [SerializeField] private UISprite playmateImage;
    [SerializeField] private GameObject newIcon;

    [SerializeField] private List<UIWidget> allUIs;

    private PetData petData;
    private InvenDialog dialog;
    private bool bAdopt = false;

    public override void InitItem(int idx)
    {
        this.idx = idx;

        UpdateUI();
    }

    public override void ClickedItem()
    {
        if (petData != null)
        {
            SoundManager.GetInstance().PlaySound(Define.SoundType.Click);
            DataManager.GetInstance().SetNowSelectedPetData(petData);
            dialog.UpdateAll();
        }
    }

    public void SetDialog(InvenDialog dialog)
    {
        this.dialog = dialog;
    }

    public void SetAdoptMode(bool bAdopt)
    {
        this.bAdopt = bAdopt;
    }

    public override void UpdateUI()
    {
        petData = dialog.GetPetDataOnList(idx);

        characterImage.spriteName = (petData == null) ? "" : petData.GetProfile();

        if (bAdopt)
        {
            int needCoin = 0;
            newIcon.SetActive(DataManager.GetInstance().IsEnoughCoin(petData.cost, ref needCoin));
        }
        else
        {
            if (petData.CanActiveSkillUpgrade())
                newIcon.SetActive(true);
            else if (petData.CanPassiveSkillUpgrade())
                newIcon.SetActive(true);
            else
                newIcon.SetActive(false);
        }

        int selectedIdx = 0;
        if (DataManager.GetInstance().IsSelectedAnimal(ref selectedIdx, petData.unqIdx))
        {
            playmateObj.SetActive(true);
            playmateLabel.text = DataManager.GetText(TextTable.playmateKey);
            switch (selectedIdx)
            {
                case 0:
                    background.spriteName = Define.profile_back_red;
                    outline.spriteName = Define.profile_outline_red;
                    playmateImage.spriteName = Define.playmate_red;
                    break;
                case 1:
                    background.spriteName = Define.profile_back_blue;
                    outline.spriteName = Define.profile_outline_blue;
                    playmateImage.spriteName = Define.playmate_blue;
                    break;
                case 2:
                default:
                    background.spriteName = Define.profile_back_green;
                    outline.spriteName = Define.profile_outline_green;
                    playmateImage.spriteName = Define.playmate_green;
                    break;
            }
        }
        else
        {
            background.spriteName = Define.profile_back;
            outline.spriteName = Define.profile_outline;
            playmateObj.SetActive(false);
        }
    }

    public void RemoveFromPanel()
    {
        for (int i = 0, max = allUIs.Count; i < max; i++)
            allUIs[i].RemoveFromPanel();
    }
}

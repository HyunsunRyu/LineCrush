using UnityEngine;
using System.Collections;

public class PlaymateItemController : UIDragListItem
{
    [SerializeField] private UISprite characterImage;
    [SerializeField] private UISprite background;
    [SerializeField] private UISprite outline;
    [SerializeField] private GameObject character;
    [SerializeField] private GameObject unselectIcon;
    [SerializeField] private UILabel unselectLabel;

    private PetData petData;
    private System.Action<PetData> clickItemCallback = null;

    public override void InitItem(int idx)
    {
        petData = null;
        DataManager.GetInstance().TryGetPlaymatePetData(ref petData, idx);

        UpdateUI();
    }

    public override void UpdateUI()
    {
        //null 인 경우, 누르면 선택한 펫이 해제되는 기능. //
        if (petData == null)
        {
            character.SetActive(false);
            background.spriteName = Define.profile_back_unselect;
            outline.spriteName = Define.profile_outline_unselect;
            unselectIcon.SetActive(true);
            unselectLabel.text = DataManager.GetText(TextTable.unselectKey);
        }
        else
        {
            character.SetActive(true);
            characterImage.spriteName = petData.GetProfile();
            background.spriteName = Define.profile_back;
            outline.spriteName = Define.profile_outline;
            unselectIcon.SetActive(false);
            unselectLabel.text = "";
        }
    }

    public override void ClickedItem()
    {
        if (clickItemCallback != null)
            clickItemCallback(petData);
    }

    public void SetClickCallback(System.Action<PetData> callback)
    {
        clickItemCallback = callback;
    }
}
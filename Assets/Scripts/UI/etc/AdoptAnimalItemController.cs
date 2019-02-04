using UnityEngine;
using System.Collections;

public class AdoptAnimalItemController : UIDragListItem
{
    [SerializeField] private UISprite character;
    [SerializeField] private UILabel nameLabel;
    [SerializeField] private UILabel skillLabel;
    [SerializeField] private UILabel addTimeLabel;
    [SerializeField] private UILabel buyBtnLabel;
    [SerializeField] private BoxCollider buyBtnCollider;
    
    private readonly Color boughtAnimalColor = Color.gray;
    private readonly Color notBuyAnimalColor = Color.black;

    private PetData petData;
    private bool hasItem = false;

    public override void InitItem(int idx)
    {
        if (!DataManager.GetInstance().TryGetAdoptPetData(ref petData, idx))
        {
            Debug.Log("something is wrong");
            petData = null;
        }

        UpdateUI();
    }
    
    public override void UpdateUI()
    {
        //nameLabel.text = petData.GetName();
        //skillLabel.text = DataManager.GetSkillText(petData.skillIdx);
        //addTimeLabel.text = DataManager.GetAddTimeText(petData.maxAddTime);
        buyBtnLabel.text = petData.cost.ToString();

        hasItem = (DataManager.GetInstance().GetHasPetDataWithUnqIdx(petData.unqIdx) != null);

        if(hasItem)
        {
            buyBtnLabel.color = boughtAnimalColor;
            buyBtnCollider.enabled = false;
        }
        else
        {
            buyBtnLabel.color = notBuyAnimalColor;
            buyBtnCollider.enabled = true;
        }
    }

    public override void ClickedItem()
    {
        Debug.Log("Show Info Popop : " + nameLabel.text);
    }

    public override void ClearData()
    {
        petData = null;
    }
}

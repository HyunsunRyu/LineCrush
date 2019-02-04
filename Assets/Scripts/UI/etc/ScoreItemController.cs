using UnityEngine;
using System.Collections;

public class ScoreItemController : UIDragListItem
{
    [SerializeField] private UILabel label;
    
    public override void InitItem(int idx)
    {
        //if (!DataManager.GetInstance().TryGetAdoptAnimalData(ref animalData, idx))
        //{
        //    Debug.Log("something is wrong");
        //    animalData = null;
        //}

        UpdateUI();
    }

    public override void UpdateUI()
    {
        //label.text = animalData.unqIdx.ToString();
    }

    public override void ClickedItem()
    {
    }
}

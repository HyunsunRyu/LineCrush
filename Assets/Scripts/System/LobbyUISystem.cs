using UnityEngine;
using System.Collections.Generic;

public class LobbyUISystem : UISystem
{
    public enum RoomType
    {
        Lobby = 0,
        Room,
        Roof,
        None,
    }

    [SerializeField] private List<IDialog> dialogList;
    [SerializeField] private UILabel coinLabel;
    [SerializeField] private GameObject rewardNews;
    [SerializeField] private GameObject adoptNews;
    [SerializeField] private UIListDragController petListDragController;
    
    private List<int> petAnimationIdxList = new List<int>();

    private const int immFeedRate = 5;

    private Transform uiRoot;
    private int totalItemCount = 0;
    private Dictionary<string, string> buttonMethod = new Dictionary<string, string>();
    private List<RoomType> roomTypeList = new List<RoomType>();

    public override void InitSystem()
    {
        base.InitSystem();

        dialogDic.Clear();
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

        DataManager.GetInstance().SetCoinUICallback(UpdateCoin);

        UpdateAllNews();

        DataManager.GetInstance().SetAdoptPetCallback(UpdatePetTower);
    }

    public override void InitData()
    {
        UpdateCoin();
        SetAllPetsIntoRooms();
    }

    public override void ClearSystem()
    {
        DataManager.GetInstance().SetAdoptPetCallback(null);
        petListDragController.DeleteList(DeleteItem);
    }

    public Transform GetUIRoot()
    {
        if (uiRoot == null)
            uiRoot = transform.FindChild("MainRoot");
        return uiRoot;
    }

    public void OpenLobbySettupDialog()
    {
        OpenDialog(Define.DialogType.LobbyOptionDialog);
    }

    public void OpenGameReadyDialog()
    {
        if (LobbyManager.activeAdoptGuideTutorial)
            TutorialManager.GetInstance().CallReaction();

        OpenDialog(Define.DialogType.GameReadyDialog);
    }

    public void OpenRewardDialog()
    {
        if (LobbyManager.activeRewardGuideTutorial)
            TutorialManager.GetInstance().CallReaction();

        OpenDialog(Define.DialogType.RewardDialog);
    }

    public void OpenShopDialog()
    {
        DataManager.GetInstance().SetNeedCoin(0);
        OpenDialog(Define.DialogType.ShopDialog);
    }

    public void OpenInvenDialog()
    {
        if (LobbyManager.activeAdoptGuideTutorial)
            TutorialManager.GetInstance().CallReaction();

        DataManager.GetInstance().SetNowSelectedPetData(null);
        OpenDialog(Define.DialogType.InvenDialog);
    }

    private void UpdateCoin()
    {
        coinLabel.text = DataManager.GetInstance().coin.ToString();
    }

    public override void UpdateAllNews()
    {
        rewardNews.SetActive(HasRewardNews());
        adoptNews.SetActive(HasAdoptNews());
        petListDragController.UpdateAllItems();
    }

    public bool HasRewardNews()
    {
        bool hasNews = false;
        if (dialogDic.ContainsKey(Define.DialogType.RewardDialog))
            hasNews = dialogDic[Define.DialogType.RewardDialog].HasUpdateNews();

        return hasNews;
    }

    public bool HasAdoptNews()
    {
        bool hasNews = false;
        if (dialogDic.ContainsKey(Define.DialogType.InvenDialog))
            hasNews = dialogDic[Define.DialogType.InvenDialog].HasUpdateNews();

        return hasNews;
    }

    private void SetAllPetsIntoRooms()
    {
        petAnimationIdxList.Clear();
        for (int i = 0, max = DataManager.GetInstance().GetHasPetListCount(); i < max; i++)
        {
            petAnimationIdxList.Add(Random.Range(0, 4));
        }

        roomTypeList.Clear();
        if (DataManager.GetInstance().GetHasPetListCount() <= 0)
        {
            roomTypeList.Add(RoomType.None);
            roomTypeList.Add(RoomType.Roof);
            roomTypeList.Add(RoomType.Lobby);
        }
        else
        {
            roomTypeList.Add(RoomType.Roof);
            for (int i = 0, max = DataManager.GetInstance().GetHasPetListCount(); i < max; i++)
            {
                roomTypeList.Add(RoomType.Room);
            }
            roomTypeList.Add(RoomType.Lobby);
        }
        totalItemCount = roomTypeList.Count;

        buttonMethod.Clear();
        buttonMethod.Add("Room", "ClickCharacter");

        petListDragController.StartList(MakePetItem, totalItemCount, buttonMethod);
    }

    private PetController MakePetItem()
    {
        PetController item = PoolManager.GetObject<PetController>();
        item.SetClickCallback(OnClickAnimalItem);
        item.SetTypeCallback(GetRoomType);
        item.SetDataCallback(GetAnimalDataByIndex);
        item.SetAnimationIdxCallback(GetAnimationIndex);
        return item;
    }

    private void DeleteItem(UIDragListItem item)
    {
        item.ClearData();
        PoolManager.ReturnObject(item as PetController);
    }

    private void OnClickAnimalItem(PetController data)
    {
        DataManager.GetInstance().SetNowSelectedPetData(data.GetPetData());
        UISystem.OpenDialog(Define.DialogType.InvenDialog);
    }

    private RoomType GetRoomType(int idx)
    {
        if (roomTypeList.Count <= idx)
            return RoomType.None;

        return roomTypeList[idx];
    }

    private PetData GetAnimalDataByIndex(int idx)
    {
        if (GetRoomType(idx) == RoomType.Room)
        {
            //역순. //
            idx = totalItemCount - 2 - idx;
            return DataManager.GetInstance().GetHasPetDataWithNumber(idx);
        }
        return null;
    }

    private int GetAnimationIndex(int idx)
    {
        if (petAnimationIdxList.Count <= idx || idx >= 4)
            return 0;

        return petAnimationIdxList[idx];
    }

    private void UpdatePetTower()
    {
        petListDragController.DeleteList(DeleteItem);
        SetAllPetsIntoRooms();
        petListDragController.SetFocusItem(0);
    }
}

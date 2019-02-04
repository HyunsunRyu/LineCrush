using UnityEngine;

public class PetController : UIDragListItem
{
    [SerializeField] private GameObject lobby;
    [SerializeField] private GameObject roof;
    [SerializeField] private GameObject room;
    [SerializeField] private GameObject newIcon;
    [SerializeField] private UISprite characterImage;

    public int posIdx { get; private set; }
    public int animationIdx { get; private set; }

    private PetData petData;
    private LobbyUISystem.RoomType petType;
    private AnimationTable animData;

    private System.Action<PetController> clickItemCallback = null;
    private System.Func<int, LobbyUISystem.RoomType> getTypeFunc = null;
    private System.Func<int, PetData> getPetDataFunc = null;
    private System.Func<int, int> getAnimationIdxFunc = null;
    
    public override void InitItem(int idx)
    {
        this.idx = idx;

        petData = getPetDataFunc(idx);
        if (petData != null)
        {
            animData = DataManager.GetInstance().GetAnimation(petData.unqIdx);
        }
        else
        {
            animData = null;
        }

        petType = getTypeFunc(idx);

        UpdateUI();
    }

    public PetData GetPetData()
    {
        return petData;
    }

    public override void ClearData()
    {
    }

    public override void UpdateUI()
    {
        lobby.SetActive(petType == LobbyUISystem.RoomType.Lobby);
        roof.SetActive(petType == LobbyUISystem.RoomType.Roof);
        room.SetActive(petType == LobbyUISystem.RoomType.Room);

        if (petType == LobbyUISystem.RoomType.Room)
        {
            if (animData == null)
            {
                Debug.Log("something is wrong");
                return;
            }

            string image = "";
            if (animData.TryGetImage(getAnimationIdxFunc(idx), ref image))
            {
                characterImage.spriteName = image;
            }

            if (petData == null)
                return;

            if (petData.CanActiveSkillUpgrade())
                newIcon.SetActive(true);
            else if (petData.CanPassiveSkillUpgrade())
                newIcon.SetActive(true);
            else
                newIcon.SetActive(false);
        }
    }

    public override void ClickedItem()
    {
    }
    
    public void SetClickCallback(System.Action<PetController> callback)
    {
        clickItemCallback = callback;
    }

    public void SetTypeCallback(System.Func<int, LobbyUISystem.RoomType> func)
    {
        getTypeFunc = func;
    }

    public void SetDataCallback(System.Func<int, PetData> func)
    {
        getPetDataFunc = func;
    }

    public void SetAnimationIdxCallback(System.Func<int, int> func)
    {
        getAnimationIdxFunc = func;
    }

    private void ClickCharacter()
    {
        if (clickItemCallback != null)
            clickItemCallback(this);
    }

    public string GetPetName()
    {
        if (petData == null)
            return "";

        return petData.GetName();
    }
}
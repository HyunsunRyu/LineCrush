using UnityEngine;

public class TitleManager : SceneObject
{
    [SerializeField] private PopupSystem popupSystem;
    [SerializeField] private UISprite polarImage;
    [SerializeField] private BoxCollider playButtonCol;

    public override void ClearManager()
    {
    }

    protected override void Awake()
    {
        ScenesManager.AddScene(this);
        ScenesManager.SetEscapeMethod(ScenesManager.ExitGame);

        popupSystem.InitSystem();
    }

    protected override void OnEnable()
    {
        SoundManager.GetInstance().PlayBGM(Define.SoundType.LobbyBGM);

        popupSystem.InitData();
    }

    public void OnClickPlay()
    {
        SoundManager.GetInstance().PlaySound(Define.SoundType.Click);
        ScenesManager.ChangeScene("LobbyScene");
    }

    public void ChangePolarDepth(int depth)
    {
        polarImage.depth = depth;
    }

    public void ActivePlayButton()
    {
        playButtonCol.enabled = true;
    }

    public void InactivePlayButton()
    {
        playButtonCol.enabled = false;
    }
}

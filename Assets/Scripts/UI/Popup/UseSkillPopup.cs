using UnityEngine;
using System.Collections;

public class UseSkillPopup : IPopup
{
    [SerializeField] private AnimationCurve openMoveCurve;
    [SerializeField] private AnimationCurve closeMoveCurve;
    [SerializeField] private AnimationCurve characterShowCurve;
    [SerializeField] private AnimationCurve brightShowCurve;
    [SerializeField] private AnimationCurve brightRotateCurve;
    [SerializeField] private AnimationCurve characterHideCurve;
    [SerializeField] private AnimationCurve brightHideCurve;

    [SerializeField] private Transform character;
    [SerializeField] private Transform bright;

    [SerializeField] private UISprite characterImage;
    [SerializeField] private UISprite skillIcon;
    [SerializeField] private UILabel skillInfo;

    private System.Action openCallback;
    private System.Action closeCallback;

    public void SetData(PetData petData, System.Action closeCallback)
    {

        string image = "";
        if (petData.TryGetImage(0, ref image))
            characterImage.spriteName = image;

        skillIcon.spriteName = DataManager.GetInstance().GetSkillIcon(petData.activeSkillType);
        skillInfo.text = SkillManager.GetInstance().GetSkillInfo(petData.activeSkillType, petData.aSkillLv);
        this.closeCallback = closeCallback;
    }

    private void InitPopup()
    {
        character.gameObject.SetActive(false);
        bright.gameObject.SetActive(false);
    }

    protected override void UIOpenAnimation(System.Action callback)
    {
        openCallback = callback;
        InitPopup();

        SoundManager.GetInstance().PlaySound(Define.SoundType.ShowPopup);

        Vector3 start = new Vector3(ScreenSizeGetter.width * -0.5f, 0f, 0f);
        Vector3 end = Vector3.one;
        float animTime = 0.3f;

        AnimCurveController.Move(openMoveCurve, start, end, animTime, moveTarget, () =>
        {
            ShowCharacter();
            ShowBright();
            RotateBright();
            StartCoroutine(FuncClose());
        });
    }

    private void ShowCharacter()
    {
        Vector3 start = new Vector3(0f, -700f, 0f);
        Vector3 end = Vector3.zero;
        float animTime = 0.2f;

        character.localPosition = start;
        character.gameObject.SetActive(true);

        AnimCurveController.Move(characterShowCurve, start, end, animTime, character, null);

        StartCoroutine(PlayUseSkillSound());
    }

    private IEnumerator PlayUseSkillSound()
    {
        yield return new WaitForSeconds(0.2f);
        SoundManager.GetInstance().PlaySound(Define.SoundType.UseSkill);
    }

    private void ShowBright()
    {
        Vector3 start = new Vector3(0f, 0f, 1f);
        Vector3 end = Vector3.one;
        float animTime = 0.4f;

        bright.localScale = start;
        bright.gameObject.SetActive(true);

        AnimCurveController.Scale(brightShowCurve, start, end, animTime, bright, null);
    }

    private void RotateBright()
    {
        float start = 0f;
        float end = 90f;
        float animTime = 2f;

        bright.localRotation = Quaternion.identity;
        bright.gameObject.SetActive(true);

        AnimCurveController.Rotation(brightRotateCurve, Vector3.forward, start, end, animTime, bright, null);
    }

    private IEnumerator FuncClose()
    {
        if (openCallback != null)
            openCallback();

        yield return new WaitForSeconds(1.5f);
        HideBright();
        HideCharacter();
    }

    private void HideBright()
    {
        Vector3 start = Vector3.one;
        Vector3 end = new Vector3(0f, 0f, 1f);
        
        float animTime = 0.2f;
        
        AnimCurveController.Scale(brightHideCurve, start, end, animTime, bright, null);
    }

    private void HideCharacter()
    {
        Vector3 end = new Vector3(0f, -700f, 0f);
        Vector3 start = Vector3.zero;
        float animTime = 0.3f;

        SoundManager.GetInstance().PlaySound(Define.SoundType.HidePopup);

        AnimCurveController.Move(characterShowCurve, start, end, animTime, character, () =>
        {
            PopupSystem.ClosePopup(Define.PopupType.UseSkill, closeCallback);
        });
    }

    protected override void UICloseAnimation(System.Action callback)
    {
        Vector3 start = Vector3.one;
        Vector3 end = new Vector3(ScreenSizeGetter.width * 0.5f, 0f, 0f);
        
        float animTime = 0.2f;

        AnimCurveController.Move(closeMoveCurve, start, end, animTime, moveTarget, () =>
        {
            if (callback != null)
                callback();
        });
    }
}

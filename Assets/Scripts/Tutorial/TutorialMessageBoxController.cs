using UnityEngine;
using System.Collections;

public class TutorialMessageBoxController : MonoBehaviour
{
    [System.Serializable]
    private class AnimCurvePack
    {
        public AnimationCurve bodyCurve = null;
        public AnimationCurve characterCurve = null;
        public AnimationCurve messageCurve = null;
    }

    [SerializeField] private AnimCurvePack showCurve;
    [SerializeField] private AnimCurvePack hideCurve;
    [SerializeField] private UILabel message;
    [SerializeField] private Transform body;
    [SerializeField] private Transform bottom;
    [SerializeField] private Transform character;
    [SerializeField] private Transform messageBody;
    [SerializeField] private BoxCollider blocker;

    private int preTextIdx = -1;
    private bool bOpend = false;
    private float height = 0f;

    private const float baseBodyHideXPos = -760f;
    private const float baseBodyShowXPos = -390f;

    private const float baseCharacterHideYPos = -300f;
    private const float baseCharacterShowYPos = 0f;
    
    private const float showAnimTime = 1f;
    private const float hideAnimTime = 0.6f;

    private const float topHeight = 330f;
    private const float bottomHeight = -600f;

    public void Init()
    {
        blocker.enabled = false;
    }

    public void SetMessageBox(int textIdx, Vector3 pos, System.Action callback)
    {
        float value = (ScreenSizeGetter.width - ScreenSizeGetter.GetBaseScreenSize().x) * 0.5f;

        if (bOpend)
        {
            if (pos.y >= 0f && height != bottomHeight)
            {
                height = bottomHeight;
                body.localPosition = new Vector3(baseBodyShowXPos - value, height, 0f);
            }
            else if (pos.y < 0f && height != topHeight)
            {
                height = topHeight;
                body.localPosition = new Vector3(baseBodyShowXPos - value, height, 0f);
            }

            blocker.enabled = false;
            if (preTextIdx == textIdx)
            {
                if (callback != null)
                    callback();
            }
            else
            {
                preTextIdx = textIdx;
                message.text = DataManager.GetText(textIdx);
                if (callback != null)
                    callback();
            }
        }
        else
        {
            blocker.enabled = true;
            message.text = "";

            if (pos.y >= 0f)
                height = bottomHeight;
            else
                height = topHeight;
            
            Vector3 from = new Vector3(baseBodyHideXPos - value, height, 0f);
            Vector3 to = new Vector3(baseBodyShowXPos - value, height, 0f);
            AnimCurveController.Move(showCurve.bodyCurve, from, to, showAnimTime, body, null);

            from = new Vector3(0f, baseCharacterHideYPos, 0f);
            to = new Vector3(0f, baseCharacterShowYPos, 0f);
            AnimCurveController.Move(showCurve.characterCurve, from, to, showAnimTime, character, null);

            from = new Vector3(0f, 0f, 1f);
            to = Vector3.one;
            AnimCurveController.Scale(showCurve.messageCurve, from, to, showAnimTime, messageBody, ()=>
            {
                bOpend = true;
                if (preTextIdx == textIdx)
                {
                    if (callback != null)
                        callback();
                }
                else
                {
                    preTextIdx = textIdx;
                    message.text = DataManager.GetText(textIdx);
                    blocker.enabled = false;
                    if (callback != null)
                        callback();
                }
            });
        }
    }

    public void HideMessageBox(System.Action callback)
    {
        blocker.enabled = true;
        message.text = "";

        float value = (ScreenSizeGetter.width - ScreenSizeGetter.GetBaseScreenSize().x) * 0.5f;
        Vector3 from = new Vector3(baseBodyShowXPos - value, height, 0f);
        Vector3 to = new Vector3(baseBodyHideXPos - value, height, 0f);
        AnimCurveController.Move(hideCurve.bodyCurve, from, to, hideAnimTime, body, () =>
        {
            blocker.enabled = false;
            bOpend = false;
            if (callback != null)
                callback();
        });

        from = new Vector3(0f, baseCharacterShowYPos, 0f);
        to = new Vector3(0f, baseCharacterHideYPos, 0f);
        AnimCurveController.Move(hideCurve.characterCurve, from, to, hideAnimTime, character, null);

        from = Vector3.one;
        to = new Vector3(0f, 0f, 1f);
        AnimCurveController.Scale(hideCurve.messageCurve, from, to, hideAnimTime, messageBody, null);
    }

    public bool IsOpened()
    {
        return true;
    }
}

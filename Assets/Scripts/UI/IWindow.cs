using UnityEngine;

public abstract class IWindow : MonoBehaviour
{
    [SerializeField] protected Transform moveTarget;
    [SerializeField] private BoxCollider animBlocker;
    
    protected GameObject mObject;
    protected bool isAnim = false;
    
    public virtual void Init()
    {
        if (mObject == null)
            mObject = gameObject;

        mObject.SetActive(false);

        isAnim = false;

        if (animBlocker != null)
        {
            animBlocker.size = new Vector3(ScreenSizeGetter.width, ScreenSizeGetter.height, 0);
            animBlocker.enabled = false;
        }
    }
    
    public virtual void BeforeOpen() { }
    public virtual void AfterOpen() { }
    public virtual void BeforeClose() { }
    public virtual void AfterClose() { }

    protected void Open(System.Action callback)
    {
        if (isAnim)
            return;

        BeforeOpen();

        mObject.SetActive(true);
        isAnim = true;
        if (animBlocker != null)
            animBlocker.enabled = true;
        
        UIOpenAnimation(() =>
        {
            isAnim = false;
            if (animBlocker != null)
                animBlocker.enabled = false;
            if (callback != null)
            {
                callback();
            }
            AfterOpen();
        });
        
    }

    protected void Close(System.Action callback = null)
    {
        if (!mObject.activeSelf || isAnim)
            return;

        BeforeClose();

        isAnim = true;
        if (animBlocker != null)
            animBlocker.enabled = true;
        UICloseAnimation(() =>
        {
            isAnim = false;
            if (animBlocker != null)
                animBlocker.enabled = false;
            if (callback != null)
            {
                callback();
            }
            AfterClose();

            mObject.SetActive(false);
        });
    }

    protected abstract void UIOpenAnimation(System.Action callback);
    protected abstract void UICloseAnimation(System.Action callback);
}

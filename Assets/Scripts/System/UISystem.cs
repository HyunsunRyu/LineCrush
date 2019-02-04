using UnityEngine;
using System.Collections.Generic;

public class UISystem : ISystem
{
    [SerializeField] private GameObject dialogBlinder;
    [SerializeField] private AnimationCurve dialogOpenCurve;
    [SerializeField] private AnimationCurve dialogCloseCurve;

    private static UISystem instance;

    protected Dictionary<Define.DialogType, IDialog> dialogDic = new Dictionary<Define.DialogType, IDialog>();
    protected static Stack<Define.DialogType> openDialogStack = new Stack<Define.DialogType>();
    protected static Define.DialogType nowOpenDialog = Define.DialogType.None;

    public override void InitSystem()
    {
        openDialogStack.Clear();
        openDialogStack.Push(Define.DialogType.None);
        dialogBlinder.SetActive(false);

        instance = this;
    }

    public override void InitData()
    {
    }

    public override void ClearSystem()
    {
    }

    public static void OpenDialog(Define.DialogType dialogType, System.Action callback = null)
    {
        if (instance.dialogDic.ContainsKey(dialogType))
        {
            if (nowOpenDialog != Define.DialogType.None)
            {
                instance.JustCloseDialog(nowOpenDialog, () =>
                {
                    instance.JustOpenDialog(dialogType, () =>
                    {
                        if (callback != null)
                            callback();
                        openDialogStack.Push(dialogType);
                        nowOpenDialog = dialogType;
                    });
                });
            }
            else
            {
                instance.JustOpenDialog(dialogType, () =>
                {
                    if (callback != null)
                        callback();
                    openDialogStack.Push(dialogType);
                    nowOpenDialog = dialogType;
                });
            }   
        }
    }

    private void JustOpenDialog(Define.DialogType dialogType, System.Action callback)
    {
        if (dialogDic.ContainsKey(dialogType))
        {
            SoundManager.GetInstance().PlaySound(Define.SoundType.ShowDialog);

            dialogBlinder.SetActive(true);
            dialogDic[dialogType].OpenDialog(() =>
            {
                if (callback != null)
                    callback();
            });
        }
    }

    public static void CloseDialog(Define.DialogType dialogType, System.Action callback = null)
    {
        if (instance.dialogDic.ContainsKey(dialogType))
        {
            instance.JustCloseDialog(dialogType, () =>
            {
                openDialogStack.Pop();
                nowOpenDialog = openDialogStack.Peek();

                if (nowOpenDialog != Define.DialogType.None)
                    instance.JustOpenDialog(nowOpenDialog, null);
                else
                    instance.dialogBlinder.SetActive(false);

                if (callback != null)
                    callback();
            });
        }
    }

    private void JustCloseDialog(Define.DialogType dialogType, System.Action callback)
    {
        if (dialogDic.ContainsKey(dialogType))
        {
            SoundManager.GetInstance().PlaySound(Define.SoundType.HideDialog);
            dialogDic[dialogType].CloseDialog(() =>
            {
                if (callback != null)
                    callback();
            });
        }
    }

    public static void CloseAllDialog(System.Action callback = null)
    {
        if (instance.dialogDic.ContainsKey(nowOpenDialog) && nowOpenDialog != Define.DialogType.None)
        {
            instance.JustCloseDialog(nowOpenDialog, () =>
            {
                openDialogStack.Clear();
                openDialogStack.Push(Define.DialogType.None);

                instance.dialogBlinder.SetActive(false);

                if (callback != null)
                    callback();
            });
        }
    }

    public static void DialogStackClear()
    {
        openDialogStack.Clear();
    }

    public static AnimationCurve GetOpenDialogCurve()
    {
        if (instance == null)
            return null;
        return instance.dialogOpenCurve;
    }

    public static AnimationCurve GetCloseDialogCurve()
    {
        if (instance == null)
            return null;
        return instance.dialogCloseCurve;
    }

    public static void UpdateNews()
    {
        if (instance == null)
            return;
        instance.UpdateAllNews();
    }

    public virtual void UpdateAllNews()
    {
    }
}

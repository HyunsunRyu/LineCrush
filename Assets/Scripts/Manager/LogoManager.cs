using UnityEngine;
using System.Collections;
using System;

public class LogoManager : SceneObject
{
    public override void ClearManager()
    {
    }

    protected override void Awake()
    {
        ScenesManager.AddScene(this);
        ScenesManager.SetEscapeMethod(null);
    }

    protected override void OnEnable()
    {
        StartCoroutine(DoNext(() =>
        {
            ScenesManager.ChangeScene("TitleScene");
        }));
    }

    private IEnumerator DoNext(Action callback)
    {
        yield return new WaitForSeconds(Define.keepLogoTime);

        if(callback != null)
            callback();
    }
}

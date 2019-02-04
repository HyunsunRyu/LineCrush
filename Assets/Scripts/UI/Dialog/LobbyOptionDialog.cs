using UnityEngine;
using System.Collections;
using System;

public class LobbyOptionDialog : IDialog
{
    [SerializeField] private UILabel titleLabel;
    [SerializeField] private SwitchController soundSwitch;
    [SerializeField] private SwitchController bgmSwitch;

    private float beforeSoundVolume, beforeBGMVolume;
    
    public override void BeforeOpen()
    {
        titleLabel.text = DataManager.GetText(TextTable.setupKey);

        soundSwitch.Init(SetSoundCallback);
        bgmSwitch.Init(SetBGMCallback);

        soundSwitch.SetOnOff(SoundManager.GetInstance().soundVolume > 0f);
        bgmSwitch.SetOnOff(SoundManager.GetInstance().bgmVolume > 0f);

        beforeSoundVolume = SoundManager.GetInstance().soundVolume;
        beforeBGMVolume = SoundManager.GetInstance().bgmVolume;
    }

    private void SetSoundCallback(bool result)
    {
        SoundManager.GetInstance().SetSoundVolume(result ? 1f : 0f);
    }

    private void SetBGMCallback(bool result)
    {
        SoundManager.GetInstance().SetBGMVolume(result ? 1f : 0f);
    }

    public void OnCloseOptionDialog()
    {
        if (beforeSoundVolume != SoundManager.GetInstance().soundVolume ||
            beforeBGMVolume != SoundManager.GetInstance().bgmVolume)
        {
            DataManager.GetInstance().SaveAllData();
        }
        UISystem.CloseDialog(Define.DialogType.LobbyOptionDialog);
    }
}

using UnityEngine;
using System.Collections;
using System;

public class LogUIItem : UIDragListItem
{
	public UILabel logLabel;
	public UILabel countLabel;

	private int logIdx;
    private LogUI dialog = null;
    private LogData logData = null;

    public override void InitItem(int idx)
    {
        logData = dialog.GetSelectedLogData(idx);

        UpdateUI();
    }

    public void SetDialog(LogUI dialog)
    {
        this.dialog = dialog;
    }

    public override void ClickedItem()
    {
        dialog.ShowPopup(logData);
    }

    public override void UpdateUI()
    {
        logLabel.text = logData.log;

        logLabel.color = Color.black;

        if (logData.logType == LogType.Error)
            logLabel.color = Color.red;
        else if (logData.logType == LogType.Warning)
            logLabel.color = Color.blue;
        else if (logData.logType == LogType.Exception)
            logLabel.color = Color.red;

        logIdx = logData.index;

        countLabel.text = logIdx.ToString();
    }
}

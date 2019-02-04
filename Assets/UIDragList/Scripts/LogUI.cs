using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void delMessage();

public class LogData
{
    public int index;
    public string log;
    public LogType logType;
    public string stack;

    private static string CutString(string str, int max)
    {
        if (str.Length > max)
            str = str.Substring(0, max);
        return str;
    }

    public static LogData MakeLogData(int index, string log, string stack, LogType logType)
    {
        LogData logData = new LogData();

        logData.index = index;
        logData.log = CutString(log, 400);
        logData.stack = CutString(stack, 700);
        logData.logType = logType;

        return logData;
    }
}

public enum LogCustomType
{
    Exception = 0,
    Error,
    Log,
    Assert,
    Warning,
    All,
}

public class LogUI : MonoBehaviour
{
    public UIListDragController uiDrag;
    public GameObject popup;
    public GameObject logDialog;
    public UILabel changeLabel;
    public UILabel logDataLabel, logStackLabel, countLabel;

    private List<LogData> allLogList = new List<LogData>();
    private List<LogData> selectedLogDataList = new List<LogData>();

    private static bool onPopup = false;
    private static bool onDialog = false;
    public static bool activeUI { get { return onPopup || onDialog; } }

    private static LogUI instance = null;

    private LogCustomType nowLogType = LogCustomType.All;

    private const string fileName = "_LgER.dat";
    private List<LogData> sortList = new List<LogData>();
    

    void Awake()
    {
        Init();
    }

    void Init()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        logDialog.SetActive(false);
        popup.SetActive(false);
        onPopup = false;
        onDialog = false;
        Application.logMessageReceived += HandleLog;

        DontDestroyOnLoad(gameObject);
    }

    //로그 발생 시 호출되는 Callback 함수. //
    private void HandleLog(string log, string stack, LogType type)
    {
        int idx = allLogList.Count;
        allLogList.Add(LogData.MakeLogData(idx, log, stack, type));

        if (type == LogType.Error || type == LogType.Exception)
        {
            ShowPopup(allLogList[idx]);

            if (logDialog.activeSelf == true)
            {
                uiDrag.DeleteList(DeleteItem);
                nowLogType = ConvertLogType(type);
                SortLogList(nowLogType);
            }
        }
    }

    //해당 LogData를 팝업으로 표시. //
    public void ShowPopup(LogData logData)
    {
        onPopup = true;

        if (popup.activeSelf == false)
            popup.SetActive(true);

        countLabel.text = logData.index.ToString();
        logDataLabel.text = logData.log;
        logStackLabel.text = logData.stack;
    }

    //팝업 닫기. //
    public void ClosePopup()
    {
        popup.SetActive(false);
        onPopup = false;
    }

    //다이얼로그 열기. //
    public static void ShowDialog()
    {
        if (instance == null)
            return;
        if (onDialog)
            return;

        onDialog = true;
        instance.nowLogType = LogCustomType.All;
        instance.SortLogList(instance.nowLogType);
        instance.logDialog.SetActive(true);
    }

    public static void HideDialog()
    {
        if (instance == null)
            return;

        instance.HideLogDialog();
    }

    //다이얼로그 닫기. //
    public void HideLogDialog()
    {
        uiDrag.DeleteList(DeleteItem);
        logDialog.SetActive(false);

        onDialog = false;
    }

    //모든 로그 기록을 삭제하는 함수. //
    public void ClearAllLog()
    {
        nowLogType = LogCustomType.All;
        allLogList.Clear();
        uiDrag.DeleteList(DeleteItem);
        SortLogList(nowLogType);
    }

    public void ChangeLogType()
    {
        uiDrag.DeleteList(DeleteItem);

        int logTypeIdx = ((int)nowLogType + 1);
        if (logTypeIdx > (int)LogCustomType.All)
            logTypeIdx = 0;
        nowLogType = (LogCustomType)logTypeIdx;
        SortLogList(nowLogType);
    }

    private LogType ConvertLogType(LogCustomType type)
    {
        LogType logType = LogType.Log;

        if (type == LogCustomType.Assert)
            logType = LogType.Assert;
        else if (type == LogCustomType.Error)
            logType = LogType.Error;
        else if (type == LogCustomType.Exception)
            logType = LogType.Exception;
        else if (type == LogCustomType.Log)
            logType = LogType.Log;
        else if (type == LogCustomType.Warning)
            logType = LogType.Warning;
        return logType;
    }

    private LogCustomType ConvertLogType(LogType type)
    {
        if (type == LogType.Assert)
            return LogCustomType.Assert;
        else if (type == LogType.Error)
            return LogCustomType.Error;
        else if (type == LogType.Exception)
            return LogCustomType.Exception;
        else if (type == LogType.Log)
            return LogCustomType.Log;

        return LogCustomType.Warning;
    }

    private List<LogData> GetLogList(LogCustomType type)
    {
        if (type == LogCustomType.All)
            return allLogList;

        LogType logType = ConvertLogType(type);

        sortList.Clear();
        foreach (LogData data in allLogList)
        {
            if (data.logType == logType)
                sortList.Add(data);
        }
        return sortList;
    }

    private void SortLogList(LogCustomType type)
    {
        nowLogType = type;

        changeLabel.text = nowLogType.ToString();

        selectedLogDataList = GetLogList(nowLogType);
        uiDrag.StartList(GetListItem, selectedLogDataList.Count);
    }

    private LogUIItem GetListItem()
    {
        LogUIItem item = PoolManager.GetObject<LogUIItem>();
        item.SetDialog(this);
        return item;
    }

    private void DeleteItem(UIDragListItem item)
    {
        PoolManager.ReturnObject(item as LogUIItem);
    }

    public LogData GetSelectedLogData(int idx)
    {
        if (selectedLogDataList.Count > idx)
            return selectedLogDataList[idx];

        return null;
    }
}
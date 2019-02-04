using UnityEngine;
using System.Collections.Generic;

public class TutorialTable : DataController.MixedBase
{
    public Define.TutorialType tutorialType { get; private set; }
    public Define.TutorialEmphasis emphasis { get; private set; }
    public int textIdx { get; private set; }
    private Define.Pivot pivot;
    public Vector3 targetPos { get; private set; }
    public Vector3 targetSize { get; private set; }
    public Vector3 blockPos { get; private set; }
    public Vector3 blockSize { get; private set; }
    
    public string method { get; private set; }

    public override bool TryLoadTable(string[] str, out int key)
    {
        int arrayIdx = 0;
        try
        {
            key = int.Parse(str[arrayIdx++]);
            tutorialType = (Define.TutorialType)key;
            emphasis = (Define.TutorialEmphasis)int.Parse(str[arrayIdx++]);
            textIdx = int.Parse(str[arrayIdx++]);
            pivot = (Define.Pivot)int.Parse(str[arrayIdx++]);

            float x = float.Parse(str[arrayIdx++]);
            float y = float.Parse(str[arrayIdx++]);
            if (pivot == Define.Pivot.Left || pivot == Define.Pivot.LeftBottom)
                x -= ScreenSizeGetter.halfWidth;
            if (pivot == Define.Pivot.Bottom || pivot == Define.Pivot.LeftBottom)
                y -= ScreenSizeGetter.halfHeight;
            targetPos = new Vector3(x, y, 0f);

            x = float.Parse(str[arrayIdx++]);
            y = float.Parse(str[arrayIdx++]);
            targetSize = new Vector3(x, y, 0f);

            x = float.Parse(str[arrayIdx++]);
            y = float.Parse(str[arrayIdx++]);
            if (pivot == Define.Pivot.Left || pivot == Define.Pivot.LeftBottom)
                x -= ScreenSizeGetter.halfWidth;
            if (pivot == Define.Pivot.Bottom || pivot == Define.Pivot.LeftBottom)
                y -= ScreenSizeGetter.halfHeight;
            blockPos = new Vector3(x, y, 0f);

            x = float.Parse(str[arrayIdx++]);
            y = float.Parse(str[arrayIdx++]);
            blockSize = new Vector3(x, y, 0f);
            
            method = str[arrayIdx++];
            
            if (str.Length != arrayIdx)
                Debug.Log(str.Length + " : " + arrayIdx);
        }
        catch (System.Exception e)
        {
            key = -1;
            Debug.Log(arrayIdx);
            Debug.LogError(e.ToString());
            return false;
        }
        return true;
    }
}

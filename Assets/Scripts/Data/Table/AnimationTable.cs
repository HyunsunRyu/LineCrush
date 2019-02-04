using UnityEngine;
using System.Collections.Generic;

public class AnimationTable : DataController.ListBase
{
    public int unqIdx { get; private set; }
    private List<string> images;
    private string profile;
    private string onGameImage;
    private string popupOutline;

    public override bool TryLoadTable(string[] str)
    {
        images = new List<string>();

        int arrayIdx = 0;
        try
        {
            unqIdx = int.Parse(str[arrayIdx++]);
            for (int i = 0; i < 4; i++)
            {
                images.Add(str[arrayIdx++]);
            }
            profile = str[arrayIdx++];
            onGameImage = str[arrayIdx++];
            popupOutline = str[arrayIdx++];
        }
        catch (System.Exception e)
        {
            Debug.Log(arrayIdx);
            Debug.LogError(e.ToString());
            return false;
        }

        return true;
    }

    public bool TryGetImage(int idx, ref string image)
    {
        if (images == null || images.Count <= idx)
            return false;

        image = images[idx];
        return true;
    }

    public string GetProfile(){ return profile; }
    public string GetOnGameImage(){ return onGameImage; }
    public string GetPopupOutline() { return popupOutline; }
}
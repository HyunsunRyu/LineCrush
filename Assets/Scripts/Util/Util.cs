using UnityEngine;
using System.Collections;

public class Util
{
    private static readonly float inv255 = 1f / 255f;

    public static Vector3 GetBlockPosition(int x, int y, int maxX, int maxY, float distance, Vector3 centerPos)
    {
        float xPos = distance * ((maxX - 1) * -0.5f + x) + centerPos.x;
        float yPos = distance * ((maxY - 1) * -0.5f + y) + centerPos.y;
        return new Vector3(xPos, yPos, 0f);
    }

    public static Color GetColor(int r, int g, int b)
    {
        return new Color(r * inv255, g * inv255, b * inv255);
    }

    public static int GetRandomValue(int min, int max)
    {
        return Random.Range(min, max);
    }

    public static void DebugLog(string str)
    {
        Debug.Log(str);
    }
}

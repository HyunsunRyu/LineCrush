using UnityEngine;
using System.Collections.Generic;

public struct ClearTileData
{
    private Dictionary<int, int> clearedTiles;
    private int totalClearedLines;
    private int totalClearedTiles;

    public void ClearData()
    {
        if (clearedTiles == null)
            clearedTiles = new Dictionary<int, int>();
        clearedTiles.Clear();

        totalClearedLines = 0;
        totalClearedTiles = 0;
    }

    public void AddTotalClearedLines()
    {
        totalClearedLines++;
    }

    public void AddClearedTile(int colorType)
    {
        if (!clearedTiles.ContainsKey(colorType))
            clearedTiles.Add(colorType, 0);

        clearedTiles[colorType]++;
        totalClearedTiles++;
    }

    public int GetTotalClearedLineCount()
    {
        return totalClearedLines;
    }

    public bool TryGetClearedTilesCount(int colorType, out int count)
    {
        count = 0;
        if (clearedTiles.ContainsKey(colorType))
        {
            count = clearedTiles[colorType];
            return true;
        }
        return false;
    }

    public int GetTotalClearedTilesCount()
    {
        return totalClearedTiles;
    }
}

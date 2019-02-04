using UnityEngine;
using System.Collections.Generic;

public class MapData
{
    private TileController[,] blockMap = new TileController[Define.width, Define.height];
    private TileController[,] tempMap = new TileController[Define.width, Define.height];
    private Pos[,] convertPosMap = new Pos[Define.width, Define.height];

    private static MapData instance = null;

    public static MapData GetInstance()
    {
        if (instance == null)
            instance = new MapData();
        return instance;
    }

    public void InitData()
    {
    }

    public TileController GetBlockOnMap(int x, int y)
    {
        if (x >= Define.width || y >= Define.height)
            Debug.Log("something is wrong");
        return blockMap[x, y];
    }

    public TileController GetBlockOnMap(Pos pos)
    {
        return GetBlockOnMap(pos.x, pos.y);
    }

    public void SetBlockOnMap(int x, int y, TileController block)
    {
        blockMap[x, y] = block;
    }

    public void SetBlockOnMap(Pos pos, TileController block)
    {
        SetBlockOnMap(pos.x, pos.y, block);
    }

    public void RelinkBlockMap(Pos[,] posMap)
    {
        //받아온 맵은 기존 x, y에 있던 block이 현재 어디에 있는가에 대한 정보값이고, //
        //필요한 맵은 현 x, y에 기존의 어디에 있던 block이 있는가에 대한 정보값이다. //
        for (int y = 0; y < Define.height; y++)
        {
            for (int x = 0; x < Define.width; x++)
            {
                Pos pos = posMap[x, y];
                convertPosMap[pos.x, pos.y] = new Pos(x, y);
            }
        }

        //데이터 보존을 위한 데이터 복사. //
        for (int y = 0; y < Define.height; y++)
        {
            for (int x = 0; x < Define.width; x++)
            {
                tempMap[x, y] = blockMap[x, y];
            }
        }

        //모든 기존 연결을 끊고, convertPosMap(x, y)에 있는 block을 blockMap(x, y)와 연결시킨다. //
        for (int y = 0; y < Define.height; y++)
        {
            for (int x = 0; x < Define.width; x++)
            {
                Pos pos = convertPosMap[x, y];
                blockMap[x, y] = tempMap[pos.x, pos.y];
            }
        }
    }
}

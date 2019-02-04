using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct TileData
{
    public enum BlockType
    {
        None = 0,   //아무것도 없는. 비어있는. //
        Emphasis,   //단순 강조. 미리보기 기능. //
        Normal,     //기본 타일. //
        FillBlocks, //스킬. 놓았을 때 해당 타일들의 상하좌우 라인을 모두 타일로 채워넣음. //
        //CountBlock, //여러번 깨야 완전히 깨지는 타일. //
        //TypeBlock,  //같은 색으로 깨야만 깨지는 타일. //
        //StoneBlock  //아예 깨지지 않는 타일. //
    }

    private BlockType blockType;
    private int colorType;
    private PuzzleTable tileTable;

    private const string blankBlockImageName = "block_blank";

    public void CopyData(TileData data)
    {
        blockType = data.blockType;
        colorType = data.colorType;
        tileTable = data.tileTable;
    }
    
    public bool IsSameTile(BlockType type)
    {
        return blockType == type;
    }

    public int GetRandomTileTypeType()
    {
        return Random.Range(0, Define.selectedPetsCount);
    }

    public void SetNoneTile()
    {
        blockType = BlockType.None;
    }
    
    public void SetNormalTile(int colorType)
    {
        blockType = BlockType.Normal;
        this.colorType = colorType;
    }

    public void SetFillLinesTile()
    {
        blockType = BlockType.FillBlocks;
    }

    public void SetTileType(BlockType type)
    {
        blockType = type;
    }

    public string GetTileName()
    {
        if (blockType == BlockType.Normal)
        {
            switch (colorType)
            {
                case 0:
                    return "red_tile";
                case 1:
                    return "blue_tile";
                case 2:
                default:
                    return "green_tile";
            }
        }
        return "tile_back";
    }

    public bool TryGetTileColorType(out int tileColorType)
    {
        tileColorType = 0;
        if (blockType == BlockType.Normal || blockType == BlockType.FillBlocks)
        {
            tileColorType = this.colorType;
            return true;
        }
        return false;
    }

    public BlockType GetTileType()
    {
        return blockType;
    }

    public string GetBaseTileName()
    {
        return blankBlockImageName;
    }

    public bool TryGetTileName(out string tileName)
    {
        int colorType;
        if (TryGetTileColorType(out colorType))
        {
            switch (colorType)
            {
                case 0:
                    tileName = "block_red";
                    break;
                case 1:
                    tileName = "block_blue";
                    break;
                default:
                case 2:
                    tileName = "block_green";
                    break;
            }
            return true;
        }
        tileName = "";
        return false;
    }
}
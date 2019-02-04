using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapAssistance : Assistance
{
    [SerializeField] private AnimationCurve[] moveCurves;
    [SerializeField] private AnimationCurve[] speedCurves;

    private List<PuzzleController> tileList = new List<PuzzleController>();
    private RandomTable tileTypeRandomTable;
    private List<Pos> emphasisTiles = new List<Pos>();
    private MapData mapData;
    private int[] checkPointMap = new int[Define.width + Define.height];
    private bool[,] clearTileMap = new bool[Define.width, Define.height];
    private Pos[,] toMoveMap = new Pos[Define.width, Define.height];
    private Pos[,] fromMoveMap = new Pos[Define.width, Define.height];
    private List<Pos> clearEffectList = new List<Pos>();
    private List<PuzzleController> addPuzzleList = new List<PuzzleController>();

    private ClearTileData clearTileData = new ClearTileData();

    public static bool bClearingTiles { get; private set; }

    private ScoreAssistance scoreAssistance;
    private PetAssistance petAssistance;

    private bool activeGameGuideTutorial = false;

    private readonly Vector3[] expPos = new Vector3[]
    {
        new Vector3(-300f, -482f, 0f),
        new Vector3(0f, -482f, 0f),
        new Vector3(300f, -482f, 0f)
    };
    
    public override void Init(TileSystem tileSystem)
    {
        base.Init(tileSystem);

        InitTileTypeRandomTable();
        mapData = MapData.GetInstance();

        scoreAssistance = GetAssistance(typeof(ScoreAssistance).Name) as ScoreAssistance;
        petAssistance = GetAssistance(typeof(PetAssistance).Name) as PetAssistance;
    }

    public override void Enable()
    {
        bClearingTiles = false;

        InitBlockMap();
    }

    public override void ClearMemory()
    {
        for (int i = 0, max = tileList.Count; i < max; i++)
        {
            PoolManager.ReturnObject<PuzzleController>(tileList[i]);
        }
        tileList.Clear();

        ReturnAllBlocks();
    }

    public void SetGameGuideTutorial(bool onOff)
    {
        activeGameGuideTutorial = onOff;
    }

    private void InitTileTypeRandomTable()
    {
        tileTypeRandomTable = RandomTable.MakeRandomTable();
        for (int i = 0, max = DataManager.GetInstance().GetTileTableListCount(); i < max; i++)
        {
            PuzzleTable table = DataManager.GetInstance().GetTileTable(i);
            tileTypeRandomTable.AddRate(table.idx, table.rate);
        }
    }

    public void GameOver()
    {
        ReturnPuzzles();
    }

    public void ReturnPuzzles()
    {
        if (PuzzleController.clickedTile != null)
        {
            PuzzleReturntoReadyLine(PuzzleController.clickedTile);
            UpdateMap();
        }
    }

    private void ReturnAllBlocks()
    {
        for (int y = 0; y < Define.height; y++)
        {
            for (int x = 0; x < Define.width; x++)
            {
                if (mapData.GetBlockOnMap(x, y) != null)
                    PoolManager.ReturnObject<TileController>(mapData.GetBlockOnMap(x, y));
            }
        }
    }

    private void InitBlockMap()
    {
        for (int y = 0; y < Define.height; y++)
        {
            for (int x = 0; x < Define.width; x++)
            {
                TileController block = PoolManager.GetObject<TileController>();
                block.mTrans.parent = GameManager.tileRoot;
                block.mTrans.localScale = Vector3.one;
                block.mTrans.localPosition = GetBlockPosition(x, y);
                block.SetData(x, y);
                block.SetNoneTile();

                mapData.SetBlockOnMap(x, y, block);
            }
        }
    }

    public void ClearMap()
    {
        for (int y = 0; y < Define.height; y++)
        {
            for (int x = 0; x < Define.width; x++)
            {
                mapData.GetBlockOnMap(x, y).SetNoneTile();
            }
        }
    }

    private Vector3 GetBlockPosition(Pos pos)
    {
        return GetBlockPosition(pos.x, pos.y);
    }

    private Vector3 GetBlockPosition(int x, int y)
    {
        return Util.GetBlockPosition(x, y, Define.width, Define.height, Define.blockDistance, Define.blockCenterPos);
    }

    public IEnumerator CallInPuzzleTiles(List<PuzzleController> puzzleList, System.Action callback = null)
    {
        //우선 현재 생성되어있는 남아있는 타일들을 좌측으로 이동시킨다. //
        yield return StartCoroutine(MoveRamainingTiles());

        if (puzzleList == null)
        {
            addPuzzleList.Clear();
            if (activeGameGuideTutorial)
            {
                for (int i = tileList.Count; i < Define.readyBlockCount; i++)
                    addPuzzleList.Add(MakeTutorialTile(i));
            }
            else
            {
                for (int i = tileList.Count; i < Define.readyBlockCount; i++)
                    addPuzzleList.Add(MakeRandomTile());
            }
            puzzleList = addPuzzleList;
        }

        //부족하여 추가생성이 필요하다면, 타일들을 생성하여 채워넣고 이동시킨다. //
        yield return StartCoroutine(FillTiles(puzzleList));

        if (callback != null)
            callback();

        //게임 진행이 불가능하면, 게임오버. //
        if (CheckGameEnd())
            GameManager.GameOver(false);
    }

    //남은 퍼즐 타일들을 빈공간 없이 좌측으로 이동. //
    private IEnumerator MoveRamainingTiles()
    {
        for (int i = 0, max = tileList.Count; i < max; i++)
        {
            if (tileList[i].tileIdx > i)
            {
                tileList[i].Move(i);
                yield return new WaitForSeconds(Define.readyTileMoveTime);
            }
        }
    }

    //빈 공간이 있다면, 새로 타일을 생성하여 빈 공간으로 이동. //
    private IEnumerator FillTiles(List<PuzzleController> puzzleList)
    {
        for (int i = 0, max = puzzleList.Count; i< max; i++)
        {
            if (tileList.Count >= Define.readyBlockCount)
                break;

            PuzzleController tile = puzzleList[i];
            tile.Move(tileList.Count);
            tileList.Add(tile);

            yield return new WaitForSeconds(Define.readyTileMoveTime);
        }
    }

    //랜덤하게 퍼즐 타일을 생성. //
    private PuzzleController MakeRandomTile()
    {
        PuzzleController tile = PoolManager.GetObject<PuzzleController>();

        TileData tileData = GetTileData();
        PuzzleTable puzzleTable = DataManager.GetInstance().GetTileTable(tileTypeRandomTable.GetRandomIdx());
        
        tile.SetData(puzzleTable, tileData, Random.Range(0, 4));
        return tile;
    }

    private PuzzleController MakeTutorialTile(int idx)
    {
        PuzzleController tile = PoolManager.GetObject<PuzzleController>();

        TileData tileData = new TileData();
        tileData.SetNormalTile(idx);

        int tileType, rotation;
        switch (idx)
        {
            case 0:
                tileType = 0;
                rotation = 1;
                break;
            case 1:
                tileType = 5;
                rotation = 0;
                break;
            case 2:
            default:
                tileType = 3;
                rotation = 0;
                break;
        }

        PuzzleTable puzzleTable = DataManager.GetInstance().GetTileTable(tileType);

        tile.SetData(puzzleTable, tileData, rotation);
        return tile;
    }

    private PuzzleController MakeTile(List<Define.PuzzleType> puzzleType)
    {
        PuzzleController tile = PoolManager.GetObject<PuzzleController>();

        TileData tileData = GetTileData();
        int idx = (int)puzzleType[Random.Range(0, puzzleType.Count)];
        PuzzleTable puzzleTable = DataManager.GetInstance().GetTileTable(idx);

        tile.SetData(puzzleTable, tileData, Random.Range(0, 4));
        return tile;
    }

    private TileData GetTileData()
    {
        TileData data = new TileData();
        data.SetNormalTile(data.GetRandomTileTypeType());
        return data;
    }

    //드래그 도중 해당 타일이 맵에 위치해 있을 때 강조. //
    public void EmphasisTilesOnMap(Pos pos, PuzzleController tile)
    {
        if (tile == null)
            return;

        //이미 이전에 등록된 강조된 블록들은 정상화 시켜야 한다. //
        ClearEmphasisBlocks();
        
        //정상적이라면 해당 강조 위치의 블럭들을 강조. //
        if (!TryEmphasisBlocks(pos, tile))
        {
            EmphasisBlocks(tile.tileType);
        }
    }

    //이미 강조된 기존 블록들은 정상화. //
    private void ClearEmphasisBlocks()
    {
        for (int i = 0, max = emphasisTiles.Count; i < max; i++)
        {
            Pos pos = emphasisTiles[i];
            mapData.GetBlockOnMap(pos).SetNoneTile();
        }
        emphasisTiles.Clear();
    }

    //정상적으로 강조할 수 있는 블록들인지 확인 후 정상적이면 체크. //
    private bool TryEmphasisBlocks(Pos pos, PuzzleController tile)
    {
        bool wrongPosOnMap = false;
        for (int i = 0, max = tile.blockPos.Count; i < max; i++)
        {
            int x = tile.blockPos[i].x + pos.x;
            int y = tile.blockPos[i].y + pos.y;
            if (x < 0 || x >= Define.width || y < 0 || y >= Define.height)
            {
                wrongPosOnMap = true;
                emphasisTiles.Clear();
                break;
            }
            if (!mapData.GetBlockOnMap(x, y).IsSame(TileData.BlockType.None))
            {
                wrongPosOnMap = true;
                emphasisTiles.Clear();
                break;
            }

            emphasisTiles.Add(new Pos(x, y));
        }
        return wrongPosOnMap;
    }

    //강조하기 위해 체크된 블록들을 강조. //
    private void EmphasisBlocks(TileData type)
    {
        int max = emphasisTiles.Count;
        for (int i = 0; i < max; i++)
        {
            Pos pos = emphasisTiles[i];
            if (mapData.GetBlockOnMap(pos).IsSame(TileData.BlockType.None) || mapData.GetBlockOnMap(pos).IsSame(TileData.BlockType.Emphasis))
            {
                mapData.GetBlockOnMap(pos).EmphasisTile(type);
            }
        }
    }

    //해당 타일을 해당 위치에 놓을 수 있는가? //
    public bool IsPossibleToPutOn()
    {
        return (emphasisTiles.Count > 0);
    }

    //해당 타일을 맵 위에 놓는다. //
    public void PutPuzzleOnMap(PuzzleController tile)
    {
        tileList.Remove(tile);
        PoolManager.ReturnObject<PuzzleController>(tile);

        for (int i = 0, max = emphasisTiles.Count; i < max; i++)
        {
            mapData.GetBlockOnMap(emphasisTiles[i]).SetTileType(tile.tileType);
        }
    }

    public void FillLines(PuzzleController tile, int side)
    {
        for (int y = 0; y < Define.height; y++)
        {
            for (int x = 0; x < Define.width; x++)
            {
                if (mapData.GetBlockOnMap(x, y).IsSame(TileData.BlockType.None))
                {
                    for (int i = 0, max = emphasisTiles.Count; i < max; i++)
                    {
                        if ((emphasisTiles[i].x == x && side == 0) || (emphasisTiles[i].y == y && side == 1))
                        {
                            mapData.GetBlockOnMap(x, y).SetTileType(tile.tileType);
                            break;
                        }
                    }
                }
            }
        }
    }

    //해당 퍼즐을 원래 대기라인으로 돌려놓는다. //
    public void PuzzleReturntoReadyLine(PuzzleController tile)
    {
        StartCoroutine(tile.PositionPuzzleBasic());
    }

    //제거할 블록이 있는지 모든 블록들을 계산한다. //
    public void CalculateAllClearTiles()
    {
        //초기화. //
        for (int i = 0; i < Define.height + Define.width; i++)
        {
            checkPointMap[i] = 0;
        }
        clearTileData.ClearData();
        bool bKeepCalculate = false;

        //각각 한 횡, 열로 몇개의 타일이 셋팅되어있는지 확인하기 위한 작업. // 
        for (int y = 0; y < Define.height; y++)
        {
            for (int x = 0; x < Define.width; x++)
            {
                if (!mapData.GetBlockOnMap(x, y).IsSame(TileData.BlockType.None))
                {
                    checkPointMap[x]++;
                    checkPointMap[Define.width + y]++;

                    if (!bKeepCalculate)
                        bKeepCalculate = true;
                }
            }
        }

        //최종적으로 지워질 라인 수. //
        if (bKeepCalculate)
        {
            bKeepCalculate = false;
            for (int i = 0; i < Define.width + Define.height; i++)
            {
                if (i < Define.width && checkPointMap[i] == Define.height)
                {
                    clearTileData.AddTotalClearedLines();

                    if (!bKeepCalculate)
                        bKeepCalculate = true;
                }
                else if (i >= Define.width && checkPointMap[i] == Define.width)
                {
                    clearTileData.AddTotalClearedLines();

                    if (!bKeepCalculate)
                        bKeepCalculate = true;
                }
            }
        }

        //최종적으로 지워질 타일 수. //
        if (bKeepCalculate)
        {
            for (int y = 0; y < Define.height; y++)
            {
                for (int x = 0; x < Define.width; x++)
                {
                    if (checkPointMap[x] == Define.height || checkPointMap[y + Define.width] == Define.width)
                    {
                        int colorType;
                        if (mapData.GetBlockOnMap(x, y).TryGetColorType(out colorType))
                        {
                            clearTileData.AddClearedTile(colorType);
                            clearTileMap[x, y] = true;
                        }
                    }
                    else
                        clearTileMap[x, y] = false;
                }
            }
        }
    }

    //지워져야 할 블록이 존재하는가? //
    public bool HasClearTilesOnMap()
    {
        return clearTileData.GetTotalClearedLineCount() > 0;
    }

    public IEnumerator ClearTiles(System.Action<ClearTileData> afterClearAllBlocks)
    {
        bool bWork = true;

        SoundManager.GetInstance().PlaySound(Define.SoundType.CrushLine);

        StartCoroutine(ClearBlocksFunc((ClearTileData clearTileData) =>
        {
            bWork = false;
            if (afterClearAllBlocks != null)
                afterClearAllBlocks(clearTileData);

            //미션 달성한 것이 있는지 체크한다. //
            MissionManager.GetInstance().CheckClearedMission();
        }));

        while (bWork)
        {
            yield return null;
        }
    }

    //지워져야 할 블록들을 모두 지운다. //
    private IEnumerator ClearBlocksFunc(System.Action<ClearTileData> afterClearAllBlocks)
    {
        bClearingTiles = true;
        clearEffectList.Clear();
        for (int y = 0; y < Define.height; y++)
        {
            for (int x = 0; x < Define.width; x++)
            {
                if (clearTileMap[x, y])
                {
                    clearEffectList.Add(new Pos(x, y));
                }
            }
        }

        if (clearEffectList.Count > 0)
        {
            scoreAssistance.SetNormalPointMode(clearTileData.GetTotalClearedLineCount());

            ClearBlocks(clearEffectList, true, (int clearedCount) =>
            {
                clearEffectList.Clear();
                bClearingTiles = false;
                UpdateMap();

                if (afterClearAllBlocks != null)
                    afterClearAllBlocks(clearTileData);
            });

            int clearedLineCount = 0;

            for (int i = 0; i < Define.width + Define.height; i++)
            {
                bool bSetEffect = false;
                bool bRotation = false;
                Vector3 pos = Vector3.zero;
                if (i < Define.width && checkPointMap[i] == Define.height)
                {
                    //세로. //
                    bSetEffect = true;
                    pos = mapData.GetBlockOnMap(i, 0).mTrans.localPosition;
                    pos.y = 0;
                    bRotation = true;
                }
                else if (i >= Define.width && checkPointMap[i] == Define.width)
                {
                    //가로. //
                    bSetEffect = true;
                    pos = mapData.GetBlockOnMap(0, i - Define.width).mTrans.localPosition;
                    pos.x = 0;
                }

                if (bSetEffect)
                {
                    MakeLineEffect(pos, bRotation);
                    clearedLineCount++;
                }
            }

            MissionManager.GetInstance().SetClearedLineCount(clearedLineCount);
        }
        yield return null;
    }

    private void MakeLineEffect(Vector3 pos, bool bRotation)
    {
        LineEffectController lineEffect = PoolManager.GetObject<LineEffectController>();
        lineEffect.mTrans.parent = GameManager.tileRoot;
        lineEffect.mTrans.localScale = Vector3.one;
        lineEffect.mTrans.localPosition = pos;
        lineEffect.mTrans.localRotation = bRotation ? Quaternion.AngleAxis(90f, Vector3.forward) :
            Quaternion.identity;
    }

    public void UpdateMap()
    {
        UpdateAllTiles();
        emphasisTiles.Clear();
    }

    //지워져야 할 타일들 중, 현재 놓은 타일의 중심점을 찾는다. //
    private Pos GetCenterPosInClearBlocks()
    {
        for (int i = 0, max = emphasisTiles.Count; i < max; i++)
        {
            if (clearTileMap[emphasisTiles[i].x, emphasisTiles[i].y])
            {
                return new Pos(emphasisTiles[i].x, emphasisTiles[i].y);
            }
        }

        int minX = Define.width;
        int maxX = 0;
        int minY = Define.height;
        int maxY = 0;
        for (int y = 0; y < Define.height; y++)
        {
            for (int x = 0; x < Define.width; x++)
            {
                if (clearTileMap[x, y])
                {
                    if (minX > x)
                        minX = x;
                    if (maxX < x)
                        maxX = x;
                    if (minY > y)
                        minY = y;
                    if (maxY < y)
                        maxY = y;
                }
            }
        }
        
        return new Pos(Mathf.RoundToInt((minX + maxY) * 0.5f), Mathf.RoundToInt((minY + maxY) * 0.5f));
    }

    //배경 타일 갱신. //
    private void UpdateAllTiles()
    {
        for (int y = 0; y < Define.height; y++)
        {
            for (int x = 0; x < Define.width; x++)
            {
                mapData.GetBlockOnMap(x, y).UpdateTile();
            }
        }
    }

    private void AddRightPos(int x, int y, ref List<Pos> list)
    {
        if (x >= 0 && x < Define.width && y >= 0 && y < Define.height)
        {
            if (clearTileMap[x, y])
                list.Add(new Pos(x, y));
        }
    }

    //더 이상 타일 배치가 가능한지 여부를 체크. //
    public bool CheckGameEnd()
    {
        bool[] passTile = new bool[] { false, false, false };

        for (int y = 0; y < Define.height; y++)
        {
            for (int x = 0; x < Define.width; x++)
            {
                if (!mapData.GetBlockOnMap(x, y).IsSame(TileData.BlockType.None))
                    continue;

                for (int i = 0, iMax = tileList.Count; i < iMax; i++)
                {
                    if (passTile[i] == true)
                        continue;

                    int passCount = 0;
                    int jMax = tileList[i].blockPos.Count;
                    for (int j = 0; j < jMax; j++)
                    {
                        int posX = tileList[i].blockPos[j].x + x;
                        int posY = tileList[i].blockPos[j].y + y;

                        if (posX < 0 || posX >= Define.width || posY < 0 || posY >= Define.height)
                            continue;
                        if (!mapData.GetBlockOnMap(posX, posY).IsSame(TileData.BlockType.None))
                            continue;
                        passCount++;
                    }

                    if (passCount == jMax)
                    {
                        passTile[i] = true;
                        if (passTile[0] && passTile[1] && passTile[2])
                            return false;
                    }
                }
            }
        }

        if (!passTile[0] && !passTile[1] && !passTile[2])
        {
            return true;
        }
        return false;
    }

    private bool TryGetNextPos(ref Pos nowPos)
    {
        int x = nowPos.x;
        int y = nowPos.y;
        x++;
        if (x >= Define.width)
        {
            x -= Define.width;
            y++;
        }
        if (y >= Define.height)
            return false;
        nowPos = new Pos(x, y);
        return true;
    }

    //해당 위치의 블록을 지우는 함수. //
    private void ClearBlocks(List<Pos> clearBlocksList, bool showEffect, System.Action<int> callback)
    {
        int allClearBlocksCount = clearBlocksList.Count;
        int clearCount = 0;
        int addedScore = 0;

        int ranIdx = Random.Range(0, moveCurves.Length);
        AnimationCurve moveCurve = moveCurves[ranIdx];
        ranIdx = Random.Range(0, speedCurves.Length);
        AnimationCurve speedCurve = speedCurves[ranIdx];

        for (int i = 0; i < allClearBlocksCount; i++)
        {
            int colorType;
            if (mapData.GetBlockOnMap(clearBlocksList[i]).TryGetColorType(out colorType))
            {
                petAssistance.AddExp(colorType, 1);
            }

            Vector3 pos = GetBlockPosition(clearBlocksList[i]);
            addedScore += scoreAssistance.AddScore(pos);

            if (showEffect && DataManager.GetInstance().GetSelectedPetData(colorType) != null)
            {
                SkillPointEffect skillPoint = MakeSkillEffect(colorType);
                skillPoint.MoveTo(moveCurve, speedCurve, pos, expPos[(int)colorType], null);
            }

            mapData.GetBlockOnMap(clearBlocksList[i]).ClearBlock(() =>
            {
                clearCount++;
                //모든 블록이 제거되면, //
                if (clearCount >= allClearBlocksCount)
                {
                    if (callback != null)
                        callback(allClearBlocksCount);
                }
            });
        }
        MissionManager.GetInstance().AddScore(addedScore);
    }

    private SkillPointEffect MakeSkillEffect(int type)
    {
        SkillPointEffect skillPoint = PoolManager.GetObject<SkillPointEffect>();
        skillPoint.mTrans.parent = GameManager.tileRoot;
        skillPoint.mTrans.localScale = Vector3.one;
        skillPoint.Init(type);
        return skillPoint;
    }

    //스킬. 모든 블록이 빈 공간이 없도록 아래를 향해 이동한다. //
    public void AllBlocksDown(System.Action afterFinished)
    {
        bClearingTiles = true;
        int totalCount = Define.width * Define.height;

        for (int x = 0; x < Define.width; x++)
        {
            int count = 0;
            int notNoneBlockCount = 0;
            int lastNoneBlockPos = 0;
            for (int y = 0; y < Define.height; y++)
            {
                if (!mapData.GetBlockOnMap(x, y).IsSame(TileData.BlockType.None))
                {
                    toMoveMap[x, y] = new Pos(x, count);
                    fromMoveMap[x, y] = new Pos(x, y);
                    if (lastNoneBlockPos != y)
                    {
                        lastNoneBlockPos = y;
                    }
                    count++;
                }
            }

            notNoneBlockCount = count;
            count = 0;
            for (int y = 0; y < Define.height; y++)
            {
                if (mapData.GetBlockOnMap(x, y).IsSame(TileData.BlockType.None))
                {
                    toMoveMap[x, y] = new Pos(x, notNoneBlockCount + count);
                    count++;
                    fromMoveMap[x, y] = new Pos(x, lastNoneBlockPos + count);
                }

                Vector3 toPos = GetBlockPosition(toMoveMap[x, y]);
                Vector3 fromPos = GetBlockPosition(fromMoveMap[x, y]);

                StartCoroutine(mapData.GetBlockOnMap(x, y).Move(fromPos, toPos, () =>
                {
                    totalCount--;
                    //모든 블록이 이동 완료하면, //
                    if (totalCount <= 0)
                    {
                        bClearingTiles = false;
                        //이동한 상태에 맞춰서 block 데이터를 재연결한다. //
                        mapData.RelinkBlockMap(toMoveMap);
                        if (afterFinished != null)
                            afterFinished();
                    }
                }));
            }
        }
    }

    //스킬. 특정 타입의 타일을 모두 지우는 스킬. //
    //public void ClearTypeBlock(int type, System.Action callback)
    //{
    //    clearEffectList.Clear();

    //    for (int y = 0; y < Define.height; y++)
    //    {
    //        for (int x = 0; x < Define.width; x++)
    //        {
    //            int colorType;
    //            if (mapData.GetBlockOnMap(x, y).TryGetColorType(out colorType))
    //            {
    //                if (colorType == type)
    //                {
    //                    clearEffectList.Add(new Pos(x, y));
    //                }
    //            }
    //        }
    //    }

    //    if (clearEffectList.Count > 0)
    //    {
    //        scoreAssistance.SetSkillPointMode();
    //        ClearBlocks(clearEffectList, false, (int clearedCount) =>
    //        {
    //            UpdateMap();
    //            if (callback != null)
    //                callback();
    //        });
    //    }
    //    else
    //        callback();
    //}

    //스킬. 모든 타일을 모두 지우는 스킬. //
    public void ClearAllBlocks(System.Action callback)
    {
        clearEffectList.Clear();
        
        for (int y = 0; y < Define.height; y++)
        {
            for (int x = 0; x < Define.width; x++)
            {
                clearTileMap[x, y] = false;

                int colorType;
                if (mapData.GetBlockOnMap(x, y).TryGetColorType(out colorType))
                {
                    clearEffectList.Add(new Pos(x, y));
                }
            }
        }

        if (clearEffectList.Count == 0)
        {
            if (callback != null)
                callback();
        }
        else
        {
            ClearBlocks(clearEffectList, false, (int clearedCount) =>
            {
                UpdateMap();
                if (callback != null)
                    callback();
            });
        }
    }

    //스킬. 모든 퍼즐을 새로 갱신한다. //
    public void ChangeAllPuzzles(System.Action callback)
    {
        addPuzzleList.Clear();
        for (int i = tileList.Count - 1; i >= 0; i--)
        {
            PoolManager.ReturnObject<PuzzleController>(tileList[i]);
            tileList.RemoveAt(i);
        }

        List<Define.PuzzleType> puzzleTypeList = new List<Define.PuzzleType>();
        puzzleTypeList.Add(Define.PuzzleType.I2);
        puzzleTypeList.Add(Define.PuzzleType.I3);
        puzzleTypeList.Add(Define.PuzzleType.I4);
        puzzleTypeList.Add(Define.PuzzleType.Point);

        for (int i = 0; i < Define.readyBlockCount; i++)
            addPuzzleList.Add(MakeTile(puzzleTypeList));

        StartCoroutine(CallInPuzzleTiles(addPuzzleList, callback));
    }

    //스킬. 해당 퍼즐의 타입을 변경한다. //
    public void ChangeTileTypeToFillLines(int idx, System.Action callback)
    {
        if (tileList.Count <= idx || idx < 0)
            return;

        tileList[idx].ChangePuzzleTypeToFillLines(callback);
    }
}

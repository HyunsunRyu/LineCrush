using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleController : PoolObject
{
    [SerializeField] private List<TileController> tiles;
    [SerializeField] private Transform body;
    [SerializeField] private BoxCollider col;

    public PuzzleTable puzzleTable { get; private set; }

    private float nowSize;
    private bool bGetBigger, bGetSmaller;
    private Vector3 clickedPos;
    private bool isPressing;
    private bool isMoving;

    private const float speed = 3f;

    private Vector3 firstBlockPosition;

    private Pos fBlockPos;

    private int rotation;

    public List<Pos> blockPos { get; private set; }
    public int tileIdx { get; private set; }
    public TileData tileType { get; private set; }

    public static PuzzleController clickedTile { get; private set; }
    private static Action<PuzzleController> putPuzzleCallback;
    private static Action<Pos, PuzzleController> emphasisCallback;

    public static void SetCallback(Action<PuzzleController> putPuzzle, Action<Pos, PuzzleController> emphasis)
    {
        putPuzzleCallback = putPuzzle;
        emphasisCallback = emphasis;
    }

    private void OnEnable()
    {
        StartCoroutine(UpdateFunc());
    }

    private void OnDisable()
    {
        isPressing = false;
        StopCoroutine(UpdateFunc());
    }

    public void SetData(PuzzleTable puzzleTable, TileData tileData, int rotation)
    {
        fBlockPos = Pos.Nullity();

        this.puzzleTable = puzzleTable;
        this.tileType = tileData;

        if (rotation < 0)
            rotation = 0;
        else if (rotation > 3)
            rotation = 3;
        this.rotation = rotation;
        
        InitData();
        tileIdx = -1;
        mTrans.localPosition = Define.readyPosition;
    }

    private void InitData()
    {
        bGetBigger = false;
        bGetSmaller = false;
        isPressing = false;

        nowSize = Define.smallSizeRate;
        mTrans.parent = GameManager.tileRoot;

        clickedTile = null;

        SetBlocks();
        SetSize(nowSize);
    }

    public void Move(int to)
    {
        tileIdx = to;
        isMoving = true;
    }

    private void SetBlocks()
    {
        int showCount = puzzleTable.blockInfo.Count;

        for (int i = 0; i < showCount; i++)
        {
            tiles[i].mObject.SetActive(true);
        }
        for (int i = showCount, max = tiles.Count; i < max; i++)
        {
            tiles[i].mObject.SetActive(false);
        }

        float minWidth = 0f;
        float maxWidth = 0f;
        float minHeight = 0f;
        float maxHeight = 0f;
        blockPos = new List<Pos>(puzzleTable.blockInfo);
        for (int i = 0, max = blockPos.Count; i < max; i++)
        {
            switch (rotation)
            {
                default:
                case 0:
                    break;
                case 1:
                    blockPos[i] = new Pos(-blockPos[i].y, blockPos[i].x);
                    break;
                case 2:
                    blockPos[i] = new Pos(-blockPos[i].x, -blockPos[i].y);
                    break;
                case 3:
                    blockPos[i] = new Pos(blockPos[i].y, -blockPos[i].x);
                    break;
            }
            Vector3 position = new Vector3(blockPos[i].x * Define.blockDistance, blockPos[i].y * Define.blockDistance, 0f);
            tiles[i].mTrans.localPosition = position;

            if (minWidth > position.x)
                minWidth = position.x;
            if (maxWidth < position.x)
                maxWidth = position.x;
            if (minHeight > position.y)
                minHeight = position.y;
            if (maxHeight < position.y)
                maxHeight = position.y;

            tiles[i].SetTileType(tileType);
        }

        body.localPosition = new Vector3((minWidth + maxWidth) * -0.5f, (minHeight + maxHeight) * -0.5f, 0f);
        col.size = new Vector3(Mathf.Max((maxWidth - minWidth + Define.blockDistance) * 1.5f, Define.minColliderSize),
                               Mathf.Max((maxHeight - minHeight + Define.blockDistance) * 1.5f, Define.minColliderSize),
                               0f);
    }

    private void OnPress(bool bPressed)
    {
        if (isMoving || !GameManager.IsPlaying() || !TileSystem.CanClickTile())
        {
            if (clickedTile == null)
                return;
            else
                bPressed = false;
        }

        isPressing = bPressed;

        if (bPressed)
        {
            clickedTile = this;

            clickedPos = new Vector3(UICamera.lastTouchPosition.x, UICamera.lastTouchPosition.y, 0f);
            clickedPos *= ScreenSizeGetter.width / (float)Screen.width;
            clickedPos -= new Vector3(ScreenSizeGetter.halfWidth, ScreenSizeGetter.halfHeight);
            clickedPos -= mTrans.localPosition;

            //Vector3 fingerDistance = Vector3.zero;
            //switch (DataManager.GetInstance().touchOption)
            //{
            //    case 0:
            //        fingerDistance.y = 100f;
            //        break;
            //    case 1:
            //        fingerDistance.y = 300f;
            //        break;
            //    default:
            //    case 2:
            //        fingerDistance.y = 500f;
            //        break;
            //}

            //clickedPos += fingerDistance;
            clickedPos += Define.fingerDistance;

            body.localPosition += clickedPos;

            bGetBigger = true;
            bGetSmaller = false;
        }
        else
        {
            clickedTile = null;
            if (putPuzzleCallback != null)
                putPuzzleCallback(this);
        }
    }

    private void OnDrag(Vector2 delta)
    {
        if (isMoving || !GameManager.IsPlaying() || !TileSystem.CanClickTile())
        {
            if (clickedTile == null)
                return;
            else if (clickedTile != this)
                return;
        }

        if (clickedTile != this)
        {
            return;
        }

        mTrans.localPosition += new Vector3(delta.x * ScreenSizeGetter.invRate, delta.y * ScreenSizeGetter.invRate, 0f);
    }

    private IEnumerator UpdateFunc()
    {
        while (true)
        {
            if (isMoving)
            {
                Moving();
            }
            else
            {
                ScaleFunc();
                CheckTileFunc();
            }
            yield return null;
        }
    }

    private void ScaleFunc()
    {
        if (!bGetBigger && !bGetSmaller)
        {
            return;
        }

        if (bGetBigger)
        {
            if (nowSize >= 1f)
            {
                nowSize = 1f;
                bGetBigger = false;
                return;
            }
            nowSize += Time.deltaTime * speed;
        }
        else if (bGetSmaller)
        {
            if (nowSize <= Define.smallSizeRate)
            {
                nowSize = Define.smallSizeRate;
                bGetSmaller = false;
                return;
            }
            nowSize -= Time.deltaTime * speed;
        }

        if (nowSize >= 1f)
        {
            nowSize = 1f;
        }
        else if (nowSize <= Define.smallSizeRate)
        {
            nowSize = Define.smallSizeRate;
        }
        SetSize(nowSize);
    }

    private void SetSize(float sizeRate)
    {
        mTrans.localScale = new Vector3(sizeRate, sizeRate, 1f);
    }

    private void CheckTileFunc()
    {
        if (isPressing)
        {
            Vector3 firstBlockPos = tiles[0].mTrans.localPosition + mTrans.localPosition + body.localPosition;

            if (firstBlockPosition.x != firstBlockPos.x || firstBlockPosition.y != firstBlockPos.y)
            {
                firstBlockPosition = firstBlockPos;
                Pos pos = GetPos(firstBlockPos.x, firstBlockPos.y);
                if (fBlockPos != pos)
                {
                    fBlockPos = pos;
                    if (emphasisCallback != null)
                        emphasisCallback(pos, this);
                }
            }
        }
    }

    private Pos GetPos(float xPos, float yPos)
    {
        //set base to zero.
        xPos = (xPos - Define.blockCenterPos.x + Define.width * Define.blockDistance * 0.5f - Define.blockDistance * 0.5f) / Define.blockDistance;
        yPos = (yPos - Define.blockCenterPos.y + Define.height * Define.blockDistance * 0.5f - Define.blockDistance * 0.5f) / Define.blockDistance;

        int x = Mathf.RoundToInt(xPos);
        int y = Mathf.RoundToInt(yPos);
        return new Pos(x, y);
    }

    private void Moving()
    {
        Vector3 target = GetPos(tileIdx);
        Vector3 now = mTrans.localPosition;
        now -= new Vector3(Define.readyBlockSpeed * Time.deltaTime, 0f, 0f);
        if (now.x <= target.x)
        {
            isMoving = false;
            now.x = target.x;
        }
        mTrans.localPosition = now;
    }

    private Vector3 GetPos(int idx)
    {
        return new Vector3(((Define.readyBlockCount - 1) * -0.5f + idx) * Define.readyWidth,
                           Define.readyHeight,
                           0f);
    }

    public IEnumerator PositionPuzzleBasic()
    {
        SoundManager.GetInstance().PlaySound(Define.SoundType.TileMoveBack);

        Vector3 pos = GetPos(tileIdx);
        InitData();
        mTrans.localPosition = pos;
        yield return null;

        clickedTile = null;
    }

    public void ChangePuzzleTypeToFillLines(System.Action callback)
    {
        TileData data = new TileData();
        data.CopyData(tileType);
        data.SetFillLinesTile();
        tileType = data;
        
        for (int i = 0, max = tiles.Count; i < max; i++)
        {
            tiles[i].SetTileType(data);
        }

        if (callback != null)
            callback();
    }
}
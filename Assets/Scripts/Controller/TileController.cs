using UnityEngine;
using System.Collections;

public class TileController : PoolObject
{
    [SerializeField] private UISprite tileImage;
    [SerializeField] private UISprite fillLineBlockOutline;
    [SerializeField] private GameObject fillLineBlockOutlineBody;
    //[SerializeField] private Animator anim;

    private TileData tileType;

    private float speed = 1200f;

    private const float boomBlockTime = 0.5f;
    //private const string animIdle = "block_idle";
    //private const string animBoom = "block_boom";
    private readonly Color emphasisColor = Util.GetColor(200, 200, 200);

    public int x { get; private set; }
    public int y { get; private set; }
    
    public void SetData(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void EmphasisTile(TileData type)
    {
        UpdateTileImage(type, true);
    }

    public void SetTileType(TileData type)
    {
        this.tileType = type;
        UpdateTile();
    }

    public void SetNoneTile()
    {
        tileType.SetNoneTile();
        UpdateTile();
    }

    public void UpdateTile()
    {
        UpdateTileImage(tileType);
    }
     
	private void UpdateTileImage(TileData type, bool bEmphasis = false)
    {
        if (type.IsSameTile(TileData.BlockType.None))
        {
            tileImage.spriteName = tileType.GetBaseTileName();
        }
        else
        {
            string tileName;
            if (type.TryGetTileName(out tileName))
                tileImage.spriteName = tileName;
        }

        Color tileColor = Color.white;
        
        if (bEmphasis)
            tileColor = emphasisColor;

        if (type.IsSameTile(TileData.BlockType.FillBlocks))
        {
            if (fillLineBlockOutlineBody != null)
                fillLineBlockOutlineBody.SetActive(true);

            if (fillLineBlockOutline != null)
            {
                int colorType = 0;
                if (type.TryGetTileColorType(out colorType))
                {
                    switch (colorType)
                    {
                        case 0:
                            fillLineBlockOutline.color = Util.GetColor(255, 94, 94);
                            break;
                        case 1:
                            fillLineBlockOutline.color = Util.GetColor(107, 132, 248);
                            break;
                        case 2:
                        default:
                            fillLineBlockOutline.color = Util.GetColor(89, 180, 38);
                            break;
                    }
                }
            }
        }
        else
        {
            if (fillLineBlockOutlineBody != null)
                fillLineBlockOutlineBody.SetActive(false);
        }

        tileImage.color = tileColor;

        //anim.Play(animIdle);
    }

    public void ClearBlock(System.Action callback)
    {
        ShowEffect();
        //anim.Play(animBoom);
        StartCoroutine(BoomBlocks(callback));
    }

    private void ShowEffect()
    {
        ClearEffectController effectController = PoolManager.GetObject<ClearEffectController>();
        effectController.mTrans.parent = GameManager.tileRoot;
        effectController.mTrans.localScale = Vector3.one;
        effectController.mTrans.localPosition = mTrans.localPosition;

        effectController.Init();
    }

    private IEnumerator BoomBlocks(System.Action callback)
    {
        yield return new WaitForSeconds(boomBlockTime);

        tileType.SetTileType(TileData.BlockType.None);
        UpdateTileImage(tileType);
        
        if (callback != null)
            callback();
    }
        
    public bool IsSame(TileData.BlockType type)
    {
        return tileType.IsSameTile(type);
    }

    public bool TryGetColorType(out int colorType)
    {
        return tileType.TryGetTileColorType(out colorType);
    }

    public IEnumerator Move(Vector3 fromPos, Vector3 toPos, System.Action afterMove)
    {
        if (fromPos != toPos)
        {
            bool bMove = true;
            Vector3 direction = (toPos - fromPos).normalized;
            float length = (toPos - fromPos).magnitude;
            float value = 0f;
            while (bMove)
            {
                value += speed * Time.deltaTime;
                if (value >= length)
                {
                    value = length;
                    bMove = false;
                }
                mTrans.localPosition = fromPos + direction * value;
                yield return null;
            }
        }
        if (afterMove != null)
            afterMove();
    }

    public TileData.BlockType GetTileType()
    {
        return tileType.GetTileType();
    }
}

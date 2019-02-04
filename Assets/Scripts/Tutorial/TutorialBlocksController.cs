using UnityEngine;
using System.Collections;

public class TutorialBlocksController : MonoBehaviour
{
    [SerializeField] private BoxCollider leftBlock;
    [SerializeField] private BoxCollider rightBlock;
    [SerializeField] private BoxCollider topBlock;
    [SerializeField] private BoxCollider bottomBlock;
    [SerializeField] private BoxCollider targetBlock;

    public void Init()
    {
        SetAllBlock(false);
    }

    public void SetAllBlock(bool onOff)
    {
        targetBlock.enabled = false;
        leftBlock.enabled = false;
        rightBlock.enabled = false;
        topBlock.enabled = false;
        bottomBlock.enabled = onOff;

        if (onOff)
        {
            bottomBlock.size = new Vector3(ScreenSizeGetter.width, ScreenSizeGetter.height, 0f);
            bottomBlock.center = Vector3.zero;
        }
    }

    public void SetTouchTarget()
    {
        targetBlock.enabled = true;
        leftBlock.enabled = false;
        rightBlock.enabled = false;
        topBlock.enabled = false;
        bottomBlock.enabled = false;
    }

    public void SetEmphasis(Vector3 pos, Vector3 size)
    {
        targetBlock.enabled = false;
        leftBlock.enabled = true;
        rightBlock.enabled = true;
        topBlock.enabled = true;
        bottomBlock.enabled = true;

        float x1 = pos.x - (size.x * 0.5f);
        float x2 = pos.x + (size.x * 0.5f);
        float y1 = pos.y - (size.y * 0.5f);
        float y2 = pos.y + (size.y * 0.5f);

        leftBlock.size = new Vector3(x1 + ScreenSizeGetter.halfWidth, ScreenSizeGetter.height, 0f);
        rightBlock.size = new Vector3(ScreenSizeGetter.halfWidth - x2, ScreenSizeGetter.height, 0f);
        bottomBlock.size = new Vector3(x2 - x1, y1 + ScreenSizeGetter.halfHeight, 0f);
        topBlock.size = new Vector3(x2 - x1, ScreenSizeGetter.halfHeight - y2, 0f);

        leftBlock.center = new Vector3(leftBlock.size.x * 0.5f - ScreenSizeGetter.halfWidth, 0f, 0f);
        rightBlock.center = new Vector3(rightBlock.size.x * 0.5f + x2, 0f, 0f);
        bottomBlock.center = new Vector3(x1 + bottomBlock.size.x * 0.5f, bottomBlock.size.y * 0.5f - ScreenSizeGetter.halfHeight, 0f);
        topBlock.center = new Vector3(x1 + topBlock.size.x * 0.5f, y2 + topBlock.size.y * 0.5f, 0f);
    }
}

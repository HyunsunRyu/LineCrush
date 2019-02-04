using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIListScrollbar : MonoBehaviour
{
    public enum ScrollBarState { Show = 0, Hide, Showing, Hiding }
    [SerializeField]
    private UIListDragController dragList;
    [SerializeField]
    private UISprite image;
    [SerializeField]
    private Vector2 startPos;
    [SerializeField]
    private Vector2 endPos;
    [SerializeField]
    private Vector2 startScale = new Vector2(12, 56);
    [SerializeField]
    private Vector2 minimumSize = new Vector3(12, 12);
    [SerializeField]
    private float scrollSizeRate = 0.1f;   //이동거리에 비례, 제한 영역을 넘어섰을 때 스크롤 사이즈가 바뀌는 비율. //
    [SerializeField]
    private bool hideBarWhenNotMoving = true;
    private bool doNotWorkScrollbar = false;

    private bool init = false;
    private ScrollBarState state = ScrollBarState.Hide;
    private Transform target;
    private Transform dragListPanel;
    private Transform imageTrans;
    private bool horizon;
    private float panelStartPos;
    private float sizeRate = 0;
    private Color imageColor;
    private Vector3 lastPanelPos;
    private float totalDistance;
    private float invDistance;

    public void Init(int totalCount)
    {
        init = true;

        target = transform;
        target.localPosition = startPos;
        dragListPanel = dragList.target;
        imageTrans = image.transform;
        lastPanelPos = dragListPanel.localPosition;

        horizon = (dragList.arrangement == UIListDragController.Arrangement.Horizontal) ? true : false;
        //패널이 이동할 수 있는 총 거리. //
        float panelTotalDistance = horizon ?
            totalCount * dragList.cellDistance - dragList.range :
            totalCount * dragList.cellDistance - dragList.range;
        //패널이 이동할 시작점과 끝점을 찾는다. //
        panelStartPos = horizon ? dragList.startPos.x : dragList.startPos.y;
        //스크롤바가 이동할 수 있는 총 거리. //
        totalDistance = horizon ? (endPos.x - startPos.x) : (startPos.y - endPos.y);
        //타겟의 이동한 거리 따라 스크롤바가 이동할 거리의 비율을 계산한다. //
        if (panelTotalDistance <= 0f)
        {
            doNotWorkScrollbar = true;
            sizeRate = 0f;
        }
        else
        {
            doNotWorkScrollbar = false;
            sizeRate = totalDistance / panelTotalDistance;
        }
        invDistance = 1f / totalDistance;
        //스크롤 이미지의 색상이 변하는 시간 계산. //
        //스크롤 색상 셋팅. //
        imageColor.a = 0;
        state = hideBarWhenNotMoving || doNotWorkScrollbar ? ScrollBarState.Hide : ScrollBarState.Show;
        SetColor();

        image.SetDimensions((int)startScale.x, (int)startScale.y);
        imageTrans.localPosition = Vector3.zero;
    }

    void LateUpdate()
    {
        if (init == false || doNotWorkScrollbar)
            return;

        SetScrollBar();

        if (!hideBarWhenNotMoving)
            return;

        if (state == ScrollBarState.Showing || state == ScrollBarState.Hiding)
        {
            SetColor();
        }
        else if (state == ScrollBarState.Hide && imageColor.a != 0f)
        {
            SetColor();
        }
        else if (state == ScrollBarState.Show && imageColor.a != 0.5f)
        {
            SetColor();
        }
    }

    void SetScrollBar()
    {
        if (lastPanelPos == dragListPanel.localPosition || doNotWorkScrollbar)
            return;

        //현재 패널이 시작점에서 얼마나 이동했는지 거리를 구한다.. //
        float pos = horizon ?
            (panelStartPos - dragListPanel.localPosition.x) :
            (dragListPanel.localPosition.y - panelStartPos);
        //패널이 시작점에서 떨어진 거리를 스크롤의 거리로 변환한다. //
        pos *= sizeRate;
        //스크롤의 시작점으로부터의 거리로 전환한다. //
        pos = horizon ? (startPos.x + pos) : (startPos.y - pos);
        //스크롤의 위치가 제한 영역을 벗어나지 않게 설정한다. //
        float overDistance = 0; //제한영역을 벗어난 거리. //
        if (horizon)
        {
            if (pos < startPos.x)
            {
                overDistance = pos - startPos.x;
                pos = startPos.x;
            }
            else if (pos > endPos.x)
            {
                overDistance = pos - endPos.x;
                pos = endPos.x;
            }
        }
        else
        {
            if (pos > startPos.y)
            {
                overDistance = startPos.y - pos;
                pos = startPos.y;
            }
            else if (pos < endPos.y)
            {
                overDistance = endPos.y - pos;
                pos = endPos.y;
            }
        }
        //스크롤의 위치를 갱신한다. //
        Vector3 vPos = target.localPosition;
        if (horizon)
            vPos.x = pos;
        else
            vPos.y = pos;
        target.localPosition = vPos;
        lastPanelPos = dragListPanel.localPosition;
        //스크롤의 크기를 갱신한다. //
        Vector2 scale = new Vector3(startScale.x, startScale.y);
        if (overDistance == 0f)
        {
            //스크롤의 사이즈 조정. //
            image.SetDimensions((int)scale.x, (int)scale.y);
            //스크롤의 줄어든 만큼의 미세 위치 조정. //
            imageTrans.localPosition = Vector3.zero;
        }
        else
        {
            //이동거리에 비례하여 스크롤이 줄어든 정도. //
            float scrollRate = (totalDistance - (Mathf.Abs(overDistance) * scrollSizeRate)) * invDistance;
            //줄어든 후의 스크롤 사이즈. //
            float scrollSize = horizon ? (startScale.x * scrollRate) : (startScale.y * scrollRate);
            //최소 크기보다 작게 줄어들지 못하게 제한한다. //
            if (scrollSize < (horizon ? minimumSize.x : minimumSize.y))
                scrollSize = horizon ? minimumSize.x : minimumSize.y;
            scrollSize = (float)Mathf.RoundToInt(scrollSize);
            float decSize = horizon ? startScale.x - scrollSize : startScale.y - scrollSize;

            if (horizon)
                scale.x = scrollSize;
            else
                scale.y = scrollSize;
            //스크롤의 사이즈 조정. //
            image.SetDimensions((int)scale.x, (int)scale.y);
            //스크롤의 줄어든 만큼의 미세 위치 조정. //
            if (overDistance < 0)   //제한 시작점 이전으로 넘어섰을 때. //
            {
                imageTrans.localPosition = horizon ?
                    new Vector3(-decSize * 0.5f, 0, 0) : new Vector3(0, decSize * 0.5f, 0);
            }
            else if (overDistance > 0)  //제한 끝점 이후로 넘어섰을 때. //
            {
                imageTrans.localPosition = horizon ?
                    new Vector3(decSize * 0.5f, 0, 0) : new Vector3(0, -decSize * 0.5f, 0);
            }
        }
    }

    void SetColor()
    {
        if (state == ScrollBarState.Showing)
        {
            imageColor.a = 0.5f;
            state = ScrollBarState.Show;
        }
        else if (state == ScrollBarState.Hiding)
        {
            imageColor.a -= Time.deltaTime;
            if (imageColor.a < 0)
            {
                imageColor.a = 0;
                state = ScrollBarState.Hide;
            }
        }
        else if (state == ScrollBarState.Show)
        {
            imageColor.a = 0.5f;
        }
        else if (state == ScrollBarState.Hide)
        {
            imageColor.a = 0;
        }
        image.color = imageColor;
    }

    public void ShowScrollBar()
    {
        if (!hideBarWhenNotMoving)
            return;
        if (state == ScrollBarState.Show)
            return;
        state = ScrollBarState.Showing;
    }

    public void HideScrollBar()
    {
        if (!hideBarWhenNotMoving)
            return;
        if (state == ScrollBarState.Hide)
            return;
        state = ScrollBarState.Hiding;
    }
}
using UnityEngine;
using System.Collections.Generic;

public abstract class UIDragListItem : PoolObject
{
    protected int idx;

    public abstract void InitItem(int idx);
    public abstract void ClickedItem();
    public virtual void ClearData() { }
    public abstract void UpdateUI();
}

public class UIListDragController : MonoBehaviour
{
    private static readonly string clickMethod = "ClickedItem";

    public delegate UIDragListItem MakeListItem();

    public delegate void listDel(List<UIDragListItem> list, int startIdx, int total);
    public Transform target;
    public Transform leader;
    public Arrangement arrangement = Arrangement.Horizontal;
    public enum Arrangement
    {
        Horizontal,
        Vertical,
    }
    public Vector3 startPos;
    public float range;

    public float cellDistance = 200f;
    public UIListScrollbar uiScroll;
    public float maxDraggingLimit = 100f;

    public float force = 1;

    private float momentumAmount = 5f;
    private const float stopDistance = 0.1f;
    public int maxCount = 6;
    private int lastIndex = 0;
    private float invMaxDraggingLimit = 0;

    private bool isLinked = false;
    private Vector3 scale = Vector3.one;
    private bool horizon;
    private List<UIDragListItem> itemScList = new List<UIDragListItem>();
    private List<Transform> transList = new List<Transform>();
    private bool canMove = false;
    private bool countLack = false;
    private int totalCount = 0;
    private float overDistance = 0;

    private MakeListItem makeItem;
    private Dictionary<string, string> buttonNameWithMethod;
    private bool hasButton = false;

    //Basic. //
    Vector3 mLastPos;
    Vector3 leaderLastPos;
    UIPanel mPanel;
    Plane mPlane;
    float mMomentum = 0;
    bool pressed = false;
    Vector2 mBound = Vector2.zero;

    public void StartList(MakeListItem makeItem, int totalCount)
    {
        this.makeItem = makeItem;
        this.totalCount = totalCount;
        hasButton = false;

        EnableList();
    }

    public void StartList(MakeListItem makeItem, int totalCount, Dictionary<string, string> buttonNameWithMethod)
    {
        this.makeItem = makeItem;
        this.totalCount = totalCount;
        this.buttonNameWithMethod = buttonNameWithMethod;
        hasButton = true;

        EnableList();
    }

    //해당 프리팹으로 입력받은 갯수만큼 아이템을 만들어 리스트에 넣고, 아이템을 클릭했을 때 호출할 몸체와 메소드 입력. //
    //해당 아이템 리스트를 반환. 반환받은 아이템 리스트를 가지고 초기화 필요. //
    private void EnableList()
    {
        if (uiScroll != null)
            uiScroll.Init(totalCount);

        isLinked = true;
        invMaxDraggingLimit = 1f / maxDraggingLimit;

        Transform mTrans = transform;
        mPanel = mTrans.GetComponent<UIPanel>();
        horizon = (arrangement == Arrangement.Horizontal) ? true : false;
        scale = horizon ? new Vector3(1, 0, 0) : new Vector3(0, 1, 0);

        GameObject item;
        UIDragListItem itemSc;
        UIMultiPick pick;
        itemScList.Clear();
        transList.Clear();
        countLack = false;
        //입력받은 갯수만큼이 아니라 최대 일정 개수 만큼의 아이템을 생성하고 이를 재활용 한다. //
        for (int i = 0; i < maxCount; i++)
        {
            //만약 총 갯수가 최대치보다 적을 때에는 총 갯수만큼 만들고 이를 반환한다. //
            if (i >= totalCount)
            {
                countLack = true;
                break;
            }

            itemSc = makeItem();
            itemScList.Add(itemSc);

            item = itemSc.gameObject;
            item.transform.parent = target;
            item.transform.localScale = Vector3.one;
            transList.Add(item.transform);
            item.SetActive(true);

            pick = item.GetComponent<UIMultiPick>();
            pick.messageList.Clear();
            pick.messageList.Add(new UIMultiPick.Message(gameObject, UIMultiPick.TriggerType.Press));
            pick.messageList.Add(new UIMultiPick.Message(gameObject, UIMultiPick.TriggerType.Drag));
            pick.messageList.Add(new UIMultiPick.Message(item, UIMultiPick.TriggerType.Click, clickMethod));

            if (hasButton)
            {
                foreach (KeyValuePair<string, string> btn in buttonNameWithMethod)
                {
                    Transform button = item.transform.FindChild(btn.Key);
                    if (button == null)
                        continue;
                    pick = button.GetComponent<UIMultiPick>();
                    if (pick != null)
                    {
                        pick.messageList.Clear();
                        pick.messageList.Add(new UIMultiPick.Message(gameObject, UIMultiPick.TriggerType.Press));
                        pick.messageList.Add(new UIMultiPick.Message(gameObject, UIMultiPick.TriggerType.Drag));
                        pick.messageList.Add(new UIMultiPick.Message(item, UIMultiPick.TriggerType.Click, btn.Value));
                    }
                }
            }
        }

        //아이템들을 정렬하여 위치시킨다. //
        Reposition(transList);

        //이동 제한 영역을 설정한다. //
        float totalDistance = horizon ? totalCount * cellDistance - range : totalCount * cellDistance - range;
        canMove = (totalDistance <= 0) ? false : true;
        if (canMove)
        {
            // horizon 이면 x가 y보다 크고, 아니면 x 가 y보다 작다. //
            mBound.x = horizon ? startPos.x : startPos.y;
            mBound.y = horizon ? startPos.x - totalDistance : startPos.y + totalDistance;
        }

        //아이템 재배치에 필요한 수치를 초기화한다. //
        lastIndex = 0;
        overDistance = 0;

        //타겟의 위치를 시작점으로 옮긴다. //
        target.localPosition = startPos;

        //리스트 이동에 관련된 정보들을 초기화시킨다. //
        mMomentum = 0;

        SetList(itemScList, 0, itemScList.Count);
    }

    private void SetList(List<UIDragListItem> list, int startIdx, int totalCount)
    {
        for (int i = 0; i < totalCount; i++)
        {
            int dataIdx = i + startIdx;
            list[i].InitItem(dataIdx);
        }
    }

    public int GetIndexOnAll(int itemIdxOnPanel)
    {
        return itemIdxOnPanel + lastIndex;
    }

    public void DeleteList(System.Action<UIDragListItem> deleteItem)
    {
        for (int i = itemScList.Count-1; i >= 0; i--)
        {
            if (itemScList[i] != null)
            {
                if (deleteItem != null)
                    deleteItem(itemScList[i]);
                else
                    Destroy(itemScList[i].gameObject);
            }
        }

        itemScList.Clear();
        transList.Clear();
    }

    //입력받은 리스트대로 재정렬 하는 함수. //
    private void Reposition(List<Transform> tlist)
    {
        int index = 0;
        foreach (Transform t in tlist)
        {
            t.localPosition = horizon ?
                new Vector3(cellDistance * index, 0f, 0f) :
                new Vector3(0f, -cellDistance * index, 0f);
            index++;
        }
    }

    void OnDisable()
    {
        if (leader != null)
        {
            leader.localPosition = Vector3.zero;
            leaderLastPos = Vector3.zero;
        }
    }

    //리스트 아이템을 눌렀을 때, 혹은 떼었을 때. //
    void OnPress(bool press)
    {
        if (isLinked == false) return;
        if (canMove == false) return;

        pressed = press;

        if (press)
        {
            //			leaderLastPos = Vector3.zero;
            //			leader.localPosition = Vector3.zero;
            // Remember the hit position
            mLastPos = UICamera.lastHit.point;

            // Create the plane to drag along
            Transform trans = UICamera.currentCamera.transform;
            mPlane = new Plane((mPanel != null ? mPanel.cachedTransform.rotation : trans.rotation) * Vector3.back, mLastPos);

            if (uiScroll != null)
                uiScroll.ShowScrollBar();
        }
        else
        {
            if (SetBound(ref overDistance))	//드래그로 인해 영역을 벗어나면 모멘텀을 0으로. //
            {
                mMomentum = 0;
                if (overDistance < 0)   //영역 이전. //
                {
                }
                else if (overDistance > 0)  //영역 이후. //
                {
                }
            }
        }
    }

    //리스트 아이템을 잡고 드래그 중일 때. //
    void OnDrag(Vector2 delta)
    {
        if (isLinked == false) return;
        if (canMove == false) return;

        Ray ray = UICamera.currentCamera.ScreenPointToRay(UICamera.lastTouchPosition);
        float dist = 0f;

        if (mPlane.Raycast(ray, out dist))
        {
            //리더의 이동. 리더의 localPosition의 변화에 따라서 타겟을 이동시킨다. //
            Vector3 currentPos = ray.GetPoint(dist);
            Vector3 offset = currentPos - mLastPos;
            mLastPos = currentPos;
            if (offset.x != 0f || offset.y != 0f)
            {
                offset = target.InverseTransformDirection(offset);
                offset.Scale(scale);
                offset = target.TransformDirection(offset);
            }
            leader.position += offset;

            //리더의 이동에 따라 패널을 이동시키는 함수. position 이동을 localPosition 으로 이동시키기 위한 방편. //
            Vector3 leaderNowPos = leader.localPosition;
            Vector3 leaderOffset = leaderNowPos - leaderLastPos;
            leaderLastPos = leaderNowPos;

            //영역 밖으로 넘어선 경우, 이동 폭을 제한시킨다. //
            if (SetBound(ref overDistance))	//영역을 넘어서면 패널의 이동이 원래보다 느려진다. //
            {
                LimitDragging(ref leaderOffset, overDistance);
            }

            //모멘텀을 구한다. //
            mMomentum = horizon ? leaderOffset.x : leaderOffset.y;
            if (mMomentum > 0f)
                Mathf.Min(mMomentum, float.MaxValue);
            else
                Mathf.Max(mMomentum, float.MinValue);

            target.localPosition += leaderOffset;

            if (countLack == false)
                ResetItems();
        }
    }

    void LateUpdate()
    {
        if (isLinked == false) return;
        if (canMove == false) return;
        if (pressed)
        {
        }
        else
        {
            if (SetBound(ref overDistance))	//영역을 넘어서면 돌아가려는 힘을 가한다. //
            {
                SpringMomentum(ref mMomentum, overDistance);
                ReduceMomentum(ref mMomentum);
            }

            if (IsMoving(mMomentum, stopDistance))	//특정 속도 이상일 때. //
            {
                //모멘텀을 항시 감속시킨다. //
                ReduceMomentum(ref mMomentum);
            }
            else
            {
                if (mMomentum != 0)
                    mMomentum = 0;

                if (uiScroll != null)
                    uiScroll.HideScrollBar();
            }

            //모멘텀에 맞게 패널을 이동시킨다. //
            target.localPosition += horizon ? new Vector3(mMomentum, 0, 0) : new Vector3(0, mMomentum, 0);
            //mMomentum = momentum;
            if (countLack == false)
                ResetItems();
        }
    }

    //해당 속도가 일정 영역내에 들어오면 정지, False 반환. //
    bool IsMoving(float momentum, float distance)
    {
        if (mMomentum > distance || mMomentum < -distance)
        {
            return true;
        }

        return false;
    }

    //감속하는 함수. //
    void ReduceMomentum(ref float momentum)
    {
        if (momentum == 0)
        {
            momentum = 0;
            return;
        }
        momentum = Mathf.Lerp(momentum, 0, momentumAmount * RealTime.deltaTime * force);
    }

    public void SetFocusItem(int itemIndex)
    {
        if (!canMove)
            return;

        Vector3 addPos = horizon ? new Vector3(-cellDistance * itemIndex, 0, 0) : new Vector3(0, cellDistance * itemIndex, 0);
        Vector3 forcePos = addPos + startPos;
        if (horizon && mBound.y > forcePos.x)
        {
            forcePos.x = mBound.y;
        }
        else if (!horizon && mBound.y < forcePos.y)
        {
            forcePos.y = mBound.y;
        }
        target.localPosition = forcePos;

        mMomentum = 0;

        ResetItems(true);
    }

    //영역 충돌 처리. 영역 밖으로 넘어서면 True, 아니면 False 반환. //
    //영역 이전이면 overDis 는 음수, 이후면 양수, 내부면 0 반환. //
    bool SetBound(ref float overDis)
    {
        if (horizon)	//수평 이동일 때. //
        {
            if (target.localPosition.x > mBound.x)	//영역 시작점 이전. //
            {
                overDis = mBound.x - target.localPosition.x;
                return true;
            }
            else if (target.localPosition.x < mBound.y)	//영역 끝점 이후. //
            {
                overDis = mBound.y - target.localPosition.x;
                return true;
            }
        }
        else 	//수직 이동일 때. //
        {
            if (target.localPosition.y < mBound.x)	//영역 시작점 이전. //
            {
                overDis = target.localPosition.y - mBound.x;
                return true;
            }
            else if (target.localPosition.y > mBound.y)	//영역 끝점 이후. //
            {
                overDis = target.localPosition.y - mBound.y;
                return true;
            }
        }
        overDis = 0;
        return false;
    }

    //아이템들을 재정렬하고 재정렬한대로 정보를 셋팅한다. //
    public void ResetItems(bool force = false)
    {
        float fIndex = horizon ? (mBound.x - target.localPosition.x) : (target.localPosition.y - mBound.x);
        int index = Mathf.FloorToInt(fIndex / cellDistance);	//현재 최상단의 아이템 Idx. //

        if (index + maxCount > totalCount)
            index = totalCount - maxCount;
        if (index < 0)
            index = 0;

        if (lastIndex == index && force == false)
            return;

        Vector3 pos = Vector3.zero;
        for (int i = 0; i < maxCount; i++)
        {
            if (i >= totalCount)
                break;
            if (horizon)
                pos.x = (i + index) * cellDistance;
            else
                pos.y = (i + index) * -cellDistance;

            if (transList[i] != null)
                transList[i].localPosition = pos;
        }

        SetList(itemScList, index, Mathf.Min(maxCount, totalCount));
        lastIndex = index;
    }

    //영역 밖으로 벗어났을 때 영역 안으로 돌아가려는 힘을 가한다. //
    void SpringMomentum(ref float momentum, float overDis)
    {
        if (overDis == 0f)
            return;

        //영역 이전이라면 overDis는 음수, 이후라면 overDis는 양수. //
        //overDis의 영역이 클 수록 추가되는 momentum 의 크기도 커진다. //
        //수평모드일 경우. overDis의 정방향로 momentum을 더한다. //
        //수직모드일 경우. overDis의 역방향으로 momentum을 더한다. //
        float dis = (overDis > 0) ? 1f : -1f;
        dis = horizon ? dis : dis * -1f;
        overDis = Mathf.Abs(overDis);

        bool keepCalculate = true;
        //한창 저항을 받는 중. //
        if ((momentum > 0f && dis < 0f) || (momentum < 0f || dis > 0f))
        {
            if (overDis < maxDraggingLimit)
            {
                keepCalculate = false;

                float resistance = Mathf.Lerp(0, overDis * dis, RealTime.deltaTime * 10f * force);
                momentum = Mathf.Lerp(momentum, resistance, RealTime.deltaTime * 10f * force);
            }
        }

        if (keepCalculate)
        {
            float resistance = overDis * dis;
            momentum = resistance * RealTime.deltaTime * force * 10f;
        }
    }

    //드래그 할 때, 제한영역을 벗어나면 이동하는 거리가 줄어들게 하는 함수. //
    void LimitDragging(ref Vector3 offset, float overDis)
    {
        float distance = Mathf.Abs(overDis);
        if (distance > maxDraggingLimit)
            distance = maxDraggingLimit;
        float draggingRate = 1f - (distance * invMaxDraggingLimit);
        if (horizon)
            offset.x *= draggingRate;
        else
            offset.y *= draggingRate;
    }

    public void UpdateAllItems()
    {
        for (int i = 0, max = itemScList.Count; i < max; i++)
        {
            itemScList[i].UpdateUI();
        }
    }
}

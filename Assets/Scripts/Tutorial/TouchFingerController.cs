using UnityEngine;
using System.Collections;

public class TouchFingerController : MonoBehaviour
{
    [SerializeField] private GameObject mObject;
    [SerializeField] private Transform mTrans;

    public void Init()
    {
        Inactive();
    }

    public void Active(Vector3 pos)
    {
        mTrans.localPosition = pos;
        mObject.SetActive(true);
    }

    public void Inactive()
    {
        mObject.SetActive(false);
    }
}

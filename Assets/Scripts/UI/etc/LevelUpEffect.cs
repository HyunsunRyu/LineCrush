using UnityEngine;
using System.Collections;

public class LevelUpEffect : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private GameObject gObj;
    [SerializeField] private Transform mTrans;

    [SerializeField] private float startHeight = -121f;
    [SerializeField] private float endHeight = -100f;

    private bool isAnim = false;

    public void Hide()
    {
        isAnim = false;
        gObj.SetActive(false);
    }

    public void LevelUp()
    {
        if (isAnim)
            return;

        gObj.SetActive(true);

        Vector3 start = new Vector3(0f, startHeight, 0f);
        Vector3 end = new Vector3(0f, endHeight, 0f);
        
        isAnim = true;
        AnimCurveController.Move(curve, start, end, 0.6f, mTrans, () =>
        {
            isAnim = false;
        });
    }
}

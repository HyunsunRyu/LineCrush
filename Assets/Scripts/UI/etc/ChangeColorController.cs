using UnityEngine;
using System.Collections;

public class ChangeColorController : MonoBehaviour
{
    [SerializeField] private UIWidget widget;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private Color start;
    [SerializeField] private Color end;
    [SerializeField] private float totalTime;
    [SerializeField] private bool bLoop = false;

    private void OnEnable()
    {
        ChangeColor();
    }

    private void ChangeColor()
    {
        AnimCurveController.Color(curve, start, end, totalTime, widget, () =>
        {
            if (bLoop)
                ChangeColor();
        });
    }
}

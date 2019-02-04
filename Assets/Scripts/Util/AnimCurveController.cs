using UnityEngine;
using System.Collections;

public class AnimCurveController : MonoBehaviour
{
    private static AnimCurveController instance = null;

    private static AnimCurveController GetInstance()
    {
        if (instance == null)
            instance = FindObjectOfType<AnimCurveController>();
        if (instance == null)
        {
            GameObject obj = new GameObject("AnimCurveController");
            instance = obj.AddComponent<AnimCurveController>();
        }
        return instance;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public static void Move(AnimationCurve curve, Vector3 from, Vector3 to, float totalTime, Transform target, System.Action callback)
    {
        Vector3 direction = to - from;
        GetInstance().StartCoroutine(GetInstance().StartAnimation(curve, totalTime, (bool finish, float rate)=>
        {
            Vector3 pos = from + direction * rate;
            target.localPosition = pos;

            if (finish)
            {
                if (callback != null)
                    callback();
            }
        }));
    }

    public static void Scale(AnimationCurve curve, Vector3 start, Vector3 end, float totalTime, Transform target, System.Action callback)
    {
        Vector3 value = end - start;
        GetInstance().StartCoroutine(GetInstance().StartAnimation(curve, totalTime, (bool finish, float rate) =>
        {
            Vector3 scale = start + value * rate;
            target.localScale = scale;

            if (finish)
            {
                if (callback != null)
                    callback();
            }
        }));
    }

    public static void Rotation(AnimationCurve curve, Vector3 axis, float startAngle, float endAngle, float totalTime, Transform target, System.Action callback)
    {
        float value = endAngle - startAngle;
        GetInstance().StartCoroutine(GetInstance().StartAnimation(curve, totalTime, (bool finish, float rate) =>
        {
            float angle = startAngle + value * rate;
            target.rotation = Quaternion.AngleAxis(angle, axis);

            if (finish)
            {
                if (callback != null)
                    callback();
            }
        }));
    }

    public static void FadeIn(AnimationCurve curve, float totalTime, UIWidget widget, System.Action callback)
    {
        Fade(curve, 1f, 0f, totalTime, widget, callback);
    }

    public static void FadeOut(AnimationCurve curve, float totalTime, UIWidget widget, System.Action callback)
    {
        Fade(curve, 0f, 1f, totalTime, widget, callback);
    }

    public static void Fade(AnimationCurve curve, float startAlpha, float endAlpha, float totalTime, UIWidget widget, System.Action callback)
    {
        Color color = widget.color;
        float value = endAlpha - startAlpha;
        GetInstance().StartCoroutine(GetInstance().StartAnimation(curve, totalTime, (bool finish, float rate) =>
        {
            color.a = startAlpha + value * rate;
            widget.color = color;

            if (finish)
            {
                if (callback != null)
                    callback();
            }
        }));
    }

    public static void Color(AnimationCurve curve, Color start, Color end, float totalTime, UIWidget target, System.Action callback)
    {
        float r = end.r - start.r;
        float g = end.g - start.g;
        float b = end.b - start.b;

        GetInstance().StartCoroutine(GetInstance().StartAnimation(curve, totalTime, (bool finish, float rate) =>
        {
            Color color = new Color(start.r + r * rate, start.g + g * rate, start.b + b * rate);
            target.color = color;

            if (finish)
            {
                if (callback != null)
                    callback();
            }
        }));
    }

    private IEnumerator StartAnimation(AnimationCurve curve, float totalTime, System.Action<bool, float> callback)
    {
        float end = curve[curve.length - 1].time;
        float delta = 0f;
        float invDelta = 1f / totalTime;

        bool bWork = true;
        while (bWork)
        {
            delta += Time.deltaTime * invDelta;
            if (delta >= end)
            {
                delta = end;
                bWork = false;
            }
            float value = curve.Evaluate(delta);
            callback(!bWork, value);
            yield return null;
        }
    }

    public static float GetValue(AnimationCurve curve, float from, float to, float rate)
    {
        float end = curve[curve.length - 1].time;
        if (rate < 0f)
            rate = 0f;
        else if (rate > end)
            rate = end;
        
        return (to - from) * curve.Evaluate(end * rate) + from;
    }

    public static Vector3 GetValue(AnimationCurve curve, Vector3 from, Vector3 to, float rate)
    {
        return (to - from) * GetValue(curve, 0f, 1f, rate) + from;
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillPointEffect : PoolObject
{
    [System.Serializable]
    private class Effect
    {
        public GameObject root = null;
        public Transform body = null;
        public List<Transform> piece = null;
    }
    
    [SerializeField] private Effect[] effects;
    [SerializeField] private float time = 1f;

    private System.Action finishCallback = null;
    private Effect selectedEffect = null;

    private List<Vector3> posList = new List<Vector3>();

    public void Init(int idx)
    {
        posList.Clear();
        selectedEffect = null;
        for (int i = 0, iMax = effects.Length; i < iMax; i++)
        {
            if (i == idx)
            {
                selectedEffect = effects[i];

                effects[i].root.SetActive(true);

                for (int j = 0, jMax = selectedEffect.piece.Count; j < jMax; j++)
                {
                    selectedEffect.piece[j].localPosition = Vector3.zero;
                }
            }
            else
                effects[i].root.SetActive(false);
        }
    }

    public void MoveTo(AnimationCurve moveCurve, AnimationCurve speedCurve, Vector3 from, Vector3 to, System.Action callback)
    {
        if (selectedEffect == null)
            return;

        finishCallback = callback;
        selectedEffect.body.localPosition = from;

        Vector3 direction = to - from;
        float distance = direction.magnitude;
        selectedEffect.body.localPosition = from;
        selectedEffect.body.right = direction.normalized;

        StartCoroutine(StartAnimation(speedCurve, time, (bool finish, float value) =>
        {
            float x = value * distance;
            float y = moveCurve.Evaluate(value) * distance;

            if (selectedEffect.piece.Count > posList.Count)
                posList.Add(new Vector3(x, y, 0f));
            else
            {
                posList.RemoveAt(0);
                posList.Add(new Vector3(x, y, 0f));
            }

            int count = selectedEffect.piece.Count - 1;
            int max = Mathf.Min(posList.Count, selectedEffect.piece.Count);
            for (int i = 0; i < max; i++)
            {
                selectedEffect.piece[count - i].localPosition = posList[i];
            }
            
            if (finish)
            {
                if (finishCallback != null)
                    finishCallback();

                PoolManager.ReturnObject(this);
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
}

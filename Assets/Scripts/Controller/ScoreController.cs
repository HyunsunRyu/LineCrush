using UnityEngine;
using System.Collections;

public class ScoreController : PoolObject
{
    [SerializeField] private UILabel scoreLabel;
    [SerializeField] private Transform labelBody;

    public void ShowScore(string scoreText, Define.ScoreType type, System.Action callback)
    {
        scoreLabel.text = scoreText;

        switch (type)
        {
            case Define.ScoreType.Basic:
                scoreLabel.color = Util.GetColor(171, 197, 158);
                scoreLabel.fontSize = 18;
                labelBody.localScale = new Vector3(1.8f, 1.8f, 1f);
                break;
            case Define.ScoreType.Double:
                scoreLabel.color = Util.GetColor(123, 178, 95);
                scoreLabel.fontSize = 20;
                labelBody.localScale = new Vector3(2f, 2f, 1f);
                break;
            case Define.ScoreType.Triple:
                scoreLabel.color = Util.GetColor(100, 229, 34);
                scoreLabel.fontSize = 24;
                labelBody.localScale = new Vector3(2f, 2f, 1f);
                break;
            case Define.ScoreType.Quad:
                scoreLabel.color = Util.GetColor(60, 226, 241);
                scoreLabel.fontSize = 30;
                labelBody.localScale = new Vector3(2f, 2f, 1f);
                break;
            case Define.ScoreType.Amazing:
                scoreLabel.color = Util.GetColor(85, 129, 255);
                scoreLabel.fontSize = 36;
                labelBody.localScale = new Vector3(2f, 2f, 1f);
                break;
        }

        StartCoroutine(ScoreAnim(() =>
        {
            if (callback != null)
                callback();
        }));
    }

    private IEnumerator ScoreAnim(System.Action callback)
    {
        yield return new WaitForSeconds(1f);
        //yield return null;

        if (callback != null)
            callback();
    }
}

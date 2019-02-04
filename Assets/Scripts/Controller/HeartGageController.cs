using UnityEngine;
using System.Collections;

public class HeartGageController : MonoBehaviour
{
    [SerializeField] private UISprite heartGage;
    [SerializeField] private UILabel gageLabel;

    private float nowValue, targetValue;
    private bool bFill = false;
    private const float fillSpeed = 4f;

    public void Init()
    {
        heartGage.fillAmount = 0f;
        nowValue = 0f;
        targetValue = 0f;
        gageLabel.text = "";
        bFill = false;
    }

    public void SetGage(int count, int max)
    {
        targetValue = (float)count / (float)max;
        gageLabel.text = count.ToString() + "/" + max.ToString();
        if (!bFill)
            StartCoroutine(FillGage());
    }

    private IEnumerator FillGage()
    {
        bFill = true;
        while (bFill)
        {
            nowValue += Time.deltaTime * fillSpeed;
            if (nowValue >= targetValue)
            {
                nowValue = targetValue;
                bFill = false;
            }
            heartGage.fillAmount = nowValue;   
            yield return null;
        }
    }
}

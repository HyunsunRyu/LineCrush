using UnityEngine;
using System.Collections;

public class AddTimeController : PoolObject
{
    [SerializeField] private UILabel label;

    public void SetAddTime(float seconds)
    {
        label.text = "+" + seconds.ToString();
    }

    public void AfterFinishedAnim()
    {
        label.color = Color.white;

        PoolManager.ReturnObject<AddTimeController>(this);
    }
}

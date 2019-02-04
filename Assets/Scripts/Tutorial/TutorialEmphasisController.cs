using UnityEngine;
using System.Collections;

public class TutorialEmphasisController : MonoBehaviour
{
    [SerializeField] private GameObject allBlinder;
    [SerializeField] private GameObject targetBlinder;
    [SerializeField] private UISprite targetBlinderImage;

    public void Init()
    {
        SetAllBlind(false);
    }

    public void SetAllBlind(bool onOff)
    {
        allBlinder.SetActive(onOff);
        targetBlinder.SetActive(false);
    }

    public void SetEmphasis(Vector3 pos, Vector3 size)
    {
        targetBlinder.SetActive(true);
        allBlinder.SetActive(false);

        targetBlinder.transform.localPosition = pos;
        targetBlinderImage.width = (int)size.x;
        targetBlinderImage.height = (int)size.y;
    }
}

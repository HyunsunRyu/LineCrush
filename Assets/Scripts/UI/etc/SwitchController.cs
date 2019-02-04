using UnityEngine;

public class SwitchController : MonoBehaviour
{
    [SerializeField] private GameObject onObject;
    [SerializeField] private GameObject offObject;

    public bool onOff { get; private set; }

    private System.Action<bool> switchCallback;

    public void Init(System.Action<bool> switchCallback)
    {
        this.switchCallback = switchCallback;
        onOff = false;
    }

    public void Switch()
    {
        SoundManager.GetInstance().PlaySound(Define.SoundType.Click);
        SetOnOff(!onOff);
    }

    public void SetOnOff(bool onOff)
    {
        this.onOff = onOff;

        onObject.SetActive(onOff);
        offObject.SetActive(!onOff);

        if (switchCallback != null)
            switchCallback(onOff);
    }
}

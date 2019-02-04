using UnityEngine;
using System.Collections;

public abstract class TutorialBase : MonoBehaviour
{
    [SerializeField] private Define.TutorialType type;

    public abstract void Init();
    public abstract void CallMethod(string methodName);

    public Define.TutorialType GetTutorialType()
    {
        return type;
    }
}

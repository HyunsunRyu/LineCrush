using UnityEngine;

//모든 씬은 씬 내에서만 일어나는 모든 과정을 담당하는 Manager가 있어야 하고, //
//해당 Manager는 SceneObject를 상속받은채로 SceneManager를 통해 관리된다. //
public abstract class SceneObject : MonoBehaviour
{
    public abstract void ClearManager();
    protected abstract void Awake();
    protected abstract void OnEnable();
}

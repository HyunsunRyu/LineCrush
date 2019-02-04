using UnityEngine;
using System.Collections;

public abstract class ISystem : MonoBehaviour
{
	public abstract void InitSystem();
	public abstract void ClearSystem();
    public abstract void InitData();
}

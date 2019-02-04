using UnityEngine;
using System.Collections;

public class LineEffectController : PoolObject
{
    [SerializeField] private Animator anim;

    private void OnEnable()
    {
        anim.Play("line_effect");
    }

    public void ReturnObject()
    {
        PoolManager.ReturnObject<LineEffectController>(this);
    }
}

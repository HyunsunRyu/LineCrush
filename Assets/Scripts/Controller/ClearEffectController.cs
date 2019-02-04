using UnityEngine;
using System.Collections;

public class ClearEffectController : PoolObject
{
    [SerializeField] private ParticleSystem[] effects;
    private ParticleSystem.EmissionModule[] emissions;

    private const float destroyTime = 1f;
    
    public void Init()
    {
        if (effects == null)
            return;

        if (emissions == null)
        {
            int count = effects.Length;
            emissions = new ParticleSystem.EmissionModule[count];
            for (int i = 0; i < count; i++)
            {
                emissions[i] = effects[i].emission;
            }
        }

        for (int i = 0; i < effects.Length; i++)
        {
            emissions[i].enabled = true;
        }

        StartCoroutine(DestroyDelay());
    }

    private IEnumerator DestroyDelay()
    {
        yield return new WaitForSeconds(destroyTime);
        ReturnEffect();
    }

    public void ImmDestroy()
    {
        StopCoroutine(DestroyDelay());
        ReturnEffect();
    }

    private void ReturnEffect()
    {
        PoolManager.ReturnObject(this);
    }
}

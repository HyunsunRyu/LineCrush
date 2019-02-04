using UnityEngine;
using System.Collections.Generic;

public class Assistance : MonoBehaviour
{
    private static Dictionary<string, Assistance> settedAssistanceDic = new Dictionary<string, Assistance>();

    protected TileSystem tileSystem;

    private int keyValue;

    public virtual void Init(TileSystem tileSystem)
    {
        this.tileSystem = tileSystem;
    }

    public virtual void Enable()
    {
    }

    public virtual void ClearMemory()
    {
    }

    public static void ClearAssistances()
    {
        settedAssistanceDic.Clear();
    }

    public static void SetAssistance(Assistance assistance)
    {
        settedAssistanceDic.Add(assistance.GetType().Name, assistance);
    }

    public static void InitAllAssistances(TileSystem system)
    {
        foreach (KeyValuePair<string, Assistance> item in settedAssistanceDic)
        {
            item.Value.Init(system);
        }
    }

    public static void EnableAllAssistances()
    {
        foreach (KeyValuePair<string, Assistance> item in settedAssistanceDic)
        {
            item.Value.Enable();
        }
    }

    public static void ClearMemoryAllAssistances()
    {
        foreach (KeyValuePair<string, Assistance> item in settedAssistanceDic)
        {
            item.Value.ClearMemory();
        }
    }

    public static Assistance GetAssistance(string name)
    {
        if (settedAssistanceDic.ContainsKey(name))
            return settedAssistanceDic[name];

        return null;
    }
}

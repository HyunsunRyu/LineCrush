using UnityEngine;
using System.Collections.Generic;

public struct RandomTable
{
	private List<int> rateList;
	private List<int> idxList;
	private int totalRate;

	public static RandomTable MakeRandomTable()
	{
        RandomTable table = new RandomTable();
        table.Init();
        return table;
	}

    private void Init()
    {
        rateList = new List<int>();
        idxList = new List<int>();
        totalRate = 0;
        Clear();
    }

	public void Clear()
	{
		totalRate = 0;
		idxList.Clear();
		rateList.Clear();
	}

	public void AddRate(int idx, int rate)
	{
		totalRate += rate;
		idxList.Add(idx);
		rateList.Add(totalRate);
	}

	public int GetRandomIdx()
	{
		int value = Random.Range(0, totalRate);
		for(int i=0, max=rateList.Count; i<max; i++)
		{
			if(rateList[i] > value)
			{
				return idxList[i];
			}
		}
		Debug.Log("something is wrong");
		return 0;
	}

    public static List<T> GetSelectRandomMembers<T>(List<T> baseList, int selectCount) where T : struct
    {
        List<T> returnlist = new List<T>();
        int count = baseList.Count;
        selectCount = Mathf.Min(count, selectCount);

        while(selectCount > 0)
        {
            int idx = Random.Range(0, selectCount);
            returnlist.Add(baseList[idx]);
            baseList.RemoveAt(idx);
            selectCount--;
        }
        return returnlist;
    }
}

using UnityEngine;
using System.Collections.Generic;

public class PuzzleTable : DataController.ListBase
{
	public int idx { get; private set; }
	public int rate { get; private set; }
	public List<Pos> blockInfo { get; private set; }

	public override bool TryLoadTable( string[] str )
	{
		int arrayIdx = 0;
		try
		{
			idx = int.Parse(str[arrayIdx++]);
			rate = int.Parse(str[arrayIdx++]);

			blockInfo = new List<Pos>();
			int maxCount = str.Length - 1;

			while(arrayIdx < maxCount)
			{
				int x = int.Parse(str[arrayIdx++]);
				int y = int.Parse(str[arrayIdx++]);
				Pos pos = new Pos(x, y);
				blockInfo.Add(pos);
			}
		}
		catch(System.Exception e)
		{
			Debug.Log(arrayIdx);
			if(e != null)
			{
				Debug.LogError(e);
			}
			return false;
		}
		return true;
	}
}
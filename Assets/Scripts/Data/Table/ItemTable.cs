using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemTable : DataController.ListBase
{
	public int idx { get; protected set; }
	public Define.ItemType type { get; protected set; }
	public int rate { get; protected set; }
	
	public override bool TryLoadTable( string[] str )
	{
		int arrayIdx = 0;
		try
		{
			idx = int.Parse(str[arrayIdx++]);
			type = (Define.ItemType)(int.Parse(str[arrayIdx++]));
			rate = int.Parse(str[arrayIdx++]);
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
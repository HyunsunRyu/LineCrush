using UnityEngine;
using System.Collections;

public struct Pos
{
	private const int minValue = -2147483648;
	
	public int x { get; private set; }
	public int y { get; private set; }
	
	public Pos(int x, int y)
	{
		this.x = x;
		this.y = y;
	}
	
	public Pos(Pos pos)
	{
		this.x = pos.x;
		this.y = pos.y;
	}
	
	public static Pos Base()
	{
		return new Pos(0, 0);
	}
	
	public static Pos Nullity()
	{
		return new Pos(minValue, minValue);
	}
	
	public static bool operator == (Pos lhs, Pos rhs)
	{
		return lhs.x == rhs.x && lhs.y == rhs.y;
	}
	
	public static bool operator != (Pos lhs, Pos rhs)
	{
		return lhs.x != rhs.x || lhs.y != rhs.y;
	}
	
	public bool IsNullity()
	{
		return (this.x == minValue && this.y == minValue);
	}
	
	public override bool Equals(object other)
	{
		if(!(other is Pos))
		{
			return false;
		}
		Pos vector = (Pos)other;
		return this.x.Equals(vector.x) && this.y.Equals(vector.y);
	}
	
	public override int GetHashCode()
	{
		return this.x.GetHashCode() ^ this.y.GetHashCode() << 2;
	}
}

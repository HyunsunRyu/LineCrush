using UnityEngine;
using System.Collections.Generic;

public class UIMultiPick : MonoBehaviour
{
	public enum TriggerType { Press = 0, Release, Click, Drag, Scroll }
	
	[System.Serializable]
	public class Message
	{
		public GameObject listener = null;
		public TriggerType type = TriggerType.Press;
		public string methodName = null;
		
		public Message (){}
		public Message ( GameObject go, TriggerType t )
		{
			listener = go;
			type = t;
		}
		public Message ( GameObject go, TriggerType t, string method )
		{
			listener = go;
			type = t;
			methodName = method;
		}
	}
	public List<Message> messageList = new List<Message>();
	
	void OnPress( bool pressed )
	{
		for( int i=0, max=messageList.Count; i<max; i++ )
		{
			if( messageList[i].type == TriggerType.Press )
			{
				if( messageList[i].methodName == null || messageList[i].methodName == "" )
					messageList[i].listener.SendMessage( "OnPress", pressed, SendMessageOptions.DontRequireReceiver );
				else
					messageList[i].listener.SendMessage( messageList[i].methodName, pressed, SendMessageOptions.DontRequireReceiver );
			}
		}
	}
	
	void OnDrag( Vector2 delta )
	{
		for( int i=0, max=messageList.Count; i<max; i++ )
		{
			if(messageList[i].type == TriggerType.Drag)
			{
				if( messageList[i].methodName == null || messageList[i].methodName == "" )
					messageList[i].listener.SendMessage( "OnDrag", delta, SendMessageOptions.DontRequireReceiver );
				else
					messageList[i].listener.SendMessage( messageList[i].methodName, delta, SendMessageOptions.DontRequireReceiver );
			}
		}
	}
	
	void OnScroll( float delta )
	{
		for( int i=0, max=messageList.Count; i<max; i++ )
		{
			if(messageList[i].type == TriggerType.Drag)
			{
				if( messageList[i].methodName == null || messageList[i].methodName == "" )
					messageList[i].listener.SendMessage( "OnScroll", delta, SendMessageOptions.DontRequireReceiver );
				else
					messageList[i].listener.SendMessage( messageList[i].methodName, delta, SendMessageOptions.DontRequireReceiver );
			}
		}
	}
	
	void OnClick()
	{
		OnReleaseNotMove();
	}
	
	void OnReleaseNotMove()
	{
		for( int i=0, max=messageList.Count; i<max; i++ )
		{
			if(messageList[i].type == TriggerType.Click)
			{
				if( messageList[i].methodName == null || messageList[i].methodName == "" )
					messageList[i].listener.SendMessage( "OnClick", SendMessageOptions.DontRequireReceiver );
				else
					messageList[i].listener.SendMessage( messageList[i].methodName, gameObject ,SendMessageOptions.DontRequireReceiver );
			}
		}
	}
}

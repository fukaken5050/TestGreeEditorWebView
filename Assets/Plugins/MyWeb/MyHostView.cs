using UnityEngine;
using System;

//typedef
using HostView = System.Object;//UnityEngine.Object;
using System.Reflection;

public class MyHostView : UnityEngine.Object
{
	public HostView host = null;
	public bool hasFocus
	{
		get
		{
			var prop = GetType().GetProperty( "hasFocus", BindingFlags.NonPublic | BindingFlags.Instance );
			if( prop != null )
				return Convert.ToBoolean( prop.GetValue( host, null ) );// .GetValue( host ) );
			return false;
		}
	}
	public System.Object actualView
	{
		get
		{
			var field = GetType().GetField( "actualView", BindingFlags.NonPublic | BindingFlags.Instance );
			return field.GetValue( host );
		}
	}

	public static MyHostView Create( HostView host )
	{
		var hostView = new MyHostView();
		hostView.host = host;
		return hostView;
	}

	MyHostView()
	{
	}

	public void RepaintImmediately()
	{
		var method = GetType().GetMethod( "RepaintImmediately", BindingFlags.NonPublic | BindingFlags.Instance );
		method.Invoke( host, new object[]{});
	}
}
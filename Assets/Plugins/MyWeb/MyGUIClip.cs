using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class MyGUIClip
{
	public static Rect Unclip( Rect rect )
	{
	    var assemblyEngine = System.Reflection.Assembly.Load( "UnityEngine.dll" );
        var type = assemblyEngine.GetType( "UnityEngine.GUIClip" );
		var method = type.GetMethod( "Unclip", BindingFlags.Static | BindingFlags.Public, null, new Type[]{ typeof( Rect ) }, null );
		var res = (Rect)method.Invoke( null, new object[]{ rect } );
		return res;
	}
}
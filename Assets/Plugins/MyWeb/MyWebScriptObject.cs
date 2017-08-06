
//typedef
using System.Reflection;
using UnityEngine;
using WebScriptObject = UnityEngine.Object;

public class MyWebScriptObject : UnityEngine.Object
{
	public WebScriptObject m_webScriptObject = null;
	public new HideFlags hideFlags { get{ return m_webScriptObject.hideFlags; } set{ m_webScriptObject.hideFlags = value; } }

	private MyWebView m_webView = null;
	public MyWebView webView
	{ 
		get
		{
			return m_webView;
		}
		set
		{
			m_webView = value;
			var field = m_webScriptObject.GetType().GetField( "m_WebView", BindingFlags.NonPublic | BindingFlags.Instance );
			field.SetValue( m_webScriptObject, m_webView.m_webView );
		}
	}

	public static MyWebScriptObject Create()
	{
		return new MyWebScriptObject();
	}

	MyWebScriptObject()
	{
		var assemblyEditor = System.Reflection.Assembly.Load( "UnityEditor.dll" );
		var typeWebScriptObject = assemblyEditor.GetType( "UnityEditor.Web.WebScriptObject" );
		this.m_webScriptObject = ScriptableObject.CreateInstance( typeWebScriptObject );
	}
}
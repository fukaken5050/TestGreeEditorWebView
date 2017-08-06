using UnityEditor;
using UnityEngine;
//typedef
using WebView = UnityEngine.Object;
using GUIView = UnityEngine.Object;
using System.Reflection;
using System;

public class MyWebView : UnityEngine.ScriptableObject
{
	public string m_url = null;
	public WebView m_webView = null;
	public new HideFlags hideFlags { get{ return m_webView.hideFlags; } set{ m_webView.hideFlags = value; } }

	public static MyWebView Create()
	{
		var webView = ScriptableObject.CreateInstance<MyWebView>();

		var assemblyEditor = System.Reflection.Assembly.Load( "UnityEditor.dll" );
		var type = assemblyEditor.GetType( "UnityEditor.WebView" );
		webView.m_webView = ScriptableObject.CreateInstance( type );
		
		return webView;
	}

	MyWebView()
	{
	}

	public static implicit operator bool( MyWebView exists )
	{
		//return (UnityEngine.Object) exists != (UnityEngine.Object) null && !exists.IntPtrIsNull();
		return ( exists != null );// && exists.m_webView != null && !exists.IntPtrIsNull() );
	}

	public void OnDestroy()
	{
		m_webView = null;
	}

	private void DestroyWebView()
	{
		Debug.Log( "DestroyWebView" );
		InvokeMethod( "DestroyWebView", new object[] {} );
	}

	public void InitWebView( GUIView host, int x, int y, int width, int height, bool showResizeHandle )
	{
		Debug.Log( "InitWebView" );
		InvokeMethod( "InitWebView", new object[] { host, x, y, width, height, showResizeHandle } );
	}

	public void ExecuteJavascript( string scriptCode )
	{
		Debug.Log( "ExecuteJavascript" );
		if( m_webView == null )
			return;
		InvokeMethod( "ExecuteJavascript", new object[] { scriptCode } );
	}

	public void LoadURL( string url )
	{
		Debug.Log( "LoadURL" );
		if( m_webView == null )
			return;
		m_url = url;
		InvokeMethod( "LoadURL", new object[] { url } );
	}

	public void LoadFile( string path )
	{
		Debug.Log( "LoadFile:" + path );
		if( m_webView == null )
			return;
#if true //Add
		var localFile = "file://";
		if( path.StartsWith( localFile ) == true )
		{
			path = path.Substring( localFile.Length );
			Debug.Log( "path:" + path );
		}
#endif
		InvokeMethod( "LoadFile", new object[] { path } );
	}

	public bool DefineScriptObject( string path, ScriptableObject obj )
	{
		Debug.Log( "DefineScriptObject" );
		if( m_webView == null )
			return false;
		return BoolInvokeMethod( "DefineScriptObject", new object[] { path, obj } );
	}

	public void SetDelegateObject( ScriptableObject value )
	{
		Debug.Log( "SetDelegateObject" );
		if( m_webView == null )
			return;
		InvokeMethod( "SetDelegateObject", new object[] { value } );
	}

	public void SetHostView( GUIView view )
	{
	//	Debug.Log( "SetHostView" );
		if( m_webView == null )
			return;
		InvokeMethod( "SetHostView", new object[] { view } );
	}

	public void SetSizeAndPosition( int x, int y, int width, int height )
	{
	//	Debug.Log( "SetSizeAndPosition" );
		if( m_webView == null )
			return;
		InvokeMethod( "SetSizeAndPosition", new object[] { x, y, width, height } );
	}

	public void SetFocus( bool value )
	{
	//	Debug.Log( "SetFocus" );
		if( m_webView == null )
			return;
		InvokeMethod( "SetFocus", new object[] { value } );
	}

	public bool HasApplicationFocus()
	{
		Debug.Log( "HasApplicationFocus" );
		if( m_webView == null )
			return false;
		return BoolInvokeMethod( "HasApplicationFocus", new object[] {} );
	}

	public void SetApplicationFocus(bool applicationFocus)
	{
		Debug.Log( "SetApplicationFocus" );
		if( m_webView == null )
			return;
		InvokeMethod( "SetApplicationFocus", new object[]{ applicationFocus } );
	}

	public void Show()
	{
		Debug.Log( "Show" );
		if( m_webView == null )
			return;
		InvokeMethod( "Show", new object[]{} );
	}

	public void Hide()
	{
		Debug.Log( "Hide" );
		if( m_webView == null )
			return;
		InvokeMethod( "Hide", new object[]{} );
	}

	public void Back()
	{
		Debug.Log( "Back" );
		if( m_webView == null )
			return;
		InvokeMethod( "Back", new object[]{} );
	}

	public void Forward()
	{
		Debug.Log( "Forward" );
		if( m_webView == null )
			return;
		InvokeMethod( "Forward", new object[]{} );
	}

	public void SendOnEvent(string jsonStr)
	{
		Debug.Log( "SendOnEvent" );
		if( m_webView == null )
			return;
		InvokeMethod( "SendOnEvent", new object[]{ jsonStr } );
	}

	public void Reload()
	{
		Debug.Log( "Reload" );
		if( m_webView == null )
			return;
		InvokeMethod( "Reload", new object[]{} );
	}

	public void AllowRightClickMenu(bool allowRightClickMenu)
	{
		Debug.Log( "AllowRightClickMenu" );
		if( m_webView == null )
			return;
		InvokeMethod( "AllowRightClickMenu", new object[]{ allowRightClickMenu } );
	}

	public void ShowDevTools()
	{
		Debug.Log( "ShowDevTools" );
		if( m_webView == null )
			return;
		InvokeMethod( "ShowDevTools", new object[]{} );
	}

	public void ToggleMaximize()
	{
		Debug.Log( "ToggleMaximize" );
		if( m_webView == null )
			return;
		InvokeMethod( "ToggleMaximize", new object[]{} );
	}

	static void OnDomainReload()
	{
		Debug.Log( "OnDomainReload" );
		StaticInvokeMethod( "OnDomainReload", new object[]{}, BindingFlags.NonPublic | BindingFlags.Static );
	}

	private bool IntPtrIsNull()
	{
		Debug.Log( "IntPtrIsNull" );
		if( m_webView == null )
			return true;
		return BoolInvokeMethod( "IntPtrIsNull", new object[] {}, BindingFlags.NonPublic | BindingFlags.Instance );
	}
	//-----------------------------------
	//
	//-----------------------------------
	private void InvokeMethod( string name, object[] param, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance )
	{
		try
		{
			var method = m_webView.GetType().GetMethod( name, bindingFlags );
            method.Invoke( m_webView, param );
		}
		catch ( TargetInvocationException ex )
		{
			Debug.Log( ex.Message );
			return;
		}        
	}

	private bool BoolInvokeMethod( string name, object[] param, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance )
	{
		try
		{
			var method = m_webView.GetType().GetMethod( name, bindingFlags );
            var res = Convert.ToBoolean( method.Invoke( m_webView, param ) );
			return res;
		}
		catch ( TargetInvocationException ex )
		{
			Debug.Log( ex.Message );
			return false;
		}        
	}

	private static void StaticInvokeMethod( string name, object[] param, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static )
	{
		try
		{
			var assemblyEditor = System.Reflection.Assembly.Load( "UnityEditor.dll" );
			var type = assemblyEditor.GetType( "UnityEditor.WebView" );
			var method = type.GetMethod( name, bindingFlags );
            method.Invoke( null, param );
		}
		catch ( TargetInvocationException ex )
		{
			Debug.Log( ex.Message );
			return;
		}        
	}
}
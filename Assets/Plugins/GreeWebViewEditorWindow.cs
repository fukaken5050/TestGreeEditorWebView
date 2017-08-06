using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using System;
using System.IO;

public class GreeWebViewEditorWindow : MyWebViewEditorWindow
{
	public UnityAction<string> onCallFromJS = null;
	public UnityAction<string> onLoaded = null;
	public UnityAction<string> onError = null;
	

	private object m_GlobalObject = (object) null;
	private IEnumerator _opInitScripting = null;

	public override void Init()
	{
		Debug.Log( "Init()" );
		if ( this.m_GlobalObject != null || string.IsNullOrEmpty( this.m_GlobalObjectTypeName ) )
			return;
		Type type = System.Type.GetType( this.m_GlobalObjectTypeName );
		if ( type != null )
		{
			this.m_GlobalObject = AddGlobalObject( type );
		}
	}

	private object AddGlobalObject( Type type )
	{
		Debug.Log( "AddGlobalObject:" + type.Name );

		var assemblyEditor = System.Reflection.Assembly.Load( "UnityEditor.dll" );
		var typeJsProxyMgr = assemblyEditor.GetType( "UnityEditor.Web.JSProxyMgr" );
		var instance = typeJsProxyMgr.GetMethod( "GetInstance" ).Invoke( null, new object[]{} );

		if ( typeJsProxyMgr != null && instance != null)
		{
			var obj = this;//ScriptableObject.CreateInstance( type );
			typeJsProxyMgr.GetMethod( "AddGlobalObject" ).Invoke( instance, new object[] { type.Name, obj } );
			return obj;
		}
		else
			return null;
	}

	public override void OnEnable()
	{
		Debug.Log( "OnEnable" );
		base.OnEnable();
		EditorApplication.update += OnUpdate;
	}

	public void OnDisable()
	{
		Debug.Log( "OnDisable" );
		EditorApplication.update -= OnUpdate;
	}

	public override void OnInitScripting()
	{
		Debug.Log( "OnInitScripting" );
		base.OnInitScripting();
		if( _opInitScripting == null )
			_opInitScripting = OpInitScripting();
	}

	private IEnumerator OpInitScripting()
	{
		var time = 0.3f;
		while( time > 0.0f )
		{
			time -= Time.deltaTime;
			yield return null;
		}
	//	yield return new WaitForSeconds( 3.0f );

		var jsUnityCall = Uri.EscapeUriString( Path.Combine( Application.streamingAssetsPath, "unity_call.js" ) );
		ExecuteJavascript(
			"var script = document.createElement('script');"+
			"script.type = 'text/javascript';"+
			"script.src = '" + jsUnityCall + "';"+
			"document.body.appendChild(script);"
		);
		yield return null;
#if false
		//ソース表示
		ExecuteJavascript(
		"var div = document.createElement('div');"+
		"div.innerText = document.getElementsByTagName('html')[0].innerHTML;"+
		"document.body.appendChild( div );"
		);
#endif
		if( onLoaded != null )
			onLoaded.Invoke( webView.m_url );
		yield break;
	}

	private void OnUpdate()
	{
		if( _opInitScripting != null && _opInitScripting.MoveNext() == false )
			_opInitScripting = null;
	}

	public override void OnGUI()
	{
		base.OnGUI();
	}

	public void CallFromJS( string message )
	{
		Debug.Log( "CallFromJS:" + message );
		if( onCallFromJS != null )
			onCallFromJS.Invoke( message );
	}
}

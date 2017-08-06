using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class TestMyWebViewEditorWindowTabs : MyWebViewEditorWindowTabs
{
	protected object m_GlobalObject = (object) null;

	[MenuItem ("Test/MyWebViewTabs")]
	static void Create()
	{
		TestMyWebViewEditorWindowTabs.Create<TestMyWebViewEditorWindowTabs>( "MyWebViewTabs", null, 200, 530, 800, 600 );
	}

	public override void Init()
	{
		Debug.Log( "Init()" );
		if ( this.m_GlobalObject != null || string.IsNullOrEmpty( this.m_GlobalObjectTypeName ) )
			return;
		Type type = System.Type.GetType( this.m_GlobalObjectTypeName );
		if ( type != null )
		{
		//	this.m_GlobalObject = (object) ScriptableObject.CreateInstance( type );
			this.m_GlobalObject = AddGlobalObject( type );//this.m_GlobalObject.GetType().Name, this.m_GlobalObject );
		}
	//	LoadUri( Path.Combine( Application.streamingAssetsPath, "sample.html" ) );
		LoadUri( Path.Combine( Application.dataPath, "HTML/mytab_index.html" ) );
	}

	private object AddGlobalObject( Type type )
	{
		Debug.Log( "AddGlobalObject:" + type.Name );

		var assemblyEditor = System.Reflection.Assembly.Load( "UnityEditor.dll" );
		var typeJsProxyMgr = assemblyEditor.GetType( "UnityEditor.Web.JSProxyMgr" );
		var instance = typeJsProxyMgr.GetMethod( "GetInstance" ).Invoke( null, new object[]{} );

		if ( typeJsProxyMgr != null && instance != null)
		{
		//	var obj = this;//ScriptableObject.CreateInstance( type );
			var obj = ScriptableObject.CreateInstance( type );
		//	var obj = Activator.CreateInstance( type );
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
	}

	public void OnDisable()
	{
		Debug.Log( "OnDisable" );
	}

	public override void OnInitScripting()
	{
		Debug.Log( "OnInitScripting" );
		base.OnInitScripting();
	}

	public void Play()
	{
		EditorApplication.isPlaying = !EditorApplication.isPlaying;
	}

	public void Pause()
	{
		EditorApplication.isPaused = !EditorApplication.isPaused;
	}

	public void Step()
	{
		EditorApplication.Step();
	}
}

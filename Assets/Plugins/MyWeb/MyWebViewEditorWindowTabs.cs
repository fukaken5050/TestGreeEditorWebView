using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//typedef
using GUIView = UnityEngine.Object;

public class MyWebViewEditorWindowTabs : MyWebViewEditorWindow, IHasCustomMenu, ISerializationCallbackReceiver
{
	protected object m_GlobalObject = (object) null;
	internal MyWebView m_WebView;
	[SerializeField]
	private List<string> m_RegisteredViewURLs;
	[SerializeField]
	private List<MyWebView> m_RegisteredViewInstances;
	private Dictionary<string, MyWebView> m_RegisteredViews;
	private MyHostView m_Parent { get; set; }

	public override MyWebView webView
	{
		get
		{
			return this.m_WebView;
		}
		set
		{
			this.m_WebView = value;
		}
	}

	protected MyWebViewEditorWindowTabs()
	{
		this.m_RegisteredViewURLs = new List<string>();
		this.m_RegisteredViewInstances = new List<MyWebView>();
		this.m_RegisteredViews = new Dictionary<string, MyWebView>();
		this.m_GlobalObject = (object) null;
	}

	public override void Init()
	{
		Debug.Log( "Tabs Init" );
		if (this.m_GlobalObject != null || string.IsNullOrEmpty(this.m_GlobalObjectTypeName))
			return;
		System.Type type = System.Type.GetType(this.m_GlobalObjectTypeName);

		if (type != null)
		{
			//this.m_GlobalObject = (object) ScriptableObject.CreateInstance(type);
			//JSProxyMgr.GetInstance().AddGlobalObject(this.m_GlobalObject.GetType().Name, this.m_GlobalObject);
			AddGlobalObject( type );
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
		//	var obj = this;//ScriptableObject.CreateInstance( type );
			var obj = ScriptableObject.CreateInstance( type );
		//	var obj = Activator.CreateInstance( type );
			typeJsProxyMgr.GetMethod( "AddGlobalObject" ).Invoke( instance, new object[] { type.Name, obj } );
			return obj;
		}
		else
			return null;
	}

	public override void OnDestroy()
	{
		if ((UnityEngine.Object) this.webView != (UnityEngine.Object) null)
			UnityEngine.Object.DestroyImmediate((UnityEngine.Object) this.webView);
		this.m_GlobalObject = (object) null;
		foreach ( MyWebView webView in this.m_RegisteredViews.Values )
		{
			if ((UnityEngine.Object) webView != (UnityEngine.Object) null)
			UnityEngine.Object.DestroyImmediate((UnityEngine.Object) webView);
		}
		this.m_RegisteredViews.Clear();
		this.m_RegisteredViewURLs.Clear();
		this.m_RegisteredViewInstances.Clear();
	}

	public void OnBeforeSerialize()
	{
		Debug.Log( "Tabs OnBeforeSerialize" );
		this.m_RegisteredViewURLs = new List<string>();
		this.m_RegisteredViewInstances = new List<MyWebView>();
		foreach (KeyValuePair<string, MyWebView> registeredView in this.m_RegisteredViews)
		{
			this.m_RegisteredViewURLs.Add(registeredView.Key);
			this.m_RegisteredViewInstances.Add(registeredView.Value);
		}
	}

	public void OnAfterDeserialize()
	{
		Debug.Log( "Tabs OnAfterDeserialize" );
		this.m_RegisteredViews = new Dictionary<string, MyWebView>();
		for (int index = 0; index != Math.Min(this.m_RegisteredViewURLs.Count, this.m_RegisteredViewInstances.Count); ++index)
			this.m_RegisteredViews.Add(this.m_RegisteredViewURLs[index], this.m_RegisteredViewInstances[index]);
	}

	private static string MakeUrlKey(string webViewUrl)
	{
		Debug.Log( "Tabs MakeUrlKey" );
		int length1 = webViewUrl.IndexOf("#");
		string str = length1 == -1 ? webViewUrl : webViewUrl.Substring(0, length1);
		int length2 = str.LastIndexOf("/");
		if (length2 == str.Length - 1)
			return str.Substring(0, length2);
		return str;
	}

	protected void UnregisterWebviewUrl(string webViewUrl)
	{
		Debug.Log( "Tabs UnregisterWebviewUrl" );
		this.m_RegisteredViews[MyWebViewEditorWindowTabs.MakeUrlKey(webViewUrl)] = (MyWebView) null;
	}

	private void RegisterWebviewUrl(string webViewUrl, MyWebView view)
	{
		Debug.Log( "Tabs RegisterWebviewUrl" );
		this.m_RegisteredViews[MyWebViewEditorWindowTabs.MakeUrlKey(webViewUrl)] = view;
	}

	private bool FindWebView(string webViewUrl, out MyWebView webView)
	{
		webView = (MyWebView) null;
		return this.m_RegisteredViews.TryGetValue(MyWebViewEditorWindowTabs.MakeUrlKey(webViewUrl), out webView);
	}

	public MyWebView GetWebViewFromURL(string url)
	{
		return this.m_RegisteredViews[MyWebViewEditorWindowTabs.MakeUrlKey(url)];
	}

	public override void OnInitScripting()
	{
		Debug.Log( "Tabs OnInitScripting" );
		this.SetScriptObject();
	}

	protected override void InitWebView(Rect webViewRect)
	{
		Debug.Log( "Tabs InitWebView" );
		base.InitWebView(webViewRect);
		if (this.m_InitialOpenURL == null || !((UnityEngine.Object) this.webView != (UnityEngine.Object) null))
			return;
		this.RegisterWebviewUrl(this.m_InitialOpenURL, this.webView);
	}

	protected override void LoadPage()
	{
		Debug.Log( "Tabs LoadPage" );
		if (!(bool) this.webView)
			return;
		MyWebView webView;
		if (!this.FindWebView(this.m_InitialOpenURL, out webView) || (UnityEngine.Object) webView == (UnityEngine.Object) null)
		{
			this.NotifyVisibility(false);
			this.webView.SetHostView((GUIView) null);
			this.webView = (MyWebView) null;
			this.InitWebView(MyGUIClip.Unclip(new Rect(0.0f, 0.0f, this.position.width, this.position.height)));
			this.RegisterWebviewUrl(this.m_InitialOpenURL, this.webView);
			this.NotifyVisibility(true);
		}
		else
		{
			if ((UnityEngine.Object) webView != (UnityEngine.Object) this.webView)
			{
				this.NotifyVisibility(false);
				webView.SetHostView((GUIView) this.m_Parent.host);
				this.webView.SetHostView((GUIView) null);
				this.webView = webView;
				this.NotifyVisibility(true);
				this.webView.Show();
			}
			this.LoadUri();
		}
	}
}

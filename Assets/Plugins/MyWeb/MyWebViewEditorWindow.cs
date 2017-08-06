using System;
using System.IO;
using System.Text;
using System.Timers;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using System.Reflection;
//typedef
using GUIView = UnityEngine.Object;

public class MyWebViewEditorWindow : EditorWindow, IHasCustomMenu
{
	[SerializeField]
	public string m_InitialOpenURL;
	[SerializeField]
	public string m_GlobalObjectTypeName;
	internal MyWebScriptObject scriptObject;
	protected bool m_SyncingFocus;
	protected bool m_HasDelayedRefresh;
	private Timer m_PostLoadTimer;
	private const int k_RepaintTimerDelay = 30;

	//internal
	private MyHostView m_Parent { get; set; }

	private bool m_hasFocus
	{
		get
		{
			var prop = GetType().GetProperty( "hasFocus", BindingFlags.NonPublic | BindingFlags.Instance );
			if( prop != null )
				return Convert.ToBoolean( prop.GetValue( this, null ) );//prop.GetValue( this )
			return false;
		}
	}

	public string initialOpenUrl
	{
		get
		{
			return this.m_InitialOpenURL;
		}
		set
		{
			this.m_InitialOpenURL = value;
		}
	}

	public virtual MyWebView webView { get; set; }

	protected MyWebViewEditorWindow()
	{
		this.m_HasDelayedRefresh = false;
	}

	public static T Create<T>(string title, string sourcesPath, int minWidth, int minHeight, int maxWidth, int maxHeight) where T : MyWebViewEditorWindow
	{
		T instance = ScriptableObject.CreateInstance<T>();
		instance.m_GlobalObjectTypeName = typeof (T).FullName;
		MyWebViewEditorWindow.CreateWindowCommon<T>(instance, title, sourcesPath, minWidth, minHeight, maxWidth, maxHeight);
		instance.Show();
		return instance;
	}

	public static T CreateUtility<T>(string title, string sourcesPath, int minWidth, int minHeight, int maxWidth, int maxHeight) where T : MyWebViewEditorWindow
	{
		T instance = ScriptableObject.CreateInstance<T>();
		instance.m_GlobalObjectTypeName = typeof (T).FullName;
		MyWebViewEditorWindow.CreateWindowCommon<T>(instance, title, sourcesPath, minWidth, minHeight, maxWidth, maxHeight);
		instance.ShowUtility();
		return instance;
	}

	public static T CreateBase<T>(string title, string sourcesPath, int minWidth, int minHeight, int maxWidth, int maxHeight) where T : MyWebViewEditorWindow
	{
		T window = EditorWindow.GetWindow<T>(title);
		MyWebViewEditorWindow.CreateWindowCommon<T>(window, title, sourcesPath, minWidth, minHeight, maxWidth, maxHeight);
		window.Show();
		return window;
	}

	public virtual void AddItemsToMenu(GenericMenu menu)
	{
		menu.AddItem(new GUIContent("Reload"), false, new GenericMenu.MenuFunction(this.Reload));
		if (!Unsupported.IsDeveloperBuild())
		return;
		menu.AddItem(new GUIContent("About"), false, new GenericMenu.MenuFunction(this.About));
	}

	public void Reload()
	{
		if ((UnityEngine.Object) this.webView == (UnityEngine.Object) null)
			return;
		this.webView.Reload();
	}

	public void About()
	{
		if ((UnityEngine.Object) this.webView == (UnityEngine.Object) null)
			return;
		this.webView.LoadURL("chrome://version");
	}

	public void OnLoadError(string url)
	{
		if (!(bool) this.webView)
		;
	}

	public void ToggleMaximize()
	{
		this.maximized = !this.maximized;
		this.Refresh();
		this.SetFocus(true);
	}

	public virtual void Init()
	{
	}

	public virtual void OnGUI()
	{
		Rect rect = MyGUIClip.Unclip(new Rect(0.0f, 0.0f, this.position.width, this.position.height));
		GUILayout.BeginArea(rect);
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		GUILayout.Label("Loading...", EditorStyles.label, new GUILayoutOption[0]);
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.EndArea();
		if (Event.current.type == EventType.Repaint && this.m_HasDelayedRefresh)
		{
			this.Refresh();
			this.m_HasDelayedRefresh = false;
		}
		if (this.m_InitialOpenURL == null)
			return;
		if (!(bool) this.webView)
			this.InitWebView(rect);
		if (Event.current.type == EventType.Repaint)
		{
			this.webView.SetHostView( (GUIView)this.m_Parent.host );
			this.webView.SetSizeAndPosition((int) rect.x, (int) rect.y, (int) rect.width, (int) rect.height);
		}
	}

	public void OnBatchMode()
	{
		Rect m_WebViewRect = MyGUIClip.Unclip(new Rect(0.0f, 0.0f, this.position.width, this.position.height));
		if (this.m_InitialOpenURL == null || (bool) this.webView)
		return;
		this.InitWebView(m_WebViewRect);
	}

	public void Refresh()
	{
		if ((UnityEngine.Object) this.webView == (UnityEngine.Object) null)
		return;
		this.webView.Hide();
		this.webView.Show();
	}

	public void OnFocus()
	{
		this.SetFocus(true);
	}

	public void OnLostFocus()
	{
		this.SetFocus(false);
	}

	public virtual void OnEnable()
	{
		this.Init();
	}

	public void OnBecameInvisible()
	{
		if (!(bool) this.webView)
		return;
		this.webView.SetHostView((GUIView) null);
	}

	public virtual void OnDestroy()
	{
		if (!((UnityEngine.Object) this.webView != (UnityEngine.Object) null))
		return;
		UnityEngine.Object.DestroyImmediate((UnityEngine.Object) this.webView);
	}

	private void DoPostLoadTask()
	{
		Debug.Log( "DoPostLoadTask" );
		EditorApplication.update -= new EditorApplication.CallbackFunction(this.DoPostLoadTask);
		this.RepaintImmediately();
	}

	void RepaintImmediately()
	{
		Debug.Log( "RepaintImmediately" );
		if (!(bool) ((UnityEngine.Object) this.m_Parent) || !((UnityEngine.Object) this.m_Parent.actualView == (UnityEngine.Object) this))
		return;
		this.m_Parent.RepaintImmediately();
	}

	private void RaisePostLoadCondition(object obj, ElapsedEventArgs args)
	{
		Debug.Log( "RaisePostLoadCondition" );
		this.m_PostLoadTimer.Stop();
		this.m_PostLoadTimer = (Timer) null;
		EditorApplication.update += new EditorApplication.CallbackFunction(this.DoPostLoadTask);
	}

	protected void LoadUri()
	{
		LoadUri( this.m_InitialOpenURL );
/*
		if (this.m_InitialOpenURL.StartsWith("http"))
		{
			this.webView.LoadURL(this.m_InitialOpenURL);
			this.m_PostLoadTimer = new Timer(30.0);
			this.m_PostLoadTimer.Elapsed += new ElapsedEventHandler(this.RaisePostLoadCondition);
			this.m_PostLoadTimer.Enabled = true;
		}
		else if (this.m_InitialOpenURL.StartsWith("file"))
			this.webView.LoadFile(this.m_InitialOpenURL);
		else
			this.webView.LoadFile(Path.Combine(Uri.EscapeUriString(Path.Combine(EditorApplication.applicationContentsPath, "Resources")), this.m_InitialOpenURL));
*/
	}

	public virtual void LoadUri( string uri )
	{
		if( webView == null )
		{
			this.m_InitialOpenURL = uri;
			return;
		}

		if ( uri.StartsWith("http") )
		{
			this.webView.LoadURL( uri );
			this.m_PostLoadTimer = new Timer(30.0);
			this.m_PostLoadTimer.Elapsed += new ElapsedEventHandler( this.RaisePostLoadCondition );
			this.m_PostLoadTimer.Enabled = true;
		}
		else if ( uri.StartsWith("file") )
			this.webView.LoadFile( uri );
		else
			this.webView.LoadFile( Path.Combine( Uri.EscapeUriString( Path.Combine( EditorApplication.applicationContentsPath, "Resources" ) ), uri ) );
	}

	protected virtual void InitWebView(Rect m_WebViewRect)
	{
		if ( !(bool) this.webView )
		{
			var field = GetType().GetField( "m_Parent", BindingFlags.NonPublic | BindingFlags.Instance );
			this.m_Parent = MyHostView.Create( field.GetValue( this ) );

			int x = (int) m_WebViewRect.x;
			int y = (int) m_WebViewRect.y;
			int width = (int) m_WebViewRect.width;
			int height = (int) m_WebViewRect.height;
			this.webView = MyWebView.Create();
			this.webView.InitWebView((GUIView) this.m_Parent.host, x, y, width, height, true );//false);
			this.webView.hideFlags = HideFlags.HideAndDontSave;
			this.SetFocus(this.m_hasFocus);
		}
		this.webView.SetDelegateObject((ScriptableObject)this);
		this.LoadUri();
		this.SetFocus(true);
	}

	public virtual void OnInitScripting()
	{
		Debug.Log( "OnInitScripting" );
		this.SetScriptObject();
	}

	public void NotifyVisibility(bool visible)
	{
		if ((UnityEngine.Object) this.webView == (UnityEngine.Object) null)
		return;
		this.webView.ExecuteJavascript("document.dispatchEvent(new CustomEvent('showWebView',{ detail: { visible:" + (!visible ? "false" : "true") + "}, bubbles: true, cancelable: false }));");
	}

	protected virtual void LoadPage()
	{
		if (!(bool) this.webView)
			return;
		this.NotifyVisibility(false);
		this.LoadUri();
		this.webView.Show();
	}

	protected void SetScriptObject()
	{
		if (!(bool) this.webView)
			return;
		this.CreateScriptObject();
		this.webView.DefineScriptObject("window.webScriptObject", (ScriptableObject) this.scriptObject.m_webScriptObject );
	}

	private static void CreateWindowCommon<T>(T window, string title, string sourcesPath, int minWidth, int minHeight, int maxWidth, int maxHeight) where T : MyWebViewEditorWindow
	{
		window.titleContent = new GUIContent(title);
		window.minSize = new Vector2((float) minWidth, (float) minHeight);
		window.maxSize = new Vector2((float) maxWidth, (float) maxHeight);
		window.m_InitialOpenURL = sourcesPath;
		window.Init();
	}

	private void CreateScriptObject()
	{
		if ((UnityEngine.Object) this.scriptObject != (UnityEngine.Object) null)
		return;

		this.scriptObject = MyWebScriptObject.Create();//ScriptableObject.CreateInstance( typeWebScriptObject );//<WebScriptObject>();
		this.scriptObject.hideFlags = HideFlags.HideAndDontSave;
		this.scriptObject.webView = this.webView;
	}

	private void InvokeJSMethod(string objectName, string name, params object[] args)
	{
		if (!(bool) this.webView)
			return;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(objectName);
		stringBuilder.Append('.');
		stringBuilder.Append(name);
		stringBuilder.Append('(');
		bool flag1 = true;
		foreach (object obj in args)
		{
		if (!flag1)
			stringBuilder.Append(',');
		bool flag2 = obj is string;
		if (flag2)
			stringBuilder.Append('"');
		stringBuilder.Append(obj);
		if (flag2)
			stringBuilder.Append('"');
		flag1 = false;
		}
		stringBuilder.Append(");");
		this.webView.ExecuteJavascript(stringBuilder.ToString());
	}

	public void ExecuteJavascript( string code )
	{
		if (!(bool) this.webView)
			return;
		this.webView.ExecuteJavascript( code );
	}

	private void SetFocus( bool value )
	{
		if ( this.m_SyncingFocus )
			return;
		this.m_SyncingFocus = true;
		if ( (UnityEngine.Object) this.webView != (UnityEngine.Object) null )
		{
			if (value)
			{
				this.webView.SetHostView( (GUIView) this.m_Parent.host );
				if ( Application.platform != RuntimePlatform.WindowsEditor )
					this.m_HasDelayedRefresh = true;
				else
					this.webView.Show();
			}
			this.webView.SetApplicationFocus((UnityEngine.Object) this.m_Parent != (UnityEngine.Object) null && this.m_Parent.hasFocus && this.m_hasFocus);
			this.webView.SetFocus( value );
		}
		this.m_SyncingFocus = false;
	}
}

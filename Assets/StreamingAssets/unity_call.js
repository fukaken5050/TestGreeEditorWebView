var Unity = function(){};
Unity.call = function( jsCode )
{
  unityAsync ({ 
	className : "window.webScriptObject" , 
	funcName : "ProcessMessage", 
	funcArgs : [JSON.stringify ({ 
			type : "INVOKE", 
			messageID : 1, 
			version : "0", 
			reference : "GreeWebViewEditorWindow", 
			destination : "GreeWebViewEditorWindow", 
			method :  "CallFromJS", 
			params : [ jsCode ] 
		})], 
	onSuccess : function ( a )  { 
		console.log ( "returnValue : "  +  a ) 
	}
  }) 
};

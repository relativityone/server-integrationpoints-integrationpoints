using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------



//
//This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.42000.
//
 // ERROR: Not supported in C#: OptionDeclaration
namespace kCura.EDDS.WebAPI.ExportManagerBase
{

	///<remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"), System.Diagnostics.DebuggerStepThroughAttribute(), System.ComponentModel.DesignerCategoryAttribute("code"), System.Web.Services.WebServiceBindingAttribute(Name = "ExportManagerSoap", Namespace = "http://www.kCura.com/EDDS/ExportManager"), System.Xml.Serialization.XmlIncludeAttribute(typeof(object[]))]
	public partial class ExportManager : System.Web.Services.Protocols.SoapHttpClientProtocol
	{

		private System.Threading.SendOrPostCallback InitializeSearchExportOperationCompleted;

		private System.Threading.SendOrPostCallback InitializeFolderExportOperationCompleted;

		private System.Threading.SendOrPostCallback InitializeProductionExportOperationCompleted;

		private System.Threading.SendOrPostCallback RetrieveResultsBlockForProductionOperationCompleted;

		private System.Threading.SendOrPostCallback RetrieveResultsBlockOperationCompleted;

		private System.Threading.SendOrPostCallback HasExportPermissionsOperationCompleted;

		private bool useDefaultCredentialsSetExplicitly;

		///<remarks/>
		public ExportManager() : base()
		{
			this.Url = global::My_Project.MySettings.Default.kCura_WinEDDS_kCura_EDDS_WebAPI_ExportManagerBase_ExportManager;
			if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
				this.UseDefaultCredentials = true;
				this.useDefaultCredentialsSetExplicitly = false;
			} else {
				this.useDefaultCredentialsSetExplicitly = true;
			}
		}

		public new string Url {
			get { return base.Url; }
			set {
				if ((((this.IsLocalFileSystemWebService(base.Url) == true) && (this.useDefaultCredentialsSetExplicitly == false)) && (this.IsLocalFileSystemWebService(value) == false))) {
					base.UseDefaultCredentials = false;
				}
				base.Url = value;
			}
		}

		public new bool UseDefaultCredentials {
			get { return base.UseDefaultCredentials; }
			set {
				base.UseDefaultCredentials = value;
				this.useDefaultCredentialsSetExplicitly = true;
			}
		}

		///<remarks/>
		public event InitializeSearchExportCompletedEventHandler InitializeSearchExportCompleted;

		///<remarks/>
		public event InitializeFolderExportCompletedEventHandler InitializeFolderExportCompleted;

		///<remarks/>
		public event InitializeProductionExportCompletedEventHandler InitializeProductionExportCompleted;

		///<remarks/>
		public event RetrieveResultsBlockForProductionCompletedEventHandler RetrieveResultsBlockForProductionCompleted;

		///<remarks/>
		public event RetrieveResultsBlockCompletedEventHandler RetrieveResultsBlockCompleted;

		///<remarks/>
		public event HasExportPermissionsCompletedEventHandler HasExportPermissionsCompleted;

		///<remarks/>
		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/ExportManager/InitializeSearchExport", RequestNamespace = "http://www.kCura.com/EDDS/ExportManager", ResponseNamespace = "http://www.kCura.com/EDDS/ExportManager", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
		public InitializationResults InitializeSearchExport(int appID, int searchArtifactID, int[] avfIds, int startAtRecord)
		{
			object[] results = this.Invoke("InitializeSearchExport", new object[] {
				appID,
				searchArtifactID,
				avfIds,
				startAtRecord
			});
			return (InitializationResults)results[0];
		}

		///<remarks/>
		public System.IAsyncResult BeginInitializeSearchExport(int appID, int searchArtifactID, int[] avfIds, int startAtRecord, System.AsyncCallback callback, object asyncState)
		{
			return this.BeginInvoke("InitializeSearchExport", new object[] {
				appID,
				searchArtifactID,
				avfIds,
				startAtRecord
			}, callback, asyncState);
		}

		///<remarks/>
		public InitializationResults EndInitializeSearchExport(System.IAsyncResult asyncResult)
		{
			object[] results = this.EndInvoke(asyncResult);
			return (InitializationResults)results[0];
		}

		///<remarks/>
		public void InitializeSearchExportAsync(int appID, int searchArtifactID, int[] avfIds, int startAtRecord)
		{
			this.InitializeSearchExportAsync(appID, searchArtifactID, avfIds, startAtRecord, null);
		}

		///<remarks/>
		public void InitializeSearchExportAsync(int appID, int searchArtifactID, int[] avfIds, int startAtRecord, object userState)
		{
			if ((this.InitializeSearchExportOperationCompleted == null)) {
				this.InitializeSearchExportOperationCompleted = this.OnInitializeSearchExportOperationCompleted;
			}
			this.InvokeAsync("InitializeSearchExport", new object[] {
				appID,
				searchArtifactID,
				avfIds,
				startAtRecord
			}, this.InitializeSearchExportOperationCompleted, userState);
		}

		private void OnInitializeSearchExportOperationCompleted(object arg)
		{
			if ((((this.InitializeSearchExportCompleted) != null))) {
				System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = (System.Web.Services.Protocols.InvokeCompletedEventArgs)arg;
				if (InitializeSearchExportCompleted != null) {
					InitializeSearchExportCompleted(this, new InitializeSearchExportCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
				}
			}
		}

		///<remarks/>
		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/ExportManager/InitializeFolderExport", RequestNamespace = "http://www.kCura.com/EDDS/ExportManager", ResponseNamespace = "http://www.kCura.com/EDDS/ExportManager", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
		public InitializationResults InitializeFolderExport(int appID, int viewArtifactID, int parentArtifactID, bool includeSubFolders, int[] avfIds, int startAtRecord, int artifactTypeID)
		{
			object[] results = this.Invoke("InitializeFolderExport", new object[] {
				appID,
				viewArtifactID,
				parentArtifactID,
				includeSubFolders,
				avfIds,
				startAtRecord,
				artifactTypeID
			});
			return (InitializationResults)results[0];
		}

		///<remarks/>
		public System.IAsyncResult BeginInitializeFolderExport(int appID, int viewArtifactID, int parentArtifactID, bool includeSubFolders, int[] avfIds, int startAtRecord, int artifactTypeID, System.AsyncCallback callback, object asyncState)
		{
			return this.BeginInvoke("InitializeFolderExport", new object[] {
				appID,
				viewArtifactID,
				parentArtifactID,
				includeSubFolders,
				avfIds,
				startAtRecord,
				artifactTypeID
			}, callback, asyncState);
		}

		///<remarks/>
		public InitializationResults EndInitializeFolderExport(System.IAsyncResult asyncResult)
		{
			object[] results = this.EndInvoke(asyncResult);
			return (InitializationResults)results[0];
		}

		///<remarks/>
		public void InitializeFolderExportAsync(int appID, int viewArtifactID, int parentArtifactID, bool includeSubFolders, int[] avfIds, int startAtRecord, int artifactTypeID)
		{
			this.InitializeFolderExportAsync(appID, viewArtifactID, parentArtifactID, includeSubFolders, avfIds, startAtRecord, artifactTypeID, null);
		}

		///<remarks/>
		public void InitializeFolderExportAsync(int appID, int viewArtifactID, int parentArtifactID, bool includeSubFolders, int[] avfIds, int startAtRecord, int artifactTypeID, object userState)
		{
			if ((this.InitializeFolderExportOperationCompleted == null)) {
				this.InitializeFolderExportOperationCompleted = this.OnInitializeFolderExportOperationCompleted;
			}
			this.InvokeAsync("InitializeFolderExport", new object[] {
				appID,
				viewArtifactID,
				parentArtifactID,
				includeSubFolders,
				avfIds,
				startAtRecord,
				artifactTypeID
			}, this.InitializeFolderExportOperationCompleted, userState);
		}

		private void OnInitializeFolderExportOperationCompleted(object arg)
		{
			if ((((this.InitializeFolderExportCompleted) != null))) {
				System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = (System.Web.Services.Protocols.InvokeCompletedEventArgs)arg;
				if (InitializeFolderExportCompleted != null) {
					InitializeFolderExportCompleted(this, new InitializeFolderExportCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
				}
			}
		}

		///<remarks/>
		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/ExportManager/InitializeProductionExport", RequestNamespace = "http://www.kCura.com/EDDS/ExportManager", ResponseNamespace = "http://www.kCura.com/EDDS/ExportManager", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
		public InitializationResults InitializeProductionExport(int appID, int productionArtifactID, int[] avfIds, int startAtRecord)
		{
			object[] results = this.Invoke("InitializeProductionExport", new object[] {
				appID,
				productionArtifactID,
				avfIds,
				startAtRecord
			});
			return (InitializationResults)results[0];
		}

		///<remarks/>
		public System.IAsyncResult BeginInitializeProductionExport(int appID, int productionArtifactID, int[] avfIds, int startAtRecord, System.AsyncCallback callback, object asyncState)
		{
			return this.BeginInvoke("InitializeProductionExport", new object[] {
				appID,
				productionArtifactID,
				avfIds,
				startAtRecord
			}, callback, asyncState);
		}

		///<remarks/>
		public InitializationResults EndInitializeProductionExport(System.IAsyncResult asyncResult)
		{
			object[] results = this.EndInvoke(asyncResult);
			return (InitializationResults)results[0];
		}

		///<remarks/>
		public void InitializeProductionExportAsync(int appID, int productionArtifactID, int[] avfIds, int startAtRecord)
		{
			this.InitializeProductionExportAsync(appID, productionArtifactID, avfIds, startAtRecord, null);
		}

		///<remarks/>
		public void InitializeProductionExportAsync(int appID, int productionArtifactID, int[] avfIds, int startAtRecord, object userState)
		{
			if ((this.InitializeProductionExportOperationCompleted == null)) {
				this.InitializeProductionExportOperationCompleted = this.OnInitializeProductionExportOperationCompleted;
			}
			this.InvokeAsync("InitializeProductionExport", new object[] {
				appID,
				productionArtifactID,
				avfIds,
				startAtRecord
			}, this.InitializeProductionExportOperationCompleted, userState);
		}

		private void OnInitializeProductionExportOperationCompleted(object arg)
		{
			if ((((this.InitializeProductionExportCompleted) != null))) {
				System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = (System.Web.Services.Protocols.InvokeCompletedEventArgs)arg;
				if (InitializeProductionExportCompleted != null) {
					InitializeProductionExportCompleted(this, new InitializeProductionExportCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
				}
			}
		}

		///<remarks/>
		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/ExportManager/RetrieveResultsBlockForProduction", RequestNamespace = "http://www.kCura.com/EDDS/ExportManager", ResponseNamespace = "http://www.kCura.com/EDDS/ExportManager", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
		public object[] RetrieveResultsBlockForProduction(int appID, System.Guid runId, int artifactTypeID, int[] avfIds, int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter, char nestedValueDelimiter, int[] textPrecedenceAvfIds, int productionId)
		{
			object[] results = this.Invoke("RetrieveResultsBlockForProduction", new object[] {
				appID,
				runId,
				artifactTypeID,
				avfIds,
				chunkSize,
				displayMulticodesAsNested,
				multiValueDelimiter,
				nestedValueDelimiter,
				textPrecedenceAvfIds,
				productionId
			});
			return (object[])results[0];
		}

		///<remarks/>
		public System.IAsyncResult BeginRetrieveResultsBlockForProduction(int appID, System.Guid runId, int artifactTypeID, int[] avfIds, int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter, char nestedValueDelimiter, int[] textPrecedenceAvfIds, int productionId,
		System.AsyncCallback callback, object asyncState)
		{
			return this.BeginInvoke("RetrieveResultsBlockForProduction", new object[] {
				appID,
				runId,
				artifactTypeID,
				avfIds,
				chunkSize,
				displayMulticodesAsNested,
				multiValueDelimiter,
				nestedValueDelimiter,
				textPrecedenceAvfIds,
				productionId
			}, callback, asyncState);
		}

		///<remarks/>
		public object[] EndRetrieveResultsBlockForProduction(System.IAsyncResult asyncResult)
		{
			object[] results = this.EndInvoke(asyncResult);
			return (object[])results[0];
		}

		///<remarks/>
		public void RetrieveResultsBlockForProductionAsync(int appID, System.Guid runId, int artifactTypeID, int[] avfIds, int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter, char nestedValueDelimiter, int[] textPrecedenceAvfIds, int productionId)
		{
			this.RetrieveResultsBlockForProductionAsync(appID, runId, artifactTypeID, avfIds, chunkSize, displayMulticodesAsNested, multiValueDelimiter, nestedValueDelimiter, textPrecedenceAvfIds, productionId,
			null);
		}

		///<remarks/>
		public void RetrieveResultsBlockForProductionAsync(int appID, System.Guid runId, int artifactTypeID, int[] avfIds, int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter, char nestedValueDelimiter, int[] textPrecedenceAvfIds, int productionId,
		object userState)
		{
			if ((this.RetrieveResultsBlockForProductionOperationCompleted == null)) {
				this.RetrieveResultsBlockForProductionOperationCompleted = this.OnRetrieveResultsBlockForProductionOperationCompleted;
			}
			this.InvokeAsync("RetrieveResultsBlockForProduction", new object[] {
				appID,
				runId,
				artifactTypeID,
				avfIds,
				chunkSize,
				displayMulticodesAsNested,
				multiValueDelimiter,
				nestedValueDelimiter,
				textPrecedenceAvfIds,
				productionId
			}, this.RetrieveResultsBlockForProductionOperationCompleted, userState);
		}

		private void OnRetrieveResultsBlockForProductionOperationCompleted(object arg)
		{
			if ((((this.RetrieveResultsBlockForProductionCompleted) != null))) {
				System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = (System.Web.Services.Protocols.InvokeCompletedEventArgs)arg;
				if (RetrieveResultsBlockForProductionCompleted != null) {
					RetrieveResultsBlockForProductionCompleted(this, new RetrieveResultsBlockForProductionCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
				}
			}
		}

		///<remarks/>
		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/ExportManager/RetrieveResultsBlock", RequestNamespace = "http://www.kCura.com/EDDS/ExportManager", ResponseNamespace = "http://www.kCura.com/EDDS/ExportManager", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
		public object[] RetrieveResultsBlock(int appID, System.Guid runId, int artifactTypeID, int[] avfIds, int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter, char nestedValueDelimiter, int[] textPrecedenceAvfIds)
		{
			object[] results = this.Invoke("RetrieveResultsBlock", new object[] {
				appID,
				runId,
				artifactTypeID,
				avfIds,
				chunkSize,
				displayMulticodesAsNested,
				multiValueDelimiter,
				nestedValueDelimiter,
				textPrecedenceAvfIds
			});
			return (object[])results[0];
		}

		///<remarks/>
		public System.IAsyncResult BeginRetrieveResultsBlock(int appID, System.Guid runId, int artifactTypeID, int[] avfIds, int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter, char nestedValueDelimiter, int[] textPrecedenceAvfIds, System.AsyncCallback callback,
		object asyncState)
		{
			return this.BeginInvoke("RetrieveResultsBlock", new object[] {
				appID,
				runId,
				artifactTypeID,
				avfIds,
				chunkSize,
				displayMulticodesAsNested,
				multiValueDelimiter,
				nestedValueDelimiter,
				textPrecedenceAvfIds
			}, callback, asyncState);
		}

		///<remarks/>
		public object[] EndRetrieveResultsBlock(System.IAsyncResult asyncResult)
		{
			object[] results = this.EndInvoke(asyncResult);
			return (object[])results[0];
		}

		///<remarks/>
		public void RetrieveResultsBlockAsync(int appID, System.Guid runId, int artifactTypeID, int[] avfIds, int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter, char nestedValueDelimiter, int[] textPrecedenceAvfIds)
		{
			this.RetrieveResultsBlockAsync(appID, runId, artifactTypeID, avfIds, chunkSize, displayMulticodesAsNested, multiValueDelimiter, nestedValueDelimiter, textPrecedenceAvfIds, null);
		}

		///<remarks/>
		public void RetrieveResultsBlockAsync(int appID, System.Guid runId, int artifactTypeID, int[] avfIds, int chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter, char nestedValueDelimiter, int[] textPrecedenceAvfIds, object userState)
		{
			if ((this.RetrieveResultsBlockOperationCompleted == null)) {
				this.RetrieveResultsBlockOperationCompleted = this.OnRetrieveResultsBlockOperationCompleted;
			}
			this.InvokeAsync("RetrieveResultsBlock", new object[] {
				appID,
				runId,
				artifactTypeID,
				avfIds,
				chunkSize,
				displayMulticodesAsNested,
				multiValueDelimiter,
				nestedValueDelimiter,
				textPrecedenceAvfIds
			}, this.RetrieveResultsBlockOperationCompleted, userState);
		}

		private void OnRetrieveResultsBlockOperationCompleted(object arg)
		{
			if ((((this.RetrieveResultsBlockCompleted) != null))) {
				System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = (System.Web.Services.Protocols.InvokeCompletedEventArgs)arg;
				if (RetrieveResultsBlockCompleted != null) {
					RetrieveResultsBlockCompleted(this, new RetrieveResultsBlockCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
				}
			}
		}

		///<remarks/>
		[System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://www.kCura.com/EDDS/ExportManager/HasExportPermissions", RequestNamespace = "http://www.kCura.com/EDDS/ExportManager", ResponseNamespace = "http://www.kCura.com/EDDS/ExportManager", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
		public bool HasExportPermissions(int appID)
		{
			object[] results = this.Invoke("HasExportPermissions", new object[] { appID });
			return Convert.ToBoolean(results[0]);
		}

		///<remarks/>
		public System.IAsyncResult BeginHasExportPermissions(int appID, System.AsyncCallback callback, object asyncState)
		{
			return this.BeginInvoke("HasExportPermissions", new object[] { appID }, callback, asyncState);
		}

		///<remarks/>
		public bool EndHasExportPermissions(System.IAsyncResult asyncResult)
		{
			object[] results = this.EndInvoke(asyncResult);
			return Convert.ToBoolean(results[0]);
		}

		///<remarks/>
		public void HasExportPermissionsAsync(int appID)
		{
			this.HasExportPermissionsAsync(appID, null);
		}

		///<remarks/>
		public void HasExportPermissionsAsync(int appID, object userState)
		{
			if ((this.HasExportPermissionsOperationCompleted == null)) {
				this.HasExportPermissionsOperationCompleted = this.OnHasExportPermissionsOperationCompleted;
			}
			this.InvokeAsync("HasExportPermissions", new object[] { appID }, this.HasExportPermissionsOperationCompleted, userState);
		}

		private void OnHasExportPermissionsOperationCompleted(object arg)
		{
			if ((((this.HasExportPermissionsCompleted) != null))) {
				System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = (System.Web.Services.Protocols.InvokeCompletedEventArgs)arg;
				if (HasExportPermissionsCompleted != null) {
					HasExportPermissionsCompleted(this, new HasExportPermissionsCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
				}
			}
		}

		///<remarks/>
		public new void CancelAsync(object userState)
		{
			base.CancelAsync(userState);
		}

		private bool IsLocalFileSystemWebService(string url)
		{
			if (((url == null) || (object.ReferenceEquals(url, string.Empty)))) {
				return false;
			}
			System.Uri wsUri = new System.Uri(url);
			if (((wsUri.Port >= 1024) && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
				return true;
			}
			return false;
		}
	}

	///<remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.6.1055.0"), System.SerializableAttribute(), System.Diagnostics.DebuggerStepThroughAttribute(), System.ComponentModel.DesignerCategoryAttribute("code"), System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.kCura.com/EDDS/ExportManager")]
	public partial class InitializationResults
	{

		private System.Guid runIdField;

		private long rowCountField;

		private string[] columnNamesField;

		///<remarks/>
		public System.Guid RunId {
			get { return this.runIdField; }
			set { this.runIdField = value; }
		}

		///<remarks/>
		public long RowCount {
			get { return this.rowCountField; }
			set { this.rowCountField = value; }
		}

		///<remarks/>
		public string[] ColumnNames {
			get { return this.columnNamesField; }
			set { this.columnNamesField = value; }
		}
	}

	///<remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
	public delegate void InitializeSearchExportCompletedEventHandler(object sender, InitializeSearchExportCompletedEventArgs e);

	///<remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"), System.Diagnostics.DebuggerStepThroughAttribute(), System.ComponentModel.DesignerCategoryAttribute("code")]
	public partial class InitializeSearchExportCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
	{

		private object[] results;

		internal InitializeSearchExportCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : base(exception, cancelled, userState)
		{
			this.results = results;
		}

		///<remarks/>
		public InitializationResults Result {
			get {
				this.RaiseExceptionIfNecessary();
				return (InitializationResults)this.results[0];
			}
		}
	}

	///<remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
	public delegate void InitializeFolderExportCompletedEventHandler(object sender, InitializeFolderExportCompletedEventArgs e);

	///<remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"), System.Diagnostics.DebuggerStepThroughAttribute(), System.ComponentModel.DesignerCategoryAttribute("code")]
	public partial class InitializeFolderExportCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
	{

		private object[] results;

		internal InitializeFolderExportCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : base(exception, cancelled, userState)
		{
			this.results = results;
		}

		///<remarks/>
		public InitializationResults Result {
			get {
				this.RaiseExceptionIfNecessary();
				return (InitializationResults)this.results[0];
			}
		}
	}

	///<remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
	public delegate void InitializeProductionExportCompletedEventHandler(object sender, InitializeProductionExportCompletedEventArgs e);

	///<remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"), System.Diagnostics.DebuggerStepThroughAttribute(), System.ComponentModel.DesignerCategoryAttribute("code")]
	public partial class InitializeProductionExportCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
	{

		private object[] results;

		internal InitializeProductionExportCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : base(exception, cancelled, userState)
		{
			this.results = results;
		}

		///<remarks/>
		public InitializationResults Result {
			get {
				this.RaiseExceptionIfNecessary();
				return (InitializationResults)this.results[0];
			}
		}
	}

	///<remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
	public delegate void RetrieveResultsBlockForProductionCompletedEventHandler(object sender, RetrieveResultsBlockForProductionCompletedEventArgs e);

	///<remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"), System.Diagnostics.DebuggerStepThroughAttribute(), System.ComponentModel.DesignerCategoryAttribute("code")]
	public partial class RetrieveResultsBlockForProductionCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
	{

		private object[] results;

		internal RetrieveResultsBlockForProductionCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : base(exception, cancelled, userState)
		{
			this.results = results;
		}

		///<remarks/>
		public object[] Result {
			get {
				this.RaiseExceptionIfNecessary();
				return (object[])this.results[0];
			}
		}
	}

	///<remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
	public delegate void RetrieveResultsBlockCompletedEventHandler(object sender, RetrieveResultsBlockCompletedEventArgs e);

	///<remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"), System.Diagnostics.DebuggerStepThroughAttribute(), System.ComponentModel.DesignerCategoryAttribute("code")]
	public partial class RetrieveResultsBlockCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
	{

		private object[] results;

		internal RetrieveResultsBlockCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : base(exception, cancelled, userState)
		{
			this.results = results;
		}

		///<remarks/>
		public object[] Result {
			get {
				this.RaiseExceptionIfNecessary();
				return (object[])this.results[0];
			}
		}
	}

	///<remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
	public delegate void HasExportPermissionsCompletedEventHandler(object sender, HasExportPermissionsCompletedEventArgs e);

	///<remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0"), System.Diagnostics.DebuggerStepThroughAttribute(), System.ComponentModel.DesignerCategoryAttribute("code")]
	public partial class HasExportPermissionsCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs
	{

		private object[] results;

		internal HasExportPermissionsCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : base(exception, cancelled, userState)
		{
			this.results = results;
		}

		///<remarks/>
		public bool Result {
			get {
				this.RaiseExceptionIfNecessary();
				return Convert.ToBoolean(this.results[0]);
			}
		}
	}
}

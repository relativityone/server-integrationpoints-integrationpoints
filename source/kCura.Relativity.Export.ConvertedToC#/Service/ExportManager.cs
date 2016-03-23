using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Service
{
	public class ExportManager : kCura.EDDS.WebAPI.ExportManagerBase.ExportManager
	{

		protected override System.Net.WebRequest GetWebRequest(System.Uri uri)
		{
			System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)base.GetWebRequest(uri);
			wr.UnsafeAuthenticatedConnectionSharing = true;
			wr.Credentials = this.Credentials;
			return wr;
		}

		public ExportManager(System.Net.ICredentials credentials, System.Net.CookieContainer cookieContainer) : base()
		{

			this.Credentials = credentials;
			this.CookieContainer = cookieContainer;
			this.Url = string.Format("{0}ExportManager.asmx", Config.WebServiceURL);
			this.Timeout = Settings.DefaultTimeOut;
		}

		private T MakeCallAttemptReLogin<T>(Func<T> f)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return f();
				} catch (System.Exception ex) {
					UnpackHandledException(ex);
					if (ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("NeedToReLoginException") != -1 && tries < Config.MaxReloginTries) {
						//Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
					} else {
						throw;
					}
				}
			}
			return default(T);
		}

		public new kCura.EDDS.WebAPI.ExportManagerBase.InitializationResults InitializeFolderExport(Int32 appID, Int32 viewArtifactID, Int32 parentArtifactID, bool includeSubFolders, Int32[] avfIds, Int32 startAtRecord, Int32 artifactTypeID)
		{
			return MakeCallAttemptReLogin(() => base.InitializeFolderExport(appID, viewArtifactID, parentArtifactID, includeSubFolders, avfIds, startAtRecord, artifactTypeID));
		}

		public new kCura.EDDS.WebAPI.ExportManagerBase.InitializationResults InitializeProductionExport(Int32 appID, Int32 productionArtifactID, Int32[] avfIds, Int32 startAtRecord)
		{
			return MakeCallAttemptReLogin(() => base.InitializeProductionExport(appID, productionArtifactID, avfIds, startAtRecord));
		}

		public new kCura.EDDS.WebAPI.ExportManagerBase.InitializationResults InitializeSearchExport(Int32 appID, Int32 searchArtifactID, Int32[] avfIds, Int32 startAtRecord)
		{
			return MakeCallAttemptReLogin(() => base.InitializeSearchExport(appID, searchArtifactID, avfIds, startAtRecord));
		}

		public new object[] RetrieveResultsBlock(Int32 appID, Guid runId, Int32 artifactTypeID, Int32[] avfIds, Int32 chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter, char nestedValueDelimiter, Int32[] textPrecedenceAvfIds)
		{
			object[] retval = MakeCallAttemptReLogin(() => base.RetrieveResultsBlock(appID, runId, artifactTypeID, avfIds, chunkSize, displayMulticodesAsNested, multiValueDelimiter, nestedValueDelimiter, textPrecedenceAvfIds));
			if ((retval != null)) {
				foreach (object[] row in retval) {
					if (row == null) {
						throw new System.Exception("Invalid (null) row retrieved from server");
					}
					for (Int32 i = 0; i <= row.Length - 1; i++) {
						if (row[i] is byte[])
							row[i] = System.Text.Encoding.Unicode.GetString((byte[])row[i]);
					}
				}
			}
			return retval;
		}

		public new object[] RetrieveResultsBlockForProduction(Int32 appID, Guid runId, Int32 artifactTypeID, Int32[] avfIds, Int32 chunkSize, bool displayMulticodesAsNested, char multiValueDelimiter, char nestedValueDelimiter, Int32[] textPrecedenceAvfIds, Int32 productionId)
		{
			object[] retval = MakeCallAttemptReLogin(() => base.RetrieveResultsBlockForProduction(appID, runId, artifactTypeID, avfIds, chunkSize, displayMulticodesAsNested, multiValueDelimiter, nestedValueDelimiter, textPrecedenceAvfIds, productionId));
			if ((retval != null)) {
				foreach (object[] row in retval) {
					if (row == null) {
						throw new System.Exception("Invalid (null) row retrieved from server");
					}
					for (Int32 i = 0; i <= row.Length - 1; i++) {
						if (row[i] is byte[])
							row[i] = System.Text.Encoding.Unicode.GetString((byte[])row[i]);
					}
				}
			}
			return retval;
		}

		public new bool HasExportPermissions(Int32 appID)
		{
			return MakeCallAttemptReLogin(() => base.HasExportPermissions(appID));
		}

		private void UnpackHandledException(System.Exception ex)
		{
			System.Web.Services.Protocols.SoapException soapEx = ex as System.Web.Services.Protocols.SoapException;
			if (soapEx == null)
				return;
			System.Exception x = null;
			try {
				if (soapEx.Detail.SelectNodes("ExceptionType").Item(0).InnerText == "Relativity.Core.Exception.InsufficientAccessControlListPermissions") {
					x = new InsufficientPermissionsForExportException(soapEx.Detail.SelectNodes("ExceptionMessage")[0].InnerText, ex);
				}
			} catch {
			}
			if ((x != null))
				throw x;
		}

		public class InsufficientPermissionsForExportException : System.Exception
		{

			public InsufficientPermissionsForExportException(string message) : base(message)
			{
			}
			public InsufficientPermissionsForExportException(string message, System.Exception ex) : base(message, ex)
			{
			}
		}

	}
}


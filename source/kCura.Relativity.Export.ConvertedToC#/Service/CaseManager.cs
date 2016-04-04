
using System;

using CaseInfo = Relativity.CaseInfo;

namespace kCura.Relativity.Export.Service
{
	public class CaseManager : kCura.EDDS.WebAPI.CaseManagerBase.CaseManager
	{

		public CaseManager(System.Net.ICredentials credentials, System.Net.CookieContainer cookieContainer) : base()
		{

			this.Credentials = credentials;
			this.CookieContainer = cookieContainer;
			this.Url = string.Format("{0}CaseManager.asmx", Config.WebServiceURL);
			this.Timeout = Settings.DefaultTimeOut;
		}

		protected override System.Net.WebRequest GetWebRequest(System.Uri uri)
		{
			System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)base.GetWebRequest(uri);
			wr.UnsafeAuthenticatedConnectionSharing = true;
			wr.Credentials = this.Credentials;
			return wr;
		}

		public static CaseInfo ConvertToCaseInfo(kCura.EDDS.WebAPI.CaseManagerBase.CaseInfo toConvert)
		{
			CaseInfo c = new CaseInfo();
			var _with1 = toConvert;
			c.ArtifactID = _with1.ArtifactID;
			c.MatterArtifactID = _with1.MatterArtifactID;
			c.Name = _with1.Name;
			c.RootArtifactID = _with1.RootArtifactID;
			c.RootFolderID = _with1.RootFolderID;
			c.StatusCodeArtifactID = _with1.StatusCodeArtifactID;
			c.EnableDataGrid = _with1.EnableDataGrid;
			c.DocumentPath = _with1.DocumentPath;
			c.DownloadHandlerURL = _with1.DownloadHandlerURL;
			return c;
		}

		#region " Shadow Functions "
		public new System.Data.DataSet RetrieveAll()
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					if (Config.UsesWebAPI) {
						return base.RetrieveAllEnabled();
					} else {
						//Return _caseManager.RetrieveAll(_identity).ToDataSet()
					}
				} catch (System.Exception ex) {
					if (ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("NeedToReLoginException") != -1 && tries < Config.MaxReloginTries) {
						//Helper.AttemptReLogin(this.Credentials, this.CookieContainer, tries, false);
					} else {
						throw;
					}
				}
			}
			return null;
		}

		public new CaseInfo Read(Int32 caseArtifactID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return ConvertToCaseInfo(base.Read(caseArtifactID));
				} catch (System.Exception ex) {
					if (ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("NeedToReLoginException") != -1) {
						if (tries < Config.MaxReloginTries) {
							//Helper.AttemptReLogin(this.Credentials, this.CookieContainer, tries, false);
						} else {
							throw ex;
						}
					} else {
						throw;
					}
				}
			}
			return null;
		}
		#endregion

	}
}

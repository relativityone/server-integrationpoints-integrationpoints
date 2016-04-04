using System;
using System.Data;

namespace kCura.Relativity.Export.Service
{
	public class UserManager : kCura.EDDS.WebAPI.UserManagerBase.UserManager
	{

		public UserManager(System.Net.ICredentials credentials, System.Net.CookieContainer cookieContainer) : base()
		{

			this.Credentials = credentials;
			this.CookieContainer = cookieContainer;
			this.Url = string.Format("{0}UserManager.asmx", Config.WebServiceURL);
			this.Timeout = Settings.DefaultTimeOut;
		}

		#region " Shadow Methods "
		public new bool Login(string emailAddress, string password)
		{
			return this.RetryOnReLoginException<bool>(() => this.LoginInternal(emailAddress, password));
		}

		private bool LoginInternal(string emailAddress, string password)
		{
			if (Config.UsesWebAPI) {
				try {
					//ClearCookiesBeforeLogin call MUST be made before Login web method is called
					base.ClearCookiesBeforeLogin();
					return base.Login(emailAddress, password);
				} catch (System.Exception ex) {
					throw;
				}
			} else {
				return false;
			}
		}

		public new System.Data.DataSet RetrieveAllAssignableInCase(Int32 caseContextArtifactID)
		{
			return this.RetryOnReLoginException<DataSet>(() => this.RetrieveAllAssignableInCaseInternal(caseContextArtifactID));
		}

		private System.Data.DataSet RetrieveAllAssignableInCaseInternal(Int32 caseContextArtifactID)
		{
			if (Config.UsesWebAPI) {
				return base.RetrieveAllAssignableInCase(caseContextArtifactID);
			} else {
				return null;
			}
		}

		public new string GenerateAuthenticationToken()
		{
			return this.RetryOnReLoginException<string>(() => base.GenerateAuthenticationToken());
		}

		public new string GenerateDistributedAuthenticationToken(bool retryOnFailure = true)
		{
			return this.RetryOnReLoginException<string>(() => base.GenerateDistributedAuthenticationToken(), retryOnFailure);
		}

		#endregion
	}
}

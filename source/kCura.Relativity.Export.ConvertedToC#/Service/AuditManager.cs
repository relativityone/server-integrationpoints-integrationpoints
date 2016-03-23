using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Service
{
	public class AuditManager : kCura.EDDS.WebAPI.AuditManagerBase.AuditManager
	{

		#region "Constructors"

		public AuditManager(System.Net.ICredentials credentials, System.Net.CookieContainer cookieContainer) : base()
		{

			this.Credentials = credentials;
			this.CookieContainer = cookieContainer;
			this.Url = string.Format("{0}AuditManager.asmx", Config.WebServiceURL);
			this.Timeout = Settings.DefaultTimeOut;
		}

		#endregion

		#region " Shadow Methods "
		public new bool? CreateAuditRecord(Int32 caseContextArtifactID, Int32 artifactID, Int32 action, string details, string origination)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return base.CreateAuditRecord(caseContextArtifactID, artifactID, action, details, origination);
				} catch (System.Exception ex) {
					if (ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("NeedToReLoginException") != -1 && tries < Config.MaxReloginTries) {
						//Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
					} else {
						throw;
					}
				}
			}
			return null;
		}

		public new bool? AuditImageImport(Int32 appID, string runId, bool isFatalError, kCura.EDDS.WebAPI.AuditManagerBase.ImageImportStatistics importStats)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return base.AuditImageImport(appID, runId, isFatalError, importStats);
				} catch (System.Exception ex) {
					if (ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("NeedToReLoginException") != -1 && tries < Config.MaxReloginTries) {
						//Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
					} else {
						throw;
					}
				}
			}
			return null;
		}

		public new bool? AuditObjectImport(Int32 appID, string runId, bool isFatalError, kCura.EDDS.WebAPI.AuditManagerBase.ObjectImportStatistics importStats)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return base.AuditObjectImport(appID, runId, isFatalError, importStats);
				} catch (System.Exception ex) {
					if (ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("NeedToReLoginException") != -1 && tries < Config.MaxReloginTries) {
						//Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
					} else {
						throw;
					}
				}
			}
			return null;
		}

		public new bool? AuditExport(Int32 appID, bool isFatalError, kCura.EDDS.WebAPI.AuditManagerBase.ExportStatistics exportStats)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return base.AuditExport(appID, isFatalError, exportStats);
				} catch (System.Exception ex) {
					if (ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("NeedToReLoginException") != -1 && tries < Config.MaxReloginTries) {
						//Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
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

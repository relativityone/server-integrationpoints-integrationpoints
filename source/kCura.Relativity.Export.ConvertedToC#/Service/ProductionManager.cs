using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Service
{
	public class ProductionManager : kCura.EDDS.WebAPI.ProductionManagerBase.ProductionManager
	{

		public ProductionManager(System.Net.ICredentials credentials, System.Net.CookieContainer cookieContainer) : base()
		{

			this.Credentials = credentials;
			this.CookieContainer = cookieContainer;
			this.Url = string.Format("{0}ProductionManager.asmx", Config.WebServiceURL);
			this.Timeout = Settings.DefaultTimeOut;
		}

		protected override System.Net.WebRequest GetWebRequest(System.Uri uri)
		{
			System.Net.HttpWebRequest wr = (System.Net.HttpWebRequest)base.GetWebRequest(uri);
			wr.UnsafeAuthenticatedConnectionSharing = true;
			wr.Credentials = this.Credentials;
			return wr;
		}

		#region " Shadow Functions "
		public new System.Data.DataSet RetrieveProducedByContextArtifactID(Int32 caseContextArtifactID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					if (Config.UsesWebAPI) {
						return base.RetrieveProducedByContextArtifactID(caseContextArtifactID);
					} else {
						//Return _productionManager.ExternalRetrieveProducedByContextArtifactID(contextArtifactID, _identity)
					}
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

		public new System.Data.DataSet RetrieveImportEligibleByContextArtifactID(Int32 caseContextArtifactID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					if (Config.UsesWebAPI) {
						return base.RetrieveImportEligibleByContextArtifactID(caseContextArtifactID);
					} else {
						//Return _productionManager.RetrieveImportEligibleByContextArtifactID(contextArtifactID, _identity)
					}
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

		public new kCura.EDDS.WebAPI.ProductionManagerBase.ProductionInfo Read(Int32 caseContextArtifactID, Int32 productionArtifactID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					if (Config.UsesWebAPI) {
						return base.Read(caseContextArtifactID, productionArtifactID);
					} else {
						//Return Me.DTOToWebAPIProduction(_productionManager.Read(productionArtifactID, _identity))
					}
				} catch (System.Exception ex) {
					if (ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("Need To Re Login") != -1 && tries < Config.MaxReloginTries) {
						//Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
					} else {
						throw;
					}
				}
			}
			return null;
		}

		public new System.Data.DataSet RetrieveProducedWithSecurity(Int32 contextArtifactID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					if (Config.UsesWebAPI) {
						return base.RetrieveProducedByContextArtifactID(contextArtifactID);
					} else {
						//Return _productionManager.ExternalRetrieveProducedWithSecurity(contextArtifactID, _identity)
					}
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

		public new void DoPostImportProcessing(Int32 contextArtifactID, Int32 productionArtifactID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					if (Config.UsesWebAPI) {
						base.DoPostImportProcessing(contextArtifactID, productionArtifactID);
						return;
					}
				} catch (System.Exception ex) {
					if (ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("NeedToReLoginException") != -1 && tries < Config.MaxReloginTries) {
						//Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
					} else {
						throw;
					}
				}
			}
		}

		public new void DoPreImportProcessing(Int32 contextArtifactID, Int32 productionArtifactID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					if (Config.UsesWebAPI) {
						base.DoPreImportProcessing(contextArtifactID, productionArtifactID);
						return;
					}
				} catch (System.Exception ex) {
					if (ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("NeedToReLoginException") != -1 && tries < Config.MaxReloginTries) {
						//Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
					} else {
						throw;
					}
				}
			}
		}

		public new bool MigrationJobExists(Int32 contextArtifactID, Int32 productionArtifactID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					if (Config.UsesWebAPI) {
						return base.MigrationJobExists(contextArtifactID, productionArtifactID);
					}
				} catch (System.Exception ex) {
					if (ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("NeedToReLoginException") != -1 && tries < Config.MaxReloginTries) {
						//Helper.AttemptReLogin(Me.Credentials, Me.CookieContainer, tries)
					} else {
						throw;
					}
				}
			}
			return false;
		}
		#endregion

	}
}

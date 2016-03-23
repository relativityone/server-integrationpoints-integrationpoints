using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
namespace kCura.Relativity.Export.Service
{
	public class SearchManager : kCura.EDDS.WebAPI.SearchManagerBase.SearchManager
	{

		public SearchManager(System.Net.ICredentials credentials, System.Net.CookieContainer cookieContainer) : base()
		{

			this.Credentials = credentials;
			this.CookieContainer = cookieContainer;
			this.Url = string.Format("{0}SearchManager.asmx", Config.WebServiceURL);
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
		public new System.Data.DataSet RetrieveNativesForProduction(Int32 caseContextArtifactID, Int32 productionArtifactID, string documentArtifactIDs)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return base.RetrieveNativesForProduction(caseContextArtifactID, productionArtifactID, documentArtifactIDs);
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

		public new System.Data.DataSet RetrieveNativesForSearch(Int32 caseContextArtifactID, string documentArtifactIDs)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return base.RetrieveNativesForSearch(caseContextArtifactID, documentArtifactIDs);
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

		public new System.Data.DataSet RetrieveFilesForDynamicObjects(Int32 caseContextArtifactID, Int32 fileFieldArtifactID, Int32[] objectIds)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return base.RetrieveFilesForDynamicObjects(caseContextArtifactID, fileFieldArtifactID, objectIds);
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

		public System.Data.DataSet RetrieveImagesForDocuments(Int32 caseContextArtifactID, Int32[] documentArtifactIDs)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return RetrieveImagesForSearch(caseContextArtifactID, documentArtifactIDs);
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

		public System.Data.DataSet RetrieveImagesForProductionDocuments(Int32 caseContextArtifactID, Int32[] documentArtifactIDs, Int32 productionArtifactID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return this.RetrieveImagesByProductionArtifactIDForProductionExportByDocumentSet(caseContextArtifactID, productionArtifactID, documentArtifactIDs);
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

		public new System.Data.DataSet RetrieveImagesForSearch(Int32 caseContextArtifactID, Int32[] documentArtifactIDs)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return base.RetrieveImagesForSearch(caseContextArtifactID, documentArtifactIDs);
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

		public new System.Data.DataSet RetrieveImagesByProductionIDsAndDocumentIDsForExport(Int32 caseContextArtifactID, Int32[] productionArtifactIDs, Int32[] documentArtifactIDs)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return base.RetrieveImagesByProductionIDsAndDocumentIDsForExport(caseContextArtifactID, productionArtifactIDs, documentArtifactIDs);
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

		public new System.Data.DataSet RetrieveViewsByContextArtifactID(Int32 caseContextArtifactID, Int32 artifactTypeID, bool isSearch)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return base.RetrieveViewsByContextArtifactID(caseContextArtifactID, artifactTypeID, isSearch);
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

		public new System.Data.DataSet RetrieveSearchFields(Int32 caseContextArtifactID, Int32 viewArtifactID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return base.RetrieveSearchFields(caseContextArtifactID, viewArtifactID);
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

		public new System.Data.DataSet RetrieveSearchFieldsForProduction(Int32 caseContextArtifactID, Int32 productionArtifactID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return base.RetrieveSearchFieldsForProduction(caseContextArtifactID, productionArtifactID);
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

		public new System.Collections.Specialized.HybridDictionary RetrieveDefaultViewFieldsForIdList(Int32 caseContextArtifactID, Int32 artifactTypeID, Int32[] artifactIdList, bool isProductionList)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					System.Data.DataTable dt = base.RetrieveDefaultViewFieldsForIdList(caseContextArtifactID, artifactTypeID, artifactIdList, isProductionList).Tables[0];
					System.Collections.Specialized.HybridDictionary retval = new System.Collections.Specialized.HybridDictionary();
					foreach (System.Data.DataRow row in dt.Rows) {
						if (!retval.Contains(row["ArtifactID"])) {
							retval.Add(row["ArtifactID"], new ArrayList());
						}
						((ArrayList)retval[row["ArtifactID"]]).Add(row["ArtifactViewFieldID"]);
					}
					return retval;
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

		public new Types.ViewFieldInfo[] RetrieveAllExportableViewFields(Int32 caseContextArtifactID, Int32 artifactTypeID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					System.Data.DataTable dt = base.RetrieveAllExportableViewFields(caseContextArtifactID, artifactTypeID).Tables[0];
					System.Collections.ArrayList retval = new System.Collections.ArrayList();
					foreach (System.Data.DataRow row in dt.Rows) {
						retval.Add(new Types.ViewFieldInfo(row));
					}
					return (Types.ViewFieldInfo[])retval.ToArray(typeof(Types.ViewFieldInfo));
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

		public new bool[] IsAssociatedSearchProviderAccessible(Int32 caseContextArtifactID, Int32 searchArtifactID)
		{
			Int32 tries = 0;
			while (tries < Config.MaxReloginTries) {
				tries += 1;
				try {
					return base.IsAssociatedSearchProviderAccessible(caseContextArtifactID, searchArtifactID);
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

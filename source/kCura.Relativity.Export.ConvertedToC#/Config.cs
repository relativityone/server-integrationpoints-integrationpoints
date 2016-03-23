using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;

namespace kCura.Relativity.Export
{
	public class Config
	{

		#region " ConfigSettings "


		private static System.Object _LoadLock = new System.Object();
		private static System.Collections.IDictionary _configDictionary;
		public static System.Collections.IDictionary ConfigSettings {
			get {
				// You may ask, why are there two identical if/then statements?
				// If the config dictionary is already set, we want to return it.  And 99% of the time, the if/then will handle this, not needing a synclock.  The synclock would just slow things down.

				// If the config dictionary is not set, we want to load it.  Previously it wasn't thread safe, so the synclock was added.

				// However, it is possible that one thread could start this and get into the synclock, and another thread comes in before _configDictionary is set, and tries to enter the load process.
				// This is why we want the second if/then.
				// Next, we don't set _configDictionary until the temporary dictionary is fully formed.  It was possible for multiple threads to throw an error because _configDictionary had been created, 
				// but hadn't had all its values added to it yet, thus code looking for a specific value threw a null reference exception.

				// So here you go, this is a good example of how to make loading a static collection thread safe, and still keep some performance.  - slm - 5/24/2012

				if (_configDictionary == null) {
					lock (_LoadLock) {
						if (_configDictionary == null) {
							System.Collections.IDictionary tempDict = null;
							tempDict = (System.Collections.IDictionary)System.Configuration.ConfigurationManager.GetSection("kCura.WinEDDS");
							if (tempDict == null)
								tempDict = new System.Collections.Hashtable();
							if (!tempDict.Contains("ImportBatchSize"))
								tempDict.Add("ImportBatchSize", "1000");
							if (!tempDict.Contains("WebAPIOperationTimeout"))
								tempDict.Add("WebAPIOperationTimeout", "600000");
							if (!tempDict.Contains("DynamicBatchResizingOn"))
								tempDict.Add("DynamicBatchResizingOn", "True");
							if (!tempDict.Contains("MinimumBatchSize"))
								tempDict.Add("MinimumBatchSize", "100");
							if (!tempDict.Contains("ImportBatchMaxVolume"))
								tempDict.Add("ImportBatchMaxVolume", "10485760");
							//10(2^20) - don't know what 10MB standard is
							if (!tempDict.Contains("ExportBatchSize"))
								tempDict.Add("ExportBatchSize", "1000");
							if (!tempDict.Contains("EnableSingleModeImport"))
								tempDict.Add("EnableSingleModeImport", "False");
							if (!tempDict.Contains("CreateErrorForEmptyNativeFile"))
								tempDict.Add("CreateErrorForEmptyNativeFile", "False");
							if (!tempDict.Contains("AuditLevel"))
								tempDict.Add("AuditLevel", "FullAudit");
							if (!tempDict.Contains("CreateFoldersInWebAPI"))
								tempDict.Add("CreateFoldersInWebAPI", "True");
							if (!tempDict.Contains("ForceWebUpload"))
								tempDict.Add("ForceWebUpload", "False");
							_configDictionary = tempDict;
						}
					}
				}
				return _configDictionary;
			}
		}

		#endregion

		#region " Unused or shouldn't be used "

		public static bool UsesWebAPI {
				//Return CType(ConfigSettings("UsesWebAPI"), Boolean)
			get { return true; }
		}

		#endregion

		#region " Constants "

		public static Int32 MaxReloginTries {
			get { return 4; }
		}

		public static Int32 WaitBeforeReconnect {
			//Milliseconds
			get { return 2000; }
		}


		private const string webServiceUrlKeyName = "WebServiceURL";

		public const Int32 PREVIEW_THRESHOLD = 1000;
		public static string FileTransferModeExplanationText(bool includeBulk) {
		
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb.Append("FILE TRANSFER MODES:" + Constants.vbNewLine);
				sb.Append(" • Web • ");
				sb.Append(Constants.vbNewLine + "The document repository is accessed through the Relativity web service API.  This is the slower of the two methods, but is globally available.");
				sb.Append(Constants.vbNewLine + Constants.vbNewLine);
				sb.Append(" • Direct • ");
				sb.Append(Constants.vbNewLine);
				sb.Append("Direct mode is significantly faster than Web mode.  To use Direct mode, you must:");
				sb.Append(Constants.vbNewLine + Constants.vbNewLine);
				sb.Append(" - Have Windows rights to the document repository.");
				sb.Append(Constants.vbNewLine);
				sb.Append(" - Be logged into the document repository’s network.");
				sb.Append(Constants.vbNewLine + Constants.vbNewLine + "If you meet the above criteria, Relativity will automatically load in Direct mode.  If you are loading in Web mode and think you should have Direct mode, contact your Relativity Administrator to establish the correct rights.");
				sb.Append(Constants.vbNewLine + Constants.vbNewLine);
				if (includeBulk) {
					sb.Append("SQL INSERT MODES:" + Constants.vbNewLine);
					sb.Append(" • Bulk • " + Constants.vbNewLine);
					sb.Append("The upload process has access to the SQL share on the appropriate case database.  This ensures the fastest transfer of information between the desktop client and the relativity servers.");
					sb.Append(Constants.vbNewLine + Constants.vbNewLine);
					sb.Append(" • Single •" + Constants.vbNewLine);
					sb.Append("The upload process has NO access to the SQL share on the appropriate case database.  This is a slower method of import. If the process is using single mode, contact your Relativity Database Administrator to see if a SQL share can be opened for the desired case.");
				}
				return sb.ToString();
		
		}

		#endregion

		#region "Registry Helpers"

		private static string GetRegistryKeyValue(string keyName)
		{
			Microsoft.Win32.RegistryKey regKey = Config.GetRegistryKey(false);
			string value = Convert.ToString(regKey.GetValue(keyName, ""));
			regKey.Close();
			return value;
		}

		private static string SetRegistryKeyValue(string keyName, string keyVal)
		{
			Microsoft.Win32.RegistryKey regKey = Config.GetRegistryKey(true);
			regKey.SetValue(keyName, keyVal);
			regKey.Close();
			return null;

		}

		private static Microsoft.Win32.RegistryKey GetRegistryKey(bool write) {
		
				Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("software\\kCura\\Relativity", write);
				if (regKey == null) {
					Microsoft.Win32.Registry.CurrentUser.CreateSubKey("software\\\\kCura\\\\Relativity");
					regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("software\\kCura\\Relativity", write);
				}
				return regKey;
		
		}

		private static string ValidateURIFormat(string returnValue)
		{
			if (!string.IsNullOrEmpty(returnValue) && !returnValue.Trim().EndsWith("/")) {
				returnValue = returnValue.Trim() + "/";
			}

			//NOTE: This is here for validation; an improper URI will cause this to throw an
			// exception. We set it then to 'Nothing' to avoid a warning-turned-error about
			// having an unused variable. -Phil S. 12/05/2011
			// fixed 1/24/2012 - slm - return an empty string if invalid uri format.  this will cause the 
			// rdc to pop up its dialog prompting the user to enter a valid address

			try {
				Uri uriObj = new Uri(returnValue);
				uriObj = null;
			} catch {
				returnValue = string.Empty;
			}

			return returnValue;
		}

		#endregion

		public static Int32 ImportBatchMaxVolume {
			//Volume in bytes
			get {
				try {
					return (Int32)ConfigSettings["ImportBatchMaxVolume"];
				} catch {
					return 1000000;
				}
			}
		}

		public static Int32 ImportBatchSize {
			//Number of records
			get {
				try {
					return (Int32)ConfigSettings["ImportBatchSize"];
				} catch (Exception ex) {
					return 500;
				}
			}
		}

		public static Int32 WebAPIOperationTimeout {
			get {
				try {
					return (Int32)ConfigSettings["WebAPIOperationTimeout"];
				} catch (Exception ex) {
					return 600000;
				}
			}
		}

		/// <summary>
		/// If True, Folders which are created in Append mode are created in the WebAPI.
		/// If False, Folders which are created in Append mode are created in RDC/ImportAPI.
		/// If the value is not set in the config file, True is returned.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks>This property is part of the fix for Dominus# 1127879</remarks>
		public static bool CreateFoldersInWebAPI {
			get {
				try {
					return Convert.ToBoolean(ConfigSettings["CreateFoldersInWebAPI"]);
				} catch (Exception ex) {
					return true;
					// Default changed from False to True for Atlas' IAPI Folder changes Oct/Nov 2013
				}
			}
		}

		public static bool DynamicBatchResizingOn {
			//Allow or not to automatically decrease import batch size while import is in progress
			get { return Convert.ToBoolean(ConfigSettings["DynamicBatchResizingOn"]); }
		}

		public static Int32 DefaultMaximumErrorCount {
			get { return 1000; }
		}

		public static Int32 MinimumBatchSize {
			//When AutoBatch is on. This is the lower ceiling up to which batch will decrease
			get { return (Int32)ConfigSettings["MinimumBatchSize"]; }
		}

		public static Int32 ExportBatchSize {
			//Number of records
			get { return (Int32)ConfigSettings["ExportBatchSize"]; }
		}

		public static bool DisableImageTypeValidation {
			get { return Convert.ToBoolean(ConfigSettings["DisableImageTypeValidation"]); }
		}

		public static bool DisableImageLocationValidation {
			get { return Convert.ToBoolean(ConfigSettings["DisableImageLocationValidation"]); }
		}

		public static bool DisableNativeValidation {
			get { return Convert.ToBoolean(ConfigSettings["DisableNativeValidation"]); }
		}

		public static bool DisableNativeLocationValidation {
			get { return Convert.ToBoolean(ConfigSettings["DisableNativeLocationValidation"]); }
		}

		public static bool CreateErrorForEmptyNativeFile {
			get { return Convert.ToBoolean(ConfigSettings["CreateErrorForEmptyNativeFile"]); }
		}

		//Public Shared ReadOnly Property AuditLevel() As kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel
		//	Get
		//		Return DirectCast([Enum].Parse(GetType(kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel), CStr(ConfigSettings("AuditLevel"))), kCura.EDDS.WebAPI.BulkImportManagerBase.ImportAuditLevel)
		//	End Get
		//End Property

		//This hidden configuration setting makes it easier to test WebUpload.  Testing is important.
		public static bool ForceWebUpload {
			get { return Convert.ToBoolean(ConfigSettings["ForceWebUpload"]); }
		}

		public static bool ForceFolderPreview {
			get {
				string registryValue = Config.GetRegistryKeyValue("ForceFolderPreview");
				if (string.IsNullOrEmpty(registryValue)) {
					Config.SetRegistryKeyValue("ForceFolderPreview", "true");
					return true;
				}
				return registryValue.ToLower().Equals("true");
			}
			set {
				string registryValue = "false";
				if (value)
					registryValue = "true";
				Config.SetRegistryKeyValue("ForceFolderPreview", registryValue);
			}
		}

		public static IList<Int32> ObjectFieldIdListContainsArtifactId {
			get {
				string registryValue = Config.GetRegistryKeyValue("ObjectFieldIdListContainsArtifactId");
				if (string.IsNullOrEmpty(registryValue)) {
					Config.SetRegistryKeyValue("ObjectFieldIdListContainsArtifactId", "");
					return new List<Int32>();
				}
				return registryValue.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select((string s) => Int32.Parse(s)).ToArray();
			}
			set { Config.SetRegistryKeyValue("ObjectFieldIdListContainsArtifactId", string.Join(",", value.Select(x => x.ToString()).ToArray())); }
		}

		public static string WebServiceURL {
			get {
				string returnValue = null;

				//Programmatic ServiceURL
				if (!string.IsNullOrWhiteSpace(_programmaticServiceURL)) {
					returnValue = _programmaticServiceURL;
				} else if (ConfigSettings.Contains(webServiceUrlKeyName)) {
					//App.config ServiceURL
					string regUrl = Convert.ToString(ConfigSettings[webServiceUrlKeyName]);

					if (!string.IsNullOrWhiteSpace(regUrl)) {
						returnValue = regUrl;
					}
				}

				//Registry ServiceURL
				if (string.IsNullOrWhiteSpace(returnValue)) {
					returnValue = GetRegistryKeyValue(webServiceUrlKeyName);
				}

				return ValidateURIFormat(returnValue);
			}
			set {
				string properURI = ValidateURIFormat(value);

				SetRegistryKeyValue(webServiceUrlKeyName, properURI);
			}
		}


		private static string _programmaticServiceURL = null;
		public static string ProgrammaticServiceURL {
			get { return _programmaticServiceURL; }
			set { _programmaticServiceURL = value; }
		}

		public static bool EnableSingleModeImport {
			get {
				try {
					return Convert.ToBoolean(ConfigSettings["EnableSingleModeImport"]);
				} catch (Exception ex) {
					return false;
				}
			}
		}

		public static Int32 WebBasedFileDownloadChunkSize {
			get {
				if (!ConfigSettings.Contains("WebBasedFileDownloadChunkSize")) {
					ConfigSettings.Add("WebBasedFileDownloadChunkSize", 1048576);
				}
				return System.Math.Max((Int32)ConfigSettings["WebBasedFileDownloadChunkSize"], 1024);
			}
		}
	}
}

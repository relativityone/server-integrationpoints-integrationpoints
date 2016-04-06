using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using kCura.Relativity.Export.FileObjects;
using kCura.Relativity.Export.Service;
using kCura.Relativity.Export.Types;
using Relativity;

namespace kCura.Relativity.Export.Exports
{
	public class Exporter
	{

		#region "Members"

		private Service.SearchManager _searchManager;
		public Service.ExportManager ExportManager;
		private Service.FieldManager _fieldManager;
		private Service.AuditManager _auditManager;
		private Service.CaseManager _caseManager;
		private UserManager _userManager;

		private ExportFile _exportFile;
		private System.Collections.ArrayList _columns;

		public Int32 DocumentsExported;
		public Int32 TotalExportArtifactCount;
		private kCura.Windows.Process.Controller withEventsField__processController;
		private kCura.Windows.Process.Controller _processController {
			get { return withEventsField__processController; }
			set {
				if (withEventsField__processController != null) {
					withEventsField__processController.HaltProcessEvent -= _processController_HaltProcessEvent;
				}
				withEventsField__processController = value;
				if (withEventsField__processController != null) {
					withEventsField__processController.HaltProcessEvent += _processController_HaltProcessEvent;
				}
			}
		}
		private FileDownloader withEventsField__downloadHandler;
		private FileDownloader _downloadHandler {
			get { return withEventsField__downloadHandler; }
			set {
				if (withEventsField__downloadHandler != null) {
					withEventsField__downloadHandler.UploadModeChangeEvent -= _downloadHandler_UploadModeChangeEvent;
				}
				withEventsField__downloadHandler = value;
				if (withEventsField__downloadHandler != null) {
					withEventsField__downloadHandler.UploadModeChangeEvent += _downloadHandler_UploadModeChangeEvent;
				}
			}
		}
		private bool _halt;
		private VolumeManager _volumeManager;
		private Service.ProductionManager _productionManager;
		private Types.ExportNativeWithFilenameFrom _exportNativesToFileNamedFrom;
		private string _beginBatesColumn = "";
		private kCura.Utility.Timekeeper _timekeeper = new kCura.Utility.Timekeeper();
		private Int32[] _productionArtifactIDs;
		private long _lastStatusMessageTs = System.DateTime.Now.Ticks;
		private Int32 _lastDocumentsExportedCountReported = 0;
		private Process.ExportStatistics _statistics = new Process.ExportStatistics();
		private IDictionary _lastStatisticsSnapshot;
		private System.DateTime _start;
		private Int32 _warningCount = 0;
		private Int32 _errorCount = 0;
		private Int64 _fileCount = 0;
		private kCura.EDDS.WebAPI.ProductionManagerBase.ProductionInfo _productionExportProduction;

		private System.Collections.Generic.Dictionary<Int32, kCura.EDDS.WebAPI.ProductionManagerBase.ProductionInfo> _productionLookup = new System.Collections.Generic.Dictionary<Int32, kCura.EDDS.WebAPI.ProductionManagerBase.ProductionInfo>();
		#endregion

		#region "Accessors"

		public ExportFile Settings {
			get { return _exportFile; }
			set { _exportFile = value; }
		}

		public System.Collections.ArrayList Columns {
			get { return _columns; }
			set { _columns = value; }
		}

		public ExportNativeWithFilenameFrom ExportNativesToFileNamedFrom {
			get { return _exportNativesToFileNamedFrom; }
			set { _exportNativesToFileNamedFrom = value; }
		}

		public string ErrorLogFileName {
			get {
				if ((_volumeManager != null)) {
					return _volumeManager.ErrorLogFileName;
				} else {
					return null;
				}
			}
		}

		protected virtual Int32 NumberOfRetries {
			get { return kCura.Utility.Config.ExportErrorNumberOfRetries; }
		}

		protected virtual Int32 WaitTimeBetweenRetryAttempts {
			get { return kCura.Utility.Config.ExportErrorWaitTimeInSeconds; }
		}

		#endregion

		public event ShutdownEventEventHandler ShutdownEvent;
		public delegate void ShutdownEventEventHandler();
		public void Shutdown()
		{
			if (ShutdownEvent != null) {
				ShutdownEvent();
			}
		}

		#region "Constructors"

		public Exporter(ExportFile exportFile, kCura.Windows.Process.Controller processController)
		{
			_userManager = new UserManager(exportFile.Credential, exportFile.CookieContainer);
			_userManager.Login(exportFile.Credential.UserName, exportFile.Credential.Password);

			_caseManager = new CaseManager(exportFile.Credential, exportFile.CookieContainer);
			_searchManager = new SearchManager(exportFile.Credential, exportFile.CookieContainer);

			PopulateSettings(exportFile);
			
			_downloadHandler = new FileDownloader(exportFile.Credential, exportFile.CaseInfo.DocumentPath + "\\EDDS" + exportFile.CaseInfo.ArtifactID, exportFile.CaseInfo.DownloadHandlerURL, exportFile.CookieContainer, kCura.Relativity.Export.Settings.AuthenticationToken);
			FileDownloader.TotalWebTime = 0;
			_productionManager = new ProductionManager(exportFile.Credential, exportFile.CookieContainer);
			_auditManager = new AuditManager(exportFile.Credential, exportFile.CookieContainer);
			_fieldManager = new FieldManager(exportFile.Credential, exportFile.CookieContainer);
			

			this.ExportManager = new ExportManager(exportFile.Credential, exportFile.CookieContainer);

			

			_halt = false;
			_processController = processController;
			this.DocumentsExported = 0;
			this.TotalExportArtifactCount = 1;
			this.Settings = exportFile;
			this.Settings.FolderPath = this.Settings.FolderPath + "\\";
			this.ExportNativesToFileNamedFrom = exportFile.ExportNativesToFileNamedFrom;
		}

		#endregion

		public bool ExportSearch()
		{
			try {
				_start = System.DateTime.Now;
				this.Search();
			} catch (System.Exception ex) {
				this.WriteFatalError(string.Format("A fatal error occurred on document #{0}", this.DocumentsExported), ex);
				if ((_volumeManager != null)) {
					_volumeManager.Close();
				}
			}
			return string.IsNullOrEmpty(this.ErrorLogFileName);
		}

		private bool IsExtractedTextSelected()
		{
			foreach (Types.ViewFieldInfo vfi in this.Settings.SelectedViewFields) {
				if (vfi.Category == FieldCategory.FullText)
					return true;
			}
			return false;
		}

		private Types.ViewFieldInfo ExtractedTextField()
		{
			foreach (Types.ViewFieldInfo v in this.Settings.AllExportableFields) {
				if (v.Category == FieldCategory.FullText)
					return v;
			}
			throw new System.Exception("Full text field somehow not in all fields");
		}

		private void PopulateSettings(ExportFile exportFile)
		{
			if (string.IsNullOrEmpty(exportFile.CaseInfo.DocumentPath))
			{
				var caseInfo = _caseManager.Read(exportFile.CaseInfo.ArtifactID);
				exportFile.CaseInfo = caseInfo;
			}
			
			exportFile.AllExportableFields = _searchManager.RetrieveAllExportableViewFields(exportFile.CaseInfo.ArtifactID,
				exportFile.ArtifactTypeID);

			var selectedViewFields = exportFile.AllExportableFields.Where(item => exportFile.SelectedViewFields.Any(selectedItem => selectedItem.FieldArtifactId == item.FieldArtifactId));
			exportFile.SelectedViewFields = selectedViewFields.ToArray();
		}

		private bool? Search()
		{
			Int32 tries = 0;
			Int32 maxTries = NumberOfRetries + 1;

			string typeOfExportDisplayString = "";
			string errorOutputFilePath = _exportFile.FolderPath + "\\" + _exportFile.LoadFilesPrefix + "_img_errors.txt";
			if (System.IO.File.Exists(errorOutputFilePath) && _exportFile.Overwrite)
				kCura.Utility.File.Instance.Delete(errorOutputFilePath);
			this.WriteUpdate("Retrieving export data from the server...");
			Int64 startTicks = System.DateTime.Now.Ticks;
			kCura.EDDS.WebAPI.ExportManagerBase.InitializationResults exportInitializationArgs = null;
			string columnHeaderString = this.LoadColumns();
			System.Collections.Generic.List<Int32> allAvfIds = new System.Collections.Generic.List<Int32>();
			for (Int32 i = 0; i <= _columns.Count - 1; i++) {
				if (!(_columns[i] is CoalescedTextViewField)) {
					allAvfIds.Add(this.Settings.SelectedViewFields[i].AvfId);
				}
			}
			kCura.EDDS.WebAPI.ProductionManagerBase.ProductionInfo production = null;


			if (this.Settings.TypeOfExport == ExportFile.ExportType.Production) {
				tries = 0;
				while (tries < maxTries) {
					tries += 1;
					try {
						production = _productionManager.Read(this.Settings.CaseArtifactID, this.Settings.ArtifactID);
						break; // TODO: might not be correct. Was : Exit While
					} catch (System.Exception ex) {
						if (tries < maxTries && !(ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("Need To Re Login") != -1)) {
							this.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Error occurred, attempting retry number " + tries + ", in " + WaitTimeBetweenRetryAttempts + " seconds...", true);
							System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000);
						} else {
							throw;
						}
					}
				}

				_productionExportProduction = production;
				var _with1 = _fieldManager.Read(this.Settings.CaseArtifactID, production.BeginBatesReflectedFieldId);
				_beginBatesColumn = SqlNameHelper.GetSqlFriendlyName(_with1.DisplayName);
				if (!allAvfIds.Contains(_with1.ArtifactViewFieldID))
					allAvfIds.Add(_with1.ArtifactViewFieldID);
			}

			if (this.Settings.ExportImages && this.Settings.LogFileFormat == LoadFileType.FileFormat.IPRO_FullText) {
				if (!this.IsExtractedTextSelected()) {
					allAvfIds.Add(this.ExtractedTextField().AvfId);
				}
			}
			tries = 0;
			switch (this.Settings.TypeOfExport) {
				case ExportFile.ExportType.ArtifactSearch:
					typeOfExportDisplayString = "search";
					exportInitializationArgs = CallServerWithRetry(() => this.ExportManager.InitializeSearchExport(_exportFile.CaseInfo.ArtifactID, this.Settings.ArtifactID, allAvfIds.ToArray(), this.Settings.StartAtDocumentNumber + 1), maxTries);

					break;
				case ExportFile.ExportType.ParentSearch:
					typeOfExportDisplayString = "folder";
					exportInitializationArgs = CallServerWithRetry(() => this.ExportManager.InitializeFolderExport(this.Settings.CaseArtifactID, this.Settings.ViewID, this.Settings.ArtifactID, false, allAvfIds.ToArray(), this.Settings.StartAtDocumentNumber + 1, this.Settings.ArtifactTypeID), maxTries);

					break;
				case ExportFile.ExportType.AncestorSearch:
					typeOfExportDisplayString = "folder and subfolder";
					exportInitializationArgs = CallServerWithRetry(() => this.ExportManager.InitializeFolderExport(this.Settings.CaseArtifactID, this.Settings.ViewID, this.Settings.ArtifactID, true, allAvfIds.ToArray(), this.Settings.StartAtDocumentNumber + 1, this.Settings.ArtifactTypeID), maxTries);

					break;
				case ExportFile.ExportType.Production:
					typeOfExportDisplayString = "production";
					exportInitializationArgs = CallServerWithRetry(() => this.ExportManager.InitializeProductionExport(_exportFile.CaseInfo.ArtifactID, this.Settings.ArtifactID, allAvfIds.ToArray(), this.Settings.StartAtDocumentNumber + 1), maxTries);

					break;
			}
			this.TotalExportArtifactCount = (Int32)exportInitializationArgs.RowCount;
			if (this.TotalExportArtifactCount - 1 < this.Settings.StartAtDocumentNumber) {
				string msg = string.Format("The chosen start item number ({0}) exceeds the number of {2} items in the export ({1}).  Export halted.", this.Settings.StartAtDocumentNumber + 1, this.TotalExportArtifactCount, Microsoft.VisualBasic.Constants.vbNewLine);
				//TODO - log in backend
				//MsgBox(msg, MsgBoxStyle.Critical, "Error")
				this.Shutdown();
				return false;
			} else {
				this.TotalExportArtifactCount -= this.Settings.StartAtDocumentNumber;
			}
			_statistics.MetadataTime += System.Math.Max(System.DateTime.Now.Ticks - startTicks, 1);
			if (FileTransferModeChangeEvent != null) {
				FileTransferModeChangeEvent(_downloadHandler.UploaderType.ToString());
			}
			_volumeManager = new VolumeManager(this.Settings, this.Settings.FolderPath, this.Settings.Overwrite, this.TotalExportArtifactCount, this, _downloadHandler, _timekeeper, exportInitializationArgs.ColumnNames, _statistics);
			this.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Created search log file.", true);
			_volumeManager.ColumnHeaderString = columnHeaderString;
			this.WriteUpdate("Data retrieved. Beginning " + typeOfExportDisplayString + " export...");

			object[] records = null;
			Int32 start = default(Int32);
			Int32 realStart = default(Int32);
			Int32 lastRecordCount = -1;
			while (lastRecordCount != 0) {
				realStart = start + this.Settings.StartAtDocumentNumber;
				_timekeeper.MarkStart("Exporter_GetDocumentBlock");
				startTicks = System.DateTime.Now.Ticks;
				Int32[] textPrecedenceAvfIds = null;
				if ((this.Settings.SelectedTextFields != null) && this.Settings.SelectedTextFields.Any())
					textPrecedenceAvfIds = this.Settings.SelectedTextFields.Select(f => f.AvfId).ToArray();

				if (this.Settings.TypeOfExport == ExportFile.ExportType.Production) {
					records = CallServerWithRetry(() => this.ExportManager.RetrieveResultsBlockForProduction(this.Settings.CaseInfo.ArtifactID, exportInitializationArgs.RunId, this.Settings.ArtifactTypeID, allAvfIds.ToArray(), Config.ExportBatchSize, this.Settings.MulticodesAsNested, this.Settings.MultiRecordDelimiter, this.Settings.NestedValueDelimiter, textPrecedenceAvfIds, this.Settings.ArtifactID), maxTries);
				} else {
					records = CallServerWithRetry(() => this.ExportManager.RetrieveResultsBlock(this.Settings.CaseInfo.ArtifactID, exportInitializationArgs.RunId, this.Settings.ArtifactTypeID, allAvfIds.ToArray(), Config.ExportBatchSize, this.Settings.MulticodesAsNested, this.Settings.MultiRecordDelimiter, this.Settings.NestedValueDelimiter, textPrecedenceAvfIds), maxTries);
				}


				if (records == null)
					break; // TODO: might not be correct. Was : Exit While
				if (this.Settings.TypeOfExport == ExportFile.ExportType.Production && production != null && production.DocumentsHaveRedactions) {
					WriteStatusLineWithoutDocCount(kCura.Windows.Process.EventType.Warning, "Please Note - Documents in this production were produced with redactions applied.  Ensure that you have exported text that was generated via OCR of the redacted documents.", true);
				}
				lastRecordCount = records.Length;
				_statistics.MetadataTime += System.Math.Max(System.DateTime.Now.Ticks - startTicks, 1);
				_timekeeper.MarkEnd("Exporter_GetDocumentBlock");
				ArrayList artifactIDs = new ArrayList();
				Int32 artifactIdOrdinal = _volumeManager.OrdinalLookup["ArtifactID"];
				if (records.Length > 0) {
					foreach (object[] artifactMetadata in records) {
						artifactIDs.Add(artifactMetadata[artifactIdOrdinal]);
					}
					ExportChunk((Int32[])artifactIDs.ToArray(typeof(Int32)), records);
					artifactIDs.Clear();
					records = null;
				}
				if (_halt)
					break; // TODO: might not be correct. Was : Exit While
			}

			this.WriteStatusLine(kCura.Windows.Process.EventType.Status, FileDownloader.TotalWebTime.ToString(), true);
			_timekeeper.GenerateCsvReportItemsAsRows();
			_volumeManager.Finish();
			this.AuditRun(true);
			return null;
		}


		private T CallServerWithRetry<T>(Func<T> f, Int32 maxTries)
		{
			int tries = 0;
			T records = default(T);

			tries = 0;
			while (tries < maxTries) {
				tries += 1;
				try {
					records = f();
					break; // TODO: might not be correct. Was : Exit While
				} catch (System.Exception ex) {
					if ((ex) is System.InvalidOperationException && ex.Message.Contains("empty response")) {
						throw new Exception("Communication with the WebAPI server has failed, possibly because values for MaximumLongTextSizeForExportInCell and/or MaximumTextVolumeForExportChunk are too large.  Please lower them and try again.", ex);
					} else if (tries < maxTries && !(ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("Need To Re Login") != -1)) {
						this.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Error occurred, attempting retry number " + tries + ", in " + WaitTimeBetweenRetryAttempts + " seconds...", true);
						System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000);
					} else {
						throw;
					}
				}
			}
			return records;
		}

		#region "Private Helper Functions"

		private void ExportChunk(Int32[] documentArtifactIDs, object[] records)
		{
			Int32 tries = 0;
			Int32 maxTries = NumberOfRetries + 1;

			System.Data.DataView natives = new System.Data.DataView();
			System.Data.DataView images = new System.Data.DataView();
			System.Data.DataView productionImages = new System.Data.DataView();
			Int32 i = 0;
			Int32 productionArtifactID = 0;
			Int64 start = default(Int64);
			if (this.Settings.TypeOfExport == ExportFile.ExportType.Production)
				productionArtifactID = Settings.ArtifactID;
			if (this.Settings.ExportNative) {
				start = System.DateTime.Now.Ticks;
				if (this.Settings.TypeOfExport == ExportFile.ExportType.Production) {
					tries = 0;
					while (tries < maxTries) {
						tries += 1;
						try {
							natives.Table = _searchManager.RetrieveNativesForProduction(this.Settings.CaseArtifactID, productionArtifactID, kCura.Utility.Array.ToCsv(documentArtifactIDs)).Tables[0];
							break; // TODO: might not be correct. Was : Exit While
						} catch (System.Exception ex) {
							if (tries < maxTries && !(ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("Need To Re Login") != -1)) {
								this.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Error occurred, attempting retry number " + tries + ", in " + WaitTimeBetweenRetryAttempts + " seconds...", true);
								System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000);
							} else {
								throw;
							}
						}
					}
				} else if (this.Settings.ArtifactTypeID == (int)ArtifactType.Document) {
					tries = 0;
					while (tries < maxTries) {
						tries += 1;
						try {
							natives.Table = _searchManager.RetrieveNativesForSearch(this.Settings.CaseArtifactID, kCura.Utility.Array.ToCsv(documentArtifactIDs)).Tables[0];
							break; // TODO: might not be correct. Was : Exit While
						} catch (System.Exception ex) {
							if (tries < maxTries && !(ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("Need To Re Login") != -1)) {
								this.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Error occurred, attempting retry number " + tries + ", in " + WaitTimeBetweenRetryAttempts + " seconds...", true);
								System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000);
							} else {
								throw;
							}
						}
					}
				} else {
					System.Data.DataTable dt = null;
					tries = 0;
					while (tries < maxTries) {
						tries += 1;
						try {
							dt = _searchManager.RetrieveFilesForDynamicObjects(this.Settings.CaseArtifactID, this.Settings.FileField.FieldID, documentArtifactIDs).Tables[0];
							break; // TODO: might not be correct. Was : Exit While
						} catch (System.Exception ex) {
							if (tries < maxTries && !(ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("Need To Re Login") != -1)) {
								this.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Error occurred, attempting retry number " + tries + ", in " + WaitTimeBetweenRetryAttempts + " seconds...", true);
								System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000);
							} else {
								throw;
							}
						}
					}
					if (dt == null) {
						natives = null;
					} else {
						natives.Table = dt;
					}
				}
				_statistics.MetadataTime += System.Math.Max(System.DateTime.Now.Ticks - start, 1);
			}
			if (this.Settings.ExportImages) {
				_timekeeper.MarkStart("Exporter_GetImagesForDocumentBlock");
				start = System.DateTime.Now.Ticks;

				tries = 0;
				while (tries < maxTries) {
					tries += 1;
					try {
						images.Table = this.RetrieveImagesForDocuments(documentArtifactIDs, this.Settings.ImagePrecedence);
						break; // TODO: might not be correct. Was : Exit While
					} catch (System.Exception ex) {
						if (tries < maxTries && !(ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("Need To Re Login") != -1)) {
							this.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Error occurred, attempting retry number " + tries + ", in " + WaitTimeBetweenRetryAttempts + " seconds...", true);
							System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000);
						} else {
							throw;
						}
					}
				}

				tries = 0;
				while (tries < maxTries) {
					tries += 1;
					try {
						productionImages.Table = this.RetrieveProductionImagesForDocuments(documentArtifactIDs, this.Settings.ImagePrecedence);
						break; // TODO: might not be correct. Was : Exit While
					} catch (System.Exception ex) {
						if (tries < maxTries && !(ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("Need To Re Login") != -1)) {
							this.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Error occurred, attempting retry number " + tries + ", in " + WaitTimeBetweenRetryAttempts + " seconds...", true);
							System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000);
						} else {
							throw;
						}
					}
				}

				_statistics.MetadataTime += System.Math.Max(System.DateTime.Now.Ticks - start, 1);
				_timekeeper.MarkEnd("Exporter_GetImagesForDocumentBlock");
			}
			Int32 beginBatesColumnIndex = -1;
			if (this.ExportNativesToFileNamedFrom == ExportNativeWithFilenameFrom.Production && _volumeManager.OrdinalLookup.ContainsKey(_beginBatesColumn)) {
				beginBatesColumnIndex = _volumeManager.OrdinalLookup[_beginBatesColumn];
			}
			string identifierColumnName = SqlNameHelper.GetSqlFriendlyName(this.Settings.IdentifierColumnName);
			Int32 identifierColumnIndex = _volumeManager.OrdinalLookup[identifierColumnName];
			for (i = 0; i <= documentArtifactIDs.Length - 1; i++) {
				ObjectExportInfo artifact = new ObjectExportInfo();
				object[] record = (object[])records[i];
				System.Data.DataRowView nativeRow = GetNativeRow(natives, documentArtifactIDs[i]);
				if (this.ExportNativesToFileNamedFrom == ExportNativeWithFilenameFrom.Production && beginBatesColumnIndex != -1) {
					artifact.ProductionBeginBates = record[beginBatesColumnIndex].ToString();
				}
				artifact.IdentifierValue = record[identifierColumnIndex].ToString();
				artifact.Images = this.PrepareImages(images, productionImages, documentArtifactIDs[i], artifact.IdentifierValue, artifact, this.Settings.ImagePrecedence);
				if (nativeRow == null) {
					artifact.NativeFileGuid = "";
					artifact.OriginalFileName = "";
					artifact.NativeSourceLocation = "";
				} else {
					artifact.OriginalFileName = nativeRow["Filename"].ToString();
					artifact.NativeSourceLocation = nativeRow["Location"].ToString();
					if (this.Settings.ArtifactTypeID == (int)ArtifactType.Document) {
						artifact.NativeFileGuid = nativeRow["Guid"].ToString();
					} else {
						artifact.FileID = (Int32)nativeRow["FileID"];
					}
				}
				if (nativeRow == null) {
					artifact.NativeExtension = "";
				} else if (nativeRow["Filename"].ToString().IndexOf(".") != -1) {
					artifact.NativeExtension = nativeRow["Filename"].ToString().Substring(nativeRow["Filename"].ToString().LastIndexOf(".") + 1);
				} else {
					artifact.NativeExtension = "";
				}
				artifact.ArtifactID = documentArtifactIDs[i];
				artifact.Metadata = (object[])records[i];

				tries = 0;
				while (tries < maxTries) {
					tries += 1;
					try {
						_fileCount += _volumeManager.ExportArtifact(artifact).Value;
						break; // TODO: might not be correct. Was : Exit While
					} catch (System.Exception ex) {
						if (tries < maxTries && !(ex is System.Web.Services.Protocols.SoapException && ex.ToString().IndexOf("Need To Re Login") != -1)) {
							this.WriteStatusLine(kCura.Windows.Process.EventType.Status, "Error occurred, attempting retry number " + tries + ", in " + WaitTimeBetweenRetryAttempts + " seconds...", true);
							System.Threading.Thread.CurrentThread.Join(WaitTimeBetweenRetryAttempts * 1000);
						} else {
							throw;
						}
					}
				}

				_lastStatisticsSnapshot = _statistics.ToDictionary();
				this.WriteUpdate("Exported document " + i + 1, i == documentArtifactIDs.Length - 1);
				if (_halt)
					return;
			}
		}

		private System.Collections.ArrayList PrepareImagesForProduction(System.Data.DataView imagesView, Int32 documentArtifactID, string batesBase, ObjectExportInfo artifact)
		{
			System.Collections.ArrayList retval = new System.Collections.ArrayList();
			if (!this.Settings.ExportImages)
				return retval;
			DataRow[] matchingRows = imagesView.Table.Select("DocumentArtifactID = " + documentArtifactID.ToString());
			Int32 i = 0;
			//DAS034 There is at least one case where all production images for a document will end up with the same filename.
			//This happens when the production uses Existing production numbering, and the base production used Document numbering.
			//This case cannot be detected using current available information about the Production that we get from WebAPI.
			//To be on the safe side, keep track of the first image filename, and if another image has the same filename, add i + 1 onto it.
			string firstImageFileName = null;
			if (matchingRows.Count() > 0) {
				System.Data.DataRow dr = null;
				foreach (DataRow dr_loopVariable in matchingRows) {
					dr = dr_loopVariable;
					ImageExportInfo image = new ImageExportInfo();
					image.FileName = dr["ImageFileName"].ToString();
					image.FileGuid = dr["ImageGuid"].ToString();
					image.ArtifactID = documentArtifactID;
					image.PageOffset = kCura.Utility.NullableTypesHelper.DBNullConvertToNullable<Int32>(dr["ByteRange"]);
					image.BatesNumber = dr["BatesNumber"].ToString();
					image.SourceLocation = dr["Location"].ToString();
					string filenameExtension = "";
					if (image.FileName.IndexOf(".") != -1) {
						filenameExtension = "." + image.FileName.Substring(image.FileName.LastIndexOf(".") + 1);
					}
					string filename = image.BatesNumber;
					if (i == 0) {
						firstImageFileName = filename;
					}
					if ((i > 0) && (IsDocNumberOnlyProduction(_productionExportProduction) || filename.Equals(firstImageFileName, StringComparison.OrdinalIgnoreCase))) {
						filename += "_" + (i + 1).ToString();
					}
					image.FileName = filename + filenameExtension;
					if (!string.IsNullOrEmpty(image.FileGuid)) {
						retval.Add(image);
					}
					i += 1;
				}
			}
			return retval;
		}

		private kCura.EDDS.WebAPI.ProductionManagerBase.ProductionInfo GetProduction(string productionArtifactId)
		{
			Int32 id = Convert.ToInt32(productionArtifactId);
			if (!_productionLookup.ContainsKey(id)) {
				_productionLookup.Add(id, _productionManager.Read(this.Settings.CaseArtifactID, id));
			}
			return _productionLookup[id];
		}

		private bool IsDocNumberOnlyProduction(kCura.EDDS.WebAPI.ProductionManagerBase.ProductionInfo production)
		{
			return (production != null) && production.BatesNumbering == false && production.UseDocumentLevelNumbering && !production.IncludeImageLevelNumberingForDocumentLevelNumbering;
		}

		private System.Collections.ArrayList PrepareImages(System.Data.DataView imagesView, System.Data.DataView productionImagesView, Int32 documentArtifactID, string batesBase, ObjectExportInfo artifact, IEnumerable<Pair> productionOrderList)
		{
			System.Collections.ArrayList retval = new System.Collections.ArrayList();
			if (!this.Settings.ExportImages)
				return retval;
			if (this.Settings.TypeOfExport == ExportFile.ExportType.Production) {
				productionImagesView.Sort = "DocumentArtifactID ASC";
				return this.PrepareImagesForProduction(productionImagesView, documentArtifactID, batesBase, artifact);
			}
			Pair item = null;
			foreach (Pair item_loopVariable in productionOrderList) {
				item = item_loopVariable;
				if (item.Value == "-1") {
					return this.PrepareOriginalImages(imagesView, documentArtifactID, batesBase, artifact);
				} else {
					productionImagesView.RowFilter = string.Format("DocumentArtifactID = {0} AND ProductionArtifactID = {1}", documentArtifactID, item.Value);
					if (productionImagesView.Count > 0) {
						System.Data.DataRowView drv = null;
						Int32 i = 0;
						foreach (DataRowView drv_loopVariable in productionImagesView) {
							drv = drv_loopVariable;
							ImageExportInfo image = new ImageExportInfo();
							image.FileName = drv["ImageFileName"].ToString();
							image.FileGuid = drv["ImageGuid"].ToString();
							if (!string.IsNullOrEmpty(image.FileGuid)) {
								image.ArtifactID = documentArtifactID;
								image.BatesNumber = drv["BatesNumber"].ToString();
								image.PageOffset = kCura.Utility.NullableTypesHelper.DBNullConvertToNullable<Int32>(drv["ByteRange"]);
								string filenameExtension = "";
								if (image.FileName.IndexOf(".") != -1) {
									filenameExtension = "." + image.FileName.Substring(image.FileName.LastIndexOf(".") + 1);
								}
								string filename = image.BatesNumber;
								if (IsDocNumberOnlyProduction(this.GetProduction(item.Value)) && i > 0)
									filename += "_" + (i + 1).ToString();
								image.FileName = filename + filenameExtension;
								image.SourceLocation = drv["Location"].ToString();
								retval.Add(image);
								i += 1;
							}
						}
						return retval;
					}
				}
			}
			return retval;
		}

		private System.Collections.ArrayList PrepareOriginalImages(System.Data.DataView imagesView, Int32 documentArtifactID, string batesBase, ObjectExportInfo artifact)
		{
			System.Collections.ArrayList retval = new System.Collections.ArrayList();
			if (!this.Settings.ExportImages)
				return retval;
			imagesView.RowFilter = "DocumentArtifactID = " + documentArtifactID.ToString();
			Int32 i = 0;
			if (imagesView.Count > 0) {
				System.Data.DataRowView drv = null;
				foreach (DataRowView drv_loopVariable in imagesView) {
					drv = drv_loopVariable;
					ImageExportInfo image = new ImageExportInfo();
					image.FileName = drv["Filename"].ToString();
					image.FileGuid = drv["Guid"].ToString();
					image.ArtifactID = documentArtifactID;
					image.PageOffset = kCura.Utility.NullableTypesHelper.DBNullConvertToNullable<Int32>(drv["ByteRange"]);
					if (i == 0) {
						image.BatesNumber = artifact.IdentifierValue;
					} else {
						image.BatesNumber = drv["Identifier"].ToString();
						if (image.BatesNumber.IndexOf(image.FileGuid) != -1) {
							image.BatesNumber = artifact.IdentifierValue + "_" + i.ToString().PadLeft(imagesView.Count.ToString().Length + 1, '0');
						}
					}
					//image.BatesNumber = drv("Identifier").ToString
					string filenameExtension = "";
					if (image.FileName.IndexOf(".") != -1) {
						filenameExtension = "." + image.FileName.Substring(image.FileName.LastIndexOf(".") + 1);
					}
					image.FileName = kCura.Utility.File.Instance.ConvertIllegalCharactersInFilename(image.BatesNumber.ToString() + filenameExtension);
					image.SourceLocation = drv["Location"].ToString();
					retval.Add(image);
					i += 1;
				}
			}
			return retval;
		}

		private System.Data.DataRowView GetNativeRow(System.Data.DataView dv, Int32 artifactID)
		{
			if (!this.Settings.ExportNative)
				return null;
			if (this.Settings.ArtifactTypeID == 10) {
				dv.RowFilter = "DocumentArtifactID = " + artifactID.ToString();
			} else {
				dv.RowFilter = "ObjectArtifactID = " + artifactID.ToString();
			}
			if (dv.Count > 0) {
				return dv[0];
			} else {
				return null;
			}
		}

		/// <summary>
		/// Sets the member variable _columns to contain an array of each Field which will be exported.
		/// _columns is an array of ViewFieldInfo, but for the "Text Precedence" column, the array item is
		/// a CoalescedTextViewField (a subclass of ViewFieldInfo).
		/// </summary>
		/// <returns>A string containing the contents of the export file header.  For example, if _exportFile.LoadFile is false,
		/// and the fields selected to export are (Control Number and ArtifactID), along with the Text Precendence which includes
		/// Extracted Text, then the following string would be returned: ""Control Number","Artifact ID","Text Precedence" "
		/// </returns>
		/// <remarks></remarks>
		private string LoadColumns()
		{
			System.Text.StringBuilder retString = new System.Text.StringBuilder();
			if (_exportFile.LoadFileIsHtml) {
				retString.Append("<html><head><title>" + System.Web.HttpUtility.HtmlEncode(_exportFile.CaseInfo.Name) + "</title>");
				retString.Append("<style type='text/css'>" + Microsoft.VisualBasic.Constants.vbNewLine);
				retString.Append("td {vertical-align: top;background-color:#EEEEEE;}" + Microsoft.VisualBasic.Constants.vbNewLine);
				retString.Append("th {color:#DDDDDD;text-align:left;}" + Microsoft.VisualBasic.Constants.vbNewLine);
				retString.Append("table {background-color:#000000;}" + Microsoft.VisualBasic.Constants.vbNewLine);
				retString.Append("</style>" + Microsoft.VisualBasic.Constants.vbNewLine);
				retString.Append("</head><body>" + Microsoft.VisualBasic.Constants.vbNewLine);
				retString.Append("<table width='100%'><tr>" + Microsoft.VisualBasic.Constants.vbNewLine);
			}
			foreach (Types.ViewFieldInfo field in this.Settings.SelectedViewFields) {
				this.Settings.ExportFullText = this.Settings.ExportFullText || field.Category == FieldCategory.FullText;
			}
			_columns = new System.Collections.ArrayList(this.Settings.SelectedViewFields);
			if ((this.Settings.SelectedTextFields != null) && this.Settings.SelectedTextFields.Count() > 0) {
				List<Types.ViewFieldInfo> longTextSelectedViewFields = new List<Types.ViewFieldInfo>();
				longTextSelectedViewFields.AddRange(this.Settings.SelectedViewFields.Where((Types.ViewFieldInfo f) => f.FieldType == FieldTypeHelper.FieldType.Text || f.FieldType == FieldTypeHelper.FieldType.OffTableText));
				if ((this.Settings.SelectedTextFields.Count() == 1) && longTextSelectedViewFields.Exists((Types.ViewFieldInfo f) => f.Equals(this.Settings.SelectedTextFields.First()))) {
					Types.ViewFieldInfo selectedViewFieldToRemove = longTextSelectedViewFields.Find((Types.ViewFieldInfo f) => f.Equals(this.Settings.SelectedTextFields.First()));
					if (selectedViewFieldToRemove != null) {
						Int32 indexOfSelectedViewFieldToRemove = _columns.IndexOf(selectedViewFieldToRemove);
						_columns.RemoveAt(indexOfSelectedViewFieldToRemove);
						_columns.Insert(indexOfSelectedViewFieldToRemove, new CoalescedTextViewField(this.Settings.SelectedTextFields.First(), true));
					} else {
						_columns.Add(new CoalescedTextViewField(this.Settings.SelectedTextFields.First(), false));
					}
				} else {
					_columns.Add(new CoalescedTextViewField(this.Settings.SelectedTextFields.First(), false));
				}
			}
			for (Int32 i = 0; i <= _columns.Count - 1; i++) {
				Types.ViewFieldInfo field = (Types.ViewFieldInfo)_columns[i];
				if (_exportFile.LoadFileIsHtml) {
					retString.AppendFormat("{0}{1}{2}", "<th>", System.Web.HttpUtility.HtmlEncode(field.DisplayName), "</th>");
				} else {
					retString.AppendFormat("{0}{1}{0}", this.Settings.QuoteDelimiter, field.DisplayName);
					if (i < _columns.Count - 1)
						retString.Append(this.Settings.RecordDelimiter);
				}
			}

			if (!this.Settings.LoadFileIsHtml)
				retString = new System.Text.StringBuilder(retString.ToString().TrimEnd(this.Settings.RecordDelimiter));
			if (_exportFile.LoadFileIsHtml) {
				if (this.Settings.ExportImages && this.Settings.ArtifactTypeID == (int)ArtifactType.Document)
					retString.Append("<th>Image Files</th>");
				if (this.Settings.ExportNative)
					retString.Append("<th>Native Files</th>");
				retString.Append(Microsoft.VisualBasic.Constants.vbNewLine + "</tr>" + Microsoft.VisualBasic.Constants.vbNewLine);
			} else {
				if (this.Settings.ExportNative)
					retString.AppendFormat("{2}{0}{1}{0}", this.Settings.QuoteDelimiter, "FILE_PATH", this.Settings.RecordDelimiter);
			}
			retString.Append(System.Environment.NewLine);
			return retString.ToString();
		}

		private System.Data.DataTable RetrieveImagesForDocuments(Int32[] documentArtifactIDs, IEnumerable<Pair> productionOrderList)
		{
			switch (this.Settings.TypeOfExport) {
				case ExportFile.ExportType.Production:
					return null;
				default:
					return _searchManager.RetrieveImagesForDocuments(this.Settings.CaseArtifactID, documentArtifactIDs).Tables[0];
			}
		}

		private System.Data.DataTable RetrieveProductionImagesForDocuments(Int32[] documentArtifactIDs, IEnumerable<Pair> productionOrderList)
		{
			switch (this.Settings.TypeOfExport) {
				case ExportFile.ExportType.Production:
					return _searchManager.RetrieveImagesForProductionDocuments(this.Settings.CaseArtifactID, documentArtifactIDs, Int32.Parse(productionOrderList.ToList().FirstOrDefault().Value)).Tables[0];
				default:
					Int32[] productionIDs = this.GetProductionArtifactIDs(productionOrderList);
					if (productionIDs.Length > 0)
						return _searchManager.RetrieveImagesByProductionIDsAndDocumentIDsForExport(this.Settings.CaseArtifactID, productionIDs, documentArtifactIDs).Tables[0];
					break;
			}
			return null;
		}

		private Int32[] GetProductionArtifactIDs(IEnumerable<Pair> productionOrderList)
		{
			if (_productionArtifactIDs == null) {
				System.Collections.ArrayList retval = new System.Collections.ArrayList();
				Pair item = null;
				foreach (Pair item_loopVariable in productionOrderList) {
					item = item_loopVariable;
					if (item.Value != "-1") {
						retval.Add(Int32.Parse(item.Value));
					}
				}
				_productionArtifactIDs = (Int32[])retval.ToArray(typeof(Int32));
			}
			return _productionArtifactIDs;
		}

		#endregion


		#region "Messaging"

		private void AuditRun(bool success)
		{
			kCura.EDDS.WebAPI.AuditManagerBase.ExportStatistics args = new kCura.EDDS.WebAPI.AuditManagerBase.ExportStatistics();
			args.AppendOriginalFilenames = this.Settings.AppendOriginalFileName;
			args.Bound = this.Settings.QuoteDelimiter;
			args.ArtifactTypeID = this.Settings.ArtifactTypeID;
			switch (this.Settings.TypeOfExport) {
				case ExportFile.ExportType.AncestorSearch:
					args.DataSourceArtifactID = this.Settings.ViewID;
					break;
				case ExportFile.ExportType.ArtifactSearch:
					args.DataSourceArtifactID = this.Settings.ArtifactID;
					break;
				case ExportFile.ExportType.ParentSearch:
					args.DataSourceArtifactID = this.Settings.ViewID;
					break;
				case ExportFile.ExportType.Production:
					args.DataSourceArtifactID = this.Settings.ArtifactID;
					break;
			}
			args.Delimiter = this.Settings.RecordDelimiter;
			args.DestinationFilesystemFolder = this.Settings.FolderPath;
			args.DocumentExportCount = this.DocumentsExported;
			args.ErrorCount = _errorCount;
			if ((this.Settings.SelectedTextFields != null))
				args.ExportedTextFieldID = this.Settings.SelectedTextFields[0].FieldArtifactId;
			if (this.Settings.ExportFullTextAsFile) {
				args.ExportedTextFileEncodingCodePage = this.Settings.TextFileEncoding.CodePage;
				args.ExportTextFieldAsFiles = true;
			} else {
				args.ExportTextFieldAsFiles = false;
			}
			System.Collections.ArrayList fields = new System.Collections.ArrayList();
			foreach (Types.ViewFieldInfo field in this.Settings.SelectedViewFields) {
				if (!fields.Contains(field.FieldArtifactId))
					fields.Add(field.FieldArtifactId);
			}
			args.Fields = (Int32[])fields.ToArray(typeof(Int32));
			args.ExportNativeFiles = this.Settings.ExportNative;
			if (args.Fields.Length > 0 || this.Settings.ExportNative) {
				args.MetadataLoadFileEncodingCodePage = this.Settings.LoadFileEncoding.CodePage;
				switch (this.Settings.LoadFileExtension.ToLower()) {
					case "txt":
						args.MetadataLoadFileFormat = kCura.EDDS.WebAPI.AuditManagerBase.LoadFileFormat.Custom;
						break;
					case "csv":
						args.MetadataLoadFileFormat = kCura.EDDS.WebAPI.AuditManagerBase.LoadFileFormat.Csv;
						break;
					case "dat":
						args.MetadataLoadFileFormat = kCura.EDDS.WebAPI.AuditManagerBase.LoadFileFormat.Dat;
						break;
					case "html":
						args.MetadataLoadFileFormat = kCura.EDDS.WebAPI.AuditManagerBase.LoadFileFormat.Html;
						break;
				}
				args.MultiValueDelimiter = this.Settings.MultiRecordDelimiter;
				args.ExportMultipleChoiceFieldsAsNested = this.Settings.MulticodesAsNested;
				args.NestedValueDelimiter = this.Settings.NestedValueDelimiter;
				args.NewlineProxy = this.Settings.NewlineDelimiter;
			}
			try {
				args.FileExportCount = (Int32)_fileCount;
			} catch {
			}
			switch (this.Settings.TypeOfExportedFilePath) {
				case ExportFile.ExportedFilePathType.Absolute:
					args.FilePathSettings = "Use Absolute Paths";
					break;
				case ExportFile.ExportedFilePathType.Prefix:
					args.FilePathSettings = "Use Prefix: " + this.Settings.FilePrefix;
					break;
				case ExportFile.ExportedFilePathType.Relative:
					args.FilePathSettings = "Use Relative Paths";
					break;
			}
			if (this.Settings.ExportImages) {
				args.ExportImages = true;
				switch (this.Settings.TypeOfImage) {
					case ExportFile.ImageType.MultiPageTiff:
						args.ImageFileType = kCura.EDDS.WebAPI.AuditManagerBase.ImageFileExportType.MultiPageTiff;
						break;
					case ExportFile.ImageType.Pdf:
						args.ImageFileType = kCura.EDDS.WebAPI.AuditManagerBase.ImageFileExportType.PDF;
						break;
					case ExportFile.ImageType.SinglePage:
						args.ImageFileType = kCura.EDDS.WebAPI.AuditManagerBase.ImageFileExportType.SinglePage;
						break;
				}
				switch (this.Settings.LogFileFormat) {
					case LoadFileType.FileFormat.IPRO:
						args.ImageLoadFileFormat = kCura.EDDS.WebAPI.AuditManagerBase.ImageLoadFileFormatType.Ipro;
						break;
					case LoadFileType.FileFormat.IPRO_FullText:
						args.ImageLoadFileFormat = kCura.EDDS.WebAPI.AuditManagerBase.ImageLoadFileFormatType.IproFullText;
						break;
					case LoadFileType.FileFormat.Opticon:
						args.ImageLoadFileFormat = kCura.EDDS.WebAPI.AuditManagerBase.ImageLoadFileFormatType.Opticon;
						break;
				}
				bool hasOriginal = false;
				bool hasProduction = false;
				foreach (Pair pair in this.Settings.ImagePrecedence) {
					if (pair.Value != "-1") {
						hasProduction = true;
					} else {
						hasOriginal = true;
					}
				}
				if (hasProduction && hasOriginal) {
					args.ImagesToExport = kCura.EDDS.WebAPI.AuditManagerBase.ImagesToExportType.Both;
				} else if (hasProduction) {
					args.ImagesToExport = kCura.EDDS.WebAPI.AuditManagerBase.ImagesToExportType.Produced;
				} else {
					args.ImagesToExport = kCura.EDDS.WebAPI.AuditManagerBase.ImagesToExportType.Original;
				}
			} else {
				args.ExportImages = false;
			}
			args.OverwriteFiles = this.Settings.Overwrite;
			System.Collections.ArrayList preclist = new System.Collections.ArrayList();
			foreach (Pair pair in this.Settings.ImagePrecedence) {
				preclist.Add(Int32.Parse(pair.Value));
			}
			args.ProductionPrecedence = (Int32[])preclist.ToArray(typeof(Int32));
			args.RunTimeInMilliseconds = (Int32)System.Math.Min(System.DateTime.Now.Subtract(_start).TotalMilliseconds, Int32.MaxValue);
			if (this.Settings.TypeOfExport == ExportFile.ExportType.AncestorSearch || this.Settings.TypeOfExport == ExportFile.ExportType.ParentSearch) {
				args.SourceRootFolderID = this.Settings.ArtifactID;
			}
			args.SubdirectoryImagePrefix = this.Settings.VolumeInfo.GetSubdirectoryImagePrefix(false);
			args.SubdirectoryMaxFileCount = this.Settings.VolumeInfo.SubdirectoryMaxSize;
			args.SubdirectoryNativePrefix = this.Settings.VolumeInfo.GetSubdirectoryNativePrefix(false);
			args.SubdirectoryStartNumber = this.Settings.VolumeInfo.SubdirectoryStartNumber;
			args.SubdirectoryTextPrefix = this.Settings.VolumeInfo.GetSubdirectoryFullTextPrefix(false);
			//args.TextAndNativeFilesNamedAfterFieldID = Me.ExportNativesToFileNamedFrom
			if (this.ExportNativesToFileNamedFrom == ExportNativeWithFilenameFrom.Identifier) {
				foreach (Types.ViewFieldInfo field in this.Settings.AllExportableFields) {
					if (field.Category == FieldCategory.Identifier) {
						args.TextAndNativeFilesNamedAfterFieldID = field.FieldArtifactId;
						break; // TODO: might not be correct. Was : Exit For
					}
				}
			} else {
				foreach (Types.ViewFieldInfo field in this.Settings.AllExportableFields) {
					if (field.AvfColumnName.ToLower() == _beginBatesColumn.ToLower()) {
						args.TextAndNativeFilesNamedAfterFieldID = field.FieldArtifactId;
						break; // TODO: might not be correct. Was : Exit For
					}
				}
			}
			args.TotalFileBytesExported = _statistics.FileBytes;
			args.TotalMetadataBytesExported = _statistics.MetadataBytes;
			switch (this.Settings.TypeOfExport) {
				case ExportFile.ExportType.AncestorSearch:
					args.Type = "Folder and Subfolders";
					break;
				case ExportFile.ExportType.ArtifactSearch:
					args.Type = "Saved Search";
					break;
				case ExportFile.ExportType.ParentSearch:
					args.Type = "Folder";
					break;
				case ExportFile.ExportType.Production:
					args.Type = "Production Set";
					break;
			}
			args.VolumeMaxSize = this.Settings.VolumeInfo.VolumeMaxSize;
			args.VolumePrefix = this.Settings.VolumeInfo.VolumePrefix;
			args.VolumeStartNumber = this.Settings.VolumeInfo.VolumeStartNumber;
			args.StartExportAtDocumentNumber = this.Settings.StartAtDocumentNumber + 1;
			args.CopyFilesFromRepository = this.Settings.VolumeInfo.CopyFilesFromRepository;
			args.WarningCount = _warningCount;
			try {
				_auditManager.AuditExport(this.Settings.CaseInfo.ArtifactID, !success, args);
			} catch {
			}
		}

		internal void WriteFatalError(string line, System.Exception ex)
		{
			this.AuditRun(false);
			if (FatalErrorEvent != null) {
				FatalErrorEvent(line, ex);
			}
		}

		internal void WriteStatusLine(kCura.Windows.Process.EventType e, string line, bool isEssential)
		{
			long now = System.DateTime.Now.Ticks;
			if (now - _lastStatusMessageTs > 10000000 || isEssential) {
				_lastStatusMessageTs = now;
				string appendString = " ... " + this.DocumentsExported + _lastDocumentsExportedCountReported + " document(s) exported.";//Refactored to C# this.DocumentsExported - _lastDocumentsExportedCountReported
                _lastDocumentsExportedCountReported = this.DocumentsExported;
				if (StatusMessage != null) {
					StatusMessage(new ExportEventArgs(this.DocumentsExported, this.TotalExportArtifactCount, line + appendString, e, _lastStatisticsSnapshot));
				}
			}
		}

		internal void WriteStatusLineWithoutDocCount(kCura.Windows.Process.EventType e, string line, bool isEssential)
		{
			long now = System.DateTime.Now.Ticks;
			if (now - _lastStatusMessageTs > 10000000 || isEssential) {
				_lastStatusMessageTs = now;
				_lastDocumentsExportedCountReported = this.DocumentsExported;
				if (StatusMessage != null) {
					StatusMessage(new ExportEventArgs(this.DocumentsExported, this.TotalExportArtifactCount, line, e, _lastStatisticsSnapshot));
				}
			}
		}

		internal void WriteError(string line)
		{
			_errorCount += 1;
			WriteStatusLine(kCura.Windows.Process.EventType.Error, line, true);
		}

		internal void WriteImgProgressError(ObjectExportInfo artifact, Int32 imageIndex, System.Exception ex, string notes = "")
		{
			System.IO.StreamWriter sw = new System.IO.StreamWriter(_exportFile.FolderPath + "\\" + _exportFile.LoadFilesPrefix + "_img_errors.txt", true, _exportFile.LoadFileEncoding);
			sw.WriteLine(System.DateTime.Now.ToString("s"));
			sw.WriteLine(string.Format("DOCUMENT: {0}", artifact.IdentifierValue));
			if (imageIndex > -1 && artifact.Images.Count > 0) {
				sw.WriteLine(string.Format("IMAGE: {0} ({1} of {2})", artifact.Images[imageIndex], imageIndex + 1, artifact.Images.Count));
			}
			if (!string.IsNullOrEmpty(notes))
				sw.WriteLine("NOTES: " + notes);
			sw.WriteLine("ERROR: " + ex.ToString());
			sw.WriteLine("");
			sw.Flush();
			sw.Close();
			string errorLine = string.Format("Error processing images for document {0}: {1}. Check {2}_img_errors.txt for details", artifact.IdentifierValue, ex.Message.TrimEnd('.'), _exportFile.LoadFilesPrefix);
			this.WriteError(errorLine);
		}

		internal void WriteWarning(string line)
		{
			_warningCount += 1;
			WriteStatusLine(kCura.Windows.Process.EventType.Warning, line, true);
		}

		internal void WriteUpdate(string line, bool isEssential = true)
		{
			WriteStatusLine(kCura.Windows.Process.EventType.Progress, line, isEssential);
		}

		#endregion

		#region "Public Events"

		public event FatalErrorEventEventHandler FatalErrorEvent;
		public delegate void FatalErrorEventEventHandler(string message, System.Exception ex);
		public event StatusMessageEventHandler StatusMessage;
		public delegate void StatusMessageEventHandler(ExportEventArgs exportArgs);
		public event FileTransferModeChangeEventEventHandler FileTransferModeChangeEvent;
		public delegate void FileTransferModeChangeEventEventHandler(string mode);
		public event DisableCloseButtonEventHandler DisableCloseButton;
		public delegate void DisableCloseButtonEventHandler();
		public event EnableCloseButtonEventHandler EnableCloseButton;
		public delegate void EnableCloseButtonEventHandler();

		#endregion

		private void _processController_HaltProcessEvent(System.Guid processID)
		{
			_halt = true;
			if ((_volumeManager != null))
				_volumeManager.Halt = true;
		}

		public event UploadModeChangeEventEventHandler UploadModeChangeEvent;
		public delegate void UploadModeChangeEventEventHandler(string mode);

		private void _downloadHandler_UploadModeChangeEvent(string mode)
		{
			if (FileTransferModeChangeEvent != null) {
				FileTransferModeChangeEvent(_downloadHandler.UploaderType.ToString());
			}
		}
	}
}

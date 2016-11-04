using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using Relativity.API;
using ArtifactType = kCura.Relativity.Client.ArtifactType;

namespace kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI
{
	public class ImportService : IImportService, IBatchReporter
	{
		private const int _JOB_PROGRESS_TIMEOUT_MILLISECONDS = 5000;
		private readonly BatchManager _batchManager;
		private readonly IImportApiFactory _factory;
		private readonly Dictionary<string, int> _inputMappings;
		private readonly IAPILog _logger;

		private Workspace _currentWorkspace;
		private Dictionary<int, Field> _idToFieldDictionary;
		private IExtendedImportAPI _importApi;
		private int _itemsErrored;
		private int _itemsTransferred;
		private int _lastJobErrorUpdate;
		private int _lastJobProgressUpdate;
		private int _totalRowsImported;
		private int _totalRowsWithErrors;

		public ImportService(ImportSettings settings, Dictionary<string, int> fieldMappings, BatchManager batchManager, NativeFileImportService nativeFileImportService,
			IImportApiFactory factory, IHelper helper)
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
			Settings = settings;
			_batchManager = batchManager;
			_inputMappings = fieldMappings;
			NativeFileImportService = nativeFileImportService;
			_factory = factory;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<ImportService>();
			if (_batchManager != null)
			{
				_batchManager.OnBatchCreate += ImportService_OnBatchCreate;
			}
		}

		public NativeFileImportService NativeFileImportService { get; }

		private Dictionary<string, Field> FieldMappings { get; set; }

		private Workspace CurrentWorkspace
		{
			get
			{
				if (_currentWorkspace == null)
				{
					_currentWorkspace = _importApi.Workspaces().First(x => x.ArtifactID.Equals(Settings.CaseArtifactId));
				}
				return _currentWorkspace;
			}
		}

		public event StatusUpdate OnStatusUpdate;
		public event BatchCompleted OnBatchComplete;
		public event BatchSubmitted OnBatchSubmit;
		public event BatchCreated OnBatchCreate;
		public event JobError OnJobError;
		public event RowError OnDocumentError;

		public ImportSettings Settings { get; }

		public virtual void Initialize()
		{
			if (_importApi == null)
			{
				LogInitializingImportApi();
				Connect(Settings);
				SetupFieldDictionary(_importApi);
				Dictionary<string, int> fieldMapping = _inputMappings;
				FieldMappings = ValidateAllMappedFieldsAreInWorkspace(fieldMapping, _idToFieldDictionary);
				HashSet<string> columnNames = new HashSet<string>();
				FieldMappings.Values.ToList().ForEach(x =>
				{
					if (!columnNames.Contains(x.Name))
					{
						columnNames.Add(x.Name);
					}
				});

				_batchManager.ColumnNames = columnNames;
			}
		}

		public void AddRow(Dictionary<string, object> sourceFields)
		{
			LogAddingRowToBatchManager();
			Dictionary<string, object> importFields = GenerateImportFields(sourceFields, FieldMappings, NativeFileImportService);
			if (importFields.Count > 0)
			{
				_batchManager.Add(importFields);
				PushBatchIfFull(false);
			}
		}

		public bool PushBatchIfFull(bool forcePush)
		{
			bool isFull = _batchManager.IsBatchFull();
			if (isFull || forcePush)
			{
				LogPushingBatch(forcePush);
				try
				{
					IDataReader sourceData = _batchManager.GetBatchData();
					if (sourceData != null)
					{
						KickOffImport(sourceData);
					}
					else
					{
						CompleteBatch(DateTime.Now, DateTime.Now, 0, 0);
					}
				}
				finally
				{
					_batchManager.ClearDataSource();
				}
			}
			return isFull;
		}

		public void CleanUp()
		{
		}

		public virtual void KickOffImport(IDataReader dataReader)
		{
			LogSettingUpImportJob();
			ImportBulkArtifactJob importJob;
			if (Settings.ArtifactTypeId == (int) ArtifactType.Document)
			{
				importJob = _importApi.NewNativeDocumentImportJob(Settings.OnBehalfOfUserToken);
			}
			else
			{
				importJob = _importApi.NewObjectImportJob(Settings.ArtifactTypeId);
			}

			importJob.SourceData.SourceData = dataReader;
			importJob.Settings.ArtifactTypeId = Settings.ArtifactTypeId;
			importJob.Settings.AuditLevel = Settings.AuditLevel;
			importJob.Settings.CaseArtifactId = Settings.CaseArtifactId;
			importJob.Settings.DestinationFolderArtifactID =  GetDestinationFolderArtifactId(Settings.DestinationFolderArtifactId);
			importJob.Settings.BulkLoadFileFieldDelimiter = Settings.BulkLoadFileFieldDelimiter;
			importJob.Settings.CopyFilesToDocumentRepository = Settings.CopyFilesToDocumentRepository;
			importJob.Settings.DisableControlNumberCompatibilityMode = Settings.DisableControlNumberCompatibilityMode;
			importJob.Settings.DisableExtractedTextEncodingCheck = Settings.DisableExtractedTextEncodingCheck;
			importJob.Settings.DisableExtractedTextFileLocationValidation = Settings.DisableExtractedTextFileLocationValidation;
			importJob.Settings.DisableNativeLocationValidation = Settings.DisableNativeLocationValidation;
			importJob.Settings.DisableNativeValidation = Settings.DisableNativeValidation;
			importJob.Settings.DisableUserSecurityCheck = Settings.DisableUserSecurityCheck;
			importJob.Settings.ExtractedTextFieldContainsFilePath = Settings.ExtractedTextFieldContainsFilePath;
			// only set if the extracted file map links to extracted text location
			if (Settings.ExtractedTextFieldContainsFilePath)
			{
				importJob.Settings.ExtractedTextEncoding = Settings.ExtractedTextEncoding;
			}

			importJob.Settings.FileNameColumn = Settings.FileNameColumn;
			importJob.Settings.FileSizeColumn = Settings.FileSizeColumn;
			importJob.Settings.FileSizeMapped = Settings.FileSizeMapped;
			importJob.Settings.FolderPathSourceFieldName = Settings.FolderPathSourceFieldName;
			importJob.Settings.IdentityFieldId = Settings.IdentityFieldId;
			importJob.Settings.MaximumErrorCount = int.MaxValue - 1; //Have to pass in MaxValue - 1 because of how the ImportAPI validation works -AJK 10-July-2012
			importJob.Settings.MultiValueDelimiter = Settings.MultiValueDelimiter;
			importJob.Settings.NativeFileCopyMode = Settings.NativeFileCopyMode;
			importJob.Settings.NativeFilePathSourceFieldName = Settings.NativeFilePathSourceFieldName;
			importJob.Settings.NestedValueDelimiter = Settings.NestedValueDelimiter;
			importJob.Settings.OIFileIdColumnName = Settings.OIFileIdColumnName;
			importJob.Settings.OIFileIdMapped = Settings.OIFileIdMapped;
			importJob.Settings.OIFileTypeColumnName = Settings.OIFileTypeColumnName;
			importJob.Settings.ObjectFieldIdListContainsArtifactId = Settings.ObjectFieldIdListContainsArtifactId;
			importJob.Settings.OverwriteMode = Settings.OverwriteMode;
			importJob.Settings.OverlayBehavior = Settings.OverlayBehavior;
			importJob.Settings.ParentObjectIdSourceFieldName = Settings.ParentObjectIdSourceFieldName;
			importJob.Settings.SendEmailOnLoadCompletion = Settings.SendEmailOnLoadCompletion;
			importJob.Settings.StartRecordNumber = Settings.StartRecordNumber;
			importJob.Settings.SelectedIdentifierFieldName = _idToFieldDictionary[Settings.IdentityFieldId].Name;
			importJob.OnComplete += ImportJob_OnComplete;
			importJob.OnFatalException += ImportJob_OnComplete;

			// DO NOT MOVE THIS INTO A METHOD
			// ILmerge on our build server will fail - SAMO 6/1/2016
			importJob.OnError += row =>
			{
				_itemsTransferred--;
				_itemsErrored++;
				if (Environment.TickCount - _lastJobErrorUpdate > _JOB_PROGRESS_TIMEOUT_MILLISECONDS)
				{
					_lastJobErrorUpdate = Environment.TickCount;
					if (OnStatusUpdate != null)
					{
						OnStatusUpdate(_itemsTransferred, _itemsErrored);
						_itemsErrored = 0;
						_itemsTransferred = 0;
					}
				}
			};

			importJob.OnMessage += ImportJob_OnMessage;
			importJob.OnProgress += ImportJob_OnProgress;
			ImportService_OnBatchSubmit(_batchManager.CurrentSize, _batchManager.MinimumBatchSize);

			importJob.Execute();
			LogImportJobStarted();
		}

		private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			return EmbeddedAssembly.Get(args.Name);
		}

		internal void Connect(ImportSettings settings)
		{
			_importApi = _factory.GetImportAPI(settings);
		}

		internal void SetupFieldDictionary(IImportAPI api)
		{
			try
			{
				_idToFieldDictionary = new Dictionary<int, Field>();

				var workspaceFields = api.GetWorkspaceFields(Settings.CaseArtifactId, Settings.ArtifactTypeId);
				foreach (var field in workspaceFields)
				{
					_idToFieldDictionary.Add(field.ArtifactID, field);
				}
			}
			catch (Exception e)
			{
				LogSettingFieldDictionaryError(e);
				throw;
			}
		}

		public Dictionary<string, Field> ValidateAllMappedFieldsAreInWorkspace(Dictionary<string, int> fieldMapping, Dictionary<int, Field> rdoAllFields)
		{
			Dictionary<string, Field> mapping = new Dictionary<string, Field>();

			List<int> missingFields = new List<int>();
			foreach (string mapSourceFieldName in fieldMapping.Keys)
			{
				int mapRdoFieldId = fieldMapping[mapSourceFieldName];
				if (!rdoAllFields.ContainsKey(mapRdoFieldId))
				{
					missingFields.Add(mapRdoFieldId);
				}
				else
				{
					if (!mapping.ContainsKey(mapSourceFieldName))
					{
						Field destinationField = rdoAllFields[mapRdoFieldId];
						mapping.Add(mapSourceFieldName, destinationField);
					}
				}
			}
			if (missingFields.Count > 0)
			{
				string missingFieldFormatted = string.Join(", ", missingFields);
				var message = string.Format("Missing mapped field IDs: {0}", missingFieldFormatted);
				LogFieldInWorkspaceValidationError(message);
				throw new Exception(message);
			}
			return mapping;
		}

		public Dictionary<string, object> GenerateImportFields(Dictionary<string, object> sourceFields, Dictionary<string, Field> mapping,
			NativeFileImportService nativeFileImportService)
		{
			Dictionary<string, object> importFields = new Dictionary<string, object>();

			foreach (string sourceFieldId in sourceFields.Keys)
			{
				if (mapping.ContainsKey(sourceFieldId))
				{
					Field rdoField = mapping[sourceFieldId];

					if (!importFields.ContainsKey(rdoField.Name))
					{
						importFields.Add(rdoField.Name, sourceFields[sourceFieldId]);
					}
				}
			}
			if ((nativeFileImportService != null) && nativeFileImportService.ImportNativeFiles)
			{
				importFields.Add(nativeFileImportService.DestinationFieldName, sourceFields[nativeFileImportService.SourceFieldName]);
			}
			return importFields;
		}

		private void ImportService_OnBatchCreate(int batchSize)
		{
			LogOnBatchCreateEvent(batchSize);
			OnBatchCreate?.Invoke(batchSize);
		}

		private void ImportService_OnBatchSubmit(int currentSize, int minSize)
		{
			OnBatchSubmit?.Invoke(currentSize, minSize);
		}

		private void ImportJob_OnComplete(JobReport jobReport)
		{
			LogOnCompleteEvent();
			SaveDocumentsError(jobReport.ErrorRows);

			if (jobReport.FatalException != null)
			{
				ImportJob_OnError(jobReport.FatalException);
				CompleteBatch(jobReport.StartTime, jobReport.EndTime, 0, 0);
			}
			else
			{
				CompleteBatch(jobReport.StartTime, jobReport.EndTime, jobReport.TotalRows, jobReport.ErrorRowCount);
			}
		}

		private void ImportJob_OnError(Exception fatalException)
		{
			LogOnErrorEvent(fatalException);
			OnJobError?.Invoke(fatalException);
		}

		private void SaveDocumentsError(IList<JobReport.RowError> errors)
		{
			if (OnDocumentError != null)
			{
				foreach (JobReport.RowError error in errors)
				{
					OnDocumentError(error.Identifier, error.Message);
				}
			}
		}

		private void CompleteBatch(DateTime start, DateTime end, int totalRows, int errorRows)
		{
			LogBatchCompleted(start, end, totalRows, errorRows);
			_totalRowsImported += totalRows;
			_totalRowsWithErrors += errorRows;
			OnBatchComplete?.Invoke(start, end, _totalRowsImported, _totalRowsWithErrors);
		}

		private void ImportJob_OnMessage(Status status)
		{
			LogOnMessageEvent(status);
		}

		private void ImportJob_OnProgress(long item)
		{
			LogOnProgressEvent();
			_itemsTransferred++;
			if (Environment.TickCount - _lastJobProgressUpdate > _JOB_PROGRESS_TIMEOUT_MILLISECONDS)
			{
				_lastJobProgressUpdate = Environment.TickCount;
				if (OnStatusUpdate != null)
				{
					OnStatusUpdate(_itemsTransferred, 0);
					_itemsTransferred = 0;
				}
			}
		}

		private int GetDestinationFolderArtifactId(int folderArtifactId)
		{
			if (CurrentWorkspace != null && folderArtifactId == 0)
			{
				folderArtifactId = Settings.ArtifactTypeId == (int) ArtifactType.Document ? CurrentWorkspace.RootFolderID : CurrentWorkspace.RootArtifactID;
			}
			return folderArtifactId;
		}

		#region Logging

		private void LogInitializingImportApi()
		{
			_logger.LogInformation("Attempting to initialize Import API.");
		}

		private void LogAddingRowToBatchManager()
		{
			_logger.LogVerbose("Attempting to add row to batch manager.");
		}

		private void LogPushingBatch(bool forcePush)
		{
			_logger.LogVerbose("Attempting to push batch (ForcePush: {ForcePush}).", forcePush);
		}

		private void LogImportJobStarted()
		{
			_logger.LogVerbose("Import Job started.");
		}

		private void LogSettingUpImportJob()
		{
			_logger.LogVerbose("Setting up Import Job and attempting to start it.");
		}

		private void LogSettingFieldDictionaryError(Exception e)
		{
			_logger.LogError(e, "Failed to setup field dictionary for Import API.");
		}

		private void LogFieldInWorkspaceValidationError(string message)
		{
			_logger.LogError("Not all fields have been found in workspace. {Message}.", message);
		}

		private void LogOnBatchCreateEvent(int batchSize)
		{
			_logger.LogVerbose("ImportService OnBatchCreate event received with batch size: {BatchSize}.", batchSize);
		}

		private void LogOnErrorEvent(Exception fatalException)
		{
			_logger.LogVerbose(fatalException, "ImportJob returned FatalException.");
		}

		private void LogOnCompleteEvent()
		{
			_logger.LogVerbose("ImportJob OnComplete event received.");
		}

		private void LogBatchCompleted(DateTime start, DateTime end, int totalRows, int errorRows)
		{
			_logger.LogVerbose("Batch completed. Total rows: {TotalRows}. Rows with error: {ErrorRows}. Time in milliseconds: {Time}.", totalRows, errorRows,
				(end - start).Milliseconds);
		}

		private void LogOnProgressEvent()
		{
			_logger.LogVerbose("ImportJob OnProgress event recevied.");
		}

		private void LogOnMessageEvent(Status status)
		{
			_logger.LogVerbose("ImportJob OnMessage event received. Current status: {Status}.", status.Message);
		}

		#endregion
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using kCura.Relativity.DataReaderClient;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;

namespace kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI
{
	public class ImportService : IImportService, IBatchReporter
	{
		private IExtendedImportAPI _importApi;
		private readonly ImportApiFactory _factory;
		private readonly BatchManager _batchManager;
		private Dictionary<int, Field> _idToFieldDictionary;
		private Dictionary<string, Field> _mappings;
		private readonly Dictionary<string, int> _inputMappings;
		private int _itemsTransferred;
		private int _itemsErrored;
		private int _totalRowsImported = 0;
		private int _totalRowsWithErrors = 0;

		private const int _JOB_PROGRESS_TIMEOUT_MILLISECONDS = 5000;
		private int _lastJobProgressUpdate = 0;
		private int _lastJobErrorUpdate = 0;

		public event StatusUpdate OnStatusUpdate;
		public event BatchCompleted OnBatchComplete;
		public event BatchSubmitted OnBatchSubmit;
		public event BatchCreated OnBatchCreate;
		public event JobError OnJobError;
		public event RowError OnDocumentError;

		public ImportService(ImportSettings settings, Dictionary<string, int> fieldMappings, BatchManager batchManager, NativeFileImportService nativeFileImportService, ImportApiFactory factory)
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
			this.Settings = settings;
			this._batchManager = batchManager;
			this._inputMappings = fieldMappings;
			this.NativeFileImportService = nativeFileImportService;
			_factory = factory;
			if (_batchManager != null)
			{
				_batchManager.OnBatchCreate += ImportService_OnBatchCreate;
			}
		}

		static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			return EmbeddedAssembly.Get(args.Name);
		}

		public ImportSettings Settings { get; private set; }
		public NativeFileImportService NativeFileImportService { get; private set; }

		public virtual void Initialize()
		{
			if (_importApi == null)
			{
				Connect(Settings);
				SetupFieldDictionary(_importApi);
				Dictionary<string, int> fieldMapping = _inputMappings;
				_mappings = ValidateAllMappedFieldsAreInWorkspace(fieldMapping, _idToFieldDictionary);
				HashSet<string> columnNames = new HashSet<string>();
				_mappings.Values.ToList().ForEach(x =>
				{
					if (!columnNames.Contains(x.Name))
					{
						columnNames.Add(x.Name);
					}
				});

				this._batchManager.ColumnNames = columnNames;
			}
		}

		public void AddRow(Dictionary<string, object> sourceFields)
		{
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
				try
				{
					IDataReader sourceData = _batchManager.GetBatchData();
					if (sourceData != null)
					{
						this.KickOffImport(sourceData);
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

		public void CleanUp() { }

		public virtual void KickOffImport(IDataReader dataReader)
		{
			ImportBulkArtifactJob importJob = null;
			if (Settings.ArtifactTypeId == (int)Relativity.Client.ArtifactType.Document)
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
			importJob.Settings.DestinationFolderArtifactID = GetDestinationFolderArtifactId();
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
			importJob.Settings.MaximumErrorCount = Int32.MaxValue - 1; //Have to pass in MaxValue - 1 because of how the ImportAPI validation works -AJK 10-July-2012
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
			importJob.OnComplete += new IImportNotifier.OnCompleteEventHandler(ImportJob_OnComplete);
			importJob.OnFatalException += new IImportNotifier.OnFatalExceptionEventHandler(ImportJob_OnComplete);

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

			importJob.OnProgress += ImportJob_OnProgress;
			ImportService_OnBatchSubmit(_batchManager.CurrentSize, _batchManager.MinimumBatchSize);

			importJob.Execute();
		}

		private Dictionary<string, Field> FieldMappings
		{
			get { return _mappings; }
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
			catch (Exception)
			{
				//LoggedException.PreserveStack(ex);
				//throw new ConnectionException(RelativityExport.RelativityWorkspaceRead, ex);
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
				string missingFieldFormatted = String.Join(", ", missingFields);
				throw new Exception(string.Format("Missing mapped field IDs: {0}", missingFieldFormatted));
			}
			return mapping;
		}

		public Dictionary<string, object> GenerateImportFields(Dictionary<string, object> sourceFields, Dictionary<string, Field> mapping, NativeFileImportService nativeFileImportService)
		{
			Dictionary<string, object> importFields = new Dictionary<string, object>();

			foreach (string sourceFieldId in sourceFields.Keys)
			{
				if (mapping.ContainsKey(sourceFieldId))
				{
					Field rdoField = mapping[sourceFieldId];

					if (!importFields.ContainsKey(rdoField.Name))
						importFields.Add(rdoField.Name, sourceFields[sourceFieldId]);
				}
			}
			if (nativeFileImportService != null && nativeFileImportService.ImportNativeFiles)
			{
				importFields.Add(nativeFileImportService.DestinationFieldName, sourceFields[nativeFileImportService.SourceFieldName]);
			}
			return importFields;
		}

		private void ImportService_OnBatchCreate(int batchSize)
		{
			if (OnBatchCreate != null)
			{
				OnBatchCreate(batchSize);
			}
		}

		private void ImportService_OnBatchSubmit(int currentSize, int minSize)
		{
			if (OnBatchSubmit != null)
			{
				OnBatchSubmit(currentSize, minSize);
			}
		}

		private void ImportJob_OnComplete(JobReport jobReport)
		{
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
			if (OnJobError != null)
			{
				OnJobError(fatalException);
			}
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
			_totalRowsImported += totalRows;
			_totalRowsWithErrors += errorRows;
			if (OnBatchComplete != null)
			{
				OnBatchComplete(start, end, _totalRowsImported, _totalRowsWithErrors);
			}
		}

		private void ImportJob_OnProgress(long item)
		{
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

		private int GetDestinationFolderArtifactId()
		{
			int destinationFolderArtifactId = 0;
			if (CurrentWorkspace != null)
			{
				if (Settings.ArtifactTypeId == (int)kCura.Relativity.Client.ArtifactType.Document)
				{
					destinationFolderArtifactId = CurrentWorkspace.RootFolderID;
				}
				else
				{
					destinationFolderArtifactId = CurrentWorkspace.RootArtifactID;
				}
			}
			return destinationFolderArtifactId;
		}

		private Workspace _currentWorkspace;

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
	}
}

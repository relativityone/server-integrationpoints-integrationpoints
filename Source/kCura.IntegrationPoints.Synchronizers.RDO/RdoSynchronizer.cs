using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using Newtonsoft.Json;
using Relativity.API;
using Artifact = kCura.Relativity.Client.Artifact;
using Constants = kCura.IntegrationPoints.Domain.Constants;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RdoSynchronizer : IDataSynchronizer, IBatchReporter, IEmailBodyData
	{
		private readonly IImportApiFactory _factory;
		private readonly IImportJobFactory _jobFactory;
		private readonly IHelper _helper;
		private readonly IAPILog _logger;

		protected readonly IRelativityFieldQuery FieldQuery;
		private IImportAPI _api;

		private bool? _disableNativeLocationValidation;

		private bool? _disableNativeValidation;

		private HashSet<string> _ignoredList;

		private IImportService _importService;
		private bool _isJobComplete;
		private List<KeyValuePair<string, string>> _rowErrors;

		private string _webApiPath;

		public RdoSynchronizer(IRelativityFieldQuery fieldQuery, IImportApiFactory factory, IImportJobFactory jobFactory, IHelper helper)
		{
			FieldQuery = fieldQuery;
			_factory = factory;
			_jobFactory = jobFactory;
			_helper = helper;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RdoSynchronizer>();
		}

		private ImportSettings ImportSettings { get; set; }
		private NativeFileImportService NativeFileImportService { get; set; }

		public SourceProvider SourceProvider { get; set; }

		private HashSet<string> IgnoredList
		{
			get
			{
				// fields don't have any space in between words
				if (_ignoredList == null)
				{
					_ignoredList = new HashSet<string>
					{
						"Is System Artifact",
						"System Created By",
						"System Created On",
						"System Last Modified By",
						"System Last Modified On",
						"Artifact ID"
					};
				}
				return _ignoredList;
			}
		}

		public string WebAPIPath
		{
			get
			{
				if (string.IsNullOrEmpty(_webApiPath))
				{
					_webApiPath = Config.Config.Instance.WebApiPath;
				}
				return _webApiPath;
			}
			protected set { _webApiPath = value; }
		}

		public bool? DisableNativeLocationValidation
		{
			get
			{
				if (!_disableNativeLocationValidation.HasValue)
				{
					_disableNativeLocationValidation = Config.Config.Instance.DisableNativeLocationValidation;
				}
				return _disableNativeLocationValidation;
			}
			protected set { _disableNativeLocationValidation = value; }
		}

		public bool? DisableNativeValidation
		{
			get
			{
				if (!_disableNativeValidation.HasValue)
				{
					_disableNativeValidation = Config.Config.Instance.DisableNativeValidation;
				}
				return _disableNativeValidation;
			}
			protected set { _disableNativeValidation = value; }
		}

		public event BatchCompleted OnBatchComplete;

		public event BatchSubmitted OnBatchSubmit;

		public event BatchCreated OnBatchCreate;

		public event StatusUpdate OnStatusUpdate;

		public event JobError OnJobError;

		public event RowError OnDocumentError;

	    public void RaiseDocumentErrorEvent(string documentIdentifier, string errorMessage)
	    {
	        OnDocumentError?.Invoke(documentIdentifier, errorMessage);
	    }

		public virtual IEnumerable<FieldEntry> GetFields(string options)
		{
			LogRetrievingFields();
			HashSet<string> ignoreFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME,
				Constants.SPECIAL_SOURCEJOB_FIELD_NAME,
				DocumentFields.RelativityDestinationCase,
				DocumentFields.JobHistory
			};

			FieldEntry[] fields = GetFieldsInternal(options).Where(f => !ignoreFields.Contains(f.ActualName)).Select(f => f).ToArray();

			foreach (var field in fields.Where(field => field.IsIdentifier))
			{
				field.IsRequired = true;
			}
			return fields;
		}

		public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, string options)
		{
			LogSyncingData();

			IntializeImportJob(fieldMap, options);

			bool movedNext = true;
			IEnumerator<IDictionary<FieldEntry, object>> enumerator = data.GetEnumerator();

			do
			{
				try
				{
					movedNext = enumerator.MoveNext();
					if (movedNext)
					{
						Dictionary<string, object> importRow = GenerateImportRow(enumerator.Current, fieldMap, ImportSettings);
						if (importRow != null)
						{
							_importService.AddRow(importRow);
						}
					}
				}
				catch (ProviderReadDataException exception)
				{
					LogSyncDataError(exception);
					ItemError(exception.Identifier, exception.Message);
				}
				catch (Exception ex)
				{
					LogSyncDataError(ex);
					ItemError(string.Empty, ex.Message);
				}
			} while (movedNext);

			_importService.PushBatchIfFull(true);

			WaitUntilTheJobIsDone();

			FinalizeSyncData(data, fieldMap, ImportSettings);
		}

		public void SyncData(IDataTransferContext context, IEnumerable<FieldMap> fieldMap, string options)
		{
			LogSyncingData();

			IntializeImportJob(fieldMap, options);

			FieldMap[] fieldMaps = fieldMap as FieldMap[] ?? fieldMap.ToArray();
			if (fieldMaps.Length > 0)
			{
				context.DataReader = new RelativityReaderDecorator(context.DataReader, fieldMaps);
				_importService.KickOffImport(context);
			}
			else
			{
				_importService.KickOffImport(context);
			}
			
			WaitUntilTheJobIsDone();
		}

		public string GetEmailBodyData(IEnumerable<FieldEntry> fields, string options)
		{
			LogRetrievingEmailBody();
			ImportSettings settings = GetSettings(options);
			WorkspaceRef destinationWorkspace = GetWorkspace(settings);

			StringBuilder emailBody = new StringBuilder();
			if (destinationWorkspace != null)
			{
				emailBody.AppendLine("");
				emailBody.AppendFormat("Destination Workspace: {0}", Utils.GetFormatForWorkspaceOrJobDisplay(destinationWorkspace.Name, destinationWorkspace.Id));
			}
			return emailBody.ToString();
		}

		protected IImportAPI GetImportApi(ImportSettings settings)
		{
			return _api ?? (_api = _factory.GetImportAPI(settings));
		}

		protected List<Artifact> GetRelativityFields(ImportSettings settings)
		{
			try
			{
				List<Artifact> fields = FieldQuery.GetFieldsForRdo(settings.ArtifactTypeId);
				HashSet<int> mappableArtifactIds =
					new HashSet<int>(GetImportApi(settings).GetWorkspaceFields(settings.CaseArtifactId, settings.ArtifactTypeId).Select(x => x.ArtifactID));
				return fields.Where(x => mappableArtifactIds.Contains(x.ArtifactID)).ToList();
			}
			catch (Exception e)
			{
				LogRetrievingRelativityFieldsError(e);
				throw;
			}
		}

		private IEnumerable<FieldEntry> GetFieldsInternal(string options)
		{
			ImportSettings settings = GetSettings(options);
			List<Artifact> fields = GetRelativityFields(settings);
			return ParseFields(fields);
		}

		protected IEnumerable<FieldEntry> ParseFields(List<Artifact> fields)
		{
			foreach (var result in fields)
			{
				if (!IgnoredList.Contains(result.Name))
				{
					var idField = result.Fields.FirstOrDefault(x => x.Name.Equals("Is Identifier"));
					bool isIdentifier = false;
					if (idField != null)
					{
						isIdentifier = Convert.ToInt32(idField.Value) == 1;
						if (isIdentifier)
						{
							result.Name += " [Object Identifier]";
						}
					}

					var fieldType = result.Fields.FirstOrDefault(x => x.Name.Equals("Field Type"));
					string type = string.Empty;
					if (fieldType != null)
					{
						type = Convert.ToString(fieldType.Value);
					}

					yield return new FieldEntry
					{
						DisplayName = result.Name,
						Type = type,
						FieldIdentifier = result.ArtifactID.ToString(),
						IsIdentifier = isIdentifier,
						IsRequired = false
					};
				}
			}
		}

		private void IntializeImportJob(IEnumerable<FieldMap> fieldMap, string options)
		{
			LogInitializingImportJob();

			NativeFileImportService = new NativeFileImportService();

			ImportSettings = GetSyncDataImportSettings(fieldMap, options, NativeFileImportService);

			var importFieldMap = GetSyncDataImportFieldMap(fieldMap, ImportSettings);

			_importService = InitializeImportService(ImportSettings, importFieldMap, NativeFileImportService);

			_isJobComplete = false;
			_rowErrors = new List<KeyValuePair<string, string>>();

			_logger.LogDebug("Initializing Import Job completed.");
		}

		private void WaitUntilTheJobIsDone()
		{
			bool isJobDone;
			do
			{
				lock (_importService)
				{
					isJobDone = bool.Parse(_isJobComplete.ToString());
				}
				Thread.Sleep(1000);
			} while (!isJobDone);
		}

		protected virtual ImportService InitializeImportService(ImportSettings settings, Dictionary<string, int> importFieldMap, NativeFileImportService nativeFileImportService)
		{
			LogInitializingImportService();

			ImportService importService = new ImportService(settings, importFieldMap, new BatchManager(), nativeFileImportService, _factory, _jobFactory, _helper);
			importService.OnBatchComplete += Finish;
			importService.OnDocumentError += ItemError;
			importService.OnJobError += JobError;

			if (OnBatchComplete != null)
			{
				importService.OnBatchComplete += OnBatchComplete;
			}
			if (OnBatchSubmit != null)
			{
				importService.OnBatchSubmit += OnBatchSubmit;
			}
			if (OnBatchCreate != null)
			{
				importService.OnBatchCreate += OnBatchCreate;
			}
			if (OnStatusUpdate != null)
			{
				importService.OnStatusUpdate += OnStatusUpdate;
			}
			if (OnJobError != null)
			{
				importService.OnJobError += OnJobError;
			}
			if (OnDocumentError != null)
			{
				importService.OnDocumentError += OnDocumentError;
			}
			importService.Initialize();

			_logger.LogDebug("Initialization Import Service...finished");

			return importService;
		}

		protected virtual ImportSettings GetSyncDataImportSettings(IEnumerable<FieldMap> fieldMap, string options, NativeFileImportService nativeFileImportService)
		{
			LogRetrievingImportSettings();

			ImportSettings settings = GetSettings(options);
			if (string.IsNullOrWhiteSpace(settings.ParentObjectIdSourceFieldName) &&
				fieldMap.Any(x => x.FieldMapType == FieldMapTypeEnum.Parent))
			{
				settings.ParentObjectIdSourceFieldName =
					fieldMap.Where(x => x.FieldMapType == FieldMapTypeEnum.Parent).Select(x => x.SourceField.FieldIdentifier).First();
			}
			if ((settings.IdentityFieldId < 1) && fieldMap.Any(x => x.FieldMapType == FieldMapTypeEnum.Identifier))
			{
				settings.IdentityFieldId =
					fieldMap.Where(x => x.FieldMapType == FieldMapTypeEnum.Identifier)
						.Select(x => int.Parse(x.DestinationField.FieldIdentifier))
						.First();
			}

            if (settings.ImportNativeFile && settings.ImportNativeFileCopyMode == ImportNativeFileCopyModeEnum.CopyFiles)
			{
				nativeFileImportService.ImportNativeFiles = true;
				FieldMap field = fieldMap.FirstOrDefault(x => x.FieldMapType == FieldMapTypeEnum.NativeFilePath);
				nativeFileImportService.SourceFieldName = field != null ? field.SourceField.FieldIdentifier : Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD;
				settings.NativeFilePathSourceFieldName = nativeFileImportService.DestinationFieldName;
				settings.DisableNativeLocationValidation = DisableNativeLocationValidation;
				settings.DisableNativeValidation = DisableNativeValidation;
				settings.CopyFilesToDocumentRepository = true;

                // NOTE :: So that the destination workspace file icons correctly display, we give the import API the file name of the document
                settings.FileNameColumn = Constants.SPECIAL_FILE_NAME_FIELD_NAME;
            }
			else if (settings.ImportNativeFileCopyMode == ImportNativeFileCopyModeEnum.SetFileLinks)
            {
				nativeFileImportService.ImportNativeFiles = true;
                FieldMap field = fieldMap.FirstOrDefault(x => x.FieldMapType == FieldMapTypeEnum.NativeFilePath);
                nativeFileImportService.SourceFieldName = field != null ? field.SourceField.FieldIdentifier : Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD;
                settings.NativeFilePathSourceFieldName = nativeFileImportService.DestinationFieldName;
				settings.DisableNativeLocationValidation = DisableNativeLocationValidation;
				settings.DisableNativeValidation = DisableNativeValidation;
				settings.CopyFilesToDocumentRepository = false;

                // NOTE :: So that the destination workspace file icons correctly display, we give the import API the file name of the document
                settings.FileNameColumn = Constants.SPECIAL_FILE_NAME_FIELD_NAME;
            }
            else if (settings.ImportNativeFileCopyMode == ImportNativeFileCopyModeEnum.DoNotImportNativeFiles)
		    {
                nativeFileImportService.ImportNativeFiles = false;
                settings.DisableNativeLocationValidation = null;
                settings.DisableNativeValidation = null;
                settings.CopyFilesToDocumentRepository = false;

                // NOTE :: Determines if we want to upload/delete native files and update "Has Native", "Supported by viewer" and "Relativity Native Type" fields
                settings.NativeFilePathSourceFieldName = string.Empty;
            }


		    if (fieldMap.Any(x => x.FieldMapType == FieldMapTypeEnum.FolderPathInformation))
			{
				// NOTE :: If you expect to import the folder path, the import API will expect this field to be specified upon import. This is to avoid the field being both mapped and used as a folder path.
				settings.FolderPathSourceFieldName = Constants.SPECIAL_FOLDERPATH_FIELD_NAME;
			}

			if (settings.UseDynamicFolderPath)
			{
				settings.FolderPathSourceFieldName = Constants.SPECIAL_FOLDERPATH_DYNAMIC_FIELD_NAME;
			}

			if ((SourceProvider != null) && SourceProvider.Config.OnlyMapIdentifierToIdentifier)
			{
				FieldMap map = fieldMap.First(field => field.FieldMapType == FieldMapTypeEnum.Identifier);
				if (!(map.SourceField.IsIdentifier && map.DestinationField.IsIdentifier))
				{
					LogMissingIdentifierField();
					throw new Exception("Source Provider requires the identifier field to be mapped with another identifier field.");
				}
			}

			_logger.LogDebug($"Rip Import Settings:\n {JsonConvert.SerializeObject(settings)}");
			return settings;
		}

		protected virtual Dictionary<string, int> GetSyncDataImportFieldMap(IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			Dictionary<string, int> importFieldMap = null;
			try
			{
				importFieldMap = fieldMap.Where(IncludeFieldInImport).ToDictionary(x => x.SourceField.FieldIdentifier, x => int.Parse(x.DestinationField.FieldIdentifier));
			}
			catch (Exception ex)
			{
				LogInvalidFieldMap(ex);
				throw new Exception("Field Map is invalid.", ex);
			}
			return importFieldMap;
		}

		protected virtual Dictionary<string, object> GenerateImportRow(IDictionary<FieldEntry, object> row, IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			Dictionary<string, object> importRow = row.ToDictionary(x => x.Key.FieldIdentifier, x => x.Value);
			return importRow;
		}

		protected virtual void FinalizeSyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
		}

		protected ImportSettings GetSettings(string options)
		{
			var settings = DeserializeImportSettings(options);

			if (string.IsNullOrEmpty(settings.WebServiceURL))
			{
				settings.WebServiceURL = WebAPIPath;
				if (string.IsNullOrEmpty(settings.WebServiceURL))
				{
					LogMissingWebApiPath();
					throw new Exception("No WebAPI path set for integration points.");
				}
			}
			return settings;
		}

		private ImportSettings DeserializeImportSettings(string options)
		{
			try
			{
				return JsonConvert.DeserializeObject<ImportSettings>(options);
			}
			catch (Exception e)
			{
				LogImportSettingsDeserializationError(e);
				throw;
			}
		}

		protected bool IncludeFieldInImport(FieldMap fieldMap)
		{
			bool toInclude = (fieldMap.FieldMapType != FieldMapTypeEnum.Parent) &&
							(fieldMap.FieldMapType != FieldMapTypeEnum.NativeFilePath);
			if (toInclude && (fieldMap.FieldMapType == FieldMapTypeEnum.FolderPathInformation))
			{
				toInclude = (fieldMap.DestinationField != null) && (fieldMap.DestinationField.FieldIdentifier != null);
			}
			return toInclude;
		}

		private void Finish(DateTime startTime, DateTime endTime, int totalRows, int errorRowCount)
		{
			lock (_importService)
			{
				_isJobComplete = true;
			}
		}

		private void JobError(Exception ex)
		{
			lock (_importService)
			{
				_isJobComplete = true;
			}
		}

		private void ItemError(string documentIdentifier, string errorMessage)
		{
			_rowErrors.Add(new KeyValuePair<string, string>(documentIdentifier, errorMessage));
		}

		protected virtual WorkspaceRef GetWorkspace(ImportSettings settings)
		{
			try
			{
				WorkspaceRef workspaceRef = null;
				Workspace workspace = GetImportApi(settings).Workspaces().FirstOrDefault(x => x.ArtifactID == settings.CaseArtifactId);
				if (workspace != null)
				{
					workspaceRef = new WorkspaceRef {Id = workspace.ArtifactID, Name = workspace.Name};
				}
				return workspaceRef;
			}
			catch (Exception e)
			{
				LogRetrievingWorkspaceError(e);
				throw;
			}
		}

		#region Logging

		private void LogRetrievingWorkspaceError(Exception e)
		{
			_logger.LogError(e, "Failed to retrieve workspace.");
		}

		private void LogRetrievingFields()
		{
			_logger.LogInformation("Attempting to retrieve fields.");
		}

		private void LogSyncDataError(ProviderReadDataException exception)
		{
			_logger.LogError(exception, "Importing document {DocumentIdentifier} failed with message: {Message}.", exception.Identifier, exception.Message);
		}
		private void LogSyncDataError(Exception exception)
		{
			_logger.LogError(exception, "Importing object failed with message: {Message}.", exception.Message);
		}

		private void LogSyncingData()
		{
			_logger.LogDebug("Preparing import process in synchronizer...");
		}

		private void LogRetrievingEmailBody()
		{
			_logger.LogVerbose("Retrieving email body.");
		}

		private void LogRetrievingRelativityFieldsError(Exception e)
		{
			_logger.LogError(e, "Failed to retrieve Relativity fields.");
		}

		private void LogInitializingImportJob()
		{
			_logger.LogDebug("Initializing Import Job.");
		}

		private void LogInitializingImportService()
		{
			_logger.LogDebug("Start initializing Import Service...");
		}

		private void LogMissingIdentifierField()
		{
			_logger.LogError("Source Provider requires the identifier field to be mapped with another identifier field.");
		}

		private void LogRetrievingImportSettings()
		{
			_logger.LogDebug("Starting RIP Import Settings creation...");
		}

		private void LogInvalidFieldMap(Exception ex)
		{
			_logger.LogError(ex, "Field Map is invalid.");
		}

		private void LogMissingWebApiPath()
		{
			_logger.LogError("No WebAPI path set for integration points.");
		}

		private void LogImportSettingsDeserializationError(Exception e)
		{
			_logger.LogError(e, "Failed to deserialize Import Settings.");
		}

		#endregion
	}
}
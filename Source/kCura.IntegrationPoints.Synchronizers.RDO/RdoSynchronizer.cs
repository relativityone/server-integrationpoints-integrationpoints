using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Domain.Utils;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.Relativity.ImportAPI;
using kCura.Relativity.ImportAPI.Data;
using Newtonsoft.Json;
using Relativity.API;
using Artifact = kCura.Relativity.Client.Artifact;
using Constants = kCura.IntegrationPoints.Domain.Constants;
using Field = kCura.Relativity.Client.Field;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
	public class RdoSynchronizer : IDataSynchronizer, IBatchReporter, IEmailBodyData
	{
		private bool _isJobComplete;
		private bool? _disableNativeLocationValidation;
		private bool? _disableNativeValidation;
		private HashSet<string> _ignoredList;
		private IImportAPI _api;
		private IImportService _importService;
		private string _webApiPath;
		private readonly IAPILog _logger;
		private readonly IHelper _helper;
		private readonly IImportApiFactory _factory;
		private readonly IImportJobFactory _jobFactory;

		protected readonly IRelativityFieldQuery FieldQuery;

		public SourceProvider SourceProvider { get; set; }
		private ImportSettings ImportSettings { get; set; }

		private NativeFileImportService NativeFileImportService { get; set; }

		private HashSet<string> IgnoredList => _ignoredList ?? (_ignoredList = new HashSet<string>
			{
				"Is System Artifact",
				"System Created By",
				"System Created On",
				"System Last Modified By",
				"System Last Modified On",
				"Artifact ID"
			});

		public bool? DisableNativeLocationValidation
		{
			get
			{
				if (!_disableNativeLocationValidation.HasValue)
				{
					_disableNativeLocationValidation = Config.Config.Instance.DisableNativeLocationValidation;
					LogNewDisableNativeLocationValidationValue();
				}
				return _disableNativeLocationValidation;
			}
			protected set { _disableNativeLocationValidation = value; }
		}
		
		public string WebAPIPath
		{
			get
			{
				if (string.IsNullOrEmpty(_webApiPath))
				{
					_webApiPath = Config.Config.Instance.WebApiPath;
					LogNewWebAPIPathValue();
				}
				return _webApiPath;
			}
			protected set { _webApiPath = value; }
		}

		public bool? DisableNativeValidation
		{
			get
			{
				if (!_disableNativeValidation.HasValue)
				{
					_disableNativeValidation = Config.Config.Instance.DisableNativeValidation;
					LogNewDisableNativeValidationValue();
				}
				return _disableNativeValidation;
			}
			protected set { _disableNativeValidation = value; }
		}

		public event BatchCompleted OnBatchComplete;

		public event BatchSubmitted OnBatchSubmit;

		public event BatchCreated OnBatchCreate;

		public event StatusUpdate OnStatusUpdate;

		public event StatisticsUpdate OnStatisticsUpdate;

		public event JobError OnJobError;

		public event RowError OnDocumentError;

		public RdoSynchronizer(IRelativityFieldQuery fieldQuery, IImportApiFactory factory, IImportJobFactory jobFactory, IHelper helper)
		{
			FieldQuery = fieldQuery;
			_factory = factory;
			_jobFactory = jobFactory;
			_helper = helper;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<RdoSynchronizer>();
		}

		protected void RaiseDocumentErrorEvent(string documentIdentifier, string errorMessage)
		{
			OnDocumentError?.Invoke(documentIdentifier, errorMessage);
		}

		private void RaiseJobErrorEvent(Exception exception)
		{
			OnJobError?.Invoke(exception);
		}

		public virtual IEnumerable<FieldEntry> GetFields(DataSourceProviderConfiguration providerConfiguration)
		{
			try
			{
				LogRetrievingFields();
				HashSet<string> ignoreFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
				{
					Constants.SPECIAL_SOURCEWORKSPACE_FIELD_NAME,
					Constants.SPECIAL_SOURCEJOB_FIELD_NAME,
					DocumentFields.RelativityDestinationCase,
					DocumentFields.JobHistory
				};

				FieldEntry[] fields = GetFieldsInternal(providerConfiguration.Configuration).Where(f => !ignoreFields.Contains(f.ActualName)).Select(f => f).ToArray();

				foreach (var field in fields.Where(field => field.IsIdentifier))
				{
					field.IsRequired = true;
				}
				return fields;
			}
			catch (Exception ex)
			{
				throw LogAndCreateGetFieldsException(ex, providerConfiguration.Configuration);
			}
		}

		public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap, string options)
		{
			try
			{
				LogSyncingData();

				InitializeImportJob(fieldMap, options);

				bool rowProcessed = false;
				IEnumerator<IDictionary<FieldEntry, object>> enumerator = data.GetEnumerator();

				do
				{
					try
					{
						rowProcessed = ProcessRowForImport(fieldMap, enumerator);
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
				} while (rowProcessed);

				_importService.PushBatchIfFull(true);

				WaitUntilTheJobIsDone();
				FinalizeSyncData(data, fieldMap, ImportSettings);
			}
			catch (Exception ex)
			{
				throw LogAndCreateSyncDataException(ex, fieldMap, options);
			}
		}

		internal bool ProcessRowForImport(IEnumerable<FieldMap> fieldMap, IEnumerator<IDictionary<FieldEntry, object>> enumerator)
		{
			bool rowProcessed = enumerator.MoveNext();

			if (!rowProcessed)
			{
				return false;
			}

			Dictionary<string, object> importRow = GenerateImportRow(enumerator.Current, fieldMap, ImportSettings);
			if (importRow != null)
			{
				_importService.AddRow(importRow);
			}

			return true;
		}

		public void SyncData(IDataTransferContext context, IEnumerable<FieldMap> fieldMap, string options)
		{
			try
			{
				LogSyncingData();

				InitializeImportJob(fieldMap, options);

				FieldMap[] fieldMaps = fieldMap as FieldMap[] ?? fieldMap.ToArray();
				LogFieldMapLength(fieldMaps);
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
			catch (Exception ex)
			{
				throw LogAndCreateSyncDataException(ex, fieldMap, options);
			}
		}

		public string GetEmailBodyData(IEnumerable<FieldEntry> fields, string options)
		{
			LogRetrievingEmailBody();
			ImportSettings settings = GetSettings(options);
			WorkspaceRef destinationWorkspace = GetWorkspace(settings);

			var emailBody = new StringBuilder();
			if (destinationWorkspace != null)
			{
				emailBody.AppendLine("");
				string destinationWorkspaceAsString = WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(destinationWorkspace.Name, destinationWorkspace.Id);
				emailBody.AppendFormat("Destination Workspace: {0}", destinationWorkspaceAsString);
				LogDestinationWorkspaceAppendedToEmailBody(destinationWorkspaceAsString);
			}
			return emailBody.ToString();
		}

		protected IImportAPI GetImportApi(ImportSettings settings)
		{
			if (_api == null)
			{
				LogCreatingImportApi();
			}
			return _api ?? (_api = _factory.GetImportAPI(settings));
		}

		protected List<Artifact> GetRelativityFields(ImportSettings settings)
		{
			try
			{
				List<Artifact> fields = FieldQuery.GetFieldsForRdo(settings.ArtifactTypeId);
				var mappableArtifactIds = new HashSet<int>(GetImportApi(settings).GetWorkspaceFields(settings.CaseArtifactId, settings.ArtifactTypeId).Select(x => x.ArtifactID));
				List<Artifact> mappableFields = fields.Where(x => mappableArtifactIds.Contains(x.ArtifactID)).ToList();
				LogNumbersOfFieldAndMappableFields(fields, mappableFields);
				return mappableFields;
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

		private IEnumerable<FieldEntry> ParseFields(List<Artifact> fields)
		{
			foreach (var result in fields)
			{
				if (IgnoredList.Contains(result.Name))
				{
					continue;
				}

				Field idField = result.Fields.FirstOrDefault(x => x.Name.Equals("Is Identifier"));
				bool isIdentifier = Convert.ToInt32(idField?.Value) == 1;

				if (isIdentifier)
				{
					result.Name += " [Object Identifier]";
					LogIdentifierFields(result);
				}

				Field fieldType = result.Fields.FirstOrDefault(x => x.Name.Equals("Field Type"));
				string type = fieldType == null ? string.Empty : Convert.ToString(fieldType.Value);

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

		private void InitializeImportJob(IEnumerable<FieldMap> fieldMap, string options)
		{
			LogInitializingImportJob();

			NativeFileImportService = new NativeFileImportService();

			ImportSettings = GetSyncDataImportSettings(fieldMap, options, NativeFileImportService);

			Dictionary<string, int> importFieldMap = GetSyncDataImportFieldMap(fieldMap, ImportSettings);

			_importService = InitializeImportService(ImportSettings, importFieldMap, NativeFileImportService);

			_isJobComplete = false;

			_logger.LogDebug("Initializing Import Job completed.");
		}

		protected virtual void WaitUntilTheJobIsDone()
		{
			const int waitDuration = 1000;

			bool isJobDone;
			do
			{
				lock (_importService)
				{
					isJobDone = _isJobComplete;
				}
				_logger.LogInformation("Waiting until the job id done");
				Thread.Sleep(waitDuration);
			}
			while (!isJobDone);
		}

		protected internal virtual IImportService InitializeImportService(ImportSettings settings, Dictionary<string, int> importFieldMap, NativeFileImportService nativeFileImportService)
		{
			LogInitializingImportService();

			ImportService importService = new ImportService(settings, importFieldMap, new BatchManager(), nativeFileImportService, _factory, _jobFactory, _helper);
			importService.OnBatchComplete += Finish;
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
			if (OnStatisticsUpdate != null)
			{
				importService.OnStatisticsUpdate += OnStatisticsUpdate;
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
			try
			{
				LogRetrievingImportSettings();

				ImportSettings settings = GetSettings(options);

				BootstrapParentObjectSettings(fieldMap, settings);
				BootstrapIdentityFieldSettings(fieldMap, settings);
				BootstrapImportNativesSettings(fieldMap, nativeFileImportService, settings);
				BootstrapFolderSettings(fieldMap, settings);
				BootstrapDestinationIdentityFieldSettings(fieldMap, settings);

				_logger.LogDebug($"Rip Import Settings:\n {JsonConvert.SerializeObject(settings)}");
				return settings;

			}
			catch (Exception ex)
			{
				throw LogAndCreateGetImportSettignsException(ex, options, fieldMap);
			}
		}

		private void BootstrapDestinationIdentityFieldSettings(IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			if ((SourceProvider != null) && SourceProvider.Config.OnlyMapIdentifierToIdentifier)
			{
				FieldMap map = fieldMap.First(field => field.FieldMapType == FieldMapTypeEnum.Identifier);
				if (!(map.SourceField.IsIdentifier && map.DestinationField.IsIdentifier))
				{
					LogMissingIdentifierField();
					throw new Exception("Source Provider requires the identifier field to be mapped with another identifier field.");
				}
				settings.DestinationIdentifierField = map.DestinationField.ActualName;
			}
		}

		private void BootstrapIdentityFieldSettings(IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			if ((settings.IdentityFieldId < 1) && fieldMap.Any(x => x.FieldMapType == FieldMapTypeEnum.Identifier))
			{
				settings.IdentityFieldId =
					fieldMap.Where(x => x.FieldMapType == FieldMapTypeEnum.Identifier)
						.Select(x => int.Parse(x.DestinationField.FieldIdentifier))
						.First();
			}
		}

		private void BootstrapParentObjectSettings(IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			if (string.IsNullOrWhiteSpace(settings.ParentObjectIdSourceFieldName) &&
							fieldMap.Any(x => x.FieldMapType == FieldMapTypeEnum.Parent))
			{
				settings.ParentObjectIdSourceFieldName =
					fieldMap.Where(x => x.FieldMapType == FieldMapTypeEnum.Parent).Select(x => x.SourceField.FieldIdentifier).First();
			}
		}

		private void BootstrapFolderSettings(IEnumerable<FieldMap> fieldMap, ImportSettings settings)
		{
			if (fieldMap.Any(x => x.FieldMapType == FieldMapTypeEnum.FolderPathInformation))
			{
				// NOTE :: If you expect to import the folder path, the import API will expect this field to be specified upon import. This is to avoid the field being both mapped and used as a folder path.
				settings.FolderPathSourceFieldName = Constants.SPECIAL_FOLDERPATH_FIELD_NAME;
			}

			if (settings.UseDynamicFolderPath)
			{
				settings.FolderPathSourceFieldName = Constants.SPECIAL_FOLDERPATH_DYNAMIC_FIELD_NAME;
			}
		}

		private void BootstrapImportNativesSettings(IEnumerable<FieldMap> fieldMap, NativeFileImportService nativeFileImportService, ImportSettings settings)
		{
			if (settings.ImportNativeFile && settings.ImportNativeFileCopyMode == ImportNativeFileCopyModeEnum.CopyFiles)
			{
				nativeFileImportService.ImportNativeFiles = true;
				FieldMap field = fieldMap.FirstOrDefault(x => x.FieldMapType == FieldMapTypeEnum.NativeFilePath);
				nativeFileImportService.SourceFieldName = field != null ? field.SourceField.FieldIdentifier : Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD;
				settings.NativeFilePathSourceFieldName = nativeFileImportService.DestinationFieldName;
				settings.DisableNativeLocationValidation = DisableNativeLocationValidation;
				settings.DisableNativeValidation = DisableNativeValidation;
				settings.CopyFilesToDocumentRepository = true;
				settings.FileSizeMapped = true;
				settings.FileSizeColumn = Constants.SPECIAL_NATIVE_FILE_SIZE_FIELD_NAME;
				settings.OIFileIdMapped = true;
				settings.OIFileTypeColumnName = Constants.SPECIAL_FILE_TYPE_FIELD_NAME;
			
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
				settings.FileSizeMapped = true;
				settings.FileSizeColumn = Constants.SPECIAL_NATIVE_FILE_SIZE_FIELD_NAME;
				settings.OIFileIdMapped = true;
				settings.OIFileTypeColumnName = Constants.SPECIAL_FILE_TYPE_FIELD_NAME;

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
			ImportSettings settings = DeserializeImportSettings(options);

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
			bool toInclude = fieldMap.FieldMapType != FieldMapTypeEnum.Parent &&
			                 fieldMap.FieldMapType != FieldMapTypeEnum.NativeFilePath;

			if (toInclude && fieldMap.FieldMapType == FieldMapTypeEnum.FolderPathInformation)
			{
				toInclude = fieldMap.DestinationField?.FieldIdentifier != null;
			}

			return toInclude;
		}

		private void Finish(DateTime startTime, DateTime endTime, int totalRows, int errorRowCount)
		{
			LogLockingImportServiceInFinish();
			lock (_importService)
			{
				LogSettingJobCompleteInFinish();
				_isJobComplete = true;
			}
		}

		private void JobError(Exception ex)
		{
			LogLockingImportServiceInJobError();
			lock (_importService)
			{
				LogSettingJobCompleteInJobError();
				_isJobComplete = true;
			}
			RaiseJobErrorEvent(ex);
		}

		private void ItemError(string documentIdentifier, string errorMessage)
		{
			RaiseDocumentErrorEvent(documentIdentifier, errorMessage);
		}

		protected virtual WorkspaceRef GetWorkspace(ImportSettings settings)
		{
			try
			{
				WorkspaceRef workspaceRef = null;
				Workspace workspace = GetImportApi(settings).Workspaces().FirstOrDefault(x => x.ArtifactID == settings.CaseArtifactId);
				if (workspace != null)
				{
					LogNullWorkspaceReturnedByIAPI();
					workspaceRef = new WorkspaceRef { Id = workspace.ArtifactID, Name = workspace.Name };
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

		private IntegrationPointsException LogAndCreateGetImportSettignsException(Exception exception, string options, IEnumerable<FieldMap> fieldMap)
		{
			string message = $"Error occured while preparing import settings.";
			_logger.LogError("Error occured while preparing import settings\nOptions: {@options}\nFields: {@fieldMap}", options, fieldMap);
			return new IntegrationPointsException(message, exception) { ShouldAddToErrorsTab = true };
		}

		private IntegrationPointsException LogAndCreateGetFieldsException(Exception exception, string options)
		{
			string message = $"Error occured while getting fields.";
			_logger.LogError("Error getting fields. \nOptions: {@options}", options);
			return new IntegrationPointsException(message, exception) { ShouldAddToErrorsTab = true };
		}

		private IntegrationPointsException LogAndCreateSyncDataException(Exception ex, IEnumerable<FieldMap> fieldMap, string options)
		{
			string message = $"Error occured while syncing rdo.";
			_logger.LogError("Error occured while syncing rdo. \nOptions: {@options} \nFields: {@fieldMap}", options, fieldMap);
			return new IntegrationPointsException(message, ex) { ShouldAddToErrorsTab = true };
		}

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

		private void LogNewDisableNativeLocationValidationValue()
		{
			_logger.LogDebug("New value of DisableNativeLocationValidation retrieved from config: {value}", _disableNativeLocationValidation);
		}

		private void LogNewWebAPIPathValue()
		{
			_logger.LogDebug("New value of WebAPIPath retrieved from config: {value}", _webApiPath);
		}

		private void LogNewDisableNativeValidationValue()
		{
			_logger.LogDebug("New value of DisableNativeValidation retrieved from config: {value}", _disableNativeValidation);
		}

		private void LogFieldMapLength(FieldMap[] fieldMaps)
		{
			_logger.LogDebug("Number of items in fieldMap: {fieldMapLength}", fieldMaps.Length);
		}

		private void LogDestinationWorkspaceAppendedToEmailBody(string destinationWorkspaceAsString)
		{
			_logger.LogDebug("Adding destination workpsace to email body: {destinationWorkspace}", destinationWorkspaceAsString);
		}

		private void LogNumbersOfFieldAndMappableFields(List<Artifact> fields, List<Artifact> mappableFields)
		{
			_logger.LogDebug("Retrieved {numberOfFields} fields, {numberOfMappableFields} are mappable", fields.Count, mappableFields.Count);
		}

		private void LogCreatingImportApi()
		{
			_logger.LogDebug("ImportApi was null - new instance will be created using factory");
		}

		private void LogIdentifierFields(Artifact result)
		{
			_logger.LogDebug("Identifier field: {identifierFieldName}", result.Name);
		}

		private void LogNullWorkspaceReturnedByIAPI()
		{
			_logger.LogDebug("ImportApi returned null workspace - creating new WorkspaceRef in GetWorkspace method");
		}

		private void LogSettingJobCompleteInFinish()
		{
			_logger.LogDebug("_importService locked in Finish method of RdoSynchronizer. Job is complete");
		}

		private void LogLockingImportServiceInFinish()
		{
			_logger.LogDebug("Trying to lock _importService in Finish method of RdoSynchronizer");
		}

		private void LogSettingJobCompleteInJobError()
		{
			_logger.LogDebug("_importService locked in JobError method of RdoSynchronizer. Job is complete");
		}

		private void LogLockingImportServiceInJobError()
		{
			_logger.LogDebug("Trying to lock _importService in JobError method of RdoSynchronizer");
		}

		#endregion
	}
}
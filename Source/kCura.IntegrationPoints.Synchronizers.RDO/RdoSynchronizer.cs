using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Domain.Utils;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Internals;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping.ImportApi;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Objects.DataContracts;
using Constants = kCura.IntegrationPoints.Domain.Constants;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public class RdoSynchronizer : IDataSynchronizer, IBatchReporter, IEmailBodyData
    {
        private bool _isJobComplete;
        private bool? _disableNativeLocationValidation;
        private bool? _disableNativeValidation;
        private HashSet<string> _ignoredList;
        protected IImportService ImportService;
        private string _webApiPath;
        private readonly IAPILog _logger;
        private readonly IHelper _helper;
        private readonly IImportApiFactory _factory;
        private readonly IImportJobFactory _jobFactory;

        protected readonly IRelativityFieldQuery FieldQuery;

        public Data.SourceProvider SourceProvider { get; set; }

        public int TotalRowsProcessed => ImportService?.TotalRowsProcessed ?? 0;

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
                throw LogAndCreateGetFieldsException(ex);
            }
        }

        public void SyncData(IEnumerable<IDictionary<FieldEntry, object>> data, IEnumerable<FieldMap> fieldMap,
            string options, IJobStopManager jobStopManager, IDiagnosticLog diagnosticLog)
        {
            try
            {
                LogSyncingData();

                InitializeImportJob(fieldMap, options, jobStopManager, diagnosticLog);

                bool rowProcessed = false;
                if (jobStopManager?.ShouldDrainStop != true)
                {
                    _logger.LogInformation("Data processing loop is starting");
                    int addedRows = 0;
                    int skippedRows = 0;
                    foreach(var row in data)
                    {
                        try
                        {
                            Dictionary<string, object> importRow = GenerateImportRow(row, fieldMap, ImportSettings);
                            if (importRow != null)
                            {
                                ImportService.AddRow(importRow);
                                ++addedRows;
                            }
                            else
                                ++skippedRows;
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
                    }
                    _logger.LogInformation("Data processing loop ended. Rows added: {0}, rows skipped: {1}", addedRows, skippedRows);

                    if (!jobStopManager?.ShouldDrainStop ?? true)
                    {
                        ImportService.PushBatchIfFull(true);
                        rowProcessed = true;
                    }

                    WaitUntilTheJobIsDone(rowProcessed);
                    FinalizeSyncData(data, fieldMap, ImportSettings, jobStopManager);
                }
                else
                {
                    _logger.LogInformation("Skipping import because DrainStop was requested");
                }
            }
            catch (Exception ex)
            {
                throw LogAndCreateSyncDataException(ex, fieldMap, options);
            }
        }

        public void SyncData(
            IDataTransferContext context,
            IEnumerable<FieldMap> fieldMap,
            string options,
            IJobStopManager jobStopManager,
            IDiagnosticLog diagnosticLog)
        {
            try
            {
                LogSyncingData();

                InitializeImportJob(fieldMap, options, jobStopManager, diagnosticLog);

                FieldMap[] fieldMaps = fieldMap as FieldMap[] ?? fieldMap.ToArray();
                LogFieldMapLength(fieldMaps);
                if (fieldMaps.Length > 0)
                {
                    context.DataReader = new RelativityReaderDecorator(context.DataReader, fieldMaps);
                    ImportService.KickOffImport(context);
                }
                else
                {
                    ImportService.KickOffImport(context);
                }

                WaitUntilTheJobIsDone(true);
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
                LogDestinationWorkspaceAppendedToEmailBody();
            }
            return emailBody.ToString();
        }

        protected List<RelativityObject> GetRelativityFields(ImportSettings settings)
        {
            try
            {
                List<RelativityObject> fields = FieldQuery.GetFieldsForRdo(settings.ArtifactTypeId);
                HashSet<int> mappableArtifactIds = new HashSet<int>(GetImportApiFacade(settings)
                    .GetWorkspaceFieldsNames(settings.CaseArtifactId, settings.ArtifactTypeId)
                    .Keys);
                List<RelativityObject> mappableFields = fields.Where(x => mappableArtifactIds.Contains(x.ArtifactID)).ToList();
                LogNumbersOfFieldAndMappableFields(fields.Count, mappableFields.Count);
                return mappableFields;
            }
            catch (Exception e)
            {
                LogRetrievingRelativityFieldsError(e);
                throw;
            }
        }

        private IImportApiFacade GetImportApiFacade(ImportSettings settings)
        {
            return _factory.GetImportApiFacade(settings);
        }

        private IEnumerable<FieldEntry> GetFieldsInternal(string options)
        {
            ImportSettings settings = GetSettings(options);
            List<RelativityObject> fields = GetRelativityFields(settings);
            return ParseFields(fields);
        }

        private IEnumerable<FieldEntry> ParseFields(List<RelativityObject> fields)
        {
            foreach (RelativityObject field in fields)
            {
                if (IgnoredList.Contains(field.Name))
                {
                    continue;
                }

                bool isIdentifier = field.FixIdentifierField();
                if (isIdentifier)
                {
                    LogIdentifierFields(field);
                }

                FieldValuePair fieldType = field.FieldValues?.FirstOrDefault(x => x.Field.Name.Equals("Field Type"));
                string type = fieldType == null ? string.Empty : Convert.ToString(fieldType.Value);

                yield return new FieldEntry
                {
                    DisplayName = field.Name,
                    Type = type,
                    FieldIdentifier = field.ArtifactID.ToString(),
                    IsIdentifier = isIdentifier,
                    IsRequired = false
                };
            }
        }

        private void InitializeImportJob(IEnumerable<FieldMap> fieldMap, string options, IJobStopManager jobStopManager, IDiagnosticLog diagnosticLog)
        {
            LogInitializingImportJob();

            NativeFileImportService = new NativeFileImportService();

            ImportSettings = GetSyncDataImportSettings(fieldMap, options, NativeFileImportService);

            Dictionary<string, int> importFieldMap = GetSyncDataImportFieldMap(fieldMap, ImportSettings);

            ImportService = InitializeImportService(
                ImportSettings,
                importFieldMap,
                NativeFileImportService,
                jobStopManager,
                diagnosticLog);

            _isJobComplete = false;

            _logger.LogInformation("Initializing Import Job completed.");
        }

        protected virtual void WaitUntilTheJobIsDone(bool rowProcessed)
        {
            const int waitDuration = 1000;

            bool isJobDone;
            if (rowProcessed)
            {
                do
                {
                    lock (ImportService)
                    {
                        isJobDone = _isJobComplete;
                    }

                    _logger.LogInformation("Waiting until the job id done");
                    Thread.Sleep(waitDuration);
                } while (!isJobDone);
            }
        }

        protected internal virtual IImportService InitializeImportService(ImportSettings settings,
            Dictionary<string, int> importFieldMap, NativeFileImportService nativeFileImportService,
            IJobStopManager jobStopManager, IDiagnosticLog diagnosticLog)
        {
            LogInitializingImportService();

            ImportService importService = new ImportService(
                settings,
                importFieldMap,
                new BatchManager(),
                nativeFileImportService,
                _factory,
                _jobFactory,
                _helper,
                jobStopManager,
                diagnosticLog);

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

            _logger.LogInformation("Initialization Import Service...finished");

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

                var importSettingsForLogging = new ImportSettingsForLogging(settings);

                _logger.LogInformation("Rip Import Settings:\n {importSettings}", importSettingsForLogging);
                return settings;

            }
            catch (Exception ex)
            {
                throw LogAndCreateGetImportSettignsException(ex, fieldMap);
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
                SetupSettingsWhenImportingNatives(fieldMap, nativeFileImportService, settings, true);
            }
            else if (settings.ImportNativeFileCopyMode == ImportNativeFileCopyModeEnum.SetFileLinks)
            {
                SetupSettingsWhenImportingNatives(fieldMap, nativeFileImportService, settings, false);
            }
            else if (settings.ImportNativeFileCopyMode == ImportNativeFileCopyModeEnum.DoNotImportNativeFiles)
            {
                SetupSettingsWhenNotImportingNatives(nativeFileImportService, settings);
            }
        }

        private static void SetupSettingsWhenNotImportingNatives(NativeFileImportService nativeFileImportService, ImportSettings settings)
        {
            nativeFileImportService.ImportNativeFiles = false;
            settings.DisableNativeLocationValidation = null;
            settings.DisableNativeValidation = null;
            settings.CopyFilesToDocumentRepository = false;

            // NOTE :: Determines if we want to upload/delete native files and update "Has Native", "Supported by viewer" and "Relativity Native Type" fields
            settings.NativeFilePathSourceFieldName = string.Empty;
        }

        private void SetupSettingsWhenImportingNatives(IEnumerable<FieldMap> fieldMap, NativeFileImportService nativeFileImportService, ImportSettings settings, bool copyFilesToRepository)
        {
            nativeFileImportService.ImportNativeFiles = true;
            FieldMap field = fieldMap.FirstOrDefault(x => x.FieldMapType == FieldMapTypeEnum.NativeFilePath);
            nativeFileImportService.SourceFieldName = field != null ? field.SourceField.FieldIdentifier : Constants.SPECIAL_NATIVE_FILE_LOCATION_FIELD;
            settings.NativeFilePathSourceFieldName = nativeFileImportService.DestinationFieldName;
            settings.DisableNativeLocationValidation = DisableNativeLocationValidation;
            settings.DisableNativeValidation = DisableNativeValidation;
            settings.CopyFilesToDocumentRepository = copyFilesToRepository;
            settings.OIFileIdMapped = true;
            settings.OIFileTypeColumnName = Constants.SPECIAL_FILE_TYPE_FIELD_NAME;
            settings.SupportedByViewerColumn = Constants.SPECIAL_FILE_SUPPORTED_BY_VIEWER_FIELD_NAME;
            settings.FileSizeMapped = true;
            settings.FileSizeColumn = Constants.SPECIAL_NATIVE_FILE_SIZE_FIELD_NAME;

            // NOTE :: So that the destination workspace file icons correctly display, we give the import API the file name of the document
            settings.FileNameColumn = Constants.SPECIAL_FILE_NAME_FIELD_NAME;
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

        protected virtual void FinalizeSyncData(IEnumerable<IDictionary<FieldEntry, object>> data,
            IEnumerable<FieldMap> fieldMap, ImportSettings settings, IJobStopManager jobStopManager)
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
            lock (ImportService)
            {
                LogSettingJobCompleteInFinish();
                _isJobComplete = true;
            }
        }

        private void JobError(Exception ex)
        {
            LogLockingImportServiceInJobError();
            lock (ImportService)
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
                Dictionary<int, string> workspaces = GetImportApiFacade(settings).GetWorkspaceNames();
                if (workspaces.ContainsKey(settings.CaseArtifactId))
                {
                    LogNullWorkspaceReturnedByIAPI();
                    workspaceRef = new WorkspaceRef { Id = settings.CaseArtifactId, Name = workspaces[settings.CaseArtifactId] };
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

        private IntegrationPointsException LogAndCreateGetImportSettignsException(Exception exception, IEnumerable<FieldMap> fieldMap)
        {
            string message = "Error occured while preparing import settings.";
            IEnumerable<FieldMap> fieldMapWithoutFieldNames = CreateFieldMapWithoutFieldNames(fieldMap);
            _logger.LogError("Error occured while preparing import settings\nFields: {@fieldMap}", fieldMapWithoutFieldNames);
            return new IntegrationPointsException(message, exception) { ShouldAddToErrorsTab = true };
        }

        private IntegrationPointsException LogAndCreateGetFieldsException(Exception exception)
        {
            string message = "Error occured while getting fields.";
            _logger.LogError("Error getting fields.");
            return new IntegrationPointsException(message, exception) { ShouldAddToErrorsTab = true };
        }

        private IntegrationPointsException LogAndCreateSyncDataException(Exception ex, IEnumerable<FieldMap> fieldMap, string options)
        {
            string message = "Error occured while syncing rdo.";
            IEnumerable<FieldMap> fieldMapWithoutFieldNames = CreateFieldMapWithoutFieldNames(fieldMap);
            _logger.LogError("Error occured while syncing rdo. \nOptions: {@options} \nFields: {@fieldMap}", options, fieldMapWithoutFieldNames);
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
            _logger.LogInformation("Preparing import process in synchronizer...");
        }

        private void LogRetrievingEmailBody()
        {
            _logger.LogInformation("Retrieving email body.");
        }

        private void LogRetrievingRelativityFieldsError(Exception e)
        {
            _logger.LogError(e, "Failed to retrieve Relativity fields.");
        }

        private void LogInitializingImportJob()
        {
            _logger.LogInformation("Initializing Import Job.");
        }

        private void LogInitializingImportService()
        {
            _logger.LogInformation("Start initializing Import Service...");
        }

        private void LogMissingIdentifierField()
        {
            _logger.LogError("Source Provider requires the identifier field to be mapped with another identifier field.");
        }

        private void LogRetrievingImportSettings()
        {
            _logger.LogInformation("Starting RIP Import Settings creation...");
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
            _logger.LogInformation("New value of DisableNativeLocationValidation retrieved from config: {value}", _disableNativeLocationValidation);
        }

        private void LogNewWebAPIPathValue()
        {
            _logger.LogInformation("New value of WebAPIPath retrieved from config: {value}", _webApiPath);
        }

        private void LogNewDisableNativeValidationValue()
        {
            _logger.LogInformation("New value of DisableNativeValidation retrieved from config: {value}", _disableNativeValidation);
        }

        private void LogFieldMapLength(FieldMap[] fieldMaps)
        {
            _logger.LogInformation("Number of items in fieldMap: {fieldMapLength}", fieldMaps.Length);
        }

        private void LogDestinationWorkspaceAppendedToEmailBody()
        {
            _logger.LogInformation("Adding destination workspace to email body.");
        }

        private void LogNumbersOfFieldAndMappableFields(int fields, int mappableFields)
        {
            _logger.LogInformation("Retrieved {numberOfFields} fields, {numberOfMappableFields} are mappable", fields, mappableFields);
        }

        private void LogCreatingImportApi()
        {
            _logger.LogInformation("ImportApi was null - new instance will be created using factory");
        }

        private void LogNullWorkspaceReturnedByIAPI()
        {
            _logger.LogInformation("ImportApi returned null workspace - creating new WorkspaceRef in GetWorkspace method");
        }

        private void LogSettingJobCompleteInFinish()
        {
            _logger.LogInformation("_importService locked in Finish method of RdoSynchronizer. Job is complete");
        }

        private void LogLockingImportServiceInFinish()
        {
            _logger.LogInformation("Trying to lock _importService in Finish method of RdoSynchronizer");
        }

        private void LogSettingJobCompleteInJobError()
        {
            _logger.LogInformation("_importService locked in JobError method of RdoSynchronizer. Job is complete");
        }

        private void LogLockingImportServiceInJobError()
        {
            _logger.LogInformation("Trying to lock _importService in JobError method of RdoSynchronizer");
        }

        private void LogIdentifierFields(RelativityObject field)
        {
            _logger.LogInformation("Identifier field: {identifierFieldId}", field.ArtifactID);
        }

        private IEnumerable<FieldMap> CreateFieldMapWithoutFieldNames(IEnumerable<FieldMap> fieldMap)
        {
            IEnumerable<FieldMap> fieldMapWithoutFieldNames = fieldMap.Select(fm => new FieldMap
            {
                DestinationField = CreateFieldEntryWithoutName(fm.DestinationField),
                SourceField = CreateFieldEntryWithoutName(fm.SourceField),
                FieldMapType = fm.FieldMapType
            });
            return fieldMapWithoutFieldNames;
        }

        private FieldEntry CreateFieldEntryWithoutName(FieldEntry entry)
        {
            var newEntry = new FieldEntry
            {
                FieldIdentifier = entry.FieldIdentifier,
                FieldType = entry.FieldType,
                DisplayName = Constants.SENSITIVE_DATA_REMOVED_FOR_LOGGING
            };
            return newEntry;
        }

        #endregion
    }
}

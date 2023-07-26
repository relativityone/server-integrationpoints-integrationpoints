using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Readers;
using kCura.IntegrationPoints.Domain.Utils;
using kCura.IntegrationPoints.Synchronizers.RDO.ImportAPI;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using Microsoft.VisualBasic.FileIO;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Internals;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Objects.DataContracts;
using Constants = kCura.IntegrationPoints.Domain.Constants;

namespace kCura.IntegrationPoints.Synchronizers.RDO
{
    public class RdoSynchronizer : IDataSynchronizer, IBatchReporter, IEmailBodyData
    {
        private readonly IAPILog _logger;
        private readonly IHelper _helper;
        private readonly IDiagnosticLog _diagnosticLog;
        private readonly IImportApiFactory _factory;
        private readonly IImportJobFactory _jobFactory;
        private readonly IRelativityFieldQuery _fieldQuery;
        private bool _isJobComplete;
        private bool? _disableNativeLocationValidation;
        private bool? _disableNativeValidation;
        private HashSet<string> _ignoredList;

        protected ISerializer Serializer { get; }

        protected IImportService ImportService { get; private set; }

        public RdoSynchronizer(IRelativityFieldQuery fieldQuery, IImportApiFactory factory, IImportJobFactory jobFactory, IHelper helper, IDiagnosticLog diagnosticLog, ISerializer serializer)
        {
            _fieldQuery = fieldQuery;
            _factory = factory;
            _jobFactory = jobFactory;
            _helper = helper;
            _diagnosticLog = diagnosticLog;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<RdoSynchronizer>();
            Serializer = serializer;
        }

        public event BatchCompleted OnBatchComplete;

        public event BatchSubmitted OnBatchSubmit;

        public event BatchCreated OnBatchCreate;

        public event StatusUpdate OnStatusUpdate;

        public event StatisticsUpdate OnStatisticsUpdate;

        public event JobError OnJobError;

        public event RowError OnDocumentError;

        public Data.SourceProvider SourceProvider { get; set; }

        public int TotalRowsProcessed => ImportService?.TotalRowsProcessed ?? 0;

        protected bool? DisableNativeLocationValidation
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

            set => _disableNativeLocationValidation = value;
        }

        protected bool? DisableNativeValidation
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

            set => _disableNativeValidation = value;
        }

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

                var destinationConfiguration = Serializer.Deserialize<DestinationConfiguration>(providerConfiguration.Configuration);
                FieldEntry[] fields = GetFieldsInternal(destinationConfiguration).Where(f => !ignoreFields.Contains(f.ActualName)).Select(f => f).ToArray();

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

        public void SyncData(
            IEnumerable<IDictionary<FieldEntry, object>> data,
            IEnumerable<FieldMap> fieldMap,
            ImportSettings options,
            IJobStopManager jobStopManager,
            IDiagnosticLog diagnosticLog)
        {
            try
            {
                LogSyncingData();
                InitializeImportJob(fieldMap, options, jobStopManager, diagnosticLog);

                bool rowProcessed = false;
                if (jobStopManager?.ShouldDrainStop == true)
                {
                    _logger.LogInformation("Skipping import because DrainStop was requested");
                    return;
                }

                _logger.LogInformation("Data processing loop is starting");
                int addedRows = 0;
                int skippedRows = 0;
                _diagnosticLog.LogDiagnostic("Collection typeOf: {dataType}", data.GetType());
                try
                {
                    foreach (var row in data)
                    {
                        _diagnosticLog.LogDiagnostic("In for each loop: {addedRows}, {skippedRows}, {totalRows}", addedRows, skippedRows, addedRows + skippedRows);
                        try
                        {
                            Dictionary<string, object> importRow = GenerateImportRow(row, fieldMap, ImportSettings);
                            if (importRow != null)
                            {
                                ImportService.AddRow(importRow);
                                ++addedRows;
                            }
                            else
                            {
                                ++skippedRows;
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
                    }
                }
                catch (MalformedLineException ex)
                {
                    LogSyncDataError(ex);
                    ItemError(string.Empty, ex.Message);
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
            catch (Exception ex)
            {
                throw LogAndCreateSyncDataException(ex, fieldMap, options);
            }
        }

        public void SyncData(
            IDataTransferContext context,
            IEnumerable<FieldMap> fieldMap,
            ImportSettings options,
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

            DestinationConfiguration destinationConfiguration = Serializer.Deserialize<DestinationConfiguration>(options);
            WorkspaceRef destinationWorkspace = GetWorkspace(destinationConfiguration);

            var emailBody = new StringBuilder();
            if (destinationWorkspace != null)
            {
                emailBody.AppendLine(string.Empty);
                string destinationWorkspaceAsString = WorkspaceAndJobNameUtils.GetFormatForWorkspaceOrJobDisplay(destinationWorkspace.Name, destinationWorkspace.Id);
                emailBody.AppendFormat("Destination Workspace: {0}", destinationWorkspaceAsString);
                LogDestinationWorkspaceAppendedToEmailBody();
            }

            return emailBody.ToString();
        }

        protected List<RelativityObject> GetRelativityFields(DestinationConfiguration destinationConfiguration)
        {
            try
            {
                List<RelativityObject> fields = _fieldQuery.GetFieldsForRdo(destinationConfiguration.ArtifactTypeId);
                HashSet<int> mappableArtifactIds = new HashSet<int>(_factory.GetImportApiFacade()
                    .GetWorkspaceFieldsNames(destinationConfiguration.CaseArtifactId, destinationConfiguration.ArtifactTypeId)
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

        protected void RaiseDocumentErrorEvent(string documentIdentifier, string errorMessage)
        {
            OnDocumentError?.Invoke(documentIdentifier, errorMessage);
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
                }
                while (!isJobDone);
            }
        }

        private IImportService InitializeImportService(
            ImportSettings settings,
            Dictionary<string, int> importFieldMap,
            NativeFileImportService nativeFileImportService,
            IJobStopManager jobStopManager,
            IDiagnosticLog diagnosticLog)
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

        protected virtual ImportSettings GetSyncDataImportSettings(IEnumerable<FieldMap> fieldMap, ImportSettings options, NativeFileImportService nativeFileImportService)
        {
            try
            {
                LogRetrievingImportSettings();

                BootstrapParentObjectSettings(fieldMap, options);
                BootstrapIdentityFieldSettings(fieldMap, options);
                BootstrapImportNativesSettings(fieldMap, nativeFileImportService, options);
                BootstrapFolderSettings(fieldMap, options);
                BootstrapDestinationIdentityFieldSettings(fieldMap, options);

                return options;
            }
            catch (Exception ex)
            {
                throw LogAndCreateGetImportSettingsException(ex, fieldMap);
            }
        }

        protected virtual Dictionary<string, int> GetSyncDataImportFieldMap(IEnumerable<FieldMap> fieldMap, DestinationConfiguration destinationConfiguration)
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

        protected virtual void FinalizeSyncData(
            IEnumerable<IDictionary<FieldEntry, object>> data,
            IEnumerable<FieldMap> fieldMap,
            ImportSettings settings,
            IJobStopManager jobStopManager)
        {
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

        protected virtual WorkspaceRef GetWorkspace(DestinationConfiguration destinationConfiguration)
        {
            try
            {
                WorkspaceRef workspaceRef = null;
                Dictionary<int, string> workspaces = _factory.GetImportApiFacade().GetWorkspaceNames();
                if (workspaces.ContainsKey(destinationConfiguration.CaseArtifactId))
                {
                    LogNullWorkspaceReturnedByIAPI();
                    workspaceRef = new WorkspaceRef { Id = destinationConfiguration.CaseArtifactId, Name = workspaces[destinationConfiguration.CaseArtifactId] };
                }

                return workspaceRef;
            }
            catch (Exception e)
            {
                LogRetrievingWorkspaceError(e);
                throw;
            }
        }

        private void RaiseJobErrorEvent(Exception exception)
        {
            OnJobError?.Invoke(exception);
        }

        private IEnumerable<FieldEntry> GetFieldsInternal(DestinationConfiguration options)
        {
            List<RelativityObject> fields = GetRelativityFields(options);
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

        private void InitializeImportJob(IEnumerable<FieldMap> fieldMap, ImportSettings options, IJobStopManager jobStopManager, IDiagnosticLog diagnosticLog)
        {
            LogInitializingImportJob();

            NativeFileImportService = new NativeFileImportService();

            ImportSettings = GetSyncDataImportSettings(fieldMap, options, NativeFileImportService);

            Dictionary<string, int> importFieldMap = GetSyncDataImportFieldMap(fieldMap, ImportSettings.DestinationConfiguration);

            ImportService = InitializeImportService(
                ImportSettings,
                importFieldMap,
                NativeFileImportService,
                jobStopManager,
                diagnosticLog);

            _isJobComplete = false;

            _logger.LogInformation("Initializing Import Job completed.");
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
            if ((settings.DestinationConfiguration.IdentityFieldId < 1) && fieldMap.Any(x => x.FieldMapType == FieldMapTypeEnum.Identifier))
            {
                settings.DestinationConfiguration.IdentityFieldId =
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

            if (settings.DestinationConfiguration.UseDynamicFolderPath)
            {
                settings.FolderPathSourceFieldName = Constants.SPECIAL_FOLDERPATH_DYNAMIC_FIELD_NAME;
            }
        }

        private void BootstrapImportNativesSettings(IEnumerable<FieldMap> fieldMap, NativeFileImportService nativeFileImportService, ImportSettings settings)
        {
            if (settings.DestinationConfiguration.ImportNativeFile && settings.DestinationConfiguration.ImportNativeFileCopyMode == ImportNativeFileCopyModeEnum.CopyFiles)
            {
                SetupSettingsWhenImportingNatives(fieldMap, nativeFileImportService, settings, true);
            }
            else if (settings.DestinationConfiguration.ImportNativeFileCopyMode == ImportNativeFileCopyModeEnum.SetFileLinks)
            {
                SetupSettingsWhenImportingNatives(fieldMap, nativeFileImportService, settings, false);
            }
            else if (settings.DestinationConfiguration.ImportNativeFileCopyMode == ImportNativeFileCopyModeEnum.DoNotImportNativeFiles)
            {
                SetupSettingsWhenNotImportingNatives(nativeFileImportService, settings);
            }
        }

        private void SetupSettingsWhenNotImportingNatives(NativeFileImportService nativeFileImportService, ImportSettings settings)
        {
            nativeFileImportService.ImportNativeFiles = false;
            settings.DisableNativeLocationValidation = null;
            settings.DisableNativeValidation = null;
            settings.DestinationConfiguration.CopyFilesToDocumentRepository = false;

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
            settings.DestinationConfiguration.CopyFilesToDocumentRepository = copyFilesToRepository;
            settings.OIFileIdMapped = true;
            settings.OIFileTypeColumnName = Constants.SPECIAL_FILE_TYPE_FIELD_NAME;
            settings.OIFileIdColumnName = Constants.SPECIAL_OI_FILE_TYPE_ID_FIELD_NAME;
            settings.SupportedByViewerColumn = Constants.SPECIAL_FILE_SUPPORTED_BY_VIEWER_FIELD_NAME;
            settings.FileSizeMapped = true;
            settings.FileSizeColumn = Constants.SPECIAL_NATIVE_FILE_SIZE_FIELD_NAME;

            // NOTE :: So that the destination workspace file icons correctly display, we give the import API the file name of the document
            settings.FileNameColumn = Constants.SPECIAL_FILE_NAME_FIELD_NAME;
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

        #region Logging

        private IntegrationPointsException LogAndCreateGetImportSettingsException(Exception exception, IEnumerable<FieldMap> fieldMap)
        {
            string message = "Error occurred while preparing import settings.";
            IEnumerable<FieldMap> fieldMapWithoutFieldNames = CreateFieldMapWithoutFieldNames(fieldMap);
            _logger.LogError("Error occurred while preparing import settings\nFields: {@fieldMap}", fieldMapWithoutFieldNames);
            return new IntegrationPointsException(message, exception) { ShouldAddToErrorsTab = true };
        }

        private IntegrationPointsException LogAndCreateGetFieldsException(Exception exception)
        {
            string message = "Error occurred while getting fields.";
            _logger.LogError("Error getting fields.");
            return new IntegrationPointsException(message, exception) { ShouldAddToErrorsTab = true };
        }

        private IntegrationPointsException LogAndCreateSyncDataException(Exception ex, IEnumerable<FieldMap> fieldMap, ImportSettings options)
        {
            string message = "Error occurred while syncing rdo.";
            IEnumerable<FieldMap> fieldMapWithoutFieldNames = CreateFieldMapWithoutFieldNames(fieldMap);
            _logger.LogError("Error occurred while syncing rdo. \nOptions: {@options} \nFields: {@fieldMap}", options, fieldMapWithoutFieldNames);
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

        private void LogNewDisableNativeLocationValidationValue()
        {
            _logger.LogInformation("New value of DisableNativeLocationValidation retrieved from config: {value}", _disableNativeLocationValidation);
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
            _logger.LogInformation("ImportService locked in Finish method of RdoSynchronizer. Job is complete");
        }

        private void LogLockingImportServiceInFinish()
        {
            _logger.LogInformation("Trying to lock ImportService in Finish method of RdoSynchronizer");
        }

        private void LogSettingJobCompleteInJobError()
        {
            _logger.LogInformation("ImportService locked in JobError method of RdoSynchronizer. Job is complete");
        }

        private void LogLockingImportServiceInJobError()
        {
            _logger.LogInformation("Trying to lock ImportService in JobError method of RdoSynchronizer");
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

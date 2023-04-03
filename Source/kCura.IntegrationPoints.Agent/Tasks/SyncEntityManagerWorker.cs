using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Extensions;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Conversion;
using kCura.IntegrationPoints.Core.Services.EntityManager;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Logging;
using kCura.IntegrationPoints.Domain.Managers;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json.Linq;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Objects.DataContracts;
using Constants = kCura.IntegrationPoints.Core.Constants;
using ObjectTypeGuids = kCura.IntegrationPoints.Core.Contracts.Entity.ObjectTypeGuids;

namespace kCura.IntegrationPoints.Agent.Tasks
{
    public class SyncEntityManagerWorker : SyncWorker
    {
        private readonly IAPILog _logger;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IRelativityObjectManager _relativityObjectManager;
        private readonly IQueueQueryManager _queueQueryManager;
        private IEnumerable<FieldMap> _entityManagerFieldMap;
        private List<EntityManagerMap> _entityManagerMap;
        private bool _managerFieldIdIsBinary;
        private IEnumerable<FieldMap> _managerFieldMap;
        private string _newKeyManagerFieldID;
        private string _oldKeyManagerFieldID;

        public SyncEntityManagerWorker(
            ICaseServiceContext caseServiceContext,
            IDataProviderFactory dataProviderFactory,
            IHelper helper,
            ISerializer serializer,
            ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
            IJobHistoryService jobHistoryService,
            IJobHistoryErrorService jobHistoryErrorService,
            IJobManager jobManager,
            IQueueQueryManager queueQueryManager,
            IJobStatisticsService statisticsService,
            IManagerFactory managerFactory,
            IJobService jobService,
            IRepositoryFactory repositoryFactory,
            IRelativityObjectManager relativityObjectManager,
            IProviderTypeService providerTypeService,
            IIntegrationPointService integrationPointService,
            IDiagnosticLog diagnosticLog)
            : base(
                caseServiceContext,
                helper,
                dataProviderFactory,
                serializer,
                appDomainRdoSynchronizerFactoryFactory,
                jobHistoryService,
                jobHistoryErrorService,
                jobManager,
                null,
                statisticsService,
                managerFactory,
                jobService,
                providerTypeService,
                integrationPointService,
                diagnosticLog)
        {
            _queueQueryManager = queueQueryManager;
            _repositoryFactory = repositoryFactory;
            _relativityObjectManager = relativityObjectManager;
            _logger = helper.GetLoggerFactory().GetLogger().ForContext<SyncEntityManagerWorker>();
        }

        protected override void ExecuteTask(Job job)
        {
            try
            {
                EntityManagerJobParameters jobParameters = GetParameters(job);

                LogExecuteTaskStart(job);

                SetIntegrationPoint(job);
                SetJobHistory();

                ConfigureJobStopManager(job, true);

                bool isPrimaryBatchWorkComplete = _queueQueryManager
                    .CheckAllSyncWorkerBatchesAreFinished(
                        job.RootJobId ?? job.JobId)
                    .Execute();
                if (!isPrimaryBatchWorkComplete)
                {
                    LogOtherSyncWorkerBatchesAreInProgress(job, BatchInstance);

                    new TaskJobSubmitter(JobManager, JobService, job, TaskType.SyncEntityManagerWorker, BatchInstance).SubmitJob(jobParameters);
                    return;
                }

                IDictionary<string, string> managersLookup = new Dictionary<string, string>();

                if (SourceProvider.Identifier.Equals(Constants.IntegrationPoints.SourceProviders.LDAP, StringComparison.InvariantCultureIgnoreCase))
                {
                    // if no links to process - exit
                    if (!_entityManagerMap.Any())
                    {
                        return;
                    }

                    IDataSourceProvider sourceProvider = GetSourceProvider(SourceProvider, job);
                    List<FieldEntry> sourceFields = GetSourceFields(IntegrationPointDto.FieldMappings);
                    IList<string> managersLdapQueryStrings = GetManagersLdapQueryStrings();
                    IEnumerable<IDictionary<string, object>> managersData = ReadManagersData(sourceProvider, sourceFields,
                        managersLdapQueryStrings);

                    string identifierFieldName = GetSourceFieldIdentifierFieldName(IntegrationPointDto.FieldMappings);
                    string managerIdentifiedFieldName = GetDestinationFieldIdentifierName(jobParameters);

                    managersLookup = CreateManagersLookup(managersData, managerIdentifiedFieldName, identifierFieldName);
                    _entityManagerMap.ForEach(x => x.NewManagerID = managersLookup[x.OldManagerID]);
                }
                else
                {
                    // if no links to process - exit
                    if (!_entityManagerMap.Any())
                    {
                        return;
                    }

                    _entityManagerMap.ForEach(x => x.NewManagerID = x.OldManagerID);
                }

                string[] managerUniqueIDs = _entityManagerMap
                    .Where(x => x.NewManagerID != null)
                    .Select(x => x.NewManagerID)
                    .Distinct()
                    .ToArray();

                IDictionary<string, int> managerArtifactIDs = GetImportedManagerArtifactIDs(managerUniqueIDs);
                _entityManagerMap.ForEach(x => x.ManagerArtifactID =
                            x.NewManagerID != null && managerArtifactIDs.ContainsKey(x.NewManagerID) ? managerArtifactIDs[x.NewManagerID] : 0);

                // change import api settings to be able to overlay and set Entity/Manager links
                int entityManagerFieldArtifactID = GetEntityManagerFieldArtifactID(job.WorkspaceID);
                ImportSettings importSettings = ReconfigureImportAPISettings(entityManagerFieldArtifactID);

                // run import api to link corresponding Managers to Entity
                FieldEntry fieldEntryEntityIdentifier = _managerFieldMap.First(x => x.FieldMapType.Equals(FieldMapTypeEnum.Identifier)).SourceField;
                FieldEntry fieldEntryManagerIdentifier = _managerFieldMap.First(
                    x => x.DestinationField.FieldIdentifier.Equals(entityManagerFieldArtifactID.ToString())).SourceField;

                IEnumerable<IDictionary<FieldEntry, object>> sourceData = _entityManagerMap.Where(x => x.ManagerArtifactID != 0)
                    .Select(x => new Dictionary<FieldEntry, object>
                    {
                        { fieldEntryEntityIdentifier, x.EntityID },
                        { fieldEntryManagerIdentifier, x.ManagerArtifactID },
                    });

                IEnumerable<FieldMap> managerLinkMap = _managerFieldMap.Where(x =>
                    x.SourceField.FieldIdentifier.Equals(fieldEntryEntityIdentifier.FieldIdentifier) ||
                    x.SourceField.FieldIdentifier.Equals(fieldEntryManagerIdentifier.FieldIdentifier));

                int totalLinkedManagers = LinkManagers(job, importSettings, sourceData, managerLinkMap, JobStopManager);
                AddMissingManagersErrors(managersLookup, managerArtifactIDs);
                LogExecuteTaskSuccessfulEnd(job);

                HandleJobIfSomeManagersWerentProcessed(job, totalLinkedManagers);
            }
            catch (Exception ex)
            {
                LogExecutingTaskError(job, ex);
                JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);

                if (ex is IntegrationPointsException) // we want to rethrow, so it can be added to error tab if necessary
                {
                    throw;
                }
            }
            finally
            {
                // rdo last run and next scheduled time will be updated in Manager job
                JobHistoryErrorService.CommitErrors();
                PostExecute(job);
                LogExecuteTaskFinalize(job);
            }
        }

        private void HandleJobIfSomeManagersWerentProcessed(Job job, int totalLinkedManagers)
        {
            EntityManagerJobParameters jobParameters = GetParameters(job);
            if (totalLinkedManagers < jobParameters.EntityManagerMap.Count)
            {
                jobParameters.EntityManagerMap = jobParameters.EntityManagerMap.Skip(totalLinkedManagers).ToDictionary(x => x.Key, x => x.Value);
                TaskParameters taskParameters = new TaskParameters()
                {
                    BatchInstance = BatchInstance,
                    BatchParameters = jobParameters,
                };

                job.JobDetails = Serializer.Serialize(taskParameters);
                JobService.UpdateJobDetails(job);
            }
            else
            {
                JobStopManager.StopCheckingDrainStopAndUpdateStopState(job, false); // All Entity Managers were linked
            }
        }

        private EntityManagerJobParameters GetParameters(Job job)
        {
            TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
            BatchInstance = taskParameters.BatchInstance;

            EntityManagerJobParameters jobParameters;
            if (taskParameters.BatchParameters is JObject)
            {
                jobParameters = ((JObject)taskParameters.BatchParameters).ToObject<EntityManagerJobParameters>();
            }
            else
            {
                jobParameters = (EntityManagerJobParameters)taskParameters.BatchParameters;
            }

            _entityManagerMap = jobParameters.EntityManagerMap.Select(
                x => new EntityManagerMap
                {
                    EntityID = x.Key,
                    OldManagerID = x.Value,
                }).ToList();

            _entityManagerFieldMap = jobParameters.EntityManagerFieldMap;
            _managerFieldMap = jobParameters.ManagerFieldMap;
            _managerFieldIdIsBinary = jobParameters.ManagerFieldIdIsBinary;

            SetManagerFieldIDs(_entityManagerFieldMap, _managerFieldMap);

            return jobParameters;
        }

        private void SetManagerFieldIDs(IEnumerable<FieldMap> entityManagerFieldMap, IEnumerable<FieldMap> managerFieldMap)
        {
            _oldKeyManagerFieldID = entityManagerFieldMap.Where(x => x.FieldMapType.Equals(FieldMapTypeEnum.Identifier))
                    .Select(x => x.DestinationField.FieldIdentifier)
                    .First();
            _newKeyManagerFieldID = managerFieldMap.Where(x => x.FieldMapType.Equals(FieldMapTypeEnum.Identifier))
                    .Select(x => x.SourceField.FieldIdentifier)
                    .First();
        }

        private string GetDestinationManagerUniqueIdFieldDisplayName()
        {
            return _managerFieldMap.First(
                            x => x.SourceField.FieldIdentifier.Equals(_newKeyManagerFieldID, StringComparison.InvariantCultureIgnoreCase))
                        .DestinationField.DisplayName;
        }

        protected override List<FieldEntry> GetSourceFields(IEnumerable<FieldMap> fieldMap)
        {
            List<FieldEntry> sourceFields = base.GetSourceFields(fieldMap);
            if (!sourceFields.Any(f => f.FieldIdentifier.Equals(_oldKeyManagerFieldID, StringComparison.InvariantCultureIgnoreCase)))
            {
                sourceFields.Add(new FieldEntry { FieldIdentifier = _oldKeyManagerFieldID });
            }

            sourceFields.ForEach(f => f.IsIdentifier = f.FieldIdentifier.Equals(_oldKeyManagerFieldID, StringComparison.InvariantCultureIgnoreCase));
            return sourceFields;
        }

        private List<string> GetManagersLdapQueryStrings()
        {
            return _entityManagerMap.Select(x => ConvertObjectGuid(x.OldManagerID)).Distinct().ToList();
        }

        private string ConvertObjectGuid(string originalID)
        {
            if (!_managerFieldIdIsBinary)
            {
                return originalID;
            }

            string newID = string.Empty;
            for (int i = 0; i < originalID.Length; i = i + 2)
            {
                newID = string.Format("{0}\\{1}", newID, originalID.Substring(i, 2));
            }
            return newID;
        }

        private IEnumerable<IDictionary<string, object>> ReadManagersData(IDataSourceProvider sourceProvider,
            List<FieldEntry> sourceFields, IList<string> managersLdapQueryStrings)
        {
            using (IDataReader sourceDataReader = sourceProvider.GetData(sourceFields, managersLdapQueryStrings,
                new DataSourceProviderConfiguration(IntegrationPointDto.SourceConfiguration, IntegrationPointDto.SecuredConfiguration)))
            {
                IEnumerable<IDictionary<FieldEntry, object>> sourceData = GetSourceData(sourceFields, sourceDataReader).ToList();
                return sourceData.Select(x => x.ToDictionary(y => y.Key.FieldIdentifier, y => y.Value));
            }
        }

        protected override IEnumerable<IDictionary<FieldEntry, object>> GetSourceData(List<FieldEntry> sourceFields,
            IDataReader sourceDataReader)
        {
            return GetEntityManagerDataReaderToEnumerableService(sourceFields).GetData<IDictionary<FieldEntry, object>>(sourceDataReader);
        }

        private EntityManagerDataReaderToEnumerableService GetEntityManagerDataReaderToEnumerableService(
            List<FieldEntry> sourceFields)
        {
            var objectBuilder = new SynchronizerObjectBuilder(sourceFields, DiagnosticLog);
            EntityManagerDataReaderToEnumerableService convertDataService =
                new EntityManagerDataReaderToEnumerableService(objectBuilder, _oldKeyManagerFieldID, _newKeyManagerFieldID);
            return convertDataService;
        }

        private string GetSourceFieldIdentifierFieldName(IEnumerable<FieldMap> fieldMap)
        {
            return fieldMap.Where(x => x.FieldMapType == FieldMapTypeEnum.Identifier).Select(x => x.SourceField.ActualName).First();
        }

        private string GetDestinationFieldIdentifierName(EntityManagerJobParameters jobParameters)
        {
            return jobParameters.EntityManagerFieldMap.Where(x => x.FieldMapType == FieldMapTypeEnum.Identifier)
                .Select(x => x.DestinationField.FieldIdentifier)
                .First();
        }

        private Dictionary<string, string> CreateManagersLookup(IEnumerable<IDictionary<string, object>> managersData,
            string managerIdentifiedFieldName, string identifierFieldName)
        {
            return managersData.ToDictionary(x => (string)x[managerIdentifiedFieldName], x => (string)x[identifierFieldName]);
        }

        private IDictionary<string, int> GetImportedManagerArtifactIDs(string[] managerUniqueIDs)
        {
            LogGetImportedManagerArtifactIDsStart();

            string uniqueFieldName = GetDestinationManagerUniqueIdFieldDisplayName();

            string ids = string.Join(",", managerUniqueIDs.Select(x => $"'{x}'"));

            var queryRequest = new QueryRequest
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = ObjectTypeGuids.Entity
                },
                Fields = new[]
                {
                    new FieldRef
                    {
                        Name = uniqueFieldName
                    },
                },
                Condition = $"'{uniqueFieldName}' IN [{ids}]",
            };

            List<RelativityObject> result;
            try
            {
                result = _relativityObjectManager.Query(queryRequest);
            }
            catch (AggregateException e)
            {
                LogRetrievingManagerArtifactIds(e.InnerExceptions.Select(x => x.Message));
                throw;
            }

            IEnumerable<RelativityObject> deduplicatedResults = HandleDuplicatesInResult(result, uniqueFieldName);

            IDictionary<string, int> managerIDs =
                deduplicatedResults.ToDictionary(r => r.FieldValues.First(f => f.Field.Name == uniqueFieldName).Value?.ToString(), r => r.ArtifactID);

            LogGetImportedManagerArtifactIDsSuccessfulEnd(managerIDs);
            return managerIDs;
        }

        private IEnumerable<RelativityObject> HandleDuplicatesInResult(List<RelativityObject> result, string uniqueFieldName)
        {
            IEnumerable<IGrouping<string, RelativityObject>> duplicates = result
                .GroupBy(x => x.FieldValues.First(f => f.Field.Name == uniqueFieldName).Value?.ToString());

            if (duplicates.Any())
            {
                IEnumerable<RelativityObject> itemLevelErrors = duplicates.SelectMany(x => x.Skip(1));

                foreach (RelativityObject itemLevelError in itemLevelErrors)
                {
                    JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorItem, itemLevelError.ArtifactID.ToString(),
                    $"Duplicated entity found for: {itemLevelError.FieldValues.First().Value} with the following ArtifactID: {itemLevelError.ArtifactID}",
                    string.Empty);
                }
            }

            IEnumerable<RelativityObject> deduplicatedResults = duplicates.Select(x => x.First());

            return deduplicatedResults;
        }

        private int GetEntityManagerFieldArtifactID(int workspaceArtifactId)
        {
            try
            {
                LogGetEntityManagerFieldArtifactIdStart();
                IFieldQueryRepository fieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(workspaceArtifactId);

                int artifactID = fieldQueryRepository.ReadArtifactID(new Guid(EntityFieldGuids.Manager));
                LogGetEntityManagerFieldArtifactIdSuccessfulEnd(artifactID);
                return artifactID;
            }
            catch (Exception ex)
            {
                LogRetrievingEntityManagersIdsError(ex);
                throw;
            }
        }

        private ImportSettings ReconfigureImportAPISettings(int entityManagerFieldArtifactID)
        {
            LogReconfigureImportApiSettingsStart(entityManagerFieldArtifactID);
            ImportSettings importSettings = new ImportSettings(IntegrationPointDto.DestinationConfiguration);
            importSettings.ObjectFieldIdListContainsArtifactId = new[] { entityManagerFieldArtifactID };
            importSettings.DestinationConfiguration.ImportOverwriteMode = ImportOverwriteModeEnum.OverlayOnly;
            importSettings.DestinationConfiguration.EntityManagerFieldContainsLink = false;
            return importSettings;
        }

        private int LinkManagers(
            Job job,
            ImportSettings importSettings,
            IEnumerable<IDictionary<FieldEntry, object>> sourceData,
            IEnumerable<FieldMap> managerLinkMap,
            IJobStopManager jobStopManager)
        {
            IDataSynchronizer dataSynchronizer = GetDestinationProvider(DestinationProvider, importSettings.DestinationConfiguration, job);

            SetupJobHistoryErrorSubscriptions(dataSynchronizer);

#pragma warning disable 612
            dataSynchronizer.SyncData(sourceData, managerLinkMap, importSettings, jobStopManager, DiagnosticLog);
#pragma warning restore 612

            return dataSynchronizer.TotalRowsProcessed;
        }

        private IEnumerable<string> GetNotImportedManagersIds(IDictionary<string, string> managersLookup,
            IDictionary<string, int> importedManagers)
        {
            string[] managersIds = managersLookup.Select(x => x.Value).ToArray();
            IEnumerable<string> importedManagersIds = importedManagers.Select(x => x.Key);
            return managersIds.Except(importedManagersIds);
        }

        private void AddMissingManagersErrors(IDictionary<string, string> managersLookup,
            IDictionary<string, int> importedManagers)
        {
            IEnumerable<string> notImportedManagersIds = GetNotImportedManagersIds(managersLookup, importedManagers);
            List<KeyValuePair<string, string>> missingManagers =
                managersLookup.Join(notImportedManagersIds, managerPair => managerPair.Value,
                    notImportedManagerId => notImportedManagerId,
                    (pair, id) => new KeyValuePair<string, string>(pair.Key, pair.Value)).ToList();

            if (missingManagers.Any())
            {
                foreach (var manager in missingManagers)
                {
                    IList<string> entityIdsWithMissingManager =
                        _entityManagerMap.Where(x => x.OldManagerID == manager.Key).Select(x => x.EntityID).ToList();
                    string entityIdsWithMissingManagerString = string.Join(", ", entityIdsWithMissingManager);

                    JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorItem, manager.Value,
                        $"Could not retrieve information and link the following Manager: {manager.Key} with the following entity IDs: {entityIdsWithMissingManagerString}",
                        string.Empty);
                }
            }
        }

        #region Logging

        private void LogExecutingTaskError(Job job, Exception ex)
        {
            _logger.LogError(ex, "Failed to execute SyncEntityManagerWorker task for job {JobId}.", job.JobId);
        }

        private void LogRetrievingManagerArtifactIds(IEnumerable<string> messages)
        {
            _logger.LogError("Failed to get managers artifact ids with messages: {Message}.", string.Join(", ", messages));
        }

        private void LogRetrievingEntityManagersIdsError(Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve entity manager field artifact id with message: {Message}.", ex.Message);
        }

        private void LogExecuteTaskFinalize(Job job)
        {
            _logger.LogInformation("Finalized execution of task in SyncEntityManagerWorker. job: {JobId}", job.JobId);
        }

        private void LogExecuteTaskSuccessfulEnd(Job job)
        {
            _logger.LogInformation("Successfully executed task in SyncEntityManagerWorker. job: {JobId}", job.JobId);
        }

        private void LogExecuteTaskStart(Job job)
        {
            _logger.LogInformation("Starting execution of task in SyncEntityManagerWorker. job: {JobId}", job.JobId);
        }

        private void LogGetImportedManagerArtifactIDsStart()
        {
            _logger.LogInformation("Started getting imported manager artifactIDs.");
        }

        private void LogGetImportedManagerArtifactIDsSuccessfulEnd(IDictionary<string, int> managerIDs)
        {
            _logger.LogInformation("Successfully retrieved imported manager artifactIDs");
            _logger.LogInformation("Retrieved manager artifactIDs manager ids: {managerIDs}", managerIDs.Values);
        }

        private void LogGetEntityManagerFieldArtifactIdSuccessfulEnd(int artifactID)
        {
            _logger.LogInformation("Successfully retrieved entity manager field artifactID: {artifactID}", artifactID);
        }

        private void LogGetEntityManagerFieldArtifactIdStart()
        {
            _logger.LogInformation("Getting entity manager field artifactID.");
        }

        private void LogReconfigureImportApiSettingsStart(int entityManagerFieldArtifactID)
        {
            _logger.LogInformation("Start reconfiguring import API settings for: {entityManagerFieldArtifactID}", entityManagerFieldArtifactID);
        }

        private void LogOtherSyncWorkerBatchesAreInProgress(Job job, Guid batchInstance)
        {
            _logger.LogInformation("Other SyncWorker batches for {rootJobId} are still in progress. " +
                                   "Job {jobId} is re-submitted to the queue with batchInstance {batchInstance}",
                job.RootJobId, job.JobId, batchInstance);
        }

        #endregion
    }
}

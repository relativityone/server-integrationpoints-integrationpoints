﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Entity;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Conversion;
using kCura.IntegrationPoints.Core.Services.EntityManager;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Constants = kCura.IntegrationPoints.Core.Constants;
using Field = kCura.Relativity.Client.DTOs.Field;
using ObjectTypeGuids = kCura.IntegrationPoints.Core.Contracts.Entity.ObjectTypeGuids;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncEntityManagerWorker : SyncWorker
	{
		private readonly IAPILog _logger;
		private readonly IManagerQueueService _managerQueueService;
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly IHelperFactory _helperFactory;
		private readonly IRelativityObjectManager _relativityObjectManager;
		private IEnumerable<FieldMap> _entityManagerFieldMap;
		private List<EntityManagerMap> _entityManagerMap;
		private bool _managerFieldIdIsBinary;
		private IEnumerable<FieldMap> _managerFieldMap;
		private string _newKeyManagerFieldID;
		private string _oldKeyManagerFieldID;

		private int _workspaceArtifactId;

		public SyncEntityManagerWorker(ICaseServiceContext caseServiceContext, 
			IDataProviderFactory dataProviderFactory,
			IHelper helper, 
			ISerializer serializer, 
			ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
			IJobHistoryService jobHistoryService, 
			IJobHistoryErrorService jobHistoryErrorService, 
			IJobManager jobManager,
			IManagerQueueService managerQueueService, 
			JobStatisticsService statisticsService, 
			IManagerFactory managerFactory,
			IContextContainerFactory contextContainerFactory,
			IJobService jobService, 
			IRepositoryFactory repositoryFactory, 
			IHelperFactory helperFactory, 
			IRelativityObjectManager relativityObjectManager,
			IProviderTypeService providerTypeService,
			IIntegrationPointRepository integrationPointRepository)
			: base(caseServiceContext, 
				helper, 
				dataProviderFactory, 
				serializer,
				appDomainRdoSynchronizerFactoryFactory, 
				jobHistoryService, jobHistoryErrorService,
				jobManager, 
				null, 
				statisticsService, 
				managerFactory,
				contextContainerFactory, 
				jobService, 
				providerTypeService,
				integrationPointRepository)
		{
			_managerQueueService = managerQueueService;
			_repositoryFactory = repositoryFactory;
			_helperFactory = helperFactory;
			_relativityObjectManager = relativityObjectManager;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<SyncEntityManagerWorker>();
		}

		protected override void ExecuteTask(Job job)
		{
			try
			{
				LogExecuteTaskStart(job);
				
				//get all job parameters
				EntityManagerJobParameters jobParameters = GetParameters(job);
				SetIntegrationPoint(job);
				SetJobHistory();
				_workspaceArtifactId = job.WorkspaceID;
				
				//check if all tasks are done for this batch yet
				bool IsPrimaryBatchWorkComplete = _managerQueueService.AreAllTasksOfTheBatchDone(job, new[] { TaskType.SyncEntityManagerWorker.ToString() });
				if (!IsPrimaryBatchWorkComplete)
				{
					new TaskJobSubmitter(JobManager, job, TaskType.SyncEntityManagerWorker, BatchInstance).SubmitJob(jobParameters);
					return;
				}

				string destinationManagerUniqueIdFieldDisplayName = GetDestinationManagerUniqueIdFieldDisplayName();
				IDictionary<string, string> managersLookup = new Dictionary<string, string>();

				if (SourceProvider.Identifier.Equals(Constants.IntegrationPoints.SourceProviders.LDAP, StringComparison.InvariantCultureIgnoreCase))
				{
					//update common queue for this job using passed Entity/Manager links and get the next unprocessed links
					_entityManagerMap = _managerQueueService.GetEntityManagerLinksToProcess(job, BatchInstance,
						_entityManagerMap);

					//if no links to process - exit
					if (!_entityManagerMap.Any())
					{
						return;
					}

					IDataSourceProvider sourceProvider = GetSourceProvider(SourceProvider, job);
					IEnumerable<FieldMap> fieldMap = GetFieldMap(IntegrationPoint.FieldMappings);
					List<FieldEntry> sourceFields = GetSourceFields(fieldMap);
					IList<string> managersLdapQueryStrings = GetManagersLdapQueryStrings();
					IEnumerable<IDictionary<string, object>> managersData = ReadManagersData(sourceProvider, sourceFields,
						managersLdapQueryStrings);

					string identifierFieldName = GetSourceFieldIdentifierFieldName(fieldMap);
					string managerIdentifiedFieldName = GetDestinationFieldIdentifierName(jobParameters);

					managersLookup = CreateManagersLookup(managersData, managerIdentifiedFieldName, identifierFieldName);
					_entityManagerMap.ForEach(x => x.NewManagerID = managersLookup[x.OldManagerID]);
				}
				else
				{
					//if no links to process - exit
					if (!_entityManagerMap.Any())
					{
						return;
					}

					_entityManagerMap.ForEach(x => x.NewManagerID = x.OldManagerID);
				}

				var managerUniqueIDs = _entityManagerMap.Where(x => x.NewManagerID != null).Select(x => x.NewManagerID).Distinct().ToArray();
				IDictionary<string, int> managerArtifactIDs = GetImportedManagerArtifactIDs(destinationManagerUniqueIdFieldDisplayName, managerUniqueIDs);
				_entityManagerMap.ForEach(x => x.ManagerArtifactID =
							x.NewManagerID != null && managerArtifactIDs.ContainsKey(x.NewManagerID) ? managerArtifactIDs[x.NewManagerID] : 0);

				//change import api settings to be able to overlay and set Entity/Manager links
				int entityManagerFieldArtifactID = GetEntityManagerFieldArtifactID();
				var newDestinationConfiguration = ReconfigureImportAPISettings(entityManagerFieldArtifactID);

				//run import api to link corresponding Managers to Entity
				FieldEntry fieldEntryEntityIdentifier = _managerFieldMap.First(x => x.FieldMapType.Equals(FieldMapTypeEnum.Identifier)).SourceField;
				FieldEntry fieldEntryManagerIdentifier = _managerFieldMap.First(
					x => x.DestinationField.FieldIdentifier.Equals(entityManagerFieldArtifactID.ToString())).SourceField;

				IEnumerable<IDictionary<FieldEntry, object>> sourceData = _entityManagerMap.Where(x => x.ManagerArtifactID != 0)
					.Select(x => new Dictionary<FieldEntry, object>
					{
						{fieldEntryEntityIdentifier, x.EntityID},
						{fieldEntryManagerIdentifier, x.ManagerArtifactID}
					});

				var managerLinkMap = _managerFieldMap.Where(x =>
					x.SourceField.FieldIdentifier.Equals(fieldEntryEntityIdentifier.FieldIdentifier) ||
					x.SourceField.FieldIdentifier.Equals(fieldEntryManagerIdentifier.FieldIdentifier));

				LinkManagers(job, newDestinationConfiguration, sourceData, managerLinkMap);
				AddMissingManagersErrors(managersLookup, managerArtifactIDs);
				LogExecuteTaskSuccesfulEnd(job);

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
				//rdo last run and next scheduled time will be updated in Manager job
				JobHistoryErrorService.CommitErrors();
				PostExecute(job);
				LogExecuteTaskFinalize(job);
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
				x => new EntityManagerMap { EntityID = x.Key, OldManagerID = x.Value }).ToList();

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
				new DataSourceProviderConfiguration(IntegrationPoint.SourceConfiguration, IntegrationPoint.SecuredConfiguration)))
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
			var objectBuilder = new SynchronizerObjectBuilder(sourceFields);
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

		private IDictionary<string, int> GetImportedManagerArtifactIDs(string uniqueFieldName, string[] managerUniqueIDs)
		{
			LogGetImportedManagerArtifactIDsStart(uniqueFieldName);

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
					}
				},
				Condition = $"'{uniqueFieldName}' IN [{ids}]"
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

			IDictionary<string, int> managerIDs =
				result.ToDictionary(r => r.FieldValues.First(f => f.Field.Name == uniqueFieldName).Value?.ToString(), r => r.ArtifactID);
			LogGetImportedManagerArtifactIDsSuccesfulEnd(uniqueFieldName, managerIDs);
			return managerIDs;
		}


		private int GetEntityManagerFieldArtifactID()
		{
			LogGetEntityManagerFieldArtifactIdStart();
			IFieldQueryRepository fieldQueryRepository = _repositoryFactory.GetFieldQueryRepository(_workspaceArtifactId);
			Field dto = new Field(new Guid(EntityFieldGuids.Manager));

			ResultSet<Field> resultSet = fieldQueryRepository.Read(dto);
			if (!resultSet.Success)
			{
				var messages = resultSet.Results.Where(x => !x.Success).Select(x => x.Message);
				LogRetrievingEntityManagersIdsError(messages);
				var e = new AggregateException(resultSet.Message, messages.Select(x => new Exception(x)));
				throw e;
			}

			int artifactID = resultSet.Results[0].Artifact.ArtifactID;
			LogGetEntityManagerFieldArtifactIdSuccesfulEnd(artifactID);
			return artifactID;
		}


		private string ReconfigureImportAPISettings(int entityManagerFieldArtifactID)
		{
			LogReconfigureImportApiSettingsStart(entityManagerFieldArtifactID);
			ImportSettings importSettings = JsonConvert.DeserializeObject<ImportSettings>(IntegrationPoint.DestinationConfiguration);
			importSettings.ObjectFieldIdListContainsArtifactId = new[] { entityManagerFieldArtifactID };
			importSettings.ImportOverwriteMode = ImportOverwriteModeEnum.OverlayOnly;
			importSettings.EntityManagerFieldContainsLink = false;
			importSettings.FederatedInstanceCredentials = IntegrationPoint.SecuredConfiguration;

			if (importSettings.IsFederatedInstance())
			{
				var targetHelper = _helperFactory.CreateTargetHelper(Helper, importSettings.FederatedInstanceArtifactId, importSettings.FederatedInstanceCredentials);
				var contextContainer = ContextContainerFactory.CreateContextContainer(targetHelper);
				var instanceSettingsManager = ManagerFactory.CreateInstanceSettingsManager(contextContainer);

				if (!instanceSettingsManager.RetrieveAllowNoSnapshotImport())
				{
					importSettings.ImportAuditLevel = ImportAuditLevelEnum.FullAudit;
				}
			}

			string newDestinationConfiguration = JsonConvert.SerializeObject(importSettings);
			LogReconfigureImportApiSettingsSuccesfulEnd(entityManagerFieldArtifactID, newDestinationConfiguration);
			return newDestinationConfiguration;
		}

		private void LinkManagers(Job job, string newDestinationConfiguration, IEnumerable<IDictionary<FieldEntry, object>> sourceData,
			IEnumerable<FieldMap> managerLinkMap)
		{
			IDataSynchronizer dataSynchronizer = GetDestinationProvider(DestinationProvider, newDestinationConfiguration, job);

			SetupJobHistoryErrorSubscriptions(dataSynchronizer);

#pragma warning disable 612
			dataSynchronizer.SyncData(sourceData, managerLinkMap, newDestinationConfiguration);
#pragma warning restore 612
		}

		private IEnumerable<string> GetNotImportedManagersIds(IDictionary<string, string> managersLookup,
			IDictionary<string, int> importedManagers)
		{
			var managersIds = managersLookup.Select(x => x.Value).ToArray();
			var importedManagersIds = importedManagers.Select(x => x.Key);
			return managersIds.Except(importedManagersIds);
		}

		private void AddMissingManagersErrors(IDictionary<string, string> managersLookup,
			IDictionary<string, int> importedManagers)
		{
			var notImportedManagersIds = GetNotImportedManagersIds(managersLookup, importedManagers);
			var missingManagers =
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

		private void LogRetrievingEntityManagersIdsError(IEnumerable<string> messages)
		{
			_logger.LogError("Failed to retrieve entity manager field artifact id with messages: {Message}.",
				string.Join(", ", messages));
		}

		private void LogExecuteTaskFinalize(Job job)
		{
			_logger.LogInformation("Finalized execution of task in SyncEntityManagerWorker. job: {JobId}", job.JobId);
		}

		private void LogExecuteTaskSuccesfulEnd(Job job)
		{
			_logger.LogInformation("Succesfully executed task in SyncEntityManagerWorker. job: {JobId}", job.JobId);
		}

		private void LogExecuteTaskStart(Job job)
		{
			_logger.LogInformation("Starting execution of task in SyncEntityManagerWorker. job: {JobId}", job.JobId);
		}

		private void LogGetImportedManagerArtifactIDsStart(string uniqueFieldName)
		{
			_logger.LogInformation("Started getting imported manager artifactIDs for uniqueFieldID: {uniqueFieldName}", uniqueFieldName);
		}

		private void LogGetImportedManagerArtifactIDsSuccesfulEnd(string uniqueFieldName, IDictionary<string, int> managerIDs)
		{
			_logger.LogInformation("Succesfully rertieved imported manager artifactIDs for uniqueFieldName: {uniqueFieldName}", uniqueFieldName);
			_logger.LogDebug("Retrieved manager artifactIDs for uniqueFieldName: {uniqueFieldName}, ids: {managerIDs}", uniqueFieldName, managerIDs.Values);
		}

		private void LogGetEntityManagerFieldArtifactIdSuccesfulEnd(int artifactID)
		{
			_logger.LogInformation("Succesfully retrieved entity manager field artifactID: {artifactID}", artifactID);
		}

		private void LogGetEntityManagerFieldArtifactIdStart()
		{
			_logger.LogInformation("Getting entity manager field artifactID.");
		}
		private void LogReconfigureImportApiSettingsSuccesfulEnd(int entityManagerFieldArtifactID, string newDestinationConfiguration)
		{
			_logger.LogInformation("Succesfully reconfigured import API settings for: {entityManagerFieldArtifactID}",
				entityManagerFieldArtifactID);
			_logger.LogDebug(
				"Reconfigured import API settings for: {entityManagerFieldArtifactID}, new destination configuration: {newDestinationCOnfiguration}",
				entityManagerFieldArtifactID, newDestinationConfiguration);
		}

		private void LogReconfigureImportApiSettingsStart(int entityManagerFieldArtifactID)
		{
			_logger.LogInformation("Start reconfiguring import API settings for: {entityManagerFieldArtifactID}", entityManagerFieldArtifactID);
		}

		#endregion
	}
}
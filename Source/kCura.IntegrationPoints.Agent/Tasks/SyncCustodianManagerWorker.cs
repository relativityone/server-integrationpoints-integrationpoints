using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Custodian;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Conversion;
using kCura.IntegrationPoints.Core.Services.CustodianManager;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.CustodianManager;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Injection;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Relativity.API;
using Field = kCura.Relativity.Client.DTOs.Field;
using ObjectTypeGuids = kCura.IntegrationPoints.Core.Contracts.Custodian.ObjectTypeGuids;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncCustodianManagerWorker : SyncWorker
	{
		private readonly IAPILog _logger;
		private readonly IManagerQueueService _managerQueueService;
		private readonly IRepositoryFactory _repositoryFactory;
		private CustodianManagerDataReaderToEnumerableService _convertDataService;
		private IEnumerable<FieldMap> _custodianManagerFieldMap;

		private List<CustodianManagerMap> _custodianManagerMap;
		private string _destinationConfiguration;
		private DestinationProvider _destinationProviderRdo;
		private bool _managerFieldIdIsBinary;
		private IEnumerable<FieldMap> _managerFieldMap;
		private string _newKeyManagerFieldID;
		private string _oldKeyManagerFieldID;

		private int _workspaceArtifactId;

		public SyncCustodianManagerWorker(ICaseServiceContext caseServiceContext,
			IDataProviderFactory dataProviderFactory,
			IHelper helper,
			ISerializer serializer,
			ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
			IJobHistoryService jobHistoryService,
			IJobHistoryErrorService jobHistoryErrorService,
			IJobManager jobManager,
			IRSAPIClient workspaceRsapiClient,
			IManagerQueueService managerQueueService,
			JobStatisticsService statisticsService,
			IManagerFactory managerFactory,
			IDataReaderWrapperFactory dataReaderWrapperFactory,
			IContextContainerFactory contextContainerFactory,
			IJobService jobService,
			IRepositoryFactory repositoryFactory,
			IProviderTypeService providerTypeService)
			: base(caseServiceContext, helper, dataProviderFactory, serializer,
				appDomainRdoSynchronizerFactoryFactory, jobHistoryService, jobHistoryErrorService,
				jobManager, null, statisticsService, managerFactory, dataReaderWrapperFactory,
				contextContainerFactory, jobService, providerTypeService)
		{
			_managerQueueService = managerQueueService;
			_repositoryFactory = repositoryFactory;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<SyncCustodianManagerWorker>();
		}

		protected override void ExecuteTask(Job job)
		{
			try
			{
				InjectionManager.Instance.Evaluate("640E9695-AB99-4763-ADC5-03E1252277F7");

				//get all job parameters
				CustodianManagerJobParameters jobParameters = GetParameters(job);

				SetIntegrationPoint(job);

				SetJobHistory();

				_workspaceArtifactId = job.WorkspaceID;

				InjectionManager.Instance.Evaluate("CB070ADB-8912-4B61-99B0-3321C0670FC6");

				//check if all tasks are done for this batch yet
				bool IsPrimaryBatchWorkComplete = _managerQueueService.AreAllTasksOfTheBatchDone(job, new[] {TaskType.SyncCustodianManagerWorker.ToString()});
				if (!IsPrimaryBatchWorkComplete)
				{
					new TaskJobSubmitter(JobManager, job, TaskType.SyncCustodianManagerWorker, BatchInstance).SubmitJob(jobParameters);
					return;
				}

				List<string> missingManagers = new List<string>();
				string[] managerUniqueIDs;

				if (!SourceProvider.Identifier.Equals("85120BC8-B2B9-4F05-99E9-DE37BB6C0E15", StringComparison.InvariantCultureIgnoreCase))
				{
					//update common queue for this job using passed Custodian/Manager links and get the next unprocessed links
					_custodianManagerMap = _managerQueueService.GetCustodianManagerLinksToProcess(job, BatchInstance, _custodianManagerMap);

					//if no links to process - exit
					if (!_custodianManagerMap.Any())
					{
						return;
					}

					//import Managers as Custodians which were passed to this job
					//and create another recursive job to process Managers of these Managers if needed
					base.ExecuteTask(job);

					//Get ArtifactIDs for newly created Managers
					missingManagers = _custodianManagerMap.Where(x => !_convertDataService.ManagerOldNewKeyMap.ContainsKey(x.OldManagerID)).Select(x => x.OldManagerID).ToList();
					_custodianManagerMap.ForEach(
						x => x.NewManagerID = _convertDataService.ManagerOldNewKeyMap.ContainsKey(x.OldManagerID) ? _convertDataService.ManagerOldNewKeyMap[x.OldManagerID] : null);
					managerUniqueIDs = _custodianManagerMap.Where(x => x.NewManagerID != null).Select(x => x.NewManagerID).Distinct().ToArray();
				}
				else
				{
					//if no links to process - exit
					if (!_custodianManagerMap.Any())
					{
						return;
					}

					_destinationProviderRdo = DestinationProvider;
					_destinationConfiguration = IntegrationPoint.DestinationConfiguration;
					_custodianManagerMap.ForEach(x => x.NewManagerID = x.OldManagerID);
					managerUniqueIDs = _custodianManagerMap.Where(x => x.NewManagerID != null).Select(x => x.NewManagerID).Distinct().ToArray();
				}

				int destinationManagerUniqueIDFieldID =
					int.Parse(
						_managerFieldMap.First(x => x.SourceField.FieldIdentifier.Equals(_newKeyManagerFieldID, StringComparison.InvariantCultureIgnoreCase))
							.DestinationField.FieldIdentifier);

				IDictionary<string, int> managerArtifactIDs = GetManagerArtifactIDs(destinationManagerUniqueIDFieldID, managerUniqueIDs);
				_custodianManagerMap.ForEach(
					x => x.ManagerArtifactID = (x.NewManagerID != null) && managerArtifactIDs.ContainsKey(x.NewManagerID) ? managerArtifactIDs[x.NewManagerID] : 0);

				//change import api settings to be able to overlay and set Custodian/Manager links
				int custodianManagerFieldArtifactID = GetCustodianManagerFieldArtifactID();
				var newDestinationConfiguration = ReconfigureImportAPISettings(custodianManagerFieldArtifactID);

				//run import api to link corresponding Managers to Custodians
				FieldEntry fieldEntryCustodianIdentifier = _managerFieldMap.First(x => x.FieldMapType.Equals(FieldMapTypeEnum.Identifier)).SourceField;
				FieldEntry fieldEntryManagerIdentifier =
					_managerFieldMap.First(x => x.DestinationField.FieldIdentifier.Equals(custodianManagerFieldArtifactID.ToString())).SourceField;
				IEnumerable<IDictionary<FieldEntry, object>> sourceData = _custodianManagerMap.Where(x => x.ManagerArtifactID != 0)
					.Select(x => new Dictionary<FieldEntry, object>
					{
						{fieldEntryCustodianIdentifier, x.CustodianID},
						{fieldEntryManagerIdentifier, x.ManagerArtifactID}
					});

				var managerLinkMap = _managerFieldMap.Where(x =>
					x.SourceField.FieldIdentifier.Equals(fieldEntryCustodianIdentifier.FieldIdentifier) ||
					x.SourceField.FieldIdentifier.Equals(fieldEntryManagerIdentifier.FieldIdentifier));
				IDataSynchronizer dataSynchronizer = base.GetDestinationProvider(_destinationProviderRdo, newDestinationConfiguration, job);

				SetupJobHistoryErrorSubscriptions(dataSynchronizer);

#pragma warning disable 612
				dataSynchronizer.SyncData(sourceData, managerLinkMap, newDestinationConfiguration);
#pragma warning restore 612

				if (missingManagers.Any())
				{
					var message = string.Format("Could not retrieve information and link the following Managers: {0}", string.Join("; ", missingManagers.ToArray()));
					LogRetrievingManagersError(job, message);
					throw new Exception(message);
				}
			}
			catch (Exception ex)
			{
				LogExecutingTaskError(job, ex);
				JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}
			finally
			{
				//rdo last run and next scheduled time will be updated in Manager job
				JobHistoryErrorService.CommitErrors();
				PostExecute(job);
			}
		}

		private string ReconfigureImportAPISettings(int custodianManagerFieldArtifactID)
		{
			ImportSettings importSettings = JsonConvert.DeserializeObject<ImportSettings>(_destinationConfiguration);
			importSettings.ObjectFieldIdListContainsArtifactId = new[] {custodianManagerFieldArtifactID};
			importSettings.ImportOverwriteMode = ImportOverwriteModeEnum.OverlayOnly;
			importSettings.CustodianManagerFieldContainsLink = false;
			string newDestinationConfiguration = JsonConvert.SerializeObject(importSettings);
			return newDestinationConfiguration;
		}

		private CustodianManagerJobParameters GetParameters(Job job)
		{
			TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
			BatchInstance = taskParameters.BatchInstance;
			CustodianManagerJobParameters jobParameters = null;
			if (taskParameters.BatchParameters is JObject)
			{
				jobParameters =
					((JObject) taskParameters.BatchParameters).ToObject<CustodianManagerJobParameters>();
			}
			else
			{
				jobParameters = (CustodianManagerJobParameters) taskParameters.BatchParameters;
			}
			_custodianManagerMap =
				jobParameters.CustodianManagerMap.Select(x => new CustodianManagerMap {CustodianID = x.Key, OldManagerID = x.Value})
					.ToList();
			_custodianManagerFieldMap = jobParameters.CustodianManagerFieldMap;
			_managerFieldMap = jobParameters.ManagerFieldMap;
			_managerFieldIdIsBinary = jobParameters.ManagerFieldIdIsBinary;

			SetManagerFieldIDs(_custodianManagerFieldMap, _managerFieldMap);

			return jobParameters;
		}

		protected override IDataSynchronizer GetDestinationProvider(DestinationProvider destinationProviderRdo, string configuration, Job job)
		{
			_destinationProviderRdo = destinationProviderRdo;
			_destinationConfiguration = configuration;
			return base.GetDestinationProvider(destinationProviderRdo, configuration, job);
		}

		protected override List<FieldEntry> GetSourceFields(IEnumerable<FieldMap> fieldMap)
		{
			List<FieldEntry> sourceFields = base.GetSourceFields(fieldMap);
			if (!sourceFields.Any(f => f.FieldIdentifier.Equals(_oldKeyManagerFieldID, StringComparison.InvariantCultureIgnoreCase)))
			{
				sourceFields.Add(new FieldEntry {FieldIdentifier = _oldKeyManagerFieldID});
			}
			sourceFields.ForEach(f => f.IsIdentifier = f.FieldIdentifier == _oldKeyManagerFieldID);
			return sourceFields;
		}

		private CustodianManagerDataReaderToEnumerableService GetCustodianManagerDataReaderToEnumerableService(List<FieldEntry> sourceFields)
		{
			var objectBuilder = new SynchronizerObjectBuilder(sourceFields);
			CustodianManagerDataReaderToEnumerableService convertDataService = new CustodianManagerDataReaderToEnumerableService(objectBuilder, _oldKeyManagerFieldID,
				_newKeyManagerFieldID);
			return convertDataService;
		}

		protected override IEnumerable<IDictionary<FieldEntry, object>> GetSourceData(List<FieldEntry> sourceFields, IDataReader sourceDataReader)
		{
			_convertDataService = GetCustodianManagerDataReaderToEnumerableService(sourceFields);
			return _convertDataService.GetData<IDictionary<FieldEntry, object>>(sourceDataReader);
		}

		protected override List<string> GetEntryIDs(Job job)
		{
			return _custodianManagerMap.Select(x => ConvertObjectGuid(x.OldManagerID)).Distinct().ToList();
		}

		private void SetManagerFieldIDs(IEnumerable<FieldMap> custodianManagerFieldMap, IEnumerable<FieldMap> managerFieldMap)
		{
			_oldKeyManagerFieldID = custodianManagerFieldMap.Where(x => x.FieldMapType.Equals(FieldMapTypeEnum.Identifier)).Select(x => x.DestinationField.FieldIdentifier).First();
			_newKeyManagerFieldID = managerFieldMap.Where(x => x.FieldMapType.Equals(FieldMapTypeEnum.Identifier)).Select(x => x.SourceField.FieldIdentifier).First();
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

		private IDictionary<string, int> GetManagerArtifactIDs(int uniqueFieldID, string[] managerUniqueIDs)
		{
			IRdoRepository rdoRepository = _repositoryFactory.GetRdoRepository(_workspaceArtifactId);

			IDictionary<string, int> managerIDs = new Dictionary<string, int>();

			var query = new Query<RDO>();
			query.ArtifactTypeGuid = Guid.Parse(ObjectTypeGuids.Custodian);
			query.Condition = new TextCondition(uniqueFieldID, TextConditionEnum.In, managerUniqueIDs);

			query.Fields.Add(new FieldValue(uniqueFieldID));

			var result = rdoRepository.Query(query);
			if (!result.Success)
			{
				var messages = result.Results.Where(x => !x.Success).Select(x => x.Message);
				LogRetrievingManagerArtifactIds(messages);
				var e = new AggregateException(result.Message, messages.Select(x => new Exception(x)));
				throw e;
			}
			managerIDs = result.Results.ToDictionary(r => r.Artifact.Fields.
					Where(f => f.ArtifactID.Equals(uniqueFieldID)).First().ToString()
				, r => r.Artifact.ArtifactID);
			return managerIDs;
		}

		private int GetCustodianManagerFieldArtifactID()
		{
			IFieldRepository fieldRepository = _repositoryFactory.GetFieldRepository(_workspaceArtifactId);

			Field dto = new Field(new Guid(CustodianFieldGuids.Manager));

			ResultSet<Field> resultSet = fieldRepository.Read(dto);
			if (!resultSet.Success)
			{
				var messages = resultSet.Results.Where(x => !x.Success).Select(x => x.Message);
				LogRetrievingCustodianManagersIdsError(messages);
				var e = new AggregateException(resultSet.Message, messages.Select(x => new Exception(x)));
				throw e;
			}

			return resultSet.Results[0].Artifact.ArtifactID;
		}

		#region Logging

		private void LogExecutingTaskError(Job job, Exception ex)
		{
			_logger.LogError(ex, "Failed to execute SyncCustodianManagerWorker task for job {JobId}.", job.JobId);
		}

		private void LogRetrievingManagersError(Job job, string message)
		{
			_logger.LogError("Error during task execution for job {JobId}. Error details: {ErrorDetails}.", job.JobId, message);
		}

		private void LogRetrievingManagerArtifactIds(IEnumerable<string> messages)
		{
			_logger.LogError("Failed to get managers artifact ids with messages: {Message}.", string.Join(", ", messages));
		}

		private void LogRetrievingCustodianManagersIdsError(IEnumerable<string> messages)
		{
			_logger.LogError("Failed to retrieve custodian manager field artifact id with messages: {Message}.", string.Join(", ", messages));
		}

		#endregion
	}
}
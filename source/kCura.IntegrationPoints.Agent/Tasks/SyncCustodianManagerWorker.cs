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
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.CustodianManager;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Method.Injection;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using Newtonsoft.Json;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tasks
{
	public class SyncCustodianManagerWorker : SyncWorker
	{
		private IRSAPIClient _workspaceRsapiClient;
		private ManagerQueueService _managerQueueService;

		public SyncCustodianManagerWorker(ICaseServiceContext caseServiceContext,
			IDataProviderFactory dataProviderFactory,
			IHelper helper,
			ISerializer serializer,
			ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
			IJobHistoryService jobHistoryService,
			JobHistoryErrorService jobHistoryErrorService,
			IJobManager jobManager,
			IRSAPIClient workspaceRsapiClient,
			ManagerQueueService managerQueueService,
			JobStatisticsService statisticsService,
			IManagerFactory managerFactory,
			IContextContainerFactory contextContainerFactory,
			IJobService jobService)
				: base(caseServiceContext, helper, dataProviderFactory, serializer,
						appDomainRdoSynchronizerFactoryFactory, jobHistoryService, jobHistoryErrorService, 
						jobManager, null, statisticsService, managerFactory,
						contextContainerFactory, jobService, false)
		{
			_workspaceRsapiClient = workspaceRsapiClient;
			_managerQueueService = managerQueueService;
		}

		private List<CustodianManagerMap> _custodianManagerMap;
		private IEnumerable<FieldMap> _custodianManagerFieldMap;
		private IEnumerable<FieldMap> _managerFieldMap;
		private bool _managerFieldIdIsBinary;
		private string _oldKeyManagerFieldID;
		private string _newKeyManagerFieldID;
		private CustodianManagerDataReaderToEnumerableService _convertDataService;
		private DestinationProvider _destinationProviderRdo;
		private string _destinationConfiguration;

		protected override void ExecuteTask(Job job)
		{
			try
			{
				InjectionManager.Instance.Evaluate("640E9695-AB99-4763-ADC5-03E1252277F7");

				//get all job parameters
				CustodianManagerJobParameters jobParameters = GetParameters(job);

				base.SetIntegrationPoint(job);

				base.SetJobHistory();

				InjectionManager.Instance.Evaluate("CB070ADB-8912-4B61-99B0-3321C0670FC6");

				//check if all tasks are done for this batch yet
				bool IsPrimaryBatchWorkComplete = _managerQueueService.AreAllTasksOfTheBatchDone(job, new string[] { TaskType.SyncCustodianManagerWorker.ToString() });
				if (!IsPrimaryBatchWorkComplete)
				{
					new TaskJobSubmitter(JobManager, job, TaskType.SyncCustodianManagerWorker, this.BatchInstance).SubmitJob(jobParameters);
					return;
				}

				List<string> missingManagers = new List<string>();
				string[] managerUniqueIDs;

				if (!base.SourceProvider.Identifier.Equals("85120BC8-B2B9-4F05-99E9-DE37BB6C0E15", StringComparison.InvariantCultureIgnoreCase))
				{
					//update common queue for this job using passed Custodian/Manager links and get the next unprocessed links
					_custodianManagerMap = _managerQueueService.GetCustodianManagerLinksToProcess(job, this.BatchInstance, _custodianManagerMap);

					//if no links to process - exit
					if (!_custodianManagerMap.Any()) { return; }

					//import Managers as Custodians which were passed to this job
					//and create another recursive job to process Managers of these Managers if needed
					base.ExecuteTask(job);

					//Get ArtifactIDs for newly created Managers
					missingManagers = _custodianManagerMap.Where(x => !_convertDataService.ManagerOldNewKeyMap.ContainsKey(x.OldManagerID)).Select(x => x.OldManagerID).ToList();
					_custodianManagerMap.ForEach(x => x.NewManagerID = _convertDataService.ManagerOldNewKeyMap.ContainsKey(x.OldManagerID) ? _convertDataService.ManagerOldNewKeyMap[x.OldManagerID] : null);
					managerUniqueIDs = _custodianManagerMap.Where(x => x.NewManagerID != null).Select(x => x.NewManagerID).Distinct().ToArray();
				}
				else
				{
					//if no links to process - exit
					if (!_custodianManagerMap.Any()) { return; }

					_destinationProviderRdo = base.DestinationProvider;
					_destinationConfiguration = base.IntegrationPoint.DestinationConfiguration;
					_custodianManagerMap.ForEach(x => x.NewManagerID = x.OldManagerID);
					managerUniqueIDs = _custodianManagerMap.Where(x => x.NewManagerID != null).Select(x => x.NewManagerID).Distinct().ToArray();
				}

				int destinationManagerUniqueIDFieldID = int.Parse(_managerFieldMap.First(x => x.SourceField.FieldIdentifier.Equals(_newKeyManagerFieldID, StringComparison.InvariantCultureIgnoreCase)).DestinationField.FieldIdentifier);

				IDictionary<string, int> managerArtifactIDs = GetManagerArtifactIDs(destinationManagerUniqueIDFieldID, managerUniqueIDs);
				_custodianManagerMap.ForEach(x => x.ManagerArtifactID = (x.NewManagerID != null && managerArtifactIDs.ContainsKey(x.NewManagerID)) ? managerArtifactIDs[x.NewManagerID] : 0);

				//change import api settings to be able to overlay and set Custodian/Manager links
				int custodianManagerFieldArtifactID = GetCustodianManagerFieldArtifactID();
				var newDestinationConfiguration = ReconfigureImportAPISettings(custodianManagerFieldArtifactID);

				//run import api to link corresponding Managers to Custodians
				FieldEntry fieldEntryCustodianIdentifier = _managerFieldMap.First(x => x.FieldMapType.Equals(FieldMapTypeEnum.Identifier)).SourceField;
				FieldEntry fieldEntryManagerIdentifier = _managerFieldMap.First(x => x.DestinationField.FieldIdentifier.Equals(custodianManagerFieldArtifactID.ToString())).SourceField;
				IEnumerable<IDictionary<FieldEntry, object>> sourceData = _custodianManagerMap.Where(x => x.ManagerArtifactID != 0)
				  .Select(x => new Dictionary<FieldEntry, object>()
				{
					{ fieldEntryCustodianIdentifier, x.CustodianID },
					{ fieldEntryManagerIdentifier, x.ManagerArtifactID }
				});

				var managerLinkMap = _managerFieldMap.Where(x =>
				  (x.SourceField.FieldIdentifier.Equals(fieldEntryCustodianIdentifier.FieldIdentifier) ||
				   x.SourceField.FieldIdentifier.Equals(fieldEntryManagerIdentifier.FieldIdentifier)));
				IDataSynchronizer dataSynchronizer = base.GetDestinationProvider(_destinationProviderRdo, newDestinationConfiguration, job);

				base.SetupJobHistoryErrorSubscriptions(dataSynchronizer, job);

				dataSynchronizer.SyncData(sourceData, managerLinkMap, newDestinationConfiguration);

				if (missingManagers.Any())
				{
					throw new Exception(string.Format("Could not retrieve information and link the following Managers: {0}",
					  string.Join("; ", missingManagers.ToArray())));
				}
			}
			catch (Exception ex)
			{
				base.JobHistoryErrorService.AddError(ErrorTypeChoices.JobHistoryErrorJob, ex);
			}
			finally
			{
				//rdo last run and next scheduled time will be updated in Manager job
				base.JobHistoryErrorService.CommitErrors();
				base.PostExecute(job);
			}
		}

		private string ReconfigureImportAPISettings(int custodianManagerFieldArtifactID)
		{
			ImportSettings importSettings = JsonConvert.DeserializeObject<ImportSettings>(_destinationConfiguration);
			importSettings.ObjectFieldIdListContainsArtifactId = new int[] { custodianManagerFieldArtifactID };
			importSettings.ImportOverwriteMode = ImportOverwriteModeEnum.OverlayOnly;
			importSettings.CustodianManagerFieldContainsLink = false;
			string newDestinationConfiguration = JsonConvert.SerializeObject(importSettings);
			return newDestinationConfiguration;
		}

		private CustodianManagerJobParameters GetParameters(Job job)
		{
			TaskParameters taskParameters = Serializer.Deserialize<TaskParameters>(job.JobDetails);
			this.BatchInstance = taskParameters.BatchInstance;
			CustodianManagerJobParameters jobParameters = null;
			if (taskParameters.BatchParameters is Newtonsoft.Json.Linq.JObject)
			{
				jobParameters =
				  ((Newtonsoft.Json.Linq.JObject)taskParameters.BatchParameters).ToObject<CustodianManagerJobParameters>();
			}
			else
			{
				jobParameters = (CustodianManagerJobParameters)taskParameters.BatchParameters;
			}
			_custodianManagerMap =
			  jobParameters.CustodianManagerMap.Select(x => new CustodianManagerMap() { CustodianID = x.Key, OldManagerID = x.Value })
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
				sourceFields.Add(new FieldEntry() { FieldIdentifier = _oldKeyManagerFieldID });
			}
			sourceFields.ForEach(f => f.IsIdentifier = f.FieldIdentifier == _oldKeyManagerFieldID);
			return sourceFields;
		}

		private CustodianManagerDataReaderToEnumerableService GetCustodianManagerDataReaderToEnumerableService(List<FieldEntry> sourceFields)
		{
			var objectBuilder = new SynchronizerObjectBuilder(sourceFields);
			CustodianManagerDataReaderToEnumerableService convertDataService = new CustodianManagerDataReaderToEnumerableService(objectBuilder, _oldKeyManagerFieldID, _newKeyManagerFieldID);
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
			if (!_managerFieldIdIsBinary) return originalID;

			string newID = string.Empty;
			for (int i = 0; i < originalID.Length; i = i + 2)
			{
				newID = string.Format("{0}\\{1}", newID, originalID.Substring(i, 2));
			}
			return newID;
		}

		private IDictionary<string, int> GetManagerArtifactIDs(int uniqueFieldID, string[] managerUniqueIDs)
		{
			IDictionary<string, int> managerIDs = new Dictionary<string, int>();

			var query = new Query<RDO>();
			query.ArtifactTypeGuid = Guid.Parse(Core.Contracts.Custodian.ObjectTypeGuids.Custodian);
			query.Condition = new TextCondition(uniqueFieldID, TextConditionEnum.In, managerUniqueIDs);

			query.Fields.Add(new FieldValue(uniqueFieldID));

			var result = _workspaceRsapiClient.Repositories.RDO.Query(query);
			if (!result.Success)
			{
				var messages = result.Results.Where(x => !x.Success).Select(x => x.Message);
				var e = new AggregateException(result.Message, messages.Select(x => new Exception(x)));
				throw e;
			}
			else
			{
				managerIDs = result.Results.ToDictionary(r => r.Artifact.Fields.
				  Where(f => f.ArtifactID.Equals(uniqueFieldID)).First().ToString()
				  , r => r.Artifact.ArtifactID);
			}
			return managerIDs;
		}

		private int GetCustodianManagerFieldArtifactID()
		{
			Relativity.Client.DTOs.Field dto = new Relativity.Client.DTOs.Field(new Guid(CustodianFieldGuids.Manager));

			ResultSet<Relativity.Client.DTOs.Field> resultSet = _workspaceRsapiClient.Repositories.Field.Read(dto);
			if (!resultSet.Success)
			{
				var messages = resultSet.Results.Where(x => !x.Success).Select(x => x.Message);
				var e = new AggregateException(resultSet.Message, messages.Select(x => new Exception(x)));
				throw e;
			}

			return resultSet.Results[0].Artifact.ArtifactID;
		}
	}
}
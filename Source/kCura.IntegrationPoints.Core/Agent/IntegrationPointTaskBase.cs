using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.Conversion;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.ScheduleQueue.Core;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.FieldsMapping.Models;

namespace kCura.IntegrationPoints.Core.Agent
{
	public class IntegrationPointTaskBase
	{
		private DestinationProvider _destinationProvider;
		private readonly IAPILog _logger;

		protected ICaseServiceContext CaseServiceContext;
		protected IDataProviderFactory DataProviderFactory;
		protected IJobHistoryErrorService JobHistoryErrorService;
		protected IJobHistoryService JobHistoryService;
		protected IJobManager JobManager;
		protected IJobService JobService;
		protected IManagerFactory ManagerFactory;
		protected ISerializer Serializer;
		protected ISynchronizerFactory AppDomainRdoSynchronizerFactoryFactory;
		protected IIntegrationPointRepository IntegrationPointRepository;
		protected SourceProvider _sourceProvider;
		protected readonly IHelper Helper;

		public IntegrationPointTaskBase(
			ICaseServiceContext caseServiceContext,
			IHelper helper,
			IDataProviderFactory dataProviderFactory,
			ISerializer serializer,
			ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
			IJobHistoryService jobHistoryService,
			IJobHistoryErrorService jobHistoryErrorService,
			IJobManager jobManager,
			IManagerFactory managerFactory,
			IJobService jobService,
			IIntegrationPointRepository integrationPointRepository)
		{
			CaseServiceContext = caseServiceContext;
			Helper = helper;
			DataProviderFactory = dataProviderFactory;
			Serializer = serializer;
			AppDomainRdoSynchronizerFactoryFactory = appDomainRdoSynchronizerFactoryFactory;
			JobHistoryService = jobHistoryService;
			JobHistoryErrorService = jobHistoryErrorService;
			JobManager = jobManager;
			ManagerFactory = managerFactory;
			JobService = jobService;
			IntegrationPointRepository = integrationPointRepository;
			_logger = helper.GetLoggerFactory().GetLogger().ForContext<IntegrationPointTaskBase>();
		}

		protected DestinationProvider DestinationProvider
		{
			get
			{
				if (IntegrationPoint == null)
				{
					LogRetrievingDestinationProviderError();
					throw new ArgumentException("The Integration Point Rdo has not been set yet.");
				}
				if (_destinationProvider == null)
				{
					_destinationProvider = CaseServiceContext.RelativityObjectManagerService.RelativityObjectManager.Read<DestinationProvider>(IntegrationPoint.DestinationProvider.Value);
				}
				return _destinationProvider;
			}
		}

		protected IntegrationPoint IntegrationPoint { get; set; }
		public JobHistory JobHistory { get; set; }

		protected SourceProvider SourceProvider
		{
			get
			{
				if (IntegrationPoint == null)
				{
					LogRetrievingSourceProviderError();
					throw new ArgumentException("The Integration Point Rdo has not been set yet.");
				}
				if (_sourceProvider == null)
				{
					_sourceProvider = CaseServiceContext.RelativityObjectManagerService.RelativityObjectManager.Read<SourceProvider>(IntegrationPoint.SourceProvider.Value);
				}
				return _sourceProvider;
			}
		}

		protected Guid BatchInstance { get; set; }

		protected virtual List<FieldEntry> GetDestinationFields(IEnumerable<FieldMap> fieldMap)
		{
			return fieldMap.Select(f => f.DestinationField).ToList();
		}

		protected virtual IDataSynchronizer GetDestinationProvider(DestinationProvider destinationProviderRdo, string configuration, Job job)
		{
			LogGetDestinationProviderStart(job);
			Guid providerGuid = new Guid(destinationProviderRdo.Identifier);
			var factory = AppDomainRdoSynchronizerFactoryFactory as GeneralWithEntityRdoSynchronizerFactory;
			if (factory != null)
			{
				factory.TaskJobSubmitter = new TaskJobSubmitter(JobManager, JobService, job, TaskType.SyncEntityManagerWorker, BatchInstance);
				factory.SourceProvider = SourceProvider;
			}
			IDataSynchronizer destinationProvider = AppDomainRdoSynchronizerFactoryFactory.CreateSynchronizer(providerGuid, configuration);
			LogGetDestinationProviderSuccesfulEnd(job, destinationProvider);
			return destinationProvider;
		}

		protected virtual IEnumerable<FieldMap> GetFieldMap(string serializedFieldMappings)
		{
			IEnumerable<FieldMap> fieldMaps = Serializer.Deserialize<List<FieldMap>>(serializedFieldMappings);
			foreach (FieldMap fieldMap in fieldMaps)
			{
				bool isIdentifier = fieldMap.FieldMapType == FieldMapTypeEnum.Identifier;
				fieldMap.SourceField.IsIdentifier = isIdentifier;
			}
			return fieldMaps;
		}

		protected List<string> GetRecipientEmails()
		{
			return GetRecipientEmails(IntegrationPoint, _logger);
		}

		public static List<string> GetRecipientEmails(IntegrationPoint integrationPoint, IAPILog logger)
		{
			string emailRecipients = string.Empty;
			try
			{
				emailRecipients = integrationPoint.EmailNotificationRecipients;
			}
			catch (Exception e)
			{
				LogRetrievingRecipientEmailsError(e, logger);
				//this property might be not loaded on RDO if it's null, so suppress exception
			}

			var emailRecipientList = new List<string>();

			if (!string.IsNullOrWhiteSpace(emailRecipients))
			{
				emailRecipientList = emailRecipients.Split(';').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
			}

			return emailRecipientList;
		}

		protected virtual IEnumerable<IDictionary<FieldEntry, object>> GetSourceData(List<FieldEntry> sourceFields, IDataReader sourceDataReader)
		{
			_logger.LogInformation("Instantiating {dataReader}...");

			Stopwatch sw = new Stopwatch();
			sw.Start();
			var objectBuilder = new SynchronizerObjectBuilder(sourceFields);	
			var data = new DataReaderToEnumerableService(objectBuilder).GetData<IDictionary<FieldEntry, object>>(sourceDataReader);
			sw.Stop();

			_logger.LogInformation("DataReader was instantiated in time {seconds} [ms].", sw.ElapsedMilliseconds);

			return data;
		}

		protected virtual List<FieldEntry> GetSourceFields(IEnumerable<FieldMap> fieldMap)
		{
			return fieldMap.Select(f => f.SourceField).ToList();
		}

		protected virtual IDataSourceProvider GetSourceProvider(SourceProvider sourceProviderRdo, Job job)
		{
			Guid applicationGuid = new Guid(sourceProviderRdo.ApplicationIdentifier);
			Guid providerGuid = new Guid(sourceProviderRdo.Identifier);
			IDataSourceProvider sourceProvider = DataProviderFactory.GetDataProvider(applicationGuid, providerGuid);
			return sourceProvider;
		}

		protected void SetIntegrationPoint(Job job)
		{
			if (IntegrationPoint != null)
			{
				return;
			}

			int integrationPointId = job.RelatedObjectArtifactID;
			IntegrationPoint = IntegrationPointRepository.ReadWithFieldMappingAsync(integrationPointId).GetAwaiter().GetResult();
			if (IntegrationPoint == null)
			{
				LogSettingIntegrationPointError(job);
				throw new ArgumentException("Failed to retrieve corresponding Integration Point Rdo.");
			}
		}

		protected void SetJobHistory()
		{
			if (JobHistory != null)
			{
				return;
			}

			// TODO: it is possible here that the Job Type is not Run Now - verify expected usage
			JobHistory = JobHistoryService.CreateRdo(IntegrationPoint, BatchInstance, JobTypeChoices.JobHistoryRun, DateTime.UtcNow);
			JobHistoryErrorService.JobHistory = JobHistory;
			JobHistoryErrorService.IntegrationPoint = IntegrationPoint;
		}

		#region Logging

		private void LogRetrievingSourceProviderError()
		{
			_logger.LogError("Retrieving Source Provider: The Integration Point Rdo has not been set yet.");
		}

		private void LogRetrievingDestinationProviderError()
		{
			_logger.LogError("Retrieving Destination Provider: The Integration Point Rdo has not been set yet.");
		}

		private static void LogRetrievingRecipientEmailsError(Exception e, IAPILog logger)
		{
			logger.LogError(e, "Failed to retrieve recipient emails.");
		}

		private void LogSettingIntegrationPointError(Job job)
		{
			_logger.LogError("Failed to retrieve corresponding Integration Point Rdo for Job {JobId}.", job.JobId);
		}

		private void LogGetDestinationProviderSuccesfulEnd(Job job, IDataSynchronizer sourceProvider)
		{
			_logger.LogInformation("Integration Point Task Base: Succesfully retrieved destination provider for job: {JobId}. ", job.JobId);
			_logger.LogInformation("Retrieved source provider: {sourceProvider}", sourceProvider.ToString());
		}

		private void LogGetDestinationProviderStart(Job job)
		{
			_logger.LogInformation("Integration Point Task Base: Getting destination provider for job: {JobId}. ", job.JobId);
		}

		#endregion
	}
}
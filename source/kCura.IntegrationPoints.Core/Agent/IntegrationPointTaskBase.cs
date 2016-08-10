using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Castle.Core.Internal;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Conversion;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.Conversion;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Services.Synchronizer;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.ScheduleQueue.Core;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Agent
{
	public class IntegrationPointTaskBase
	{
		protected readonly IHelper Helper;
		protected ISynchronizerFactory AppDomainRdoSynchronizerFactoryFactory;
		protected ICaseServiceContext CaseServiceContext;
		protected IContextContainerFactory ContextContainerFactory;
		protected IDataProviderFactory DataProviderFactory;
		protected JobHistoryErrorService JobHistoryErrorService;
		protected IJobHistoryService JobHistoryService;
		protected IJobManager JobManager;
		protected IJobService JobService;
		protected IManagerFactory ManagerFactory;
		protected Apps.Common.Utils.Serializers.ISerializer Serializer;

		private DestinationProvider _destinationProvider;
		private SourceProvider _sourceProvider;

		public IntegrationPointTaskBase(
		  ICaseServiceContext caseServiceContext,
		  IHelper helper,
		  IDataProviderFactory dataProviderFactory,
		  Apps.Common.Utils.Serializers.ISerializer serializer,
		  ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
		  IJobHistoryService jobHistoryService,
		  JobHistoryErrorService jobHistoryErrorService,
		  IJobManager jobManager,
		  IManagerFactory managerFactory,
		  IContextContainerFactory contextContainerFactory,
		  IJobService jobService)
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
			ContextContainerFactory = contextContainerFactory;
		}

		protected Data.DestinationProvider DestinationProvider
		{
			get
			{
				if (this.IntegrationPoint == null)
				{
					throw new ArgumentException("The Integration Point Rdo has not been set yet.");
				}
				if (_destinationProvider == null)
				{
					_destinationProvider = CaseServiceContext.RsapiService.DestinationProviderLibrary.Read(this.IntegrationPoint.DestinationProvider.Value);
				}
				return _destinationProvider;
			}
		}

		protected Data.IntegrationPoint IntegrationPoint { get; set; }
		protected Data.JobHistory JobHistory { get; set; }

		protected Data.SourceProvider SourceProvider
		{
			get
			{
				if (this.IntegrationPoint == null)
				{
					throw new ArgumentException("The Integration Point Rdo has not been set yet.");
				}
				if (_sourceProvider == null)
				{
					_sourceProvider = CaseServiceContext.RsapiService.SourceProviderLibrary.Read(this.IntegrationPoint.SourceProvider.Value);
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
			Guid providerGuid = new Guid(destinationProviderRdo.Identifier);
			var factory = AppDomainRdoSynchronizerFactoryFactory as GeneralWithCustodianRdoSynchronizerFactory;
			if (factory != null)
			{
				factory.TaskJobSubmitter = new TaskJobSubmitter(JobManager, job, TaskType.SyncCustodianManagerWorker, this.BatchInstance);
				factory.SourceProvider = SourceProvider;
			}
			IDataSynchronizer sourceProvider = AppDomainRdoSynchronizerFactoryFactory.CreateSynchronizer(providerGuid, configuration);
			return sourceProvider;
		}

		protected virtual IEnumerable<FieldMap> GetFieldMap(string serializedFieldMappings)
		{
			IEnumerable<FieldMap> fieldMap = Serializer.Deserialize<List<FieldMap>>(serializedFieldMappings);
			fieldMap.ForEach(f => f.SourceField.IsIdentifier = f.FieldMapType == FieldMapTypeEnum.Identifier);
			return fieldMap;
		}

		protected List<string> GetRecipientEmails()
		{
			string emailRecipients = string.Empty;
			try
			{
				emailRecipients = IntegrationPoint.EmailNotificationRecipients;
			}
			catch
			{
				//this property might be not loaded on RDO if it's null, so suppress exception
			}

			var emailRecipientList = new List<string>();

			if (!String.IsNullOrWhiteSpace(emailRecipients))
			{
				emailRecipientList = emailRecipients.Split(';').Select(x => x.Trim()).Where(x => !String.IsNullOrWhiteSpace(x)).ToList();
			}

			return emailRecipientList;
		}

		protected virtual string GetSourceConfiguration(string originalSourceConfiguration)
		{
			return originalSourceConfiguration;
		}

		protected virtual IEnumerable<IDictionary<FieldEntry, object>> GetSourceData(List<FieldEntry> sourceFields, IDataReader sourceDataReader)
		{
			var objectBuilder = new SynchronizerObjectBuilder(sourceFields);
			return new DataReaderToEnumerableService(objectBuilder).GetData<IDictionary<FieldEntry, object>>(sourceDataReader);
		}

		protected virtual List<FieldEntry> GetSourceFields(IEnumerable<FieldMap> fieldMap)
		{
			return fieldMap.Select(f => f.SourceField).ToList();
		}

		protected virtual IDataSourceProvider GetSourceProvider(SourceProvider sourceProviderRdo, Job job)
		{
			Guid applicationGuid = new Guid(sourceProviderRdo.ApplicationIdentifier);
			Guid providerGuid = new Guid(sourceProviderRdo.Identifier);
			IDataSourceProvider sourceProvider = DataProviderFactory.GetDataProvider(applicationGuid, providerGuid, Helper);
			return sourceProvider;
		}

		protected void SetIntegrationPoint(Job job)
		{
			if (this.IntegrationPoint != null)
			{
				return;
			}

			int integrationPointId = job.RelatedObjectArtifactID;
			this.IntegrationPoint = CaseServiceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointId);
			if (this.IntegrationPoint == null)
			{
				throw new ArgumentException("Failed to retrieve corresponding Integration Point Rdo.");
			}
		}

		protected void SetJobHistory()
		{
			if (this.JobHistory != null)
			{
				return;
			}

			// TODO: it is possible here that the Job Type is not Run Now - verify expected usage
			this.JobHistory = JobHistoryService.CreateRdo(this.IntegrationPoint, this.BatchInstance, JobTypeChoices.JobHistoryRun, DateTime.UtcNow);
			JobHistoryErrorService.JobHistory = this.JobHistory;
			JobHistoryErrorService.IntegrationPoint = this.IntegrationPoint;
		}
	}
}
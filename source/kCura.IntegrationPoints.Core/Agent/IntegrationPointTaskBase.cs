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
        protected ICaseServiceContext _caseServiceContext;
        protected readonly IHelper _helper;
        protected IDataProviderFactory _dataProviderFactory;
        protected kCura.Apps.Common.Utils.Serializers.ISerializer _serializer;
        protected IJobHistoryService _jobHistoryService;
        protected JobHistoryErrorService _jobHistoryErrorService;
        protected ISynchronizerFactory _appDomainRdoSynchronizerFactoryFactory;
        protected IJobManager _jobManager;
	    protected IManagerFactory _managerFactory;
	    protected IContextContainerFactory _contextContainerFactory;
	    protected IJobService _jobService;

	    private Tuple<IJobStopManager, long, Guid> _jobStopManagerDetailsTuple;


        public IntegrationPointTaskBase(
          ICaseServiceContext caseServiceContext,
          IHelper helper,
          IDataProviderFactory dataProviderFactory,
          kCura.Apps.Common.Utils.Serializers.ISerializer serializer,
          ISynchronizerFactory appDomainRdoSynchronizerFactoryFactory,
          IJobHistoryService jobHistoryService,
          JobHistoryErrorService jobHistoryErrorService,
          IJobManager jobManager,
		  IManagerFactory managerFactory,
		  IContextContainerFactory contextContainerFactory,
		  IJobService jobService)
        {
            _caseServiceContext = caseServiceContext;
            _helper = helper;
            _dataProviderFactory = dataProviderFactory;
            _serializer = serializer;
            _appDomainRdoSynchronizerFactoryFactory = appDomainRdoSynchronizerFactoryFactory;
            _jobHistoryService = jobHistoryService;
            _jobHistoryErrorService = jobHistoryErrorService;
            _jobManager = jobManager;
	        _managerFactory = managerFactory;
	        _jobService = jobService;
	        _contextContainerFactory = contextContainerFactory;

        }

        protected Data.IntegrationPoint IntegrationPoint { get; set; }
        protected Data.JobHistory JobHistory { get; set; }
        protected Guid BatchInstance { get; set; }

        private SourceProvider _sourceProvider;
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
                    _sourceProvider = _caseServiceContext.RsapiService.SourceProviderLibrary.Read(this.IntegrationPoint.SourceProvider.Value);
                }
                return _sourceProvider;
            }
        }

        private DestinationProvider _destinationProvider;
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
                    _destinationProvider = _caseServiceContext.RsapiService.DestinationProviderLibrary.Read(this.IntegrationPoint.DestinationProvider.Value);
                }
                return _destinationProvider;
            }
        }

		protected IJobStopManager GetJobStopManager(Job job)
		{
			if (_jobStopManagerDetailsTuple == null)
			{
				IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(_helper);
				TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
				int jobId = Convert.ToInt32(job.JobId);
				IJobStopManager jobStopManager = _managerFactory.CreateJobStopManager(contextContainer, _jobService,
					_jobHistoryService,
					taskParameters.BatchInstance, jobId);

				_jobStopManagerDetailsTuple = new Tuple<IJobStopManager, long, Guid>(jobStopManager, job.JobId, taskParameters.BatchInstance);
			}
			else
			{
				TaskParameters taskParameters = _serializer.Deserialize<TaskParameters>(job.JobDetails);
				if (_jobStopManagerDetailsTuple.Item2 != job.JobId || _jobStopManagerDetailsTuple.Item3 != taskParameters.BatchInstance)
				{
					IContextContainer contextContainer = _contextContainerFactory.CreateContextContainer(_helper);
					IJobStopManager jobStopManager = _managerFactory.CreateJobStopManager(contextContainer, _jobService,
	_jobHistoryService,
	taskParameters.BatchInstance, Convert.ToInt32(job.JobId));

					_jobStopManagerDetailsTuple = new Tuple<IJobStopManager, long, Guid>(jobStopManager, job.JobId, taskParameters.BatchInstance);
				}
			}

			return _jobStopManagerDetailsTuple.Item1;
		}

		protected void SetIntegrationPoint(Job job)
        {
            if (this.IntegrationPoint != null)
            {
                return;
            }

            int integrationPointId = job.RelatedObjectArtifactID;
            this.IntegrationPoint = _caseServiceContext.RsapiService.IntegrationPointLibrary.Read(integrationPointId);
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
            this.JobHistory = _jobHistoryService.CreateRdo(this.IntegrationPoint, this.BatchInstance, JobTypeChoices.JobHistoryRunNow, DateTime.UtcNow);
            _jobHistoryErrorService.JobHistory = this.JobHistory;
            _jobHistoryErrorService.IntegrationPoint = this.IntegrationPoint;
        }

        protected virtual string GetSourceConfiguration(string originalSourceConfiguration)
        {
            return originalSourceConfiguration;
        }

        protected virtual IEnumerable<FieldMap> GetFieldMap(string serializedFieldMappings)
        {
            IEnumerable<FieldMap> fieldMap = _serializer.Deserialize<List<FieldMap>>(serializedFieldMappings);
            fieldMap.ForEach(f => f.SourceField.IsIdentifier = f.FieldMapType == FieldMapTypeEnum.Identifier);
            return fieldMap;
        }

        protected virtual List<FieldEntry> GetSourceFields(IEnumerable<FieldMap> fieldMap)
        {
            return fieldMap.Select(f => f.SourceField).ToList();
        }

        protected virtual List<FieldEntry> GetDestinationFields(IEnumerable<FieldMap> fieldMap)
        {
            return fieldMap.Select(f => f.DestinationField).ToList();
        }

        protected virtual IEnumerable<IDictionary<FieldEntry, object>> GetSourceData(List<FieldEntry> sourceFields, IDataReader sourceDataReader)
        {
            var objectBuilder = new SynchronizerObjectBuilder(sourceFields);
            return new DataReaderToEnumerableService(objectBuilder).GetData<IDictionary<FieldEntry, object>>(sourceDataReader);
        }

        protected virtual IDataSourceProvider GetSourceProvider(SourceProvider sourceProviderRdo, Job job)
        {
			this.ThrowIfStopRequested(job);

            Guid applicationGuid = new Guid(sourceProviderRdo.ApplicationIdentifier);
            Guid providerGuid = new Guid(sourceProviderRdo.Identifier);
            IDataSourceProvider sourceProvider = _dataProviderFactory.GetDataProvider(applicationGuid, providerGuid, _helper);
            return sourceProvider;
        }

	    protected void ThrowIfStopRequested(Job job)
	    {
		    IJobStopManager jobStopManager = GetJobStopManager(job);

			jobStopManager.ThrowIfStopRequested();
	    }

	    protected virtual IDataSynchronizer GetDestinationProvider(DestinationProvider destinationProviderRdo, string configuration, Job job)
        {
			this.ThrowIfStopRequested(job);

            Guid providerGuid = new Guid(destinationProviderRdo.Identifier);
            var factory = _appDomainRdoSynchronizerFactoryFactory as GeneralWithCustodianRdoSynchronizerFactory;
            if (factory != null)
            {
                factory.TaskJobSubmitter = new TaskJobSubmitter(_jobManager, job, TaskType.SyncCustodianManagerWorker, this.BatchInstance);
                factory.SourceProvider = SourceProvider;
            }
            IDataSynchronizer sourceProvider = _appDomainRdoSynchronizerFactoryFactory.CreateSynchronizer(providerGuid, configuration);
            return sourceProvider;
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
				emailRecipientList = emailRecipients.Split(';').Select(x => x.Trim()).Where(x=>!String.IsNullOrWhiteSpace(x)).ToList();
			}

			return emailRecipientList;
        }
    }
}
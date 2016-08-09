using System;
using System.Collections.Generic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Contexts;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests.Unit.Tasks
{
	[TestFixture]
	public class ExportServiceManagerTests
	{
		private ExportServiceManager _instance;
		private IHelper _helper;
		private ICaseServiceContext _caseContext;
		private IContextContainerFactory _contextContainerFactory;
		private ISynchronizerFactory _synchronizerFactory;
		private IExporterFactory _exporterFactory;
		private IOnBehalfOfUserClaimsPrincipalFactory _claimPrincipleFactory;
		private ISourceWorkspaceManager _sourceWorkspaceManager;
		private ISourceJobManager _sourceJobManager;
		private IRepositoryFactory _repositoryFactory;
		private IManagerFactory _managerFactory;
		private IEnumerable<IBatchStatus> _batchStatuses;
		private ISerializer _serializer;
		private IJobService _jobService;
		private IScheduleRuleFactory _scheduleRuleFactory;
		private IJobHistoryService _jobHistoryService;
		private IJobHistoryErrorService _jobHistoryErrorService;
		private IJobHistoryManager _jobHistoryManager;
		private IContextContainer _contextContainer;
		private IJobHistoryErrorManager _jobHistoryErrorManager;
		private ISavedSearchRepository _savedSearchRepository;
		private readonly int _retrySavedSearchId = 312;
		private IJobStopManager _jobStopManager;

		[SetUp]
		public void SetUp()
		{
			_helper = Substitute.For<IHelper>();
			_caseContext = Substitute.For<ICaseServiceContext>();
			_contextContainerFactory = Substitute.For<IContextContainerFactory>();
			_synchronizerFactory = Substitute.For<ISynchronizerFactory>();
			_exporterFactory = Substitute.For<IExporterFactory>();
			_claimPrincipleFactory = Substitute.For<IOnBehalfOfUserClaimsPrincipalFactory>();
			_sourceWorkspaceManager = Substitute.For<ISourceWorkspaceManager>();
			_sourceJobManager = Substitute.For<ISourceJobManager>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_managerFactory = Substitute.For<IManagerFactory>();
			_batchStatuses = Substitute.For<IEnumerable<IBatchStatus>>();
			_serializer = Substitute.For<ISerializer>();
			_jobService = Substitute.For<IJobService>();
			_scheduleRuleFactory = Substitute.For<IScheduleRuleFactory>();
			_jobHistoryService = Substitute.For<IJobHistoryService>();
			_jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();

			_jobHistoryManager = Substitute.For<IJobHistoryManager>();
			_jobHistoryErrorManager = Substitute.For<IJobHistoryErrorManager>();
			_contextContainer = Substitute.For<IContextContainer>();
			_savedSearchRepository = Substitute.For<ISavedSearchRepository>();
			_jobStopManager = Substitute.For<IJobStopManager>();

			_contextContainerFactory.CreateContextContainer(_helper).Returns(_contextContainer);

			_instance = new ExportServiceManager(_helper,
				_caseContext, _contextContainerFactory,
				_synchronizerFactory, _exporterFactory,
				_claimPrincipleFactory, _sourceWorkspaceManager,
				_sourceJobManager, _repositoryFactory,
				_managerFactory, _batchStatuses, _serializer, _jobService, _scheduleRuleFactory, _jobHistoryService,
				_jobHistoryErrorService,
				null);
		}

		[Test]
		public void Execute_FailToLoadIntegrationPointRDO()
		{
			// ARRANGE
			Job job = JobExtensions.CreateJob();
			TaskParameters taskParameters = new TaskParameters();
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(taskParameters);
			_caseContext.RsapiService.IntegrationPointLibrary.Read(job.RelatedObjectArtifactID).Returns((Data.IntegrationPoint)null);
			_managerFactory.CreateJobStopManager(_jobService, _jobHistoryService, taskParameters.BatchInstance, job.JobId).Returns(_jobStopManager);
			
			// ACT
			_instance.Execute(job);

			// ASSERT
			_jobHistoryErrorService.Received(1).AddError(Arg.Is<Choice>(choice => choice.EqualsToChoice(ErrorTypeChoices.JobHistoryErrorJob)) , Arg.Is< ArgumentException>(ex => ex.Message == "Failed to retrieved corresponding Integration Point."));
			_jobHistoryErrorService.Received().CommitErrors();
		}

		[Test]
		public void Execute_StopAtTheVeryBeginningOfTheJob()
		{
			// ARRANGE
			Job job = JobExtensions.CreateJob();
			Data.IntegrationPoint integrationPoint = new Data.IntegrationPoint()
			{
				SourceConfiguration = "source config",
				SourceProvider = 741,
				FieldMappings = "mapping"
			};
			SourceConfiguration configuration = new SourceConfiguration();
			TaskParameters taskParameters = new TaskParameters();
			JobHistory jobHistory = new JobHistory() { JobType = JobTypeChoices.JobHistoryRun };
			SourceProvider sourceProvider = new SourceProvider();
			List<FieldMap> mappings = new List<FieldMap>();
			JobHistoryErrorDTO.UpdateStatusType statusType = new JobHistoryErrorDTO.UpdateStatusType();

			_jobStopManager.When(obj => obj.ThrowIfStopRequested()).Do(info => { throw new OperationCanceledException(); });
			_caseContext.RsapiService.IntegrationPointLibrary.Read(job.RelatedObjectArtifactID).Returns(integrationPoint);
			_serializer.Deserialize<SourceConfiguration>(integrationPoint.SourceConfiguration).Returns(configuration);
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(taskParameters);
			_jobHistoryService.CreateRdo(integrationPoint, taskParameters.BatchInstance, Arg.Any<DateTime>()).Returns(jobHistory);
			_caseContext.RsapiService.SourceProviderLibrary.Read(integrationPoint.SourceProvider.Value).Returns(sourceProvider);
			_serializer.Deserialize<List<FieldMap>>(integrationPoint.FieldMappings).Returns(mappings);
			_managerFactory.CreateJobHistoryErrorManager(_contextContainer, configuration.SourceWorkspaceArtifactId, GetUniqueJobId(job, taskParameters.BatchInstance)).Returns(_jobHistoryErrorManager);
			_jobHistoryErrorManager.StageForUpdatingErrors(job, Arg.Is<Choice>(obj => obj.EqualsToChoice(JobTypeChoices.JobHistoryRun))).Returns(statusType);
			_repositoryFactory.GetSavedSearchRepository(configuration.SourceWorkspaceArtifactId, configuration.SavedSearchArtifactId).Returns(_savedSearchRepository);
			_savedSearchRepository.RetrieveSavedSearch().Returns(new SavedSearchDTO());
			_jobHistoryErrorManager.CreateItemLevelErrorsSavedSearch(job, configuration.SavedSearchArtifactId).Returns(_retrySavedSearchId);
			_managerFactory.CreateJobStopManager(_jobService, _jobHistoryService, taskParameters.BatchInstance, job.JobId).Returns(_jobStopManager);

			// ACT
			_instance.Execute(job);

			// ASSERT
			_jobStopManager.Received().Dispose();
			_jobService.Received().UpdateStopState(Arg.Is<List<long>>(lst => lst.Contains(job.JobId)), StopState.Unstoppable);
			_jobHistoryErrorService.Received().CommitErrors();
		}

		[Test]
		public void Execute_StopAfterAcquiringTheSynchronizer()
		{
			// ARRANGE
			Job job = JobExtensions.CreateJob();
			Data.IntegrationPoint integrationPoint = new Data.IntegrationPoint()
			{
				SourceConfiguration = "source config",
				SourceProvider = 741,
				FieldMappings = "mapping",
				DestinationConfiguration = "destination config"
			};
			SourceConfiguration configuration = new SourceConfiguration();
			TaskParameters taskParameters = new TaskParameters();
			JobHistory jobHistory = new JobHistory() { JobType = JobTypeChoices.JobHistoryRun };
			SourceProvider sourceProvider = new SourceProvider();
			List<FieldMap> mappings = new List<FieldMap>();
			JobHistoryErrorDTO.UpdateStatusType statusType = new JobHistoryErrorDTO.UpdateStatusType();

			_jobStopManager.When(obj => obj.ThrowIfStopRequested()).Do(Callback.First(x => { }).Then(info => { throw new OperationCanceledException(); }));

			_caseContext.RsapiService.IntegrationPointLibrary.Read(job.RelatedObjectArtifactID).Returns(integrationPoint);
			_serializer.Deserialize<SourceConfiguration>(integrationPoint.SourceConfiguration).Returns(configuration);
			_serializer.Deserialize<TaskParameters>(job.JobDetails).Returns(taskParameters);
			_jobHistoryService.CreateRdo(integrationPoint, taskParameters.BatchInstance, Arg.Any<DateTime>()).Returns(jobHistory);
			_caseContext.RsapiService.SourceProviderLibrary.Read(integrationPoint.SourceProvider.Value).Returns(sourceProvider);
			_serializer.Deserialize<List<FieldMap>>(integrationPoint.FieldMappings).Returns(mappings);
			_managerFactory.CreateJobHistoryErrorManager(_contextContainer, configuration.SourceWorkspaceArtifactId, GetUniqueJobId(job, taskParameters.BatchInstance)).Returns(_jobHistoryErrorManager);
			_jobHistoryErrorManager.StageForUpdatingErrors(job, Arg.Is<Choice>(obj => obj.EqualsToChoice(JobTypeChoices.JobHistoryRun))).Returns(statusType);
			_repositoryFactory.GetSavedSearchRepository(configuration.SourceWorkspaceArtifactId, configuration.SavedSearchArtifactId).Returns(_savedSearchRepository);
			_savedSearchRepository.RetrieveSavedSearch().Returns(new SavedSearchDTO());
			_jobHistoryErrorManager.CreateItemLevelErrorsSavedSearch(job, configuration.SavedSearchArtifactId).Returns(_retrySavedSearchId);
			_managerFactory.CreateJobStopManager(_jobService, _jobHistoryService, taskParameters.BatchInstance, job.JobId).Returns(_jobStopManager);
			_serializer.Deserialize<ImportSettings>(integrationPoint.DestinationConfiguration).Returns(new ImportSettings());

			// ACT
			_instance.Execute(job);

			// ASSERT
			_jobStopManager.Received().Dispose();
			_jobService.Received().UpdateStopState(Arg.Is<List<long>>(lst => lst.Contains(job.JobId)), StopState.Unstoppable);
			_jobHistoryErrorService.Received().CommitErrors();
		}


		private string GetUniqueJobId(Job job, Guid identifier)
		{
			return job.JobId + "_" + identifier;
		}
	}
}
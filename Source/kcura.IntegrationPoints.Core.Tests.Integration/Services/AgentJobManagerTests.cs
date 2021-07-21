using System;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using NUnit.Framework;
using Relativity.API;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class AgentJobManagerTests : RelativityProviderTemplate
	{
		private const int _ADMIN_USER_ID = 9;
		private IRepositoryFactory _repositoryFactory;
		private AgentJobManager _manager;
		private IEddsServiceContext _eddsServiceContext;
		private IJobService _jobService;
		private IHelper _helper;
		private IIntegrationPointSerializer _serializer;
		private JobTracker _jobTracker;
		private IJobTrackerQueryManager _jobTrackerQueryManager;
		private JobResourceTracker _jobResource;
		private IScratchTableRepository _scratchTableRepository;
		private string _JOB_TRACKER_TABLE_PREFIX = "RIP_JobTracker";

		public AgentJobManagerTests()
			: base("IntegrationPointService Source", null)
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			_repositoryFactory = Container.Resolve<IRepositoryFactory>(); ;
			_jobService = Container.Resolve<IJobService>();
			_serializer = Container.Resolve<IIntegrationPointSerializer>();
			_helper = Container.Resolve<IHelper>();
			_jobTrackerQueryManager = Container.Resolve<IJobTrackerQueryManager>();
			_jobResource = new JobResourceTracker(_jobTrackerQueryManager);
			_jobTracker = new JobTracker(_jobResource);
			_eddsServiceContext = Container.Resolve<IEddsServiceContext>();
			_manager = new AgentJobManager(_eddsServiceContext, _jobService, _helper, _serializer, _jobTracker);
		}

		public override void TestTeardown()
		{
			if (this._scratchTableRepository != null)
			{
				_scratchTableRepository.Dispose();
			}

		}

		[IdentifiedTest("2fb60219-c275-4f77-b4fd-fd1467a33158")]
		public void VerifyCheckBatchOnCompleteNull()
		{
			//Arrange
			Guid batchInstance = Guid.NewGuid();
			Job job = new JobBuilder().WithJobId(1)
				.WithWorkspaceId(SourceWorkspaceArtifactID)
				.WithRelatedObjectArtifactId(1)
				.WithSubmittedBy(_ADMIN_USER_ID)
				.Build();

			//Act
			bool result = _manager.CheckBatchOnJobComplete(job, batchInstance.ToString());

			//Assert
			Assert.IsFalse(result);
		}
		
		[IdentifiedTest("ef786e0e-59b0-417e-b563-8a8687c159e8")]
		[SmokeTest]
		public void VerifyCheckBatchOnCompleteTrue()
		{
			//Arrange
			IntegrationPointModel integrationModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "test", "Append Only");
			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			int jobId = 1;
			int rootJobId = 1;
			Job job = new JobBuilder()
				.WithJobId(jobId)
				.WithRootJobId(rootJobId)
				.WithRelatedObjectArtifactId(integrationPoint.ArtifactID)
				.WithWorkspaceId(SourceWorkspaceArtifactID)
				.WithSubmittedBy(_ADMIN_USER_ID)
				.Build();

			Guid batchInstance = Guid.NewGuid();
			string scratchTableSuffix = $"{SourceWorkspaceArtifactID}_{rootJobId}_{batchInstance}";
			_scratchTableRepository = _repositoryFactory.GetScratchTableRepository(SourceWorkspaceArtifactID, _JOB_TRACKER_TABLE_PREFIX, scratchTableSuffix);

			_jobTracker.CreateTrackingEntry(job, batchInstance.ToString());

			//Act
			bool result = _manager.CheckBatchOnJobComplete(job, batchInstance.ToString());

			//Assert
			Assert.IsTrue(result);
		}

		[IdentifiedTest("9bd5cbf9-ecd0-45af-bd58-288a4139c688")]
		public void VerifyCheckBatchOnCompleteFalse()
		{
			//Arrange
			IntegrationPointModel integrationModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "CheckBatchOnCompleteFalse", "Append Only");
			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			int jobId = 10;
			int jobId2 = 20;
			int rootJobId = 11;
			Job job = new JobBuilder()
				.WithJobId(jobId)
				.WithRootJobId(rootJobId)
				.WithRelatedObjectArtifactId(integrationPoint.ArtifactID)
				.WithWorkspaceId(SourceWorkspaceArtifactID)
				.WithSubmittedBy(_ADMIN_USER_ID)
				.Build();
			Job job2 = new JobBuilder().WithJob(job).WithJobId(jobId2).Build();
			Guid batchInstance = Guid.NewGuid();
			string scratchTableSuffix = $"{SourceWorkspaceArtifactID}_{rootJobId}_{batchInstance}";
			_scratchTableRepository = _repositoryFactory.GetScratchTableRepository(SourceWorkspaceArtifactID, _JOB_TRACKER_TABLE_PREFIX, scratchTableSuffix);
			_jobTracker.CreateTrackingEntry(job, batchInstance.ToString());
			_jobTracker.CreateTrackingEntry(job2, batchInstance.ToString());

			//Act
			bool result = _manager.CheckBatchOnJobComplete(job, batchInstance.ToString());

			//Assert
			Assert.IsFalse(result);
		}
		
		[IdentifiedTest("E71A5440-0C2B-48ED-8A3B-6323307AA741")]
		public void VerifyCheckBatchOnCompleteFalse_WhenNotFinished()
		{
			//Arrange
			IntegrationPointModel integrationModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "CheckBatchOnCompleteFalse", "Append Only");
			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			int jobId = 10;
			int rootJobId = 11;
			Job job = new JobBuilder()
				.WithJobId(jobId)
				.WithRootJobId(rootJobId)
				.WithRelatedObjectArtifactId(integrationPoint.ArtifactID)
				.WithWorkspaceId(SourceWorkspaceArtifactID)
				.WithSubmittedBy(_ADMIN_USER_ID)
				.Build();

			Guid batchInstance = Guid.NewGuid();
			string scratchTableSuffix = $"{SourceWorkspaceArtifactID}_{rootJobId}_{batchInstance}";
			_scratchTableRepository = _repositoryFactory.GetScratchTableRepository(SourceWorkspaceArtifactID, _JOB_TRACKER_TABLE_PREFIX, scratchTableSuffix);
			_jobTracker.CreateTrackingEntry(job, batchInstance.ToString());

			//Act
			bool result = _manager.CheckBatchOnJobComplete(job, batchInstance.ToString(), false);

			//Assert
			Assert.IsFalse(result);
		}

		[IdentifiedTest("441c4be7-42ac-4812-bb4e-fef40e7b62bf")]
		public void VerifyDeleteJob()
		{
			//Arrange
			IntegrationPointModel integrationModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "Delete", "Append Only");
			IntegrationPointModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			int jobId = 1;
			Job job = new JobBuilder().WithJobId(jobId)
				.WithWorkspaceId(SourceWorkspaceArtifactID)
				.WithRelatedObjectArtifactId(integrationPoint.ArtifactID)
				.WithSubmittedBy(_ADMIN_USER_ID)
				.Build();

			//Act
			_manager.DeleteJob(job.JobId);

			//Assert
			Assert.Null(_jobService.GetJob(job.JobId));
		}
	}
}
using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoint.Tests.Core.Templates;
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

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	public class AgentJobManagerTests : RelativityProviderTemplate
	{
		private const int _ADMIN_USER_ID = 9;
		private IRepositoryFactory _repositoryFactory;
		private AgentJobManager _manager;
		private IEddsServiceContext _eddsServiceContext;
		private IJobService _jobService;
		private IHelper _helper;
		private ISerializer _serializer;
		private JobTracker _jobTracker;
		private IWorkspaceDBContext _workspaceDbContext;
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
			_serializer = Container.Resolve<ISerializer>();
			_helper = Container.Resolve<IHelper>();
			_workspaceDbContext = Container.Resolve<IWorkspaceDBContext>();
			_jobResource = new JobResourceTracker(_repositoryFactory, _workspaceDbContext);
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

		[Test]
		public void VerifyCheckBatchOnCompleteNull()
		{
			//Arrange
			Guid batchInstance = Guid.NewGuid();
			Job job = JobExtensions.CreateJob(SourceWorkspaceArtifactId, 1, _ADMIN_USER_ID, 1);

			//Act
			bool result = _manager.CheckBatchOnJobComplete(job, batchInstance.ToString());

			//Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void VerifyCheckBatchOnCompleteTrue()
		{
			//Arrange
			IntegrationModel integrationModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "test", "Append Only");
			IntegrationModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			int jobId = 1;
			int rootJobId = 1;
			Job job = JobExtensions.CreateJob(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, _ADMIN_USER_ID, jobId, rootJobId);

			Guid batchInstance = Guid.NewGuid();
			string scratchTableSuffix = $"{SourceWorkspaceArtifactId}_{rootJobId}_{batchInstance}";
			_scratchTableRepository = _repositoryFactory.GetScratchTableRepository(SourceWorkspaceArtifactId, _JOB_TRACKER_TABLE_PREFIX, scratchTableSuffix);

			_jobTracker.CreateTrackingEntry(job, batchInstance.ToString());

			//Act
			bool result = _manager.CheckBatchOnJobComplete(job, batchInstance.ToString());

			//Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void VerifyCheckBatchOnCompleteFalse()
		{
			//Arrange
			IntegrationModel integrationModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "CheckBatchOnCompleteFalse", "Append Only");
			IntegrationModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			int jobId = 10;
			int jobId2 = 20;
			int rootJobId = 11;
			Job job = JobExtensions.CreateJob(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, _ADMIN_USER_ID, jobId, rootJobId);
			Job job2 = JobExtensions.CreateJob(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, _ADMIN_USER_ID, jobId2, rootJobId);
			Guid batchInstance = Guid.NewGuid();
			string scratchTableSuffix = $"{SourceWorkspaceArtifactId}_{rootJobId}_{batchInstance}";
			_scratchTableRepository = _repositoryFactory.GetScratchTableRepository(SourceWorkspaceArtifactId, _JOB_TRACKER_TABLE_PREFIX, scratchTableSuffix);
			_jobTracker.CreateTrackingEntry(job, batchInstance.ToString());
			_jobTracker.CreateTrackingEntry(job2, batchInstance.ToString());

			//Act
			bool result = _manager.CheckBatchOnJobComplete(job, batchInstance.ToString());

			//Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void VerifyDeleteJob()
		{
			//Arrange
			IntegrationModel integrationModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, "Delete", "Append Only");
			IntegrationModel integrationPoint = CreateOrUpdateIntegrationPoint(integrationModel);

			int jobId = 1;
			Job job = JobExtensions.CreateJob(SourceWorkspaceArtifactId, integrationPoint.ArtifactID, _ADMIN_USER_ID, jobId);

			//Act
			_manager.DeleteJob(job.JobId);

			//Assert
			Assert.Null(_jobService.GetJob(job.JobId));
		}
	}
}
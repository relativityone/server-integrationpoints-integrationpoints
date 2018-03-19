using System;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Agent.TaskFactory;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client.DTOs;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.Core;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests.TaskFactory
{
	[TestFixture]
	public class TaskFactoryJobHistoryServiceTests
	{
		private ITaskFactoryJobHistoryService _sut;
		private IJobHistoryService _jobHistoryService;
		private IJobHistoryErrorService _jobHistoryErrorService;
		private IRelativityObjectManager _objectmanager;


		[Test]
		public void ItShouldUpdateJobHistoryWhenItDoesnNotHaveValue()
		{
			SetUp();
			// Arrange
			int jobId = 123;
			Job job = JobExtensions.CreateJob(jobId);

			var jobHistory = new JobHistory
			{
				JobID = null
			};
			_jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<Choice>(), Arg.Any<DateTime?>()).Returns(jobHistory);

			// Act
			_sut.SetJobIdOnJobHistory(job);

			// Assert
			_jobHistoryService.Received().UpdateRdo(Arg.Is<JobHistory>(h => h.JobID == jobId.ToString()));
		}

		[Test]
		public void ItShouldNotUpdateJobHistoryWhenItHasValue()
		{
			SetUp();
			// Arrange
			int jobId = 123;
			Job job = JobExtensions.CreateJob(jobId);

			var jobHistory = new JobHistory
			{
				JobID = jobId.ToString()
			};
			_jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<Choice>(), Arg.Any<DateTime?>()).Returns(jobHistory);

			// Act
			_sut.SetJobIdOnJobHistory(job);

			// Assert
			_jobHistoryService.DidNotReceiveWithAnyArgs().UpdateRdo(Arg.Any<JobHistory>());
		}

		[Test]
		public void ItShouldNotUpdateJobHistoryWhenHobHistoryServiceReturnNull()
		{
			SetUp();
			// Arrange
			Job job = JobExtensions.CreateJob();
			_jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<Choice>(), Arg.Any<DateTime?>()).Returns((JobHistory)null);

			// Act
			_sut.SetJobIdOnJobHistory(job);

			// Assert
			_jobHistoryService.DidNotReceiveWithAnyArgs().UpdateRdo(Arg.Any<JobHistory>());
		}

		[Test]
		public void ItShouldNotUpdateJobHistoryWhenJobWasNull()
		{
			// Arrange
			SetUp();
			Job job = null;
			var jobHistory = new JobHistory();
			_jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<Choice>(), Arg.Any<DateTime?>()).Returns(jobHistory);

			// Act
			_sut.SetJobIdOnJobHistory(job);

			// Assert
			_jobHistoryService.DidNotReceiveWithAnyArgs().UpdateRdo(Arg.Any<JobHistory>());
		}

		[TestCase(new[] { 123 }, 123, new int[] { })]
		[TestCase(new[] { 1, 2 }, 2, new[] { 1 })]
		[TestCase(new[] { 1, 2 }, 3, new[] { 1, 2 })]
		[TestCase(new int[] { }, 3, new int[] { })]
		public void ItShouldUpdateIntegrationPointWhenRemovingJobHistoryFromIntegrationPoint(int[] ipJobHistoryIds, int jobHistoryId, int[] expectedIpJobHistoryIdsAfterUpdate)
		{
			// Arrange 
			Data.IntegrationPoint ip = GetDefaultIntegrationPoint();
			ip.JobHistory = ipJobHistoryIds;
			SetUp(ip);

			Job job = JobExtensions.CreateJob();
			var jobHistory = new JobHistory
			{
				ArtifactId = jobHistoryId
			};
			_jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<Choice>(), Arg.Any<DateTime?>()).Returns(jobHistory);

			// Act
			_sut.RemoveJobHistoryFromIntegrationPoint(job);

			// Assert
			_objectmanager.Received()
				.Update(Arg.Is<Data.IntegrationPoint>(x => x.JobHistory.SequenceEqual(expectedIpJobHistoryIdsAfterUpdate)));
		}

		[Test]
		public void ItShouldUpdateJobHistoryStatusWhenRemovingJobHistoryFromIntegrationPoint()
		{
			// Arrange 
			Data.IntegrationPoint ip = GetDefaultIntegrationPoint();
			ip.JobHistory = new int[] { };
			SetUp(ip);

			Job job = JobExtensions.CreateJob();
			var jobHistory = new JobHistory();
			_jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<Choice>(), Arg.Any<DateTime?>()).Returns(jobHistory);

			// Act
			_sut.RemoveJobHistoryFromIntegrationPoint(job);

			// Assert
			_jobHistoryService.Received().UpdateRdo(Arg.Is<JobHistory>(x => x.JobStatus.ChoiceTypeID == JobStatusChoices.JobHistoryStopped.ChoiceTypeID));
		}

		[Test]
		public void ItShouldDeleteJobHistoryWhenRemovingJobHistoryFromIntegrationPoint()
		{
			// Arrange 
			Data.IntegrationPoint ip = GetDefaultIntegrationPoint();
			ip.JobHistory = new int[] { };
			SetUp(ip);

			Job job = JobExtensions.CreateJob();
			int jobHistoryArtifactId = 654;
			var jobHistory = new JobHistory
			{
				ArtifactId = jobHistoryArtifactId
			};
			_jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<Choice>(), Arg.Any<DateTime?>()).Returns(jobHistory);

			// Act
			_sut.RemoveJobHistoryFromIntegrationPoint(job);

			// Assert
			_jobHistoryService.Received().DeleteRdo(Arg.Is<int>(x => x == jobHistoryArtifactId));
		}

		[Test]
		public void ItShouldUpdateJobHistoryStatusOnFailure()
		{
			// Arrange
			SetUp();

			Job job = JobExtensions.CreateJob();
			int jobHistoryArtifactId = 654;
			var jobHistory = new JobHistory
			{
				ArtifactId = jobHistoryArtifactId
			};
			_jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<Choice>(), Arg.Any<DateTime?>()).Returns(jobHistory);

			// Act
			_sut.UpdateJobHistoryOnFailure(job, null);

			// Assert
			_jobHistoryService.Received().UpdateRdo(Arg.Is<JobHistory>(x => x.JobStatus.Name == JobStatusChoices.JobHistoryErrorJobFailed.Name));
		}

		[Test]
		public void ItShouldAddJobHistoryErrorOnFailure()
		{
			// Arrange
			int integrationPointArtifactId = 943438;
			var ip = GetDefaultIntegrationPoint();
			ip.ArtifactId = integrationPointArtifactId;
			SetUp(ip);

			Job job = JobExtensions.CreateJob();
			int jobHistoryArtifactId = 654;
			var jobHistory = new JobHistory
			{
				ArtifactId = jobHistoryArtifactId
			};
			_jobHistoryService.CreateRdo(Arg.Any<Data.IntegrationPoint>(), Arg.Any<Guid>(), Arg.Any<Choice>(), Arg.Any<DateTime?>()).Returns(jobHistory);

			string expectedExceptionMessage = "Expected exception message";
			var exception = new Exception(expectedExceptionMessage);

			// Act
			_sut.UpdateJobHistoryOnFailure(job, exception);

			// Assert
			Assert.AreEqual(integrationPointArtifactId, _jobHistoryErrorService.IntegrationPoint.ArtifactId);
			Assert.AreEqual(jobHistoryArtifactId, _jobHistoryErrorService.JobHistory.ArtifactId);
			_jobHistoryErrorService.Received().AddError(Arg.Is<Choice>(x=>x.Name == ErrorTypeChoices.JobHistoryErrorJob.Name), exception);
		}

		public void SetUp(Data.IntegrationPoint ip = null)
		{
			var helper = Substitute.For<IHelper>();
			var helperFactory = Substitute.For<IHelperFactory>();
			var serializer = Substitute.For<IIntegrationPointSerializer>();
			serializer.Deserialize<DestinationConfiguration>(Arg.Any<string>()).Returns(new DestinationConfiguration());
			serializer.Deserialize<TaskParameters>(Arg.Any<string>()).Returns(new TaskParameters());

			_jobHistoryService = Substitute.For<IJobHistoryService>();

			var serviceFactory = Substitute.For<IServiceFactory>();
			serviceFactory.CreateJobHistoryService(Arg.Any<IHelper>(), Arg.Any<IHelper>()).Returns(_jobHistoryService);
			_jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();

			var caseServiceContext = Substitute.For<ICaseServiceContext>();
			var rsapiService = Substitute.For<IRSAPIService>();
			_objectmanager = Substitute.For<IRelativityObjectManager>();
			rsapiService.RelativityObjectManager.Returns(_objectmanager);
			caseServiceContext.RsapiService.Returns(rsapiService);

			ip = ip ?? GetDefaultIntegrationPoint();
			_sut = new TaskFactoryJobHistoryService(helper, helperFactory, serializer, serviceFactory, _jobHistoryErrorService, caseServiceContext, ip);
		}

		private Data.IntegrationPoint GetDefaultIntegrationPoint()
		{
			return new Data.IntegrationPoint
			{
				DestinationConfiguration = string.Empty,
				SecuredConfiguration = string.Empty
			};
		}
	}
}

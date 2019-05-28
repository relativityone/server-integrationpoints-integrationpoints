using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration
{
	internal sealed class JobHistoryHelperTests : RelativityProviderTemplate
	{
		private JobHistoryHelper _instance;
		private JobHistory _jobHistory;

		public JobHistoryHelperTests() : base(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
		{
		}

		public override void TestSetup()
		{
			base.TestSetup();

			IntegrationPointModel integrationPointModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOverlay, "name", "Append Only");
			integrationPointModel = CreateOrUpdateIntegrationPoint(integrationPointModel);

			_jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPointModel.ArtifactID, Guid.NewGuid(), JobTypeChoices.JobHistoryRun);

			_instance = new JobHistoryHelper();
		}

		[IdentifiedTestCase("6179f801-c79a-40c2-8666-d12294f97672", "processing")]
		[IdentifiedTestCase("48c7190c-d686-4f03-bda8-5a68a2874bcd", "synchronizing")]
		[IdentifiedTestCase("23b8ffaa-9fd9-4983-a38a-a561c5acbeac", "creating tags")]
		public async Task ItShouldUpdateStatusToProcessing(string status)
		{
			Mock<IExtendedJob> job = new Mock<IExtendedJob>();
			job.Setup(x => x.JobHistoryId).Returns(_jobHistory.ArtifactId);
			job.Setup(x => x.WorkspaceId).Returns(WorkspaceArtifactId);

			// ACT
			await _instance.UpdateJobStatusAsync(status, job.Object, Helper).ConfigureAwait(false);

			// ASSERT
			IList<JobHistory> jobHistories = Container.Resolve<IJobHistoryService>().GetJobHistory(new[] {_jobHistory.ArtifactId});
			Assert.AreEqual(jobHistories.Count, 1);
			Assert.AreEqual(jobHistories[0].JobStatus.Name, JobStatusChoices.JobHistoryProcessing.Name);
		}

		[IdentifiedTestCase("b8f98d2b-2e26-4d49-a036-237737f4b176", "validating")]
		[IdentifiedTestCase("8699c77e-7cf6-4682-8278-8f2d22d0e38f", "checking permissions")]
		public async Task ItShouldUpdateStatusToValidating(string status)
		{
			Mock<IExtendedJob> job = new Mock<IExtendedJob>();
			job.Setup(x => x.JobHistoryId).Returns(_jobHistory.ArtifactId);
			job.Setup(x => x.WorkspaceId).Returns(WorkspaceArtifactId);

			// ACT
			await _instance.UpdateJobStatusAsync(status, job.Object, Helper).ConfigureAwait(false);

			// ASSERT
			IList<JobHistory> jobHistories = Container.Resolve<IJobHistoryService>().GetJobHistory(new[] {_jobHistory.ArtifactId});
			Assert.AreEqual(jobHistories.Count, 1);
			Assert.AreEqual(jobHistories[0].JobStatus.Name, JobStatusChoices.JobHistoryValidating.Name);
		}

		[IdentifiedTest("4acfe4c1-7f96-4100-8f27-0619bc9b78a1")]
		public async Task ItShouldMarkJobAsStopped()
		{
			Mock<IExtendedJob> job = new Mock<IExtendedJob>();
			job.Setup(x => x.JobHistoryId).Returns(_jobHistory.ArtifactId);
			job.Setup(x => x.WorkspaceId).Returns(WorkspaceArtifactId);

			// ACT
			await _instance.MarkJobAsStoppedAsync(job.Object, Helper).ConfigureAwait(false);

			// ASSERT
			IList<JobHistory> jobHistories = Container.Resolve<IJobHistoryService>().GetJobHistory(new[] {_jobHistory.ArtifactId});
			Assert.AreEqual(jobHistories.Count, 1);
			Assert.IsTrue(jobHistories[0].EndTimeUTC.HasValue);
			Assert.AreEqual(jobHistories[0].JobStatus.Name, JobStatusChoices.JobHistoryStopped.Name);
		}

		[IdentifiedTest("2aaf1fbe-e284-4dcd-8ef8-e5a65d6d33b2")]
		public async Task ItShouldMarkJobAsStarted()
		{
			const int jobId = 585535;

			Mock<IExtendedJob> job = new Mock<IExtendedJob>();
			job.Setup(x => x.JobHistoryId).Returns(_jobHistory.ArtifactId);
			job.Setup(x => x.JobId).Returns(jobId);
			job.Setup(x => x.WorkspaceId).Returns(WorkspaceArtifactId);

			// ACT
			await _instance.MarkJobAsStartedAsync(job.Object, Helper).ConfigureAwait(false);

			// ASSERT
			IList<JobHistory> jobHistories = Container.Resolve<IJobHistoryService>().GetJobHistory(new[] {_jobHistory.ArtifactId});
			Assert.AreEqual(jobHistories.Count, 1);
			Assert.IsTrue(jobHistories[0].StartTimeUTC.HasValue);
			Assert.AreEqual(jobHistories[0].JobID, jobId.ToString());
		}

		[IdentifiedTest("d7b641d0-11c1-4b74-831f-b0d05c417f3f")]
		public async Task ItShouldMarkJobAsCompletedWithoutErrors()
		{
			Mock<IExtendedJob> job = new Mock<IExtendedJob>();
			job.Setup(x => x.JobHistoryId).Returns(_jobHistory.ArtifactId);
			job.Setup(x => x.WorkspaceId).Returns(WorkspaceArtifactId);

			// ACT
			await _instance.MarkJobAsCompletedAsync(job.Object, Helper).ConfigureAwait(false);

			// ASSERT
			IList<JobHistory> jobHistories = Container.Resolve<IJobHistoryService>().GetJobHistory(new[] {_jobHistory.ArtifactId});
			Assert.AreEqual(jobHistories.Count, 1);
			Assert.IsTrue(jobHistories[0].EndTimeUTC.HasValue);
			Assert.AreEqual(jobHistories[0].JobStatus.Name, JobStatusChoices.JobHistoryCompleted.Name);
		}

		[IdentifiedTest("03e9f666-c6b4-4840-8190-c4d10205c88e")]
		public async Task ItShouldMarkJobAsCompletedWithErrors()
		{
			Mock<IExtendedJob> job = new Mock<IExtendedJob>();
			job.Setup(x => x.JobHistoryId).Returns(_jobHistory.ArtifactId);
			job.Setup(x => x.WorkspaceId).Returns(WorkspaceArtifactId);

			CreateJobHistoryError(_jobHistory.ArtifactId, ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem);

			// ACT
			await _instance.MarkJobAsCompletedAsync(job.Object, Helper).ConfigureAwait(false);

			// ASSERT
			IList<JobHistory> jobHistories = Container.Resolve<IJobHistoryService>().GetJobHistory(new[] {_jobHistory.ArtifactId});
			Assert.AreEqual(jobHistories.Count, 1);
			Assert.IsTrue(jobHistories[0].EndTimeUTC.HasValue);
			Assert.AreEqual(jobHistories[0].JobStatus.Name, JobStatusChoices.JobHistoryCompletedWithErrors.Name);
		}

		[IdentifiedTest("2cc7f339-77dc-4c8a-a56a-77632d1fdca4")]
		public async Task ItShouldMarkJobAsFailed()
		{
			Mock<IExtendedJob> job = new Mock<IExtendedJob>();
			job.Setup(x => x.JobHistoryId).Returns(_jobHistory.ArtifactId);
			job.Setup(x => x.WorkspaceId).Returns(WorkspaceArtifactId);


			InvalidOperationException exception = new InvalidOperationException();

			// ACT
			await _instance.MarkJobAsFailedAsync(job.Object, exception, Helper).ConfigureAwait(false);

			// ASSERT
			IList<JobHistory> jobHistories = Container.Resolve<IJobHistoryService>().GetJobHistory(new[] {_jobHistory.ArtifactId});
			Assert.AreEqual(jobHistories.Count, 1);
			Assert.IsTrue(jobHistories[0].EndTimeUTC.HasValue);
			Assert.AreEqual(jobHistories[0].JobStatus.Name, JobStatusChoices.JobHistoryErrorJobFailed.Name);

			IList<JobHistoryError> errors = GetJobHistoryError(_jobHistory.ArtifactId);
			Assert.AreEqual(errors.Count, 1);
			Assert.AreEqual(errors[0].Error, exception.Message);
			Assert.AreEqual(errors[0].StackTrace, exception.ToString());
			Assert.AreEqual(errors[0].ErrorStatus.Name, ErrorStatusChoices.JobHistoryErrorNew.Name);
			Assert.AreEqual(errors[0].ErrorType.Name, ErrorTypeChoices.JobHistoryErrorJob.Name);
		}

		private IList<JobHistoryError> GetJobHistoryError(int jobHistoryArtifactId)
		{
			RelativityObjectManagerFactory factory = new RelativityObjectManagerFactory(Helper);
			IRelativityObjectManager relativityObjectManager = factory.CreateRelativityObjectManager(WorkspaceArtifactId);

			QueryRequest queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Guid = Guid.Parse(ObjectTypeGuids.JobHistoryError)
				},
				Fields = new[]
				{
					new FieldRef
					{
						Guid = Guid.Parse(JobHistoryErrorFieldGuids.StackTrace)
					},
					new FieldRef
					{
						Guid = Guid.Parse(JobHistoryErrorFieldGuids.ErrorType)
					},
					new FieldRef
					{
						Guid = Guid.Parse(JobHistoryErrorFieldGuids.ErrorStatus)
					},
					new FieldRef
					{
						Guid = Guid.Parse(JobHistoryErrorFieldGuids.Error)
					}
				},
				Condition = CreateCondition(new[] {jobHistoryArtifactId})
			};
			return relativityObjectManager.Query<JobHistoryError>(queryRequest);
		}

		private string CreateCondition(IEnumerable<int> historiesId)
		{
			string historiesIdList = string.Join(",", historiesId.Select(x => x.ToString()));
			return $"'{JobHistoryErrorFields.JobHistory}' IN OBJECT [{historiesIdList}]";
		}
	}
}
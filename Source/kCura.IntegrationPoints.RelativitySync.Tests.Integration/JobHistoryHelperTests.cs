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

namespace kCura.IntegrationPoints.RelativitySync.Tests.Integration
{
	internal sealed class JobHistoryHelperTests : RelativityProviderTemplate
	{
		private JobHistoryHelper _instance;
		private JobHistory _jobHistory;

		public JobHistoryHelperTests() : base(Guid.NewGuid().ToString(), new Guid().ToString())
		{
		}

		public override void TestSetup()
		{
			base.TestSetup();

			IntegrationPointModel integrationPointModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOverlay, "name", "Append Only");
			integrationPointModel = CreateOrUpdateIntegrationPoint(integrationPointModel);

			_jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPointModel.ArtifactID, new Guid(), JobTypeChoices.JobHistoryRun);

			_instance = new JobHistoryHelper();
		}

		[Test]
		public async Task ItShouldMarkJobAsStopped()
		{
			const int jobId = 196427;

			Mock<IExtendedJob> job = new Mock<IExtendedJob>();
			job.Setup(x => x.JobHistoryId).Returns(_jobHistory.ArtifactId);
			job.Setup(x => x.JobId).Returns(jobId);
			job.Setup(x => x.WorkspaceId).Returns(WorkspaceArtifactId);

			// ACT
			await _instance.MarkJobAsStopped(job.Object, Helper).ConfigureAwait(false);

			// ASSERT
			IList<JobHistory> jobHistories = Container.Resolve<IJobHistoryService>().GetJobHistory(new[] {_jobHistory.ArtifactId});
			Assert.IsTrue(jobHistories.Count == 1);
			Assert.AreEqual(jobHistories[0].JobStatus.Name, JobStatusChoices.JobHistoryStopped.Name);
			Assert.AreEqual(jobHistories[0].JobID, jobId.ToString());
		}

		[Test]
		public async Task ItShouldMarkJobAsFailed()
		{
			const int jobId = 393971;

			Mock<IExtendedJob> job = new Mock<IExtendedJob>();
			job.Setup(x => x.JobHistoryId).Returns(_jobHistory.ArtifactId);
			job.Setup(x => x.JobId).Returns(jobId);
			job.Setup(x => x.WorkspaceId).Returns(WorkspaceArtifactId);


			InvalidOperationException exception = new InvalidOperationException();

			// ACT
			await _instance.MarkJobAsFailed(job.Object, exception, Helper).ConfigureAwait(false);

			// ASSERT
			IList<JobHistory> jobHistories = Container.Resolve<IJobHistoryService>().GetJobHistory(new[] {_jobHistory.ArtifactId});
			Assert.AreEqual(jobHistories.Count, 1);
			Assert.AreEqual(jobHistories[0].JobStatus.Name, JobStatusChoices.JobHistoryErrorJobFailed.Name);
			Assert.AreEqual(jobHistories[0].JobID, jobId.ToString());

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
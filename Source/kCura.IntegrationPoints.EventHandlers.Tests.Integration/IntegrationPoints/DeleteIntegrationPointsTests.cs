using System;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration.IntegrationPoints
{
	[TestFixture]
	public class DeleteIntegrationPointsTests : RelativityProviderTemplate
	{
		private IJobHistoryService _jobHistoryService;

		private Data.IntegrationPoint _integrationPoint;
		private JobHistory _jobHistory;
		private JobHistoryError _jobHistoryError;

		public DeleteIntegrationPointsTests() : base($"DeleteIP{Utils.FormatedDateTimeNow}", $"Destination{Utils.FormatedDateTimeNow}")
		{
		}

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			_jobHistoryService = Container.Resolve<IJobHistoryService>();
		}

		public override void TestSetup()
		{
			base.TestSetup();

			var integrationPointModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, $"IP{Utils.FormatedDateTimeNow}", "Append Only");
			integrationPointModel = CreateOrUpdateIntegrationPoint(integrationPointModel);
			_integrationPoint = CaseContext.RsapiService.IntegrationPointLibrary.Read(integrationPointModel.ArtifactID);

			_jobHistory = _jobHistoryService.CreateRdo(_integrationPoint, Guid.NewGuid(), JobTypeChoices.JobHistoryRun, DateTime.Now);

			var jobHistoryErrorArtifactId = CreateJobLevelJobHistoryError(_jobHistory.ArtifactId);
			_jobHistoryError = CaseContext.RsapiService.JobHistoryErrorLibrary.Read(jobHistoryErrorArtifactId);
		}

		[Test]
		public void ItShouldDeleteIntegrationPointWithJobHistory()
		{
			// ACT
			CaseContext.RsapiService.IntegrationPointLibrary.Delete(_integrationPoint.ArtifactId);

			// ASSERT
			var query = new Query<RDO>
			{
				Fields = FieldValue.NoFields,
				Condition = new WholeNumberCondition(ArtifactQueryFieldNames.ArtifactID, NumericConditionEnum.EqualTo, _integrationPoint.ArtifactId)
			};
			var integrationPoints = CaseContext.RsapiService.IntegrationPointLibrary.Query(query);

			Assert.That(integrationPoints, Is.Null.Or.Empty);
		}

		[Test]
		public void ItShouldDeleteIntegrationPointWithoutJobHistory()
		{
			var integrationPointModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, $"IP{Utils.FormatedDateTimeNow}", "Append Only");
			integrationPointModel = CreateOrUpdateIntegrationPoint(integrationPointModel);

			// ACT
			CaseContext.RsapiService.IntegrationPointLibrary.Delete(integrationPointModel.ArtifactID);

			// ASSERT
			var query = new Query<RDO>
			{
				Fields = FieldValue.NoFields,
				Condition = new WholeNumberCondition(ArtifactQueryFieldNames.ArtifactID, NumericConditionEnum.EqualTo, integrationPointModel.ArtifactID)
			};
			var integrationPoints = CaseContext.RsapiService.IntegrationPointLibrary.Query(query);

			Assert.That(integrationPoints, Is.Null.Or.Empty);
		}

		[Test]
		public void ItShouldUnlinkJobHistory()
		{
			// ACT
			CaseContext.RsapiService.IntegrationPointLibrary.Delete(_integrationPoint.ArtifactId);

			// ASSERT
			var jobHistory = CaseContext.RsapiService.JobHistoryLibrary.Read(_jobHistory.ArtifactId);

			Assert.That(jobHistory, Is.Not.Null);
			Assert.That(jobHistory.IntegrationPoint, Is.Null.Or.Empty);
		}

		[Test]
		public void ItShouldLeaveJobHistoryErrors()
		{
			// ACT
			CaseContext.RsapiService.IntegrationPointLibrary.Delete(_integrationPoint.ArtifactId);

			// ASSERT
			Query<RDO> query = new Query<RDO>
			{
				Condition = new WholeNumberCondition(ArtifactQueryFieldNames.ArtifactID, NumericConditionEnum.EqualTo, _jobHistoryError.ArtifactId)
			};
			var jobHistoryErrors = CaseContext.RsapiService.JobHistoryErrorLibrary.Query(query);

			Assert.That(jobHistoryErrors, Is.Not.Null);
			Assert.That(jobHistoryErrors, Is.Not.Empty);
		}

		private int CreateJobLevelJobHistoryError(int jobHistoryArtifactId)
		{
			JobHistoryError jobHistoryError = new JobHistoryError
			{
				ParentArtifactId = jobHistoryArtifactId,
				JobHistory = jobHistoryArtifactId,
				Name = Guid.NewGuid().ToString(),
				SourceUniqueID = null,
				ErrorType = ErrorTypeChoices.JobHistoryErrorItem,
				ErrorStatus = ErrorStatusChoices.JobHistoryErrorNew,
				Error = "Inserted Error for testing.",
				StackTrace = "Error created from EventHandlerTests",
				TimestampUTC = DateTime.Now
			};

			return CaseContext.RsapiService.JobHistoryErrorLibrary.Create(jobHistoryError);
		}
	}
}
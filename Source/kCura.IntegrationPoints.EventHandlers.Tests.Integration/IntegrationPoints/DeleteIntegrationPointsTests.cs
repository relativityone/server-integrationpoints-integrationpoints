using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration.IntegrationPoints
{
	[TestFixture]
	public class DeleteIntegrationPointsTests : RelativityProviderTemplate
	{
		private IJobHistoryService _jobHistoryService;

		private Data.IntegrationPoint _integrationPoint;
		private JobHistory _jobHistory;
		private JobHistoryError _jobHistoryError;

		public DeleteIntegrationPointsTests() : base($"DeleteIP{Utils.FormattedDateTimeNow}", $"Destination{Utils.FormattedDateTimeNow}")
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

			var integrationPointModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, $"IP{Utils.FormattedDateTimeNow}", "Append Only");
			integrationPointModel = CreateOrUpdateIntegrationPoint(integrationPointModel);
			_integrationPoint = CaseContext.RsapiService.RelativityObjectManager.Read<Data.IntegrationPoint>(integrationPointModel.ArtifactID);

			_jobHistory = _jobHistoryService.CreateRdo(_integrationPoint, Guid.NewGuid(), JobTypeChoices.JobHistoryRun, DateTime.Now);

			var jobHistoryErrorArtifactId = CreateJobLevelJobHistoryError(_jobHistory.ArtifactId);
			_jobHistoryError = CaseContext.RsapiService.RelativityObjectManager.Read<JobHistoryError>(jobHistoryErrorArtifactId);
		}

		[Test]
		public void ItShouldDeleteIntegrationPointWithJobHistory()
		{
			// ACT
			CaseContext.RsapiService.RelativityObjectManager.Delete(_integrationPoint.ArtifactId);

			// ASSERT
			var query = new Query<RDO>
			{
				Fields = FieldValue.NoFields,
				Condition = new WholeNumberCondition(ArtifactQueryFieldNames.ArtifactID, NumericConditionEnum.EqualTo, _integrationPoint.ArtifactId)
			};
			QueryRequest request = new QueryRequest()
			{
				Condition = $"'{ArtifactQueryFieldNames.ArtifactID}' == {_integrationPoint.ArtifactId}"
			};
			var integrationPoints = CaseContext.RsapiService.RelativityObjectManager.Query<Data.IntegrationPoint>(request);

			Assert.That(integrationPoints, Is.Null.Or.Empty);
		}

		[Test]
		public void ItShouldDeleteIntegrationPointWithoutJobHistory()
		{
			var integrationPointModel = CreateDefaultIntegrationPointModel(ImportOverwriteModeEnum.AppendOnly, $"IP{Utils.FormattedDateTimeNow}", "Append Only");
			integrationPointModel = CreateOrUpdateIntegrationPoint(integrationPointModel);

			// ACT
			CaseContext.RsapiService.RelativityObjectManager.Delete(integrationPointModel.ArtifactID);

			// ASSERT
			var query = new Query<RDO>
			{
				Fields = FieldValue.NoFields,
				Condition = new WholeNumberCondition(ArtifactQueryFieldNames.ArtifactID, NumericConditionEnum.EqualTo, integrationPointModel.ArtifactID)
			};
			var request = new QueryRequest
			{
				Condition = $"'{ArtifactQueryFieldNames.ArtifactID}' == {integrationPointModel.ArtifactID}",
			};
			List<Data.IntegrationPoint> integrationPoints = CaseContext.RsapiService.RelativityObjectManager.Query<Data.IntegrationPoint>(request);

			Assert.That(integrationPoints, Is.Null.Or.Empty);
		}

		[Test]
		public void ItShouldUnlinkJobHistory()
		{
			// ACT
			CaseContext.RsapiService.RelativityObjectManager.Delete(_integrationPoint.ArtifactId);

			// ASSERT
			var jobHistory = CaseContext.RsapiService.RelativityObjectManager.Read<JobHistory>(_jobHistory.ArtifactId);

			Assert.That(jobHistory, Is.Not.Null);
			Assert.That(jobHistory.IntegrationPoint, Is.Null.Or.Empty);
		}

		[Test]
		public void ItShouldLeaveJobHistoryErrors()
		{
			// ACT
			CaseContext.RsapiService.RelativityObjectManager.Delete(_integrationPoint.ArtifactId);

			// ASSERT
			var query = new QueryRequest
			{
				Condition = $"'{ArtifactQueryFieldNames.ArtifactID}' == {_jobHistoryError.ArtifactId}"
			};
			List<JobHistoryError> jobHistoryErrors = CaseContext.RsapiService.RelativityObjectManager.Query<JobHistoryError>(query);

			Assert.That(jobHistoryErrors, Is.Not.Null);
			Assert.That(jobHistoryErrors, Is.Not.Empty);
		}

		private int CreateJobLevelJobHistoryError(int jobHistoryArtifactId)
		{
			JobHistoryError jobHistoryError = new JobHistoryError
			{
				ParentArtifactId = jobHistoryArtifactId,
				Name = Guid.NewGuid().ToString(),
				SourceUniqueID = null,
				ErrorType = ErrorTypeChoices.JobHistoryErrorItem,
				ErrorStatus = ErrorStatusChoices.JobHistoryErrorNew,
				Error = "Inserted Error for testing.",
				StackTrace = "Error created from EventHandlerTests",
				TimestampUTC = DateTime.Now
			};

			return CaseContext.RsapiService.RelativityObjectManager.Create(jobHistoryError);
		}
	}
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;
using Relativity.Services.Objects.Exceptions;

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

			IntegrationPointModel integrationPointModel = CreateDefaultIntegrationPointModel(
				ImportOverwriteModeEnum.AppendOnly, 
				$"IP{Utils.FormattedDateTimeNow}", 
				"Append Only"
			);
			integrationPointModel = CreateOrUpdateIntegrationPoint(integrationPointModel);
			_integrationPoint = IntegrationPointRepository.ReadWithFieldMappingAsync(integrationPointModel.ArtifactID).GetAwaiter().GetResult();

			_jobHistory = _jobHistoryService.CreateRdo(_integrationPoint, Guid.NewGuid(), JobTypeChoices.JobHistoryRun, DateTime.Now);

			var jobHistoryErrorArtifactId = CreateJobLevelJobHistoryError(_jobHistory.ArtifactId);
			_jobHistoryError = CaseContext.RsapiService.RelativityObjectManager.Read<JobHistoryError>(jobHistoryErrorArtifactId);
		}

		[IdentifiedTest("e9f2a23c-3eb8-4abc-ba8d-6fe32cfa7d60")]
		public void ItShouldDeleteIntegrationPointWithJobHistory()
		{
			// ACT
			IntegrationPointRepository.Delete(_integrationPoint.ArtifactId);

			// ASSERT
			Func<Task> readAction = () => IntegrationPointRepository
				.ReadAsync(_integrationPoint.ArtifactId);

			readAction.ShouldThrow<IntegrationPointsException>()
				.WithInnerException<ArtifactNotFoundException>();
		}

		[IdentifiedTest("8c13cfb5-42d1-47de-91fb-6c7cc1858432")]
		public void ItShouldDeleteIntegrationPointWithoutJobHistory()
		{
			IntegrationPointModel integrationPointModel = CreateDefaultIntegrationPointModel(
				ImportOverwriteModeEnum.AppendOnly, 
				$"IP{Utils.FormattedDateTimeNow}", 
				"Append Only"
			);
			integrationPointModel = CreateOrUpdateIntegrationPoint(integrationPointModel);

			// ACT
			IntegrationPointRepository.Delete(integrationPointModel.ArtifactID);

			// ASSERT
			Func<Task> action = () => IntegrationPointRepository
				.ReadAsync(integrationPointModel.ArtifactID);

			action.ShouldThrow<IntegrationPointsException>()
				.WithInnerException<ArtifactNotFoundException>();
		}

		[IdentifiedTest("513f1d3b-d1fb-49a4-919e-b0a6ef33529d")]
		public void ItShouldUnlinkJobHistory()
		{
			// ACT
			IntegrationPointRepository.Delete(_integrationPoint.ArtifactId);

			// ASSERT
			var jobHistory = CaseContext.RsapiService.RelativityObjectManager.Read<JobHistory>(_jobHistory.ArtifactId);

			Assert.That(jobHistory, Is.Not.Null);
			Assert.That(jobHistory.IntegrationPoint, Is.Null.Or.Empty);
		}

		[IdentifiedTest("d8447c19-db91-4d65-82d5-19f90e3a6d7b")]
		public void ItShouldLeaveJobHistoryErrors()
		{
			// ACT
			IntegrationPointRepository.Delete(_integrationPoint.ArtifactId);

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
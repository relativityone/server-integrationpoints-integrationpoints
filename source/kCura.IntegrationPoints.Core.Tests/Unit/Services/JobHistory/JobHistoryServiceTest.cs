using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Unit.Services.JobHistory
{
	[TestFixture]
	public class JobHistoryServiceTest
	{
		private ICaseServiceContext _caseServiceContext;
		private IWorkspaceRepository _workspaceRepository;

		private JobHistoryService _instance;

		[SetUp]
		public void SetUp()
		{
			_caseServiceContext = Substitute.For<ICaseServiceContext>();
			_workspaceRepository = Substitute.For<IWorkspaceRepository>();

			_instance = new JobHistoryService(_caseServiceContext, _workspaceRepository);
		}

		[Test]
		public void GetRdo_Succeeds_Test()
		{
			// Arrange
			Guid batchInstance = new Guid();
			TextCondition expectedCondition = new TextCondition(Guid.Parse(JobHistoryFieldGuids.BatchInstance),
				TextConditionEnum.EqualTo, batchInstance.ToString());

			_caseServiceContext.RsapiService.JobHistoryLibrary
				.Query(Arg.Is<Query<RDO>>(x => x.ArtifactTypeGuid == Guid.Parse(ObjectTypeGuids.JobHistory) && x.Condition is TextCondition))
				.Returns(new List<Data.JobHistory>(1) { new Data.JobHistory() });

			// Act
			Data.JobHistory actual = _instance.GetRdo(batchInstance);

			// Assert
			Assert.IsNotNull(actual);

			_caseServiceContext.RsapiService.JobHistoryLibrary.Received(1)
				.Query(Arg.Is<Query<RDO>>(x =>
					x.ArtifactTypeGuid == Guid.Parse(ObjectTypeGuids.JobHistory) && x.Condition is TextCondition &&
						ValidateCondition<TextCondition>(x.Condition, expectedCondition)));
		}

		[Test]
		public void GetJobHistory_Succeeds_Test()
		{
			// Arrange
			IList<int> jobHistoryArtifactIds = new[] { 123, 456, 789 };
			WholeNumberCondition expectedCondition = new WholeNumberCondition("ArtifactID", NumericConditionEnum.In)
			{
				Value = jobHistoryArtifactIds.ToList()
			};

			_caseServiceContext.RsapiService.JobHistoryLibrary
				.Query(Arg.Is<Query<RDO>>(x => x.ArtifactTypeGuid == Guid.Parse(ObjectTypeGuids.JobHistory) && x.Condition is WholeNumberCondition))
				.Returns(new List<Data.JobHistory>(1) { new Data.JobHistory() });

			// Act
			IList<Data.JobHistory> actual = _instance.GetJobHistory(jobHistoryArtifactIds);

			// Assert
			Assert.IsNotNull(actual);

			_caseServiceContext.RsapiService.JobHistoryLibrary.Received(1)
				.Query(Arg.Is<Query<RDO>>(x =>
					x.ArtifactTypeGuid == Guid.Parse(ObjectTypeGuids.JobHistory) && x.Condition is WholeNumberCondition &&
						ValidateCondition<WholeNumberCondition>(x.Condition, expectedCondition)));
		}

		[Test]
		public void GetLastJobHistory_QueryReturnsMultipleJobHistories_Succeeds_Test()
		{
			// Arrange
			int[] jobHistoryArtifactIds = { 123, 456, 789 };
			int expectedMaxArtifactId = 789;
			Data.IntegrationPoint integrationPoint = new Data.IntegrationPoint { JobHistory = jobHistoryArtifactIds };

			_caseServiceContext.RsapiService.JobHistoryLibrary
				.Read(Arg.Is(expectedMaxArtifactId))
				.Returns(new Data.JobHistory {ArtifactId = expectedMaxArtifactId });

			// Act
			Data.JobHistory actual = _instance.GetLastJobHistory(integrationPoint.JobHistory.ToList());

			// Assert
			Assert.IsNotNull(actual);

			_caseServiceContext.RsapiService.JobHistoryLibrary.Received(1).Read(Arg.Is(expectedMaxArtifactId));
		}


		[Test]
		public void GetLastJobHistory_QueryReturnsNoJobHistories_Succeeds_Test()
		{
			// Arrange
			int[] jobHistoryArtifactIds = { 123, 456, 789 };
			int expectedMaxArtifactId = 789;
			Data.IntegrationPoint integrationPoint = new Data.IntegrationPoint { JobHistory = jobHistoryArtifactIds };

			_caseServiceContext.RsapiService.JobHistoryLibrary
				.Read(Arg.Is(expectedMaxArtifactId))
				.ReturnsNull();

			// Act
			Data.JobHistory actual = _instance.GetLastJobHistory(integrationPoint.JobHistory.ToList());

			// Assert
			Assert.IsNull(actual);

			_caseServiceContext.RsapiService.JobHistoryLibrary.Received(1).Read(Arg.Is(expectedMaxArtifactId));
		}

		[Test]
		public void GetLastJobHistory_IntegrationPointHasNoJobHistories_Succeeds_Test()
		{
			// Arrange
			int[] jobHistoryArtifactIds = new int[0];
			Data.IntegrationPoint integrationPoint = new Data.IntegrationPoint { JobHistory = jobHistoryArtifactIds };
			WholeNumberCondition expectedCondition = new WholeNumberCondition("ArtifactID", NumericConditionEnum.In)
			{
				Value = jobHistoryArtifactIds.ToList()
			};

			// Act
			Data.JobHistory actual = _instance.GetLastJobHistory(integrationPoint.JobHistory.ToList());

			// Assert
			Assert.IsNull(actual);

			_caseServiceContext.RsapiService.JobHistoryLibrary.DidNotReceive()
				.Query(Arg.Is<Query<RDO>>(x =>
					x.ArtifactTypeGuid == Guid.Parse(ObjectTypeGuids.JobHistory) && x.Condition is WholeNumberCondition &&
						ValidateCondition<WholeNumberCondition>(x.Condition, expectedCondition)));
		}

		[Test]
		public void UpdateRdo_Succeeds_Test()
		{
			// Arrange
			Data.JobHistory jobHistory = new Data.JobHistory { ArtifactId = 456, BatchInstance = new Guid().ToString() };

			// Act
			_instance.UpdateRdo(jobHistory);

			// Assert
			_caseServiceContext.RsapiService.JobHistoryLibrary.Received(1).Update(jobHistory);
		}

		[Test]
		public void UpdateJobHistoryOnRetry()
		{
			// Arrange
			Data.JobHistory jobHistory = new Data.JobHistory
			{
				ArtifactId = 8675309,
				JobStatus = JobStatusChoices.JobHistoryErrorJobFailed
			};
			IList<JobHistoryError> jobHistoryErrors = new List<JobHistoryError>(1) { new JobHistoryError() };

			_caseServiceContext.RsapiService.JobHistoryErrorLibrary
				.Query(Arg.Is<Query<RDO>>(x => x.ArtifactTypeGuid == Guid.Parse(ObjectTypeGuids.JobHistoryError) && x.Condition is CompositeCondition))
				.Returns(jobHistoryErrors);

			// Act
			_instance.UpdateJobHistoryOnRetry(jobHistory);

			// Assert
			_caseServiceContext.RsapiService.JobHistoryLibrary.Received(1).Update(jobHistory);
			_caseServiceContext.RsapiService.JobHistoryErrorLibrary.Received(1).Update(jobHistoryErrors);
		}

		[Test]
		public void DeleteJobHistory_Succeeds()
		{
			//Arrange
			int jobHistoryId = 12345;

			//Act
			_instance.DeleteRdo(jobHistoryId);

			//Assert
			_caseServiceContext.RsapiService.JobHistoryLibrary.Received(1).Delete(jobHistoryId);
		}

		private bool ValidateCondition<T>(Condition actualCondition, Condition expectedCondition)
		{
			if (actualCondition == null && expectedCondition == null)
			{
				return true;
			}
			if (actualCondition == null || expectedCondition == null)
			{
				return false;
			}
			if (!(actualCondition is T) || !(expectedCondition is T))
			{
				return false;
			}

			if (typeof(T) == typeof(WholeNumberCondition))
			{
				return ValidateWholeNumberCondition(actualCondition, expectedCondition);
			}
			if (typeof(T) == typeof(TextCondition))
			{
				return ValidateTextCondition(actualCondition, expectedCondition);
			}

			throw new NotImplementedException("Test helper has not implemented specific condition check. Please implement.");
		}

		private bool ValidateTextCondition(Condition actualCondition, Condition expectedCondition)
		{
			var actual = actualCondition as TextCondition;
			var expected = expectedCondition as TextCondition;

			if (actual == null || expected == null)
			{
				return false;
			}
			if (actual.Guid != expected.Guid)
			{
				return false;
			}
			if (actual.Field != expected.Field)
			{
				return false;
			}
			if (actual.Value != expected.Value)
			{
				return false;
			}
			if (actual.Operator != expected.Operator)
			{
				return false;
			}

			return true;
		}

		private bool ValidateWholeNumberCondition(Condition actualCondition, Condition expectedCondition)
		{
			var actual = actualCondition as WholeNumberCondition;
			var expected = expectedCondition as WholeNumberCondition;

			if (actual == null || expected == null)
			{
				return false;
			}
			if (actual.Guid != expected.Guid)
			{
				return false;
			}
			if (actual.Field != expected.Field)
			{
				return false;
			}
			if (actual.Value.Count != expected.Value.Count)
			{
				return false;
			}
			for (int i = 0; i < actual.Value.Count; i++)
			{
				if (actual.Value[i] != expected.Value[i])
				{
					return false;
				}
			}
			if (actual.Operator != expected.Operator)
			{
				return false;
			}

			return true;
		}
	}
}

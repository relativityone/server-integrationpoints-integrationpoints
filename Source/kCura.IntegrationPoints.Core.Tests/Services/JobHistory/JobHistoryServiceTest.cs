using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Services.JobHistory
{
	[TestFixture]
	public class JobHistoryServiceTest
	{
		private ICaseServiceContext _caseServiceContext;
		private IWorkspaceRepository _workspaceRepository;
		private IHelper _helper;

		private JobHistoryService _instance;
		private ISerializer _serializer;
		private Data.IntegrationPoint _integrationPoint;
		private ImportSettings _settings;
		private WorkspaceDTO _workspace;
		private int _jobHistoryArtifactId;
		private Guid _batchGuid;

		[SetUp]
		public void SetUp()
		{
			_caseServiceContext = Substitute.For<ICaseServiceContext>();
			_workspaceRepository = Substitute.For<IWorkspaceRepository>();
			_helper = Substitute.For<IHelper>();
			_serializer = Substitute.For<ISerializer>();

			_integrationPoint = new Data.IntegrationPoint()
			{
				ArtifactId = 98475,
				Name = "RIP RIP",
				DestinationConfiguration = "dest config"
			};
			_settings = new ImportSettings()
			{
				CaseArtifactId = 987
			};
			_workspace = new WorkspaceDTO()
			{
				Name = "lol"
			};
			_batchGuid = Guid.NewGuid();
			_jobHistoryArtifactId = 987465;
			_instance = new JobHistoryService(_caseServiceContext, _workspaceRepository, _helper, _serializer);
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
		public void DeleteJobHistory_Succeeds()
		{
			//Arrange
			int jobHistoryId = 12345;

			//Act
			_instance.DeleteRdo(jobHistoryId);

			//Assert
			_caseServiceContext.RsapiService.JobHistoryLibrary.Received(1).Delete(jobHistoryId);
		}

		[Test]
		public void CreateRdo_GoldFlow()
		{
			// ARRANGE
			_caseServiceContext.RsapiService.JobHistoryLibrary.Query(Arg.Any<Query<RDO>>()).Returns(new List<Data.JobHistory>());
			_serializer.Deserialize<ImportSettings>(_integrationPoint.DestinationConfiguration).Returns(_settings);
			_workspaceRepository.Retrieve(_settings.CaseArtifactId).Returns(_workspace);
			_caseServiceContext.RsapiService.JobHistoryLibrary.Create(Arg.Any<Data.JobHistory>()).Returns(_jobHistoryArtifactId);
			
			// ACT
			Data.JobHistory jobHistory = _instance.CreateRdo(_integrationPoint, _batchGuid, JobTypeChoices.JobHistoryRun,DateTime.Now);

			// ASSERT
			ValidateJobHistory(jobHistory, JobTypeChoices.JobHistoryRun);
		}

		[Test]
		public void CreateRdo_WhenGetRdoThrowsException_NewJobHistoryCreated()
		{
			// ARRANGE
			_caseServiceContext.RsapiService.JobHistoryLibrary.Query(Arg.Any<Query<RDO>>()).Throws(new Exception("blah blah"));
			_serializer.Deserialize<ImportSettings>(_integrationPoint.DestinationConfiguration).Returns(_settings);
			_workspaceRepository.Retrieve(_settings.CaseArtifactId).Returns(_workspace);
			_caseServiceContext.RsapiService.JobHistoryLibrary.Create(Arg.Any<Data.JobHistory>()).Returns(_jobHistoryArtifactId);

			// ACT
			Data.JobHistory jobHistory = _instance.CreateRdo(_integrationPoint, _batchGuid, JobTypeChoices.JobHistoryRun, DateTime.Now);

			// ASSERT
			ValidateJobHistory(jobHistory, JobTypeChoices.JobHistoryRun);
		}

		[Test]
		public void GetOrCreateSchduleRunHistoryRdo_FoundRdo()
		{
			// ARRANGE
			Data.JobHistory history = new Data.JobHistory();
			List<Data.JobHistory> jobHistories = new List<Data.JobHistory>() { history };
			_caseServiceContext.RsapiService.JobHistoryLibrary.Query(Arg.Any<Query<RDO>>()).Returns(jobHistories);

			// ACT
			Data.JobHistory returnedJobHistory = _instance.GetOrCreateScheduledRunHistoryRdo(_integrationPoint, _batchGuid, DateTime.Now);

			// ASSERT
			Assert.AreSame(history, returnedJobHistory);
		}

		[Test]
		public void GetOrCreateSchduleRunHistoryRdo_NoExistingRdo()
		{
			// ARRANGE
			_caseServiceContext.RsapiService.JobHistoryLibrary.Query(Arg.Any<Query<RDO>>()).Returns(new List<Data.JobHistory>());
			_serializer.Deserialize<ImportSettings>(_integrationPoint.DestinationConfiguration).Returns(_settings);
			_workspaceRepository.Retrieve(_settings.CaseArtifactId).Returns(_workspace);
			_caseServiceContext.RsapiService.JobHistoryLibrary.Create(Arg.Any<Data.JobHistory>()).Returns(_jobHistoryArtifactId);

			// ACT
			Data.JobHistory returnedJobHistory = _instance.GetOrCreateScheduledRunHistoryRdo(_integrationPoint, _batchGuid, DateTime.Now);

			// ASSERT
			ValidateJobHistory(returnedJobHistory, JobTypeChoices.JobHistoryScheduledRun);
		}


		[Test]
		public void GetOrCreateSchduleRunHistoryRdo_ErrorOnGetRdo()
		{
			// ARRANGE
			_caseServiceContext.RsapiService.JobHistoryLibrary.Query(Arg.Any<Query<RDO>>()).Throws(new Exception("blah blah"));
			_serializer.Deserialize<ImportSettings>(_integrationPoint.DestinationConfiguration).Returns(_settings);
			_workspaceRepository.Retrieve(_settings.CaseArtifactId).Returns(_workspace);
			_caseServiceContext.RsapiService.JobHistoryLibrary.Create(Arg.Any<Data.JobHistory>()).Returns(_jobHistoryArtifactId);

			// ACT
			Data.JobHistory returnedJobHistory = _instance.GetOrCreateScheduledRunHistoryRdo(_integrationPoint, _batchGuid, DateTime.Now);

			// ASSERT
			ValidateJobHistory(returnedJobHistory, JobTypeChoices.JobHistoryScheduledRun);
		}

		private void ValidateJobHistory(Data.JobHistory jobHistory, Relativity.Client.DTOs.Choice jobType)
		{
			Assert.IsNotNull(jobHistory);
			Assert.AreEqual(_jobHistoryArtifactId, jobHistory.ArtifactId);
			Assert.IsTrue(jobHistory.IntegrationPoint.SequenceEqual(new[] { _integrationPoint.ArtifactId }));
			Assert.AreEqual(_batchGuid.ToString(), jobHistory.BatchInstance);
			Assert.IsTrue(jobHistory.JobType.EqualsToChoice(jobType));
			Assert.IsTrue(jobHistory.JobStatus.EqualsToChoice(JobStatusChoices.JobHistoryPending));
			Assert.AreEqual(0, jobHistory.ItemsTransferred);
			Assert.AreEqual(0, jobHistory.ItemsWithErrors);
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
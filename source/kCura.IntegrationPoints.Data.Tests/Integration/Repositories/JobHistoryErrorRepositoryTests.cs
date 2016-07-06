﻿using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration.Repositories
{
	[TestFixture]
	[Category("Integration Tests")]
	public class JobHistoryErrorRepositoryTests : RelativityProviderTemplate
	{
		private IJobHistoryErrorRepository _instance;
		private JobHistory _jobHistory;

		public JobHistoryErrorRepositoryTests()
			: base("JobHistoryErrorRepositoryTests", null)
		{
		}

		[SetUp]
		public void TestSetup()
		{
			var repositoryFactory = Container.Resolve<IRepositoryFactory>();
			_instance = repositoryFactory.GetJobHistoryErrorRepository(SourceWorkspaceArtifactId);
			IntegrationModel integrationModel = new IntegrationModel
			{
				Destination = CreateDefaultDestinationConfig(),
				DestinationProvider = DestinationProvider.ArtifactId,
				SourceProvider = RelativityProvider.ArtifactId,
				SourceConfiguration = CreateDefaultSourceConfig(),
				LogErrors = true,
				Name = "JobHistoryErrorRepositoryTests" + DateTime.Now,
				SelectedOverwrite = "Overlay Only",
				Scheduler = new Scheduler()
				{
					EnableScheduler = false
				},
				HasErrors = true,
				Map = CreateDefaultFieldMap()
			};

			//Create an Integration Point and assign a Job History
			IntegrationModel integrationPointCreated = CreateOrUpdateIntegrationPoint(integrationModel);
			Guid batchInstance = Guid.NewGuid();
			_jobHistory = CreateJobHistoryOnIntegrationPoint(integrationPointCreated.ArtifactID, batchInstance);
		}

		[Test]
		[TestCase(JobHistoryErrorDTO.Choices.ErrorType.Values.Item)]
		[TestCase(JobHistoryErrorDTO.Choices.ErrorType.Values.Job)]
		public void RetrieveJobHistoryErrorArtifactIds_NoError(JobHistoryErrorDTO.Choices.ErrorType.Values errorType)
		{
			//act
			ICollection<int> result = _instance.RetrieveJobHistoryErrorArtifactIds(_jobHistory.ArtifactId, errorType);

			//assert
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Count);
		}


		private object[] RetrieveJobHistoryErrorSources = new []
		{
			new object[] {ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorItem},
			new object[] {ErrorStatusChoices.JobHistoryErrorInProgress, ErrorTypeChoices.JobHistoryErrorItem},
			new object[] {ErrorStatusChoices.JobHistoryErrorRetried, ErrorTypeChoices.JobHistoryErrorItem},
			new object[] {ErrorStatusChoices.JobHistoryErrorExpired, ErrorTypeChoices.JobHistoryErrorItem},
			new object[] {ErrorStatusChoices.JobHistoryErrorNew, ErrorTypeChoices.JobHistoryErrorJob},
			new object[] {ErrorStatusChoices.JobHistoryErrorInProgress, ErrorTypeChoices.JobHistoryErrorJob},
			new object[] {ErrorStatusChoices.JobHistoryErrorRetried, ErrorTypeChoices.JobHistoryErrorJob},
			new object[] {ErrorStatusChoices.JobHistoryErrorExpired, ErrorTypeChoices.JobHistoryErrorJob},
		};

		[Test]
		[TestCaseSource(nameof(RetrieveJobHistoryErrorSources))]
		public void RetrieveJobHistoryErrorArtifactIds(Choice errorStatus, Choice errorType)
		{
			// arrange
			List<int> jobHistoryArtifactId = CreateJobHistoryError(_jobHistory.ArtifactId, errorStatus, errorType);
			JobHistoryErrorDTO.Choices.ErrorType.Values type = errorType == ErrorTypeChoices.JobHistoryErrorItem
				? JobHistoryErrorDTO.Choices.ErrorType.Values.Item
				: JobHistoryErrorDTO.Choices.ErrorType.Values.Job;

			// act
			ICollection<int> result = _instance.RetrieveJobHistoryErrorArtifactIds(_jobHistory.ArtifactId, type);

			// assert
			Assert.IsNotNull(result);
			CollectionAssert.AreEqual(jobHistoryArtifactId, result);
		}

		[Test]
		[TestCase(JobHistoryErrorDTO.Choices.ErrorType.Values.Item)]
		[TestCase(JobHistoryErrorDTO.Choices.ErrorType.Values.Job)]
		public void RetrieveJobHistoryErrorIdsAndSourceUniqueIds_NoError(JobHistoryErrorDTO.Choices.ErrorType.Values errorType)
		{
			//act
			IDictionary<int, string> result = _instance.RetrieveJobHistoryErrorIdsAndSourceUniqueIds(_jobHistory.ArtifactId, errorType);

			//assert
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Count);
		}

		[Test]
		[TestCaseSource(nameof(RetrieveJobHistoryErrorSources))]
		public void RetrieveJobHistoryErrorIdsAndSourceUniqueIds(Choice errorStatus, Choice errorType)
		{
			// arrange
			List<int> jobHistoryArtifactId = CreateJobHistoryError(_jobHistory.ArtifactId, errorStatus, errorType);
			JobHistoryErrorDTO.Choices.ErrorType.Values type = errorType == ErrorTypeChoices.JobHistoryErrorItem
				? JobHistoryErrorDTO.Choices.ErrorType.Values.Item
				: JobHistoryErrorDTO.Choices.ErrorType.Values.Job;

			// act
			ICollection<int> result = _instance.RetrieveJobHistoryErrorArtifactIds(_jobHistory.ArtifactId, type);

			// assert
			Assert.IsNotNull(result);
			CollectionAssert.AreEqual(jobHistoryArtifactId, result);
		}
	}
}
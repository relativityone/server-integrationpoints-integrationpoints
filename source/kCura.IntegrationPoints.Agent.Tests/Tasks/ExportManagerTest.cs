using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Core.Tests;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests.Tasks
{
	public class ExportManagerTest
	{
		#region Private Fields

		private ExportManager _instanceToTest;

		private IJobManager _jobManagerMock;
		private ICaseServiceContext _caseServiceContextMock;
		private IContextContainerFactory _contextContainerFactoryMock;
		private IHelper _helperMock;
		private IManagerFactory _managerFactoryMock;
		private ISerializer _serializerMock;
		private IRepositoryFactory _repositoryFactoryMock;

		private readonly Job _job = JobHelper.GetJob(1, 2, 3, 4, 5, 6, 7, TaskType.ExportWorker,
				DateTime.MinValue, DateTime.MinValue, null, 1, DateTime.MinValue, 2, "", null);

		#endregion //Private Fields

		[SetUp]
		public void Init()
		{
			_contextContainerFactoryMock = Substitute.For<IContextContainerFactory>();
			_jobManagerMock = Substitute.For<IJobManager>();
			_caseServiceContextMock = Substitute.For<ICaseServiceContext>();
			_helperMock = Substitute.For<IHelper>();
			_managerFactoryMock = Substitute.For<IManagerFactory>();
			_serializerMock = Substitute.For<ISerializer>();
			_repositoryFactoryMock = Substitute.For<IRepositoryFactory>();

			_instanceToTest = new ExportManager(Substitute.For<ICaseServiceContext>(),
				Substitute.For<IDataProviderFactory>(),
				_jobManagerMock,
				Substitute.For<IJobService>(),
				_helperMock,
				Substitute.For<IIntegrationPointService>(),
				_serializerMock,
				Substitute.For<IGuidService>(),
				Substitute.For<IJobHistoryService>(),
				Substitute.For<JobHistoryErrorService>(_caseServiceContextMock, _helperMock),
				Substitute.For<IScheduleRuleFactory>(),
				_managerFactoryMock,
				_contextContainerFactoryMock,
				new List<IBatchStatus>(),
				_repositoryFactoryMock);
		}

		[Test]
		[TestCase(0, 10, -20)]
		[TestCase(901, 1000, 100)]
		public void ItShouldReturnTotalExportDocsCount(int expectedExportTotalDocsCount, int totalSavedSearchCount, int startIndex)
		{
			// Arrange
			IntegrationPointDTO integrationPointDto = new IntegrationPointDTO {SourceConfiguration = "Source Configuration"};
			ExportUsingSavedSearchSettings sourceConfiguration = new ExportUsingSavedSearchSettings()
			{
				SavedSearchArtifactId = 1,
				StartExportAtRecord = startIndex
			};

			IContextContainer contextContainerMock = Substitute.For<IContextContainer>();
			IIntegrationPointManager integrationPointManagerMock = Substitute.For<IIntegrationPointManager>();
			ISavedSearchRepository savedSearchRepositoryMock = Substitute.For<ISavedSearchRepository>();

			integrationPointManagerMock.Read(_job.WorkspaceID, _job.RelatedObjectArtifactID).Returns(integrationPointDto);
			savedSearchRepositoryMock.GetTotalDocsCount().Returns(totalSavedSearchCount);

			_contextContainerFactoryMock.CreateContextContainer(_helperMock).Returns(contextContainerMock);
			_managerFactoryMock.CreateIntegrationPointManager(contextContainerMock).Returns(integrationPointManagerMock);
			_serializerMock.Deserialize<ExportUsingSavedSearchSettings>(integrationPointDto.SourceConfiguration)
				.Returns(sourceConfiguration);
			_repositoryFactoryMock.GetSavedSearchRepository(_job.WorkspaceID, sourceConfiguration.SavedSearchArtifactId)
				.Returns(savedSearchRepositoryMock);

			// Act
			int retTotalCount = _instanceToTest.BatchTask(_job, null);

			// Assert
			Assert.That(retTotalCount, Is.EqualTo(expectedExportTotalDocsCount));

			if (expectedExportTotalDocsCount > 0)
			{
				_jobManagerMock.Received().CreateJobWithTracker(_job, Arg.Any<TaskParameters>(), TaskType.ExportWorker, Arg.Any<string>());
				Assert.That(_instanceToTest.BatchJobCount, Is.EqualTo(1));
			}
		}

		[Test]
		public void ItShouldReturnExportWorker()
		{
			_instanceToTest.CreateBatchJob(_job, new List<string>());

			_jobManagerMock.Received().CreateJobWithTracker(_job, Arg.Any<TaskParameters>(), TaskType.ExportWorker, Arg.Any<string>());
		}

		[Test]
		public void ItShouldReturnNullBatch()
		{
			var unbatchedIDs = _instanceToTest.GetUnbatchedIDs(_job);
			Assert.That(!unbatchedIDs.Any());
		}
	}
}


















using System;
using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
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
using kCura.IntegrationPoints.Domain.Models;
using kCura.ScheduleQueue.Core;
using kCura.ScheduleQueue.Core.ScheduleRules;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests.Tasks
{
	public class ExportManagerTest : TestBase
	{
		#region Private Fields

		private ExportManager _instanceToTest;

		private IJobManager _jobManagerMock;
		private ICaseServiceContext _caseServiceContextMock;
		private IContextContainerFactory _contextContainerFactoryMock;
		private IHelper _helperMock;
		private IManagerFactory _managerFactoryMock;
		private ISerializer _serializerMock;
		private IExportInitProcessService _exportInitProcessService;

		private readonly Job _job = JobHelper.GetJob(1, 2, 3, 4, 5, 6, 7, TaskType.ExportWorker,
				DateTime.MinValue, DateTime.MinValue, null, 1, DateTime.MinValue, 2, "", null);

		#endregion //Private Fields

		[SetUp]
		public override void SetUp()
		{
			_contextContainerFactoryMock = Substitute.For<IContextContainerFactory>();
			_jobManagerMock = Substitute.For<IJobManager>();
			_caseServiceContextMock = Substitute.For<ICaseServiceContext>();
			_helperMock = Substitute.For<IHelper>();
			_managerFactoryMock = Substitute.For<IManagerFactory>();
			_serializerMock = Substitute.For<ISerializer>();
			_exportInitProcessService = Substitute.For<IExportInitProcessService>();

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
				_exportInitProcessService);
		}

		[TestCase(10)]
		[TestCase(0)]
		public void ItShouldReturnTotalExportDocsCount(int totalSavedSearchCount)
		{
			// Arrange
			const int artifactTypeId = 1;
			IntegrationPointDTO integrationPointDto = new IntegrationPointDTO
			{
				SourceConfiguration = "Source Configuration",
				DestinationConfiguration = $"{{ArtifactTypeId: {artifactTypeId}}}"
			};
			ExportUsingSavedSearchSettings sourceConfiguration = new ExportUsingSavedSearchSettings()
			{
				SavedSearchArtifactId = 1,
			};

			IContextContainer contextContainerMock = Substitute.For<IContextContainer>();
			IIntegrationPointManager integrationPointManagerMock = Substitute.For<IIntegrationPointManager>();

			integrationPointManagerMock.Read(_job.WorkspaceID, _job.RelatedObjectArtifactID).Returns(integrationPointDto);
			_exportInitProcessService.CalculateDocumentCountToTransfer(sourceConfiguration, artifactTypeId).Returns(totalSavedSearchCount);

			_contextContainerFactoryMock.CreateContextContainer(_helperMock).Returns(contextContainerMock);
			_managerFactoryMock.CreateIntegrationPointManager(contextContainerMock).Returns(integrationPointManagerMock);
			_serializerMock.Deserialize<ExportUsingSavedSearchSettings>(integrationPointDto.SourceConfiguration)
				.Returns(sourceConfiguration);

			// Act
			int retTotalCount = _instanceToTest.BatchTask(_job, null);

			// Assert
			Assert.That(retTotalCount, Is.EqualTo(totalSavedSearchCount));

			_jobManagerMock.Received(totalSavedSearchCount > 0 ? 1 : 0).CreateJobWithTracker(_job, Arg.Any<TaskParameters>(), TaskType.ExportWorker, Arg.Any<string>());
			Assert.That(_instanceToTest.BatchJobCount, Is.EqualTo(totalSavedSearchCount > 0 ? 1 : 0));
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


















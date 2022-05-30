using System;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core.Core;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoints.Agent.Tests
{
	[TestFixture, Category("Unit")]
	public class RelativitySyncConstrainsCheckerTests
	{
		private Mock<IIntegrationPointService> _integrationPointService;
		private Mock<IProviderTypeService> _providerTypeService;
		private Mock<IJobHistoryService> _jobHistoryService;
		private Job _job;

		private Mock<ISerializer> _configurationDeserializer;
		private SourceConfiguration _sourceConfiguration;
		private ImportSettings _importSettings;
		private TaskParameters _taskParameters;

		private readonly int _integrationPointId = 123;
		private readonly int _sourceProviderId = 987;
		private readonly int _destinationProviderId = 789;
		private readonly string _sourceConfigurationString = "Source Configuration";
		private readonly string _destinationConfigurationString = "Destination Configuration";

		private RelativitySyncConstrainsChecker _instance;

		[SetUp]
		public void SetUp()
		{
			_job = JobHelper.GetJob(1, 2, 3, 4, 5, 6, _integrationPointId, TaskType.ExportWorker,
				DateTime.MinValue, DateTime.MinValue, string.Empty, 1, DateTime.MinValue, 2, "", null);

			_sourceConfiguration = new SourceConfiguration { TypeOfExport = SourceConfiguration.ExportType.SavedSearch };

			_importSettings = new ImportSettings
            {
                ImageImport = false,
                ProductionImport = false,
                ArtifactTypeId = (int)ArtifactType.Document
            };

			_taskParameters = new TaskParameters();

			var integrationPoint = new Data.IntegrationPoint
			{
				SourceConfiguration = _sourceConfigurationString,
				DestinationConfiguration = _destinationConfigurationString,
				SourceProvider = _sourceProviderId,
				DestinationProvider = _destinationProviderId,
			};

			var log = new Mock<IAPILog>();

			_integrationPointService = new Mock<IIntegrationPointService>();
			_integrationPointService.Setup(s => s.ReadIntegrationPoint(_integrationPointId)).Returns(integrationPoint);


			_configurationDeserializer = new Mock<ISerializer>();
			_configurationDeserializer.Setup(d => d.Deserialize<SourceConfiguration>(_sourceConfigurationString))
				.Returns(_sourceConfiguration);
			_configurationDeserializer.Setup(d => d.Deserialize<ImportSettings>(_destinationConfigurationString))
				.Returns(_importSettings);
			_configurationDeserializer.Setup(x => x.Deserialize<TaskParameters>(It.IsAny<string>()))
				.Returns(_taskParameters);

			_providerTypeService = new Mock<IProviderTypeService>();
			_providerTypeService.Setup(s => s.GetProviderType(_sourceProviderId, _destinationProviderId))
				.Returns(ProviderType.Relativity);

			_jobHistoryService = new Mock<IJobHistoryService>();

			_instance = new RelativitySyncConstrainsChecker(_integrationPointService.Object,
				_providerTypeService.Object, _configurationDeserializer.Object, log.Object);
		}

		[Test]
		public void ItShouldAllowUsingSyncWorkflow()
		{
			JobHistory jobHistory = new JobHistory()
			{
				JobType = JobTypeChoices.JobHistoryRun
			};
			_jobHistoryService.Setup(x => x.GetRdo(It.IsAny<Guid>())).Returns(jobHistory);
			_providerTypeService.Setup(s => s.GetProviderType(_sourceProviderId, _destinationProviderId))
				.Returns(ProviderType.Relativity);
			_sourceConfiguration.TypeOfExport = SourceConfiguration.ExportType.SavedSearch;
			_importSettings.ImageImport = false;
			_importSettings.ProductionImport = false;

			bool result = _instance.ShouldUseRelativitySync(_job);

			Assert.IsTrue(result);
		}

		[TestCase(SourceConfiguration.ExportType.ProductionSet, false, false, ExpectedResult = false)]
		[TestCase(SourceConfiguration.ExportType.ProductionSet, true, false, ExpectedResult = false)]
		[TestCase(SourceConfiguration.ExportType.ProductionSet, false, true, ExpectedResult = false)]
		[TestCase(SourceConfiguration.ExportType.ProductionSet, true, true, ExpectedResult = false)]
		[TestCase(SourceConfiguration.ExportType.SavedSearch, false, true, ExpectedResult = false)]
		[TestCase(SourceConfiguration.ExportType.SavedSearch, true, true, ExpectedResult = false)]

		// allowed flows
		[TestCase(SourceConfiguration.ExportType.SavedSearch, true, false, ExpectedResult = true)]
		[TestCase(SourceConfiguration.ExportType.SavedSearch, false, false, ExpectedResult = true)]
		public bool ShouldUseRelativitySync_ShouldControlWorkflow(SourceConfiguration.ExportType typeOfExport, bool imageImport, bool productionImport)
		{
			// Arrange
			_sourceConfiguration.TypeOfExport = typeOfExport;
			_importSettings.ImageImport = imageImport;
			_importSettings.ProductionImport = productionImport;

			// Act
			bool result = _instance.ShouldUseRelativitySync(_job);

			// Assert
			return result;
		}

		[TestCase(ProviderType.FTP)]
		[TestCase(ProviderType.ImportLoadFile)]
		[TestCase(ProviderType.LDAP)]
		[TestCase(ProviderType.LoadFile)]
		[TestCase(ProviderType.Other)]
		public void ItShouldNotAllowUsingSyncWorkflowWithNonRelativityProviderType(ProviderType nonRelativityProviderType)
		{
			_providerTypeService.Setup(s => s.GetProviderType(_sourceProviderId, _destinationProviderId))
				.Returns(nonRelativityProviderType);

			bool result = _instance.ShouldUseRelativitySync(_job);

			Assert.IsFalse(result);
		}

		[Test]
		public void ItShouldNotAllowUsingSyncWorkflowWhenIntegrationPointServiceThrows()
		{
			_integrationPointService.Setup(s => s.ReadIntegrationPoint(_integrationPointId)).Throws<Exception>();

			bool result = _instance.ShouldUseRelativitySync(_job);

			Assert.IsFalse(result);
		}

		[Test]
		public void ItShouldNotAllowUsingSyncWorkflowWhenProviderTypeServiceThrows()
		{
			_providerTypeService.Setup(s => s.GetProviderType(_sourceProviderId, _destinationProviderId)).Throws<Exception>();

			bool result = _instance.ShouldUseRelativitySync(_job);

			Assert.IsFalse(result);
		}

		[Test]
		public void ItShouldNotAllowUsingSyncWorkflowWhenConfigurationDeserializerForSourceConfigThrows()
		{
			_configurationDeserializer.Setup(s => s.Deserialize<SourceConfiguration>(_sourceConfigurationString)).Throws<Exception>();

			bool result = _instance.ShouldUseRelativitySync(_job);

			Assert.IsFalse(result);
		}

		[Test]
		public void ItShouldNotAllowUsingSyncWorkflowWhenConfigurationDeserializerForImportSettingsThrows()
		{
			_configurationDeserializer.Setup(s => s.Deserialize<ImportSettings>(_destinationConfigurationString)).Throws<Exception>();

			bool result = _instance.ShouldUseRelativitySync(_job);

			Assert.IsFalse(result);
		}

		[Test]
		public void ItShouldAllowUsingSyncWorkflowWhenRunningScheduledJob()
		{
			_job.ScheduleRuleType = "scheduled rule";

			bool result = _instance.ShouldUseRelativitySync(_job);

			Assert.IsTrue(result);
		}

		[Test]
		public void ItShouldAllowUsingSyncWorkflowWhenRetryingJob()
		{
			JobHistory jobHistory = new JobHistory()
			{
				JobType = JobTypeChoices.JobHistoryRetryErrors
			};
			_jobHistoryService.Setup(x => x.GetRdo(It.IsAny<Guid>())).Returns(jobHistory);

			bool result = _instance.ShouldUseRelativitySync(_job);

			Assert.IsTrue(result);
		}
	}
}

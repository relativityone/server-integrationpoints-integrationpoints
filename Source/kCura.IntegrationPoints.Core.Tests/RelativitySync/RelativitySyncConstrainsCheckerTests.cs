using System;
using System.Collections.Generic;
using FluentAssertions;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Common.Toggles;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.RelativitySync;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Moq;
using NUnit.Framework;
using Relativity;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.RelativitySync
{
    [TestFixture]
    [Category("Unit")]
    public class RelativitySyncConstrainsCheckerTests
    {
        private const int _INTEGRATION_POINT_ID = 123;
        private const int _SOURCE_PROVIDER_ID = 987;
        private const int _DESTINATION_PROVIDER_ID = 789;
        private const string _SOURCE_CONFIGURATION = "Source Configuration";
        private const string _DESTINATION_CONFIGURATION = "Destination Configuration";
        private Mock<IProviderTypeService> _providerTypeService;
        private Mock<IRelativityObjectManager> _relativityObjectManager;
        private Mock<IRipToggleProvider> _toggleProvider;
        private Job _job;
        private Mock<ISerializer> _configurationDeserializer;
        private SourceConfiguration _sourceConfiguration;
        private DestinationConfiguration _importSettings;
        private TaskParameters _taskParameters;
        private RelativitySyncConstrainsChecker _sut;

        [SetUp]
        public void SetUp()
        {
            _job = JobHelper.GetJob(
                jobId: 1,
                rootJobId: 2,
                parentJobId: 3,
                agentTypeId: 4,
                lockedByAgentId: 5,
                workspaceId: 6,
                relatedObjectArtifactId: _INTEGRATION_POINT_ID,
                taskType: TaskType.ExportWorker,
                nextRunTime: DateTime.MinValue,
                lastRunTime: DateTime.MinValue,
                jobDetails: string.Empty,
                jobFlags: 1,
                submittedDate: DateTime.MinValue,
                submittedBy: 2,
                scheduleRuleType: string.Empty,
                serializedScheduleRule: null);

            _sourceConfiguration = new SourceConfiguration { TypeOfExport = SourceConfiguration.ExportType.SavedSearch };

            _importSettings = new DestinationConfiguration
            {
                ImageImport = false,
                ProductionImport = false,
                ArtifactTypeId = (int)ArtifactType.Document
            };

            _taskParameters = new TaskParameters();

            var integrationPoint = new Data.IntegrationPoint
            {
                SourceConfiguration = _SOURCE_CONFIGURATION,
                DestinationConfiguration = _DESTINATION_CONFIGURATION,
                SourceProvider = _SOURCE_PROVIDER_ID,
                DestinationProvider = _DESTINATION_PROVIDER_ID,
            };

            var log = new Mock<IAPILog>();

            _configurationDeserializer = new Mock<ISerializer>();
            _configurationDeserializer.Setup(d => d.Deserialize<SourceConfiguration>(_SOURCE_CONFIGURATION))
                .Returns(_sourceConfiguration);
            _configurationDeserializer.Setup(d => d.Deserialize<DestinationConfiguration>(_DESTINATION_CONFIGURATION))
                .Returns(_importSettings);
            _configurationDeserializer.Setup(x => x.Deserialize<TaskParameters>(It.IsAny<string>()))
                .Returns(_taskParameters);

            _relativityObjectManager = new Mock<IRelativityObjectManager>();
            _relativityObjectManager.Setup(x => x.Read<Data.IntegrationPoint>(
                    _INTEGRATION_POINT_ID,
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<ExecutionIdentity>()))
                .Returns(new Data.IntegrationPoint
                {
                    ArtifactId = _INTEGRATION_POINT_ID,
                    SourceConfiguration = _SOURCE_CONFIGURATION,
                    DestinationConfiguration = _DESTINATION_CONFIGURATION,
                    SourceProvider = _SOURCE_PROVIDER_ID,
                    DestinationProvider = _DESTINATION_PROVIDER_ID,
                });

            _toggleProvider = new Mock<IRipToggleProvider>();
            _toggleProvider.Setup(x => x.IsEnabled<EnableRelativitySyncApplicationToggle>())
                .Returns(false);

            _providerTypeService = new Mock<IProviderTypeService>();
            _providerTypeService.Setup(s => s.GetProviderType(_SOURCE_PROVIDER_ID, _DESTINATION_PROVIDER_ID))
                .Returns(ProviderType.Relativity);

            _sut = new RelativitySyncConstrainsChecker(
                _relativityObjectManager.Object,
                _providerTypeService.Object,
                _toggleProvider.Object,
                _configurationDeserializer.Object,
                log.Object);
        }

        [Test]
        public void ShouldUseRelativitySync_ShouldAllowUsingSyncWorkflow()
        {
            // Act
            bool result = _sut.ShouldUseRelativitySync(_INTEGRATION_POINT_ID);

            // Assert
            result.Should().BeTrue();
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
            bool result = _sut.ShouldUseRelativitySync(_INTEGRATION_POINT_ID);

            // Assert
            return result;
        }

        [TestCase(ProviderType.FTP)]
        [TestCase(ProviderType.ImportLoadFile)]
        [TestCase(ProviderType.LDAP)]
        [TestCase(ProviderType.LoadFile)]
        [TestCase(ProviderType.Other)]
        public void ShouldUseRelativitySync_ShouldNotAllowUsingSyncWorkflow_WithNonRelativityProviderType(ProviderType providerType)
        {
            // Arrange
            _providerTypeService.Setup(s => s.GetProviderType(_SOURCE_PROVIDER_ID, _DESTINATION_PROVIDER_ID))
                .Returns(providerType);

            // Act
            bool result = _sut.ShouldUseRelativitySync(_INTEGRATION_POINT_ID);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldUseRelativitySync_ShouldNotAllowUsingSyncWorkflow_WhenIntegrationPointReadThrows()
        {
            // Arrange
            _relativityObjectManager.Setup(x => x.Read<Data.IntegrationPoint>(
                    _INTEGRATION_POINT_ID,
                    It.IsAny<IEnumerable<Guid>>(),
                    It.IsAny<ExecutionIdentity>()))
                .Throws<Exception>();

            // Act
            bool result = _sut.ShouldUseRelativitySync(_INTEGRATION_POINT_ID);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldUseRelativitySync_ShouldNotAllowUsingSyncWorkflow_WhenProviderTypeServiceThrows()
        {
            // Arrange
            _providerTypeService.Setup(s => s.GetProviderType(_SOURCE_PROVIDER_ID, _DESTINATION_PROVIDER_ID))
                .Throws<Exception>();

            // Act
            bool result = _sut.ShouldUseRelativitySync(_INTEGRATION_POINT_ID);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldUseRelativitySync_ShouldNotAllowUsingSyncWorkflow_WhenConfigurationDeserializerForSourceConfigThrows()
        {
            // Arrange
            _configurationDeserializer.Setup(s => s.Deserialize<SourceConfiguration>(_SOURCE_CONFIGURATION)).Throws<Exception>();

            // Act
            bool result = _sut.ShouldUseRelativitySync(_INTEGRATION_POINT_ID);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldUseRelativitySync_ShouldNotAllowUsingSyncWorkflow_WhenConfigurationDeserializerForImportSettingsThrows()
        {
            // Arrange
            _configurationDeserializer.Setup(s => s.Deserialize<DestinationConfiguration>(_DESTINATION_CONFIGURATION)).Throws<Exception>();

            // Act
            bool result = _sut.ShouldUseRelativitySync(_INTEGRATION_POINT_ID);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public void ShouldUseRelativitySync_ShouldAllowUsingSyncWorkflow_WhenRunningScheduledJob()
        {
            // Arrange
            _job.ScheduleRuleType = "scheduled rule";

            // Act
            bool result = _sut.ShouldUseRelativitySync(_INTEGRATION_POINT_ID);

            // Assert
            result.Should().BeTrue();
        }

        [TestCase(false, ExpectedResult = false)]
        [TestCase(true, ExpectedResult = true)]
        public bool ShouldUseRelativitySyncAppAsync_ShouldDetermineSyncWorkflowThroughSyncRAPUsage(bool toggleValue)
        {
            // Arrange
            _toggleProvider.Setup(x => x.IsEnabled<EnableRelativitySyncApplicationToggle>())
                .Returns(toggleValue);

            // Act
            bool result = _sut.ShouldUseRelativitySyncApp(_INTEGRATION_POINT_ID);

            // Assert
            return result;
        }
    }
}

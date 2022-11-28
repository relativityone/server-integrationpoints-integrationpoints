using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.Commands;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
    [TestFixture, Category("Unit")]
    public class SetTypeOfExportDefaultValueCommandTests : TestBase
    {
        private List<Data.IntegrationPoint> _integrationPoints;
        private List<IntegrationPointProfileDto> _integrationPointProfiles;
        private IIntegrationPointRepository _integrationPointRepository;
        private IIntegrationPointProfileService _integrationPointProfileService;
        private IRelativityObjectManager _objectManager;
        private ISourceConfigurationTypeOfExportUpdater _sourceConfigurationTypeOfExportUpdater;

        public override void SetUp()
        {
            _sourceConfigurationTypeOfExportUpdater = Substitute.For<ISourceConfigurationTypeOfExportUpdater>();
            _objectManager = Substitute.For<IRelativityObjectManager>();
            SetUpIntegrationPointsMock();
            SetUpIntegrationPointProfilesMock();
        }

        [Test]
        public void Execute_UpdaterReturnsNull_DontUpdate()
        {
            _sourceConfigurationTypeOfExportUpdater
                .GetCorrectedSourceConfiguration(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string>()).Returns((string)null);
            var cmd = new SetTypeOfExportDefaultValueCommand(
                _integrationPointRepository,
                _integrationPointProfileService,
                _sourceConfigurationTypeOfExportUpdater);

            cmd.Execute();

            _integrationPointRepository.DidNotReceive().Update(Arg.Any<Data.IntegrationPoint>());
            _objectManager.DidNotReceive().Update(Arg.Any<IntegrationPointProfile>());
        }

        [Test]
        public void Execute_NoIntegrationPoints_DontUpdate()
        {
            _integrationPoints.Clear();
            _integrationPointProfiles.Clear();
            _sourceConfigurationTypeOfExportUpdater
                .GetCorrectedSourceConfiguration(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string>()).Returns("Source config");
            var cmd = new SetTypeOfExportDefaultValueCommand(
                _integrationPointRepository,
                _integrationPointProfileService,
                _sourceConfigurationTypeOfExportUpdater);

            cmd.Execute();

            _integrationPointRepository.DidNotReceive().Update(Arg.Any<Data.IntegrationPoint>());
            _objectManager.DidNotReceive().Update(Arg.Any<IntegrationPointProfile>());
        }

        [Test]
        public void Execute_UpdaterReturnsConfig_Updates()
        {
            _sourceConfigurationTypeOfExportUpdater
                .GetCorrectedSourceConfiguration(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string>()).Returns("Source config");

            var cmd = new SetTypeOfExportDefaultValueCommand(
                _integrationPointRepository,
                _integrationPointProfileService,
                _sourceConfigurationTypeOfExportUpdater);

            cmd.Execute();

            _integrationPointRepository.Received().Update(Arg.Any<Data.IntegrationPoint>());
            _integrationPointProfileService.Received().UpdateConfiguration(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>());
        }

        private void SetUpIntegrationPointProfilesMock()
        {
            PopulateIntegrationPointProfilesList();

            _integrationPointProfileService = Substitute.For<IIntegrationPointProfileService>();
            _integrationPointProfileService.ReadAll().Returns(_integrationPointProfiles);
        }

        private void PopulateIntegrationPointProfilesList()
        {
            _integrationPointProfiles =
                new List<IntegrationPointProfileDto>
                {
                    new IntegrationPointProfileDto
                    {
                        SourceProvider = (int) ProviderType.Relativity,
                        DestinationProvider = 789,
                    }
                };
        }

        private void SetUpIntegrationPointsMock()
        {
            PopulateIntegrationPointsList();

            _integrationPointRepository = Substitute.For<IIntegrationPointRepository>();
            _integrationPointRepository.ReadAll().Returns(_integrationPoints);
        }

        private void PopulateIntegrationPointsList()
        {
            _integrationPoints =
                new List<Data.IntegrationPoint>
                {
                    new Data.IntegrationPoint
                    {
                        SourceProvider = (int) ProviderType.Relativity,
                        DestinationProvider = 789,
                        SourceConfiguration = "Source configuration"
                    }
                };
        }
    }
}

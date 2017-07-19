﻿using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.Commands;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Commands
{
	public class SetTypeOfExportDefaultValueCommandTests : TestBase
    {
        private List<Data.IntegrationPoint> _integrationPoints;
        private List<IntegrationPointProfile> _integrationPointProfiles;
        private IIntegrationPointService _integrationPointService;
        private IIntegrationPointProfileService _integrationPointProfileService;
        private IGenericLibrary<Data.IntegrationPoint> _integrationPointLibrary;
        private IGenericLibrary<IntegrationPointProfile> _integrationPointProfileLibrary;
        private ISourceConfigurationTypeOfExportUpdater _sourceConfigurationTypeOfExportUpdater;

        public override void SetUp()
        {
	        _sourceConfigurationTypeOfExportUpdater = Substitute.For<ISourceConfigurationTypeOfExportUpdater>();
            SetUpIntegrationPointsMock();
            SetUpIntegrationPointProfilesMock();
		}

	    [Test]
	    public void Execute_UpdaterReturnsNull_DontUpdate()
	    {
		    _sourceConfigurationTypeOfExportUpdater
			    .GetCorrectedSourceConfiguration(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string>()).Returns((string)null);
		    var cmd = new SetTypeOfExportDefaultValueCommand(_integrationPointService, _integrationPointProfileService,
			    _integrationPointLibrary, _integrationPointProfileLibrary, _sourceConfigurationTypeOfExportUpdater);

		    cmd.Execute();

		    _integrationPointLibrary.DidNotReceive().Update(Arg.Any<Data.IntegrationPoint>());
		    _integrationPointProfileLibrary.DidNotReceive().Update(Arg.Any<IntegrationPointProfile>());
		}

	    [Test]
	    public void Execute_NoIntegrationPoints_DontUpdate()
	    {
			_integrationPoints.Clear();
			_integrationPointProfiles.Clear();
		    _sourceConfigurationTypeOfExportUpdater
			    .GetCorrectedSourceConfiguration(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string>()).Returns("Source config");
		    var cmd = new SetTypeOfExportDefaultValueCommand(_integrationPointService, _integrationPointProfileService,
			    _integrationPointLibrary, _integrationPointProfileLibrary, _sourceConfigurationTypeOfExportUpdater);

		    cmd.Execute();

		    _integrationPointLibrary.DidNotReceive().Update(Arg.Any<Data.IntegrationPoint>());
		    _integrationPointProfileLibrary.DidNotReceive().Update(Arg.Any<IntegrationPointProfile>());
	    }

		[Test]
	    public void Execute_UpdaterReturnsConfig_Updates()
	    {
			_sourceConfigurationTypeOfExportUpdater
				.GetCorrectedSourceConfiguration(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string>()).Returns("Source config");

		    var cmd = new SetTypeOfExportDefaultValueCommand(_integrationPointService, _integrationPointProfileService,
			    _integrationPointLibrary, _integrationPointProfileLibrary, _sourceConfigurationTypeOfExportUpdater);

		    cmd.Execute();

		    _integrationPointLibrary.Received().Update(Arg.Any<Data.IntegrationPoint>());
		    _integrationPointProfileLibrary.Received().Update(Arg.Any<IntegrationPointProfile>());
		}

        private void SetUpIntegrationPointProfilesMock()
        {
            PopulateIntegrationPointProfilesList();

            _integrationPointProfileService = Substitute.For<IIntegrationPointProfileService>();
            _integrationPointProfileService.GetAllRDOs().Returns(_integrationPointProfiles);
            _integrationPointProfileLibrary = Substitute.For<IGenericLibrary<IntegrationPointProfile>>();
        }

        private void PopulateIntegrationPointProfilesList()
        {
	        _integrationPointProfiles =
		        new List<IntegrationPointProfile>
		        {
			        new IntegrationPointProfile
			        {
				        SourceProvider = (int) ProviderType.Relativity,
				        DestinationProvider = 789,
				        SourceConfiguration = "Source configuration"
			        }
		        };
        }

        private void SetUpIntegrationPointsMock()
        {
            PopulateIntegrationPointsList();

            _integrationPointService = Substitute.For<IIntegrationPointService>();
            _integrationPointService.GetAllRDOs().Returns(_integrationPoints);
            _integrationPointLibrary = Substitute.For<IGenericLibrary<Data.IntegrationPoint>>();
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
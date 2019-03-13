﻿using System;
using kCura.IntegrationPoints.Agent.Toggles;
using kCura.IntegrationPoints.Core.Contracts.Agent;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Core.Tests;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.ScheduleQueue.Core;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Agent.Tests
{
	[TestFixture]
	public class RelativitySyncConstrainsCheckerTests
	{
		private Mock<IToggleProvider> _toggleProvider;
		private Mock<IIntegrationPointService> _integrationPointService;
		private Mock<IProviderTypeService> _providerTypeService;
		private Job _job;

		private Mock<IConfigurationDeserializer> _configurationDeserializer;
		private SourceConfiguration _sourceConfiguration;
		private ImportSettings _importSettings;

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
				DateTime.MinValue, DateTime.MinValue, null, 1, DateTime.MinValue, 2, "", null);

			_sourceConfiguration = new SourceConfiguration {TypeOfExport = SourceConfiguration.ExportType.SavedSearch};

			_importSettings = new ImportSettings {ImageImport = false, ProductionImport = false};

			var integrationPoint = new Data.IntegrationPoint
			{
				SourceConfiguration = _sourceConfigurationString,
				DestinationConfiguration = _destinationConfigurationString,
				SourceProvider = _sourceProviderId,
				DestinationProvider = _destinationProviderId, 
			};

			var log = new Mock<IAPILog>();
			
			_toggleProvider = new Mock<IToggleProvider>();
			_toggleProvider.Setup(p => p.IsEnabled<EnableSyncToggle>()).Returns(true);


			_integrationPointService = new Mock<IIntegrationPointService>();
			_integrationPointService.Setup(s => s.GetRdo(_integrationPointId)).Returns(integrationPoint);


			_configurationDeserializer = new Mock<IConfigurationDeserializer>();
			_configurationDeserializer.Setup(d => d.DeserializeConfiguration<SourceConfiguration>(_sourceConfigurationString))
				.Returns(_sourceConfiguration);
			_configurationDeserializer.Setup(d => d.DeserializeConfiguration<ImportSettings>(_destinationConfigurationString))
				.Returns(_importSettings);

			_providerTypeService = new Mock<IProviderTypeService>();
			_providerTypeService.Setup(s => s.GetProviderType(_sourceProviderId, _destinationProviderId))
				.Returns(ProviderType.Relativity);

			_instance = new RelativitySyncConstrainsChecker(_integrationPointService.Object,
				_providerTypeService.Object, _toggleProvider.Object, _configurationDeserializer.Object, log.Object);
		}

		[Test]
		public void ItShouldAllowUsingSyncWorkflow()
		{
			_toggleProvider.Setup(p => p.IsEnabled<EnableSyncToggle>()).Returns(true);
			_providerTypeService.Setup(s => s.GetProviderType(_sourceProviderId, _destinationProviderId))
				.Returns(ProviderType.Relativity);
			_sourceConfiguration.TypeOfExport = SourceConfiguration.ExportType.SavedSearch;
			_importSettings.ImageImport = false;
			_importSettings.ProductionImport = false;

			bool result = _instance.ShouldUseRelativitySync(_job);

			Assert.IsTrue(result);
		}

		[TestCase(SourceConfiguration.ExportType.ProductionSet, false, false)]
		[TestCase(SourceConfiguration.ExportType.ProductionSet, true, false)]
		[TestCase(SourceConfiguration.ExportType.ProductionSet, false, true)]
		[TestCase(SourceConfiguration.ExportType.ProductionSet, true, true)]
		[TestCase(SourceConfiguration.ExportType.SavedSearch, true, false)]
		[TestCase(SourceConfiguration.ExportType.SavedSearch, false, true)]
		[TestCase(SourceConfiguration.ExportType.SavedSearch, true, true)]
		public void ItShouldNotAllowUsingSyncWorkflow(SourceConfiguration.ExportType typeOfExport, bool imageImport, bool productionImport)
		{
			_sourceConfiguration.TypeOfExport = typeOfExport;
			_importSettings.ImageImport = imageImport;
			_importSettings.ProductionImport = productionImport;
			
			bool result = _instance.ShouldUseRelativitySync(_job);

			Assert.IsFalse(result);
		}

		[Test]
		public void ItShouldNotAllowUsingSyncWorkflowWhenToggleDisabled()
		{
			_toggleProvider.Setup(p => p.IsEnabled<EnableSyncToggle>()).Returns(false);

			bool result = _instance.ShouldUseRelativitySync(_job);

			Assert.IsFalse(result);
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
			_integrationPointService.Setup(s => s.GetRdo(_integrationPointId)).Throws<Exception>();

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
			_configurationDeserializer.Setup(s => s.DeserializeConfiguration<SourceConfiguration>(_sourceConfigurationString)).Throws<Exception>();

			bool result = _instance.ShouldUseRelativitySync(_job);

			Assert.IsFalse(result);
		}

		[Test]
		public void ItShouldNotAllowUsingSyncWorkflowWhenConfigurationDeserializerForImportSettingsThrows()
		{
			_configurationDeserializer.Setup(s => s.DeserializeConfiguration<ImportSettings>(_destinationConfigurationString)).Throws<Exception>();

			bool result = _instance.ShouldUseRelativitySync(_job);

			Assert.IsFalse(result);
		}

		[Test]
		public void ItShouldNotAllowUsingSyncWorkflowWhenRunningScheduledJob()
		{
			_job.ScheduleRuleType = "scheduled rule";

			bool result = _instance.ShouldUseRelativitySync(_job);

			Assert.IsFalse(result);
		}
	}
}

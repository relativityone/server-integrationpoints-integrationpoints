﻿using System;
using System.Collections.Generic;
using System.Linq;
using kCura.EventHandler;
using kCura.IntegrationPoint.Tests.Core.TestCategories;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints;
using kCura.IntegrationPoints.SourceProviderInstaller.Services;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration.Installers
{
	/// <summary>
	/// Objective : this test suite is to test the simulation of source provider install eventhandler during migration.
	/// These set of tests verify that all source providers are passed in to import service properly.
	/// </summary>
	[TestFixture]
	internal class SourceProvidersMigrationEventHandlerTests : SourceProvidersMigrationEventHandler
	{
		private static readonly IErrorService _errorService = Substitute.For<IErrorService>();

		public SourceProvidersMigrationEventHandlerTests() : base(_errorService)
		{ }

		[OneTimeSetUp]
		public void Setup()
		{
			Importer = new MockImportService();
		}

		private List<SourceProvider> _providersStub;

		protected override List<SourceProvider> GetSourceProvidersFromPreviousWorkspace()
		{
			return _providersStub;
		}

		[Test]
		public void NoProviderInPreviousWorkspace()
		{
			// arrange
			_providersStub = new List<SourceProvider>();
			Logger = Substitute.For<IAPILog>();

			Helper = CreateHelperWithLogger(Logger);

			Response actual = Execute();

			// act & assert
			Assert.IsNotNull(actual);
			Assert.IsFalse(actual.Success);
			_errorService.Received().Log(Arg.Is<ErrorModel>(error => error.Message == "Failed to migrate Source Provider."));
		}

		[SmokeTest]
		public void OneProviderInPreviousWorkspace()
		{
			Guid identifier = new Guid("e01ff2d2-2ac7-4390-bbc3-64c6c17758bc");
			Guid appIdentifier = new Guid("7cd4c64f-747b-4962-9647-671ee65b6ea4");
			const int artifactId = 798;
			const string name = "Test";
			const string url = "fake url";
			const string dataUrl = "config url";

			// arrange
			SourceProvider providerToInstalled = new SourceProvider()
			{
				ApplicationIdentifier = appIdentifier.ToString(),
				Identifier = identifier.ToString(),
				ArtifactId = artifactId,
				Name = name,
				SourceConfigurationUrl = url,
				ViewConfigurationUrl = dataUrl,
				Config = new SourceProviderConfiguration()
			};

			_providersStub = new List<SourceProvider>()
			{
				providerToInstalled
			};

			Helper = Substitute.For<IEHHelper>();
			// act
			var result = Execute();

			//assert
			Assert.That(result.Success, Is.True);
			MockImportService importService = (MockImportService)Importer;
			Assert.IsNotNull(importService.InstalledProviders);
			Assert.AreEqual(1, importService.InstalledProviders.Count());
			VerifyInstalledProvider(providerToInstalled, importService.InstalledProviders.ElementAt(0));
		}

		[Test]
		public void MultipleProvidersInPreviousWorkspace()
		{
			// arrange
			SourceProvider providerToInstalled = new SourceProvider()
			{
				ApplicationIdentifier = "72194851-ad15-4769-bec5-04011498a1b4",
				Identifier = "e01ff2d2-2ac7-4390-bbc3-64c6c17758bc",
				ArtifactId = 789,
				Name = "test",
				SourceConfigurationUrl = "fake url",
				ViewConfigurationUrl = "config url",
				Config = new SourceProviderConfiguration()
			};

			SourceProvider provider2ToInstalled = new SourceProvider()
			{
				ApplicationIdentifier = "cf3ab0f2-d26f-49fb-bd11-547423a692c1",
				Identifier = "e01ff2d2-2ac7-4390-bbc3-64c6c17758bd",
				ArtifactId = 777,
				Name = "test2",
				SourceConfigurationUrl = "fake url2",
				ViewConfigurationUrl = "config url2",
				Config = new SourceProviderConfiguration()
			};

			_providersStub = new List<SourceProvider>()
			{
				providerToInstalled,
				provider2ToInstalled
			};

			IAPILog logger = Substitute.For<IAPILog>();
			Helper = CreateHelperWithLogger(logger);

			// act
			Execute();

			//assert
			MockImportService importService = (MockImportService)Importer;
			Assert.IsNotNull(importService.InstalledProviders);

			List<SourceProviderInstaller.SourceProvider> installedProviders = importService.InstalledProviders.ToList();
			Assert.AreEqual(2, installedProviders.Count);
			VerifyInstalledProvider(providerToInstalled, importService.InstalledProviders.ElementAt(0));
			VerifyInstalledProvider(provider2ToInstalled, importService.InstalledProviders.ElementAt(1));
		}

		private IEHHelper CreateHelperWithLogger(IAPILog logger)
		{
			logger.ForContext<SourceProvidersMigrationEventHandler>().Returns(logger);

			ILogFactory loggerFactory = Substitute.For<ILogFactory>();
			loggerFactory.GetLogger().Returns(logger);

			IEHHelper helper = Substitute.For<IEHHelper>();
			helper.GetLoggerFactory().Returns(loggerFactory);
			return helper;
		}

		private class MockImportService : IImportService
		{
			public IEnumerable<SourceProviderInstaller.SourceProvider> InstalledProviders;

			public void InstallProviders(IList<SourceProviderInstaller.SourceProvider> providers)
			{
				InstalledProviders = providers;
			}

			public void UninstallProviders(int applicationID)
			{
				throw new System.NotImplementedException();
			}
		}

		private void VerifyInstalledProvider(SourceProvider providerToBeInstalled, SourceProviderInstaller.SourceProvider installedProvider)
		{
			Assert.AreEqual(providerToBeInstalled.ApplicationIdentifier, installedProvider.ApplicationGUID.ToString());
			Assert.AreEqual(providerToBeInstalled.Identifier, installedProvider.GUID.ToString());
			Assert.AreEqual(providerToBeInstalled.Name, installedProvider.Name);
			Assert.AreEqual(providerToBeInstalled.SourceConfigurationUrl, installedProvider.Url);
			Assert.AreEqual(providerToBeInstalled.ViewConfigurationUrl, installedProvider.ViewDataUrl);
		}
	}
}
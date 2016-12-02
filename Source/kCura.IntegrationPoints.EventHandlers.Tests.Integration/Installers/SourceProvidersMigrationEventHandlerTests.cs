using System;
using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints;
using kCura.IntegrationPoints.SourceProviderInstaller.Services;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration.Installers
{
	/// <summary>
	/// Objective : this test suite is to test the simulation of source provider install eventhandler during migration.
	/// These set of tests verify that all source providers are passed in to import service properly.
	/// </summary>
	[TestFixture]
	internal class SourceProvidersMigrationEventHandlerTests : SourceProvidersMigrationEventHandler
	{
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

			// act & assert
			Assert.Throws<Exception>(() =>
			{
				Execute();
			}, "No Source Providers passed.");
		}

		[Test]
		[Category(kCura.IntegrationPoint.Tests.Core.Constants.SMOKE_TEST)]
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

			// act
			Execute();

			//assert
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

		private class MockImportService : IImportService
		{
			public IEnumerable<SourceProviderInstaller.SourceProvider> InstalledProviders;

			public void InstallProviders(IEnumerable<SourceProviderInstaller.SourceProvider> providers)
			{
				InstalledProviders = providers;
			}

			public void UninstallProvider(int applicationID)
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
using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.EventHandlers.Installers;
using NUnit.Framework;
using Relativity.IntegrationPoints.Contracts;
using Relativity.Testing.Identification;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration.Installers
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	internal class RegisterDocumentTransferProviderTests
	{
		[IdentifiedTest("043d24ae-57bc-4716-a941-f8ce4e3d2849")]
		[SmokeTest]
		public void RegisterDocumentTransferProvider_ReturnCorrectInstallationDetail()
		{
			RegisterDocumentTransferProvider installer = new RegisterDocumentTransferProvider(null);
			IDictionary<Guid, SourceProvider> installationInfo = installer.GetSourceProviders();

			Assert.AreEqual(1, installationInfo.Count, "Expect the installer to only have one provider.");
			Assert.IsTrue(installationInfo.ContainsKey(new Guid(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID)), "entry in the installer must be relativity provider.");

			string expectedUrl = String.Format("/%applicationpath%/CustomPages/{0}/IntegrationPoints/{1}/", Core.Constants.IntegrationPoints.RELATIVITY_CUSTOMPAGE_GUID, Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_CONFIGURATION);

			SourceProvider provider = installationInfo[new Guid(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_GUID)];
			Assert.IsNotNull(provider);
			Assert.AreEqual(Core.Constants.IntegrationPoints.RELATIVITY_PROVIDER_NAME, provider.Name);
			Assert.AreEqual(expectedUrl, provider.Url);

		}
	}
}
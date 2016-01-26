using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.DocumentTransferProvider.Shared;
using kCura.IntegrationPoints.EventHandlers.Installers;
using kCura.IntegrationPoints.SourceProviderInstaller;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration.Installers
{
	using NUnit.Framework;

	[TestFixture]
	internal class RegisterDocumentTransferProviderTests
	{
		[Test]
		public void RegisterDocumentTransferProvider_returnCollectInstanlationDetail()
		{
			RegisterDocumentTransferProvider installer = new RegisterDocumentTransferProvider();
			IDictionary<Guid, SourceProvider> installationInfo = installer.GetSourceProviders();

			Assert.AreEqual(1, installationInfo.Count, "Expect the installer to only have one provider.");
			Assert.IsTrue(installationInfo.ContainsKey(new Guid(Constants.PROVIDER_GUID)), "entry in the installer must be relativity provider.");

			string expectedUrl = String.Format("/%applicationpath%/CustomPages/{0}/IntegrationPoints/{1}/", Constants.CUSTOMPAGE_GUID, Constants.PROVIDER_CONFIGURATION);

			SourceProvider provider = installationInfo[new Guid(Constants.PROVIDER_GUID)];
			Assert.IsNotNull(provider);
			Assert.AreEqual(Constants.PROVIDER_NAME, provider.Name);
			Assert.AreEqual(expectedUrl, provider.Url);

		}
	}
}
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Repositories;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Repositories
{
	public class ProviderRepositoryTests : TestBase
	{
		private ProviderRepository _providerRepository;
		private IRepositoryFactory _repositoryFactory;

		public override void SetUp()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();

			_providerRepository = new ProviderRepository(_repositoryFactory);
		}

		[Test]
		public void ItShouldGetSourceProviderArtifactId()
		{
			int workspaceId = 782;
			string guid = "guid_905";

			int expectedSourceProviderArtifactId = 889;

			var sourceProviderRepository = Substitute.For<ISourceProviderRepository>();
			_repositoryFactory.GetSourceProviderRepository(workspaceId).Returns(sourceProviderRepository);

			sourceProviderRepository.GetArtifactIdFromSourceProviderTypeGuidIdentifier(guid).Returns(expectedSourceProviderArtifactId);

			var actualResult = _providerRepository.GetSourceProviderArtifactId(workspaceId, guid);

			Assert.That(actualResult, Is.EqualTo(expectedSourceProviderArtifactId));
		}

		[Test]
		public void ItShouldGetDestinationProviderArtifactId()
		{
			int workspaceId = 329;
			string guid = "guid_819";

			int expectedDestinationProviderArtifactId = 889;

			var destinationProviderRepository = Substitute.For<IDestinationProviderRepository>();
			_repositoryFactory.GetDestinationProviderRepository(workspaceId).Returns(destinationProviderRepository);

			destinationProviderRepository.GetArtifactIdFromDestinationProviderTypeGuidIdentifier(guid).Returns(expectedDestinationProviderArtifactId);

			var actualResult = _providerRepository.GetDestinationProviderArtifactId(workspaceId, guid);

			Assert.That(actualResult, Is.EqualTo(expectedDestinationProviderArtifactId));
		}
	}
}
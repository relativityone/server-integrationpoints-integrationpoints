using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.QueryBuilders.Implementations;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Repositories.Implementations;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Repositories
{
	public class ProviderRepositoryTests : TestBase
	{
		private ProviderRepository _providerRepository;
		private IRepositoryFactory _repositoryFactory;
		private IRSAPIService _rsapiService;

		public override void SetUp()
		{
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_rsapiService = Substitute.For<IRSAPIService>();

			_providerRepository = new ProviderRepository(_repositoryFactory, _rsapiService);
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

		[Test]
		public void ItShouldGetAllSourceProviders()
		{
			var library = Substitute.For<IGenericLibrary<SourceProvider>>();
			_rsapiService.SourceProviderLibrary.Returns(library);

			var expectedQuery = new AllSourceProvidersQueryBuilder().Create();

			var expectedResult = new List<SourceProvider>
			{
				new SourceProvider
				{
					ArtifactId = 369,
					Name = "name_572"
				},
				new SourceProvider
				{
					ArtifactId = 229,
					Name = "name_700"
				}
			};

			library.Query(Arg.Is<Query<RDO>>(x => x.IsEqualOnTypeAndNameAndFields(expectedQuery)))
				.Returns(expectedResult);

			var actualResult = _providerRepository.GetSourceProviders(521);

			Assert.That(actualResult,
				Is.EquivalentTo(expectedResult).Using(new Func<ProviderModel, SourceProvider, bool>((x, y) => (x.Name == y.Name) && (x.ArtifactId == y.ArtifactId))));
		}

		[Test]
		public void ItShouldGetAllDestinationProviders()
		{
			var library = Substitute.For<IGenericLibrary<DestinationProvider>>();
			_rsapiService.DestinationProviderLibrary.Returns(library);

			var expectedQuery = new AllDestinationProvidersQueryBuilder().Create();

			var expectedResult = new List<DestinationProvider>
			{
				new DestinationProvider
				{
					ArtifactId = 281,
					Name = "name_818"
				},
				new DestinationProvider
				{
					ArtifactId = 918,
					Name = "name_423"
				}
			};

			library.Query(Arg.Is<Query<RDO>>(x => x.IsEqualOnTypeAndNameAndFields(expectedQuery)))
				.Returns(expectedResult);

			var actualResult = _providerRepository.GetDesinationProviders(521);

			Assert.That(actualResult,
				Is.EquivalentTo(expectedResult).Using(new Func<ProviderModel, DestinationProvider, bool>((x, y) => (x.Name == y.Name) && (x.ArtifactId == y.ArtifactId))));
		}
	}
}
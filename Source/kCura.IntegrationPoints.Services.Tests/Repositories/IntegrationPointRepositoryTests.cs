using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Repositories;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.Tests.Repositories
{
	public class IntegrationPointRepositoryTests : TestBase
	{
		private IntegrationPointRepository _integrationPointRepository;
		private IIntegrationPointService _integrationPointService;
		private IRepositoryFactory _repositoryFactory;
		private IObjectTypeRepository _objectTypeRepository;
		private IUserInfo _userInfo;

		public override void SetUp()
		{
			_integrationPointService = Substitute.For<IIntegrationPointService>();
			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_objectTypeRepository = Substitute.For<IObjectTypeRepository>();
			_userInfo = Substitute.For<IUserInfo>();

			_integrationPointRepository = new IntegrationPointRepository(_integrationPointService, _repositoryFactory, _objectTypeRepository, _userInfo);
		}

		[Test]
		public void ItShouldGetIntegrationPoint()
		{
			int artifactId = 884;
			var integrationPoint = new Data.IntegrationPoint
			{
				ArtifactId = 945,
				Name = "ip_name_126",
				SourceProvider = 962
			};

			_integrationPointService.GetRdo(artifactId).Returns(integrationPoint);

			var result = _integrationPointRepository.GetIntegrationPoint(artifactId);

			_integrationPointService.Received(1).GetRdo(artifactId);

			Assert.That(result.SourceProvider, Is.EqualTo(integrationPoint.SourceProvider));
			Assert.That(result.ArtifactId, Is.EqualTo(integrationPoint.ArtifactId));
			Assert.That(result.Name, Is.EqualTo(integrationPoint.Name));
		}

		[Test]
		public void ItShouldRunIntegrationPointWithUser()
		{
			int workspaceId = 873;
			int artifactId = 797;
			int userId = 127;

			_userInfo.ArtifactID.Returns(userId);

			_integrationPointRepository.RunIntegrationPoint(workspaceId, artifactId);

			_integrationPointService.Received(1).RunIntegrationPoint(workspaceId, artifactId, userId);
		}

		[Test]
		public void ItShouldGetAllIntegrationPoints()
		{
			var integrationPoint1 = new Data.IntegrationPoint
			{
				ArtifactId = 263,
				Name = "ip_name_987",
				SourceProvider = 764
			};
			var integrationPoint2 = new Data.IntegrationPoint
			{
				ArtifactId = 204,
				Name = "ip_name_555",
				SourceProvider = 187
			};

			var expectedResult = new List<Data.IntegrationPoint> {integrationPoint1, integrationPoint2};
			_integrationPointService.GetAllRDOs().Returns(expectedResult);

			var result = _integrationPointRepository.GetAllIntegrationPoints();

			_integrationPointService.Received(1).GetAllRDOs();

			Assert.That(result, Is.EquivalentTo(expectedResult).
				Using(new Func<IntegrationPointModel, Data.IntegrationPoint, bool>(
					(actual, expected) => (actual.Name == expected.Name) && (actual.SourceProvider == expected.SourceProvider.Value) && (actual.ArtifactId == expected.ArtifactId))));
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

			var actualResult = _integrationPointRepository.GetSourceProviderArtifactId(workspaceId, guid);

			Assert.That(actualResult, Is.EqualTo(expectedSourceProviderArtifactId));
		}

		[Test]
		public void ItShouldGetIntegrationPointArtifactTypeId()
		{
			int expectedArtifactTypeId = 975;

			_objectTypeRepository.RetrieveObjectTypeDescriptorArtifactTypeId(Arg.Any<Guid>()).Returns(expectedArtifactTypeId);

			var actualResult = _integrationPointRepository.GetIntegrationPointArtifactTypeId();

			_objectTypeRepository.Received(1).RetrieveObjectTypeDescriptorArtifactTypeId(new Guid(ObjectTypeGuids.IntegrationPoint));

			Assert.That(actualResult, Is.EqualTo(expectedArtifactTypeId));
		}
	}
}
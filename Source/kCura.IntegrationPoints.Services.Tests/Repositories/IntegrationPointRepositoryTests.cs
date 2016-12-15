using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Services.Helpers;
using kCura.IntegrationPoints.Services.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.Services.Tests.Repositories
{
	public class IntegrationPointRepositoryTests : TestBase
	{
		private IntegrationPointRepository _integrationPointRepository;
		private IIntegrationPointService _integrationPointService;
		private IObjectTypeRepository _objectTypeRepository;
		private IUserInfo _userInfo;
		private IChoiceQuery _choiceQuery;
		private IBackwardCompatibility _backwardCompatibility;

		public override void SetUp()
		{
			_integrationPointService = Substitute.For<IIntegrationPointService>();
			_objectTypeRepository = Substitute.For<IObjectTypeRepository>();
			_userInfo = Substitute.For<IUserInfo>();
			_choiceQuery = Substitute.For<IChoiceQuery>();
			_backwardCompatibility = Substitute.For<IBackwardCompatibility>();

			_integrationPointRepository = new IntegrationPointRepository(_integrationPointService, _objectTypeRepository, _userInfo, _choiceQuery, _backwardCompatibility);
		}

		[Test]
		public void ItShouldCreateIntegrationPoint()
		{
			var overwriteFieldsChoiceId = 934;
			var overwriteFieldsChoiceName = "Append/Overlay";
			var integrationPointArtifactId = 134;

			var createRequest = SetUpCreateOrUpdateTest(overwriteFieldsChoiceId, overwriteFieldsChoiceName, integrationPointArtifactId);

			_integrationPointService.SaveIntegration(Arg.Is<Core.Models.IntegrationPointModel>(x => x.ArtifactID == 0)).Returns(integrationPointArtifactId);

			_integrationPointRepository.CreateIntegrationPoint(createRequest);

			_backwardCompatibility.Received(1).FixIncompatibilities(createRequest.IntegrationPoint, overwriteFieldsChoiceName);
			_integrationPointService.Received(1).GetRdo(integrationPointArtifactId);
		}

		[Test]
		public void ItShouldUpdateIntegrationPoint()
		{
			var overwriteFieldsChoiceId = 215;
			var overwriteFieldsChoiceName = "Append/Overlay";
			var integrationPointArtifactId = 902;

			var createRequest = SetUpCreateOrUpdateTest(overwriteFieldsChoiceId, overwriteFieldsChoiceName, integrationPointArtifactId);

			_integrationPointService.SaveIntegration(Arg.Is<Core.Models.IntegrationPointModel>(x => x.ArtifactID == createRequest.IntegrationPoint.ArtifactId))
				.Returns(integrationPointArtifactId);

			_integrationPointRepository.CreateIntegrationPoint(createRequest);

			_backwardCompatibility.Received(1).FixIncompatibilities(createRequest.IntegrationPoint, overwriteFieldsChoiceName);
			_integrationPointService.Received(1).GetRdo(integrationPointArtifactId);
		}

		private UpdateIntegrationPointRequest SetUpCreateOrUpdateTest(int overwriteFieldsChoiceId, string overwriteFieldsChoiceName, int integrationPointArtifactId)
		{
			var createRequest = new UpdateIntegrationPointRequest
			{
				WorkspaceArtifactId = 640,
				IntegrationPoint = new IntegrationPointModel
				{
					OverwriteFieldsChoiceId = overwriteFieldsChoiceId,
					ArtifactId = 916,
					ScheduleRule = new ScheduleModel
					{
						EnableScheduler = false
					}
				}
			};

			_choiceQuery.GetChoicesOnField(new Guid(IntegrationPointFieldGuids.OverwriteFields)).Returns(new List<Choice>
			{
				new Choice(overwriteFieldsChoiceId)
				{
					Name = overwriteFieldsChoiceName
				}
			});
			_integrationPointService.GetRdo(integrationPointArtifactId).Returns(new Data.IntegrationPoint
			{
				ArtifactId = integrationPointArtifactId,
				Name = "name_762",
				SourceProvider = 718,
				DestinationProvider = 715
			});
			return createRequest;
		}

		[Test]
		public void ItShouldGetIntegrationPoint()
		{
			int artifactId = 884;
			var integrationPoint = new Data.IntegrationPoint
			{
				ArtifactId = 945,
				Name = "ip_name_126",
				SourceProvider = 962,
				DestinationProvider = 577
			};

			_integrationPointService.GetRdo(artifactId).Returns(integrationPoint);

			var result = _integrationPointRepository.GetIntegrationPoint(artifactId);

			_integrationPointService.Received(1).GetRdo(artifactId);

			Assert.That(result.SourceProvider, Is.EqualTo(integrationPoint.SourceProvider));
			Assert.That(result.DestinationProvider, Is.EqualTo(integrationPoint.DestinationProvider));
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
				SourceProvider = 764,
				DestinationProvider = 576
			};
			var integrationPoint2 = new Data.IntegrationPoint
			{
				ArtifactId = 204,
				Name = "ip_name_555",
				SourceProvider = 187,
				DestinationProvider = 422
			};

			var expectedResult = new List<Data.IntegrationPoint> {integrationPoint1, integrationPoint2};
			_integrationPointService.GetAllRDOs().Returns(expectedResult);

			var result = _integrationPointRepository.GetAllIntegrationPoints();

			_integrationPointService.Received(1).GetAllRDOs();

			Assert.That(result, Is.EquivalentTo(expectedResult).
				Using(new Func<IntegrationPointModel, Data.IntegrationPoint, bool>(
					(actual, expected) => (actual.Name == expected.Name) && (actual.SourceProvider == expected.SourceProvider.Value) && (actual.ArtifactId == expected.ArtifactId)
										&& (actual.DestinationProvider == expected.DestinationProvider.Value))));
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
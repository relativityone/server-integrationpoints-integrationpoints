using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Helpers;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using kCura.IntegrationPoints.Services.Repositories.Implementations;
using NSubstitute;
using NUnit.Framework;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.Services.Tests.Repositories
{
	public class IntegrationPointProfileRepositoryTests : TestBase
	{
		private IntegrationPointProfileRepository _integrationPointProfileRepository;
		private IIntegrationPointProfileService _integrationPointProfileService;
		private IChoiceQuery _choiceQuery;
		private IBackwardCompatibility _backwardCompatibility;

		public override void SetUp()
		{
			_integrationPointProfileService = Substitute.For<IIntegrationPointProfileService>();
			_choiceQuery = Substitute.For<IChoiceQuery>();
			_backwardCompatibility = Substitute.For<IBackwardCompatibility>();

			_integrationPointProfileRepository = new IntegrationPointProfileRepository(_backwardCompatibility, _integrationPointProfileService, _choiceQuery);
		}

		[Test]
		public void ItShouldCreateIntegrationPoint()
		{
			var overwriteFieldsChoiceId = 588;
			var overwriteFieldsChoiceName = "Append/Overlay";
			var integrationPointProfileArtifactId = 219;

			var createRequest = SetUpCreateOrUpdateTest(overwriteFieldsChoiceId, overwriteFieldsChoiceName, integrationPointProfileArtifactId);

			_integrationPointProfileService.SaveIntegration(Arg.Is<IntegrationPointProfileModel>(x => x.ArtifactID == 0)).Returns(integrationPointProfileArtifactId);

			_integrationPointProfileRepository.CreateIntegrationPointProfile(createRequest);

			_backwardCompatibility.Received(1).FixIncompatibilities(createRequest.IntegrationPoint, overwriteFieldsChoiceName);
			_integrationPointProfileService.Received(1).GetRdo(integrationPointProfileArtifactId);
		}

		[Test]
		public void ItShouldUpdateIntegrationPoint()
		{
			var overwriteFieldsChoiceId = 152;
			var overwriteFieldsChoiceName = "Append/Overlay";
			var integrationPointProfileArtifactId = 289;

			var createRequest = SetUpCreateOrUpdateTest(overwriteFieldsChoiceId, overwriteFieldsChoiceName, integrationPointProfileArtifactId);

			_integrationPointProfileService.SaveIntegration(Arg.Is<IntegrationPointProfileModel>(x => x.ArtifactID == createRequest.IntegrationPoint.ArtifactId))
				.Returns(integrationPointProfileArtifactId);

			_integrationPointProfileRepository.CreateIntegrationPointProfile(createRequest);

			_backwardCompatibility.Received(1).FixIncompatibilities(createRequest.IntegrationPoint, overwriteFieldsChoiceName);
			_integrationPointProfileService.Received(1).GetRdo(integrationPointProfileArtifactId);
		}

		private UpdateIntegrationPointRequest SetUpCreateOrUpdateTest(int overwriteFieldsChoiceId, string overwriteFieldsChoiceName, int integrationPointProfileArtifactId)
		{
			var createRequest = new UpdateIntegrationPointRequest
			{
				WorkspaceArtifactId = 921,
				IntegrationPoint = new IntegrationPointModel
				{
					OverwriteFieldsChoiceId = overwriteFieldsChoiceId,
					ArtifactId = 591,
					ScheduleRule = new ScheduleModel
					{
						EnableScheduler = false
					}
				}
			};

			_choiceQuery.GetChoicesOnField(new Guid(IntegrationPointProfileFieldGuids.OverwriteFields)).Returns(new List<Choice>
			{
				new Choice(overwriteFieldsChoiceId)
				{
					Name = overwriteFieldsChoiceName
				}
			});
			_integrationPointProfileService.GetRdo(integrationPointProfileArtifactId).Returns(new IntegrationPointProfile
			{
				ArtifactId = integrationPointProfileArtifactId,
				Name = "name_982",
				SourceProvider = 268,
				DestinationProvider = 288
			});
			return createRequest;
		}

		[Test]
		public void ItShouldGetIntegrationPoint()
		{
			int artifactId = 924;
			var integrationPointProfile = new IntegrationPointProfile
			{
				ArtifactId = 235,
				Name = "ip_name_350",
				SourceProvider = 471,
				DestinationProvider = 817
			};

			_integrationPointProfileService.GetRdo(artifactId).Returns(integrationPointProfile);

			var result = _integrationPointProfileRepository.GetIntegrationPointProfile(artifactId);

			_integrationPointProfileService.Received(1).GetRdo(artifactId);

			Assert.That(result.SourceProvider, Is.EqualTo(integrationPointProfile.SourceProvider));
			Assert.That(result.DestinationProvider, Is.EqualTo(integrationPointProfile.DestinationProvider));
			Assert.That(result.ArtifactId, Is.EqualTo(integrationPointProfile.ArtifactId));
			Assert.That(result.Name, Is.EqualTo(integrationPointProfile.Name));
		}

		[Test]
		public void ItShouldGetAllIntegrationPoints()
		{
			var integrationPointProfile1 = new IntegrationPointProfile
			{
				ArtifactId = 844,
				Name = "ip_name_234",
				SourceProvider = 871,
				DestinationProvider = 143
			};
			var integrationPointProfile2 = new IntegrationPointProfile
			{
				ArtifactId = 526,
				Name = "ip_name_617",
				SourceProvider = 723,
				DestinationProvider = 158
			};

			var expectedResult = new List<IntegrationPointProfile> { integrationPointProfile1, integrationPointProfile2 };
			_integrationPointProfileService.GetAllRDOs().Returns(expectedResult);

			var result = _integrationPointProfileRepository.GetAllIntegrationPointProfiles();

			_integrationPointProfileService.Received(1).GetAllRDOs();

			Assert.That(result, Is.EquivalentTo(expectedResult).
				Using(new Func<IntegrationPointModel, IntegrationPointProfile, bool>(
					(actual, expected) => (actual.Name == expected.Name) && (actual.SourceProvider == expected.SourceProvider.Value) && (actual.ArtifactId == expected.ArtifactId)
										&& (actual.DestinationProvider == expected.DestinationProvider.Value))));
		}


		[Test]
		public void ItShouldRetrieveAllOverwriteFieldChoices()
		{
			var expectedChoices = new List<Choice>
			{
				new Choice(688)
				{
					Name = "name_516"
				},
				new Choice(498)
				{
					Name = "name_712"
				}
			};

			_choiceQuery.GetChoicesOnField(Guid.Parse(IntegrationPointProfileFieldGuids.OverwriteFields)).Returns(expectedChoices);

			var actualChoicesModels = _integrationPointProfileRepository.GetOverwriteFieldChoices();

			Assert.That(actualChoicesModels,
				Is.EquivalentTo(expectedChoices).Using(new Func<OverwriteFieldsModel, Choice, bool>((x, y) => (x.Name == y.Name) && (x.ArtifactId == y.ArtifactID))));
		}
	}
}
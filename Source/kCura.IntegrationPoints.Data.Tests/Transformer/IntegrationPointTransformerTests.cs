using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Transformers;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Choice = kCura.Relativity.Client.DTOs.Choice;

namespace kCura.IntegrationPoints.Data.Tests.Transformer
{
	[TestFixture]
	public class IntegrationPointTransformerTests : TestBase
	{
		private IDtoTransformer<IntegrationPointDTO, IntegrationPoint> _testInstance;
		private IRepositoryFactory _repositoryFactory;
		private IArtifactGuidRepository _artifactGuidRepository;

		private const int WORKSPACE_ARTIFACT_ID = 100501;
		private const int OVERWRITE_FIELDS_APPEND_ONLY_CHOICE_ID = 123477;
		private const int OVERWRITE_FIELDS_APPEND_OVERLAY_CHOICE_ID = 123478;
		private readonly Guid OverwriteFieldsAppendOnlyChoice = new Guid("998C2B04-D42E-435B-9FBA-11FEC836AAD8");
		private readonly Guid OverwriteFieldsAppendOverlayChoice = new Guid("5450EBC3-AC57-4E6A-9D28-D607BBDCF6FD");

		[OneTimeSetUp]
		public override void FixtureSetUp()
		{
			base.FixtureSetUp();

			_repositoryFactory = Substitute.For<IRepositoryFactory>();
			_artifactGuidRepository = Substitute.For<IArtifactGuidRepository>();
			_testInstance = new IntegrationPointTransformer(_repositoryFactory, WORKSPACE_ARTIFACT_ID);
		}

		[SetUp]
		public override void SetUp()
		{
			
		}

		[Test]
		public void ConvertToDtoTest()
		{
			// ARRANGE
			string expectedName = "My Integration Point";
			IntegrationPoint expectedIntegrationPoint = CreateMockedIntegrationPoint(expectedName);

			_repositoryFactory.GetArtifactGuidRepository(WORKSPACE_ARTIFACT_ID).Returns(_artifactGuidRepository);

			var choiceDictionary = new Dictionary<int, Guid> {{ OVERWRITE_FIELDS_APPEND_ONLY_CHOICE_ID, OverwriteFieldsAppendOnlyChoice } };
			_artifactGuidRepository.GetGuidsForArtifactIds(Arg.Is<List<int>>(list => list.Count == 1 && list[0] == OVERWRITE_FIELDS_APPEND_ONLY_CHOICE_ID))
				.Returns(choiceDictionary);

			// ACT
			IntegrationPointDTO resultDto = _testInstance.ConvertToDto(expectedIntegrationPoint);

			// ASSERT
			Assert.IsNotNull(resultDto);
			Assert.AreEqual(expectedIntegrationPoint.ArtifactId, resultDto.ArtifactId);
			Assert.AreEqual(expectedIntegrationPoint.DestinationConfiguration, resultDto.DestinationConfiguration);
			Assert.AreEqual(expectedIntegrationPoint.DestinationProvider, resultDto.DestinationProvider);
			Assert.AreEqual(expectedIntegrationPoint.EmailNotificationRecipients, resultDto.EmailNotificationRecipients);
			Assert.AreEqual(expectedIntegrationPoint.EnableScheduler, resultDto.EnableScheduler);
			Assert.AreEqual(expectedIntegrationPoint.FieldMappings, resultDto.FieldMappings);
			Assert.AreEqual(expectedIntegrationPoint.HasErrors, resultDto.HasErrors);
			Assert.AreEqual(expectedIntegrationPoint.JobHistory, resultDto.JobHistory);
			Assert.AreEqual(expectedIntegrationPoint.LastRuntimeUTC, resultDto.LastRuntimeUTC);
			Assert.AreEqual(expectedIntegrationPoint.LogErrors, resultDto.LogErrors);
			Assert.AreEqual(expectedIntegrationPoint.Name, resultDto.Name);
			Assert.AreEqual(expectedIntegrationPoint.NextScheduledRuntimeUTC, resultDto.NextScheduledRuntimeUTC);
			Assert.AreEqual(IntegrationPointDTO.Choices.OverwriteFields.Values.AppendOnly, resultDto.OverwriteFields);
			Assert.AreEqual(expectedIntegrationPoint.ScheduleRule, resultDto.ScheduleRule);
			Assert.AreEqual(expectedIntegrationPoint.SourceConfiguration, resultDto.SourceConfiguration);
			Assert.AreEqual(expectedIntegrationPoint.SourceProvider, resultDto.SourceProvider);
		}

		[Test]
		public void ConvertMultipleToDtoTest()
		{
			// ARRANGE
			string expectedName1 = "My Integration Point 1";
			string expectedName2 = "My Integration Point 2";

			IntegrationPoint expectedIntegrationPoint1 = CreateMockedIntegrationPoint(expectedName1, OVERWRITE_FIELDS_APPEND_ONLY_CHOICE_ID);
			IntegrationPoint expectedIntegrationPoint2 = CreateMockedIntegrationPoint(expectedName2, OVERWRITE_FIELDS_APPEND_OVERLAY_CHOICE_ID);

			_repositoryFactory.GetArtifactGuidRepository(WORKSPACE_ARTIFACT_ID).Returns(_artifactGuidRepository);

			var choiceDictionary1 = new Dictionary<int, Guid> { { OVERWRITE_FIELDS_APPEND_ONLY_CHOICE_ID, OverwriteFieldsAppendOnlyChoice } };
			var choiceDictionary2 = new Dictionary<int, Guid> { { OVERWRITE_FIELDS_APPEND_OVERLAY_CHOICE_ID, OverwriteFieldsAppendOverlayChoice} };

			_artifactGuidRepository.GetGuidsForArtifactIds(Arg.Is<List<int>>(list => list.Count == 1 && list[0] == OVERWRITE_FIELDS_APPEND_ONLY_CHOICE_ID))
				.Returns(choiceDictionary1);
			_artifactGuidRepository.GetGuidsForArtifactIds(Arg.Is<List<int>>(list => list.Count == 1 && list[0] == OVERWRITE_FIELDS_APPEND_OVERLAY_CHOICE_ID))
				.Returns(choiceDictionary2);

			// ACT
			List<IntegrationPointDTO> resultDtos = _testInstance.ConvertToDto(new [] {expectedIntegrationPoint1, expectedIntegrationPoint2});

			// ASSERT
			Assert.IsNotNull(resultDtos);
			Assert.AreEqual(2, resultDtos.Count);
			//  This test only concentrates on multiple conversion, thus only few fields are tested
			Assert.AreEqual(expectedName1, resultDtos[0].Name);
			Assert.AreEqual(expectedName2, resultDtos[1].Name);
			Assert.AreEqual(IntegrationPointDTO.Choices.OverwriteFields.Values.AppendOnly, resultDtos[0].OverwriteFields);
			Assert.AreEqual(IntegrationPointDTO.Choices.OverwriteFields.Values.AppendOverlay, resultDtos[1].OverwriteFields);
		}

		public IntegrationPoint CreateMockedIntegrationPoint(string name, int overwriteFieldsChoiceId = OVERWRITE_FIELDS_APPEND_ONLY_CHOICE_ID)
		{
			return new IntegrationPoint()
			{
				ArtifactId = 100200,
				DestinationConfiguration = "My destination configuration",
				DestinationProvider = 4324234,
				EmailNotificationRecipients = "billgates@outlook.com",
				EnableScheduler = true,
				FieldMappings = "My integration point mappings",
				HasErrors = true,
				JobHistory = new[] { 100900, 100901 },
				LastRuntimeUTC = DateTime.UtcNow,
				LogErrors = true,
				Name = name,
				NextScheduledRuntimeUTC = DateTime.UtcNow.AddDays(1),
				OverwriteFields = new Choice(overwriteFieldsChoiceId) {Name=""},
				ScheduleRule = "My schedule rule",
				SourceConfiguration = "My source configuration",
				SourceProvider = 123123,
			};
		}
	}
}

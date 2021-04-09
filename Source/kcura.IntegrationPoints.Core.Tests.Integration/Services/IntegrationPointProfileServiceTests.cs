using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using Relativity.Testing.Identification;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Services.IntegrationPoint;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;

namespace kCura.IntegrationPoints.Core.Tests.Integration.Services
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Profiles]
	public class IntegrationPointProfileServiceTests : RelativityProviderTemplate
	{
		private int _longTextLimit;
		private IIntegrationPointProfileService _sut;

		private const string _LONG_TEXT_LIMIT_SECTION = "kCura.EDDS.Web";
		private const string _LONG_TEXT_LIMIT_NAME = "MaximumNumberOfCharactersSupportedByLongText";

		public IntegrationPointProfileServiceTests() : base(nameof(IntegrationPointProfileServiceTests), null)
		{ }

		public override void SuiteSetup()
		{
			base.SuiteSetup();

			IInstanceSettingRepository instanceSettings = RepositoryFactory.GetInstanceSettingRepository();
			_longTextLimit = Convert.ToInt32(instanceSettings.GetConfigurationValue(_LONG_TEXT_LIMIT_SECTION, _LONG_TEXT_LIMIT_NAME));

			_sut = Container.Resolve<IIntegrationPointProfileService>();
		}

		[IdentifiedTest("23C08A70-6AC5-4A53-B35A-D8B347B494D0")]
		public void ReadIntegrationPointProfile_ShouldRetrieveDeserializableSourceConfiguration_WhenSourceConfigurationExceedsLongTextLimit()
		{
			// Arrange
			IntegrationPointProfileModel profileModel = CreateDefaultIntegrationPointProfileModel(
				ImportOverwriteModeEnum.AppendOnly, TestContext.CurrentContext.Test.MethodName, "Append Only");

			profileModel.SourceConfiguration = Serializer.Serialize(new
			{
				SavedSearchArtifactId = SavedSearchArtifactID,
				SourceWorkspaceArtifactId = SourceWorkspaceArtifactID,
				TargetWorkspaceArtifactId = TargetWorkspaceArtifactID,
				TypeOfExport = 3,
				Filler = new String(Enumerable.Repeat('-', _longTextLimit).ToArray())
			});

			int createdProfileArtifactId = CreateOrUpdateIntegrationPointProfile(profileModel).ArtifactID;

			// Act
			IntegrationPointProfile profile = _sut.ReadIntegrationPointProfile(createdProfileArtifactId);

			// Assert
			AssertFieldDeserializationDoesNotThrow<IDictionary<string, string>>(profile.SourceConfiguration);
		}

		[IdentifiedTest("36819CF3-AA3E-44E9-ACC9-2443F858D62E")]
		public void ReadIntegrationPointProfile_ShouldRetrieveDeserializableDestinationConfiguration_WhenDestinationConfigurationExceedsLongTextLimit()
		{
			// Arrange
			IntegrationPointProfileModel profileModel = CreateDefaultIntegrationPointProfileModel(
				ImportOverwriteModeEnum.AppendOnly, TestContext.CurrentContext.Test.MethodName, "Append Only");

			profileModel.Destination = Serializer.Serialize(new
			{
				ArtifactTypeId = 10,
				CaseArtifactId = SourceWorkspaceArtifactID,
				Provider = "Relativity",
				ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly,
				ImportNativeFile = false,
				ExtractedTextFieldContainsFilePath = false,
				FieldOverlayBehavior = "Use Field Settings",
				RelativityUsername = SharedVariables.RelativityUserName,
				RelativityPassword = SharedVariables.RelativityPassword,
				DestinationProviderType = "74A863B9-00EC-4BB7-9B3E-1E22323010C6",
				DestinationFolderArtifactId = GetRootFolder(Helper, SourceWorkspaceArtifactID),
				Filler = new String(Enumerable.Repeat('-', _longTextLimit).ToArray())
			});

			int createdProfileArtifactId = CreateOrUpdateIntegrationPointProfile(profileModel).ArtifactID;

			// Act
			IntegrationPointProfile profile = _sut.ReadIntegrationPointProfile(createdProfileArtifactId);

			// Assert
			AssertFieldDeserializationDoesNotThrow<IDictionary<string, string>>(profile.DestinationConfiguration);
		}

		[IdentifiedTest("77576B90-A243-4585-A463-96B5A4CA1E74")]
		public void ReadIntegrationPointProfile_ShouldRetrieveDeserializableFieldMappings_WhenFieldMappingsExceedsLongTextLimit()
		{
			// Arrange
			IntegrationPointProfileModel profileModel = CreateDefaultIntegrationPointProfileModel(
				ImportOverwriteModeEnum.AppendOnly, TestContext.CurrentContext.Test.MethodName, "Append Only");

			FieldMap[] fieldMappings = GetDefaultFieldMap();
			fieldMappings[0].SourceField.DisplayName = new string(Enumerable.Repeat('-', _longTextLimit / 2).ToArray());
			fieldMappings[0].DestinationField.DisplayName = new string(Enumerable.Repeat('-', _longTextLimit / 2).ToArray());

			profileModel.Map = Serializer.Serialize(fieldMappings);

			int createdProfileArtifactId = CreateOrUpdateIntegrationPointProfile(profileModel).ArtifactID;

			// Act
			IntegrationPointProfile profile = _sut.ReadIntegrationPointProfile(createdProfileArtifactId);

			// Assert
			AssertFieldDeserializationDoesNotThrow<FieldMap[]>(profile.FieldMappings);
		}

		private void AssertFieldDeserializationDoesNotThrow<T>(string fieldTextValue)
		{
			Action deserializeSourceConfig = () => Serializer.Deserialize<T>(fieldTextValue);
			deserializeSourceConfig.ShouldNotThrow();
		}
	}
}

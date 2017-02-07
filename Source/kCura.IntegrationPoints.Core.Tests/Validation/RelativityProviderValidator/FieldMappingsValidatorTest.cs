using System.Collections.Generic;
using System.Linq;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator
{
	[TestFixture]
	public class FieldMappingsValidatorTest
	{
		private IValidator _instance;
		private IFieldManager _sourceFieldManager;
		private IFieldManager _targetFieldManager;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1074540;
		private const int _TARGET_WORKSPACE_ARTIFACT_ID = 1075642;
		private readonly string SourceConfiguration = "{\"SourceWorkspaceArtifactId\":\"" + _SOURCE_WORKSPACE_ARTIFACT_ID + "\",\"TargetWorkspaceArtifactId\":" + _TARGET_WORKSPACE_ARTIFACT_ID + "}";
		private readonly string DestinationConfiguration = "{\"ImportOverwriteMode\":\"AppendOnly\",\"UseFolderPathInformation\":\"false\",\"FieldOverlayBehavior\":\"Use Field Settings\"}";

		private readonly int[] _fieldsArtifactId = new int[] { 1000186, 1003667, 1035368, 1038073, 1038074, 1038389, 1035395 };

		[SetUp]
		public void Setup()
		{
			_sourceFieldManager = Substitute.For<IFieldManager>();
			_targetFieldManager = Substitute.For<IFieldManager>();
			_instance = new FieldsMappingValidator(new JSONSerializer(), _sourceFieldManager, _targetFieldManager);
		}

		[TestCase("[{\"sourceField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"}]")]
		[TestCase("[{\"sourceField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"destinationField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"fieldMapType\":\"Identifier\"},{\"sourceField\":{\"displayName\":\"Email To\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Email To\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Alert\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038073\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Visualization\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038074\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Lists\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038389\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Title\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035395\",\"isRequired\":false},\"fieldMapType\":\"None\"}]")]
		public void ItShouldValidateFieldMap(string fieldMap)
		{
			// Arrange
			IntegrationPointProviderValidationModel integrationPointProviderValidationModel = GetFieldMapValidationObject(fieldMap);
			MockFieldRepository();

			// Act
			ValidationResult result = _instance.Validate(integrationPointProviderValidationModel);

			// Assert
			Assert.IsTrue(result.IsValid);
			Assert.IsNull(result.Messages.FirstOrDefault());
		}

		[Test]
		public void ItShouldValidateDestinationFieldNotMapped()
		{
			// Arrange
			string fieldMap = "[{\"sourceField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"}]";
			string errorMessage = RelativityProviderValidationMessages.FIELD_MAP_DESTINATION_FIELD_NOT_MAPPED;

			IntegrationPointProviderValidationModel integrationPointProviderValidationModel = GetFieldMapValidationObject(fieldMap);
			MockFieldRepository();

			// Act
			ValidationResult result = _instance.Validate(integrationPointProviderValidationModel);

			// Assert
			Assert.IsFalse(result.IsValid);
			Assert.IsTrue(result.Messages.Any(x => x.Contains(errorMessage)));
		}

		[Test]
		public void ItShouldValidateSourceFieldNotMapped()
		{
			// Arrange
			string fieldMap = "[{\"destinationField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"}]";
			string errorMessage = RelativityProviderValidationMessages.FIELD_MAP_SOURCE_FIELD_NOT_MAPPED;

			IntegrationPointProviderValidationModel integrationPointProviderValidationModel = GetFieldMapValidationObject(fieldMap);
			MockFieldRepository();

			// Act
			ValidationResult result = _instance.Validate(integrationPointProviderValidationModel);

			// Assert
			Assert.IsFalse(result.IsValid);
			Assert.IsTrue(result.Messages.Any(x => x.Contains(errorMessage)));
		}

		[Test]
		public void ItShouldValidateSourceAndDestinationFieldsNotMapped()
		{
			// Arrange
			string fieldMap = "[{\"fieldMapType\":\"Identifier\"}]";
			string errorMessage = RelativityProviderValidationMessages.FIELD_MAP_SOURCE_AND_DESTINATION_FIELDS_NOT_MAPPED;

			IntegrationPointProviderValidationModel integrationPointProviderValidationModel = GetFieldMapValidationObject(fieldMap);
			MockFieldRepository();

			// Act
			ValidationResult result = _instance.Validate(integrationPointProviderValidationModel);

			// Assert
			Assert.IsFalse(result.IsValid);
			Assert.IsTrue(result.Messages.Any(x => x.Contains(errorMessage)));
		}

		[TestCase("[{\"sourceField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Control Number\",\"isIdentifier\":false,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"}]")]
		public void ItShouldValidateIdentifierNotMatchedCorrectly(string fieldMap)
		{
			// Arrange
			IntegrationPointProviderValidationModel integrationPointProviderValidationModel = GetFieldMapValidationObject(fieldMap);
			MockFieldRepository();

			// Act
			ValidationResult result = _instance.Validate(integrationPointProviderValidationModel);

			// Assert
			Assert.IsFalse(result.IsValid);
			Assert.IsTrue(result.Messages.Contains(RelativityProviderValidationMessages.FIELD_MAP_IDENTIFIERS_NOT_MATCHED));
		}

		[TestCase("[{\"sourceField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"destinationField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"fieldMapType\":\"Identifier\"},{\"sourceField\":{\"displayName\":\"Email To\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Email To\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Alert\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038073\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Visualization\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038074\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Lists\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038389\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Title\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035395\",\"isRequired\":false},\"fieldMapType\":\"None\"}]", new[] { "Control Number" })]
		[TestCase("[{\"sourceField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"destinationField\":{\"displayName\":\"Control Number [Object Identifier]\",\"isIdentifier\":true,\"fieldIdentifier\":\"1003667\",\"isRequired\":true},\"fieldMapType\":\"Identifier\"},{\"sourceField\":{\"displayName\":\"Email To [Object Identifier]\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":true},\"destinationField\":{\"displayName\":\"Email To [Object Identifier]\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035368\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Alert\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038073\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Visualization\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038074\",\"isRequired\":false},\"fieldMapType\":\"None\"},{\"sourceField\":{\"displayName\":\"Lists\",\"isIdentifier\":false,\"fieldIdentifier\":\"1038389\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Title\",\"isIdentifier\":false,\"fieldIdentifier\":\"1035395\",\"isRequired\":false},\"fieldMapType\":\"None\"}]", new[] { "Control Number", "Email To" })]
		public void ItShouldValidateAllRequiredFieldsMapped(string fieldMap, string[] requiredFields)
		{
			// Arrange
			IntegrationPointProviderValidationModel integrationPointProviderValidationModel = GetFieldMapValidationObject(fieldMap);
			MockFieldRepository(requiredFields.ToList());

			// Act
			ValidationResult result = _instance.Validate(integrationPointProviderValidationModel);

			// Assert
			Assert.IsTrue(result.IsValid);
			Assert.IsNull(result.Messages.FirstOrDefault());
		}

		[TestCase("{\"ImportOverwriteMode\":\"AppendOnly\",\"UseFolderPathInformation\":\"false\",\"FieldOverlayBehavior\":\"Use Field Settings\"}")]
		[TestCase("{\"ImportOverwriteMode\":\"AppendOnly\",\"UseFolderPathInformation\":\"true\",\"FieldOverlayBehavior\":\"Use Field Settings\",\"FolderPathSourceField\":\"1000186\"}")]
		[TestCase("{\"ImportOverwriteMode\":\"OverlayOnly\",\"UseFolderPathInformation\":\"false\",\"FieldOverlayBehavior\":\"Use Field Settings\"}")]
		[TestCase("{\"ImportOverwriteMode\":\"OverlayOnly\",\"UseFolderPathInformation\":\"false\",\"FieldOverlayBehavior\":\"Replace Values\"}")]
		[TestCase("{\"ImportOverwriteMode\":\"OverlayOnly\",\"UseFolderPathInformation\":\"false\",\"FieldOverlayBehavior\":\"Merge Values\"}")]
		[TestCase("{\"ImportOverwriteMode\":\"AppendOverlay\",\"UseFolderPathInformation\":\"false\",\"FieldOverlayBehavior\":\"Use Field Settings\"}")]
		[TestCase("{\"ImportOverwriteMode\":\"AppendOverlay\",\"UseFolderPathInformation\":\"true\",\"FieldOverlayBehavior\":\"Use Field Settings\",\"FolderPathSourceField\":\"1000186\"}")]
		[TestCase("{\"ImportOverwriteMode\":\"AppendOverlay\",\"UseFolderPathInformation\":\"false\",\"FieldOverlayBehavior\":\"Replace Values\"}")]
		[TestCase("{\"ImportOverwriteMode\":\"AppendOverlay\",\"UseFolderPathInformation\":\"false\",\"FieldOverlayBehavior\":\"Merge Values\"}")]
		public void ItShouldValidateValidSettings(string destinationConfig)
		{
			// Arrange
			const string fieldMap = "[{\"sourceField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"}]";
			IntegrationPointProviderValidationModel integrationPointProviderValidationModel = GetFieldMapValidationObject(fieldMap);
			integrationPointProviderValidationModel.DestinationConfiguration = destinationConfig;
			MockFieldRepository();

			// Act
			ValidationResult result = _instance.Validate(integrationPointProviderValidationModel);

			// Assert
			Assert.IsTrue(result.IsValid);
			Assert.IsNull(result.Messages.FirstOrDefault());
		}

		[TestCase("{\"ImportOverwriteMode\":\"AppendOnly\",\"UseFolderPathInformation\":\"false\",\"FieldOverlayBehavior\":\"Replace Values\"}")]
		[TestCase("{\"ImportOverwriteMode\":\"AppendOnly\",\"UseFolderPathInformation\":\"false\",\"FieldOverlayBehavior\":\"Merge Values\"}")]
		public void ItShouldValidateInvalidSettingsFieldOverlayBehavior_AppendOnly(string destinationConfig)
		{
			// Arrange
			const string fieldMap = "[{\"sourceField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"}]";
			IntegrationPointProviderValidationModel integrationPointProviderValidationModel = GetFieldMapValidationObject(fieldMap);
			integrationPointProviderValidationModel.DestinationConfiguration = destinationConfig;
			MockFieldRepository();

			// Act
			ValidationResult result = _instance.Validate(integrationPointProviderValidationModel);

			// Assert
			Assert.IsFalse(result.IsValid);
			Assert.IsTrue(result.Messages.Contains(RelativityProviderValidationMessages.FIELD_MAP_APPEND_ONLY_INVALID_OVERLAY_BEHAVIOR));
		}

		[TestCase("{\"ImportOverwriteMode\":\"OverlayOnly\",\"UseFolderPathInformation\":\"false\",\"FieldOverlayBehavior\":\"INVALID_FieldOverlayBehavior\"}")]
		[TestCase("{\"ImportOverwriteMode\":\"AppendOverlay\",\"UseFolderPathInformation\":\"false\",\"FieldOverlayBehavior\":\"INVALID_FieldOverlayBehavior\"}")]
		public void ItShouldValidateInvalidSettingsFieldOverlayBehavior_AppendOverlayAndOverlayOnly(string destinationConfig)
		{
			// Arrange
			const string fieldMap = "[{\"sourceField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"}]";
			IntegrationPointProviderValidationModel integrationPointProviderValidationModel = GetFieldMapValidationObject(fieldMap);
			integrationPointProviderValidationModel.DestinationConfiguration = destinationConfig;
			MockFieldRepository();

			// Act
			ValidationResult result = _instance.Validate(integrationPointProviderValidationModel);

			// Assert
			Assert.IsFalse(result.IsValid);
			Assert.IsTrue(result.Messages.Any(x => x.Contains(RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_INVALID)));
		}

		[TestCase("{\"ImportOverwriteMode\":\"OverlayOnly\",\"UseFolderPathInformation\":\"true\",\"FieldOverlayBehavior\":\"Use Field Settings\"}")]
		public void ItShouldValidateAsValidSettingsForFolderPathInformation_With_OverlayOnly(string destinationConfig)
		{
			// Arrange
			const string fieldMap = "[{\"sourceField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"}]";
			IntegrationPointProviderValidationModel integrationPointProviderValidationModel = GetFieldMapValidationObject(fieldMap);
			integrationPointProviderValidationModel.DestinationConfiguration = destinationConfig;
			MockFieldRepository();

			// Act
			ValidationResult result = _instance.Validate(integrationPointProviderValidationModel);

			// Assert
			Assert.IsTrue(result.IsValid);
		}

		[TestCase("{\"ImportOverwriteMode\":\"AppendOnly\",\"UseFolderPathInformation\":\"true\",\"FieldOverlayBehavior\":\"Use Field Settings\",\"FolderPathSourceField\":\"-1\"}")]
		[TestCase("{\"ImportOverwriteMode\":\"AppendOverlay\",\"UseFolderPathInformation\":\"true\",\"FieldOverlayBehavior\":\"Use Field Settings\",\"FolderPathSourceField\":\"-1\"}")]
		public void ItShouldValidateInvalidSettingsFolderPathInformation(string destinationConfig)
		{
			// Arrange
			const string fieldMap = "[{\"sourceField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"destinationField\":{\"displayName\":\"Control Number\",\"isIdentifier\":true,\"fieldIdentifier\":\"1000186\",\"isRequired\":false},\"fieldMapType\":\"Identifier\"}]";
			IntegrationPointProviderValidationModel integrationPointProviderValidationModel = GetFieldMapValidationObject(fieldMap);
			integrationPointProviderValidationModel.DestinationConfiguration = destinationConfig;
			MockFieldRepository();

			// Act
			ValidationResult result = _instance.Validate(integrationPointProviderValidationModel);

			// Assert
			Assert.IsFalse(result.IsValid);
			Assert.IsTrue(result.Messages.Any(x => x.Contains(RelativityProviderValidationMessages.FIELD_MAP_FIELD_NOT_EXIST_IN_SOURCE_WORKSPACE)));
		}

		private IntegrationPointProviderValidationModel GetFieldMapValidationObject(string fieldsMap)
		{
			return new IntegrationPointProviderValidationModel()
			{
				FieldsMap = fieldsMap,
				SourceProviderIdentifier = IntegrationPoints.Domain.Constants.RELATIVITY_PROVIDER_GUID,
				DestinationProviderIdentifier = Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString(),
				SourceConfiguration = SourceConfiguration,
				DestinationConfiguration = DestinationConfiguration
			};
		}

		private void MockFieldRepository(List<string> requiredFields = null)
		{
			List<ArtifactDTO> fieldArtifacts = new List<ArtifactDTO>();

			if (requiredFields != null)
			{
				foreach (string field in requiredFields)
				{
					var fields = new List<ArtifactFieldDTO>()
					{
						new ArtifactFieldDTO() {Name = RelativityProviderValidationMessages.FIELD_MAP_FIELD_NAME, Value = field},
						new ArtifactFieldDTO() {Name = RelativityProviderValidationMessages.FIELD_MAP_FIELD_IS_IDENTIFIER, Value = "1"}
					};
					fieldArtifacts.Add(new ArtifactDTO(0, 0, "", fields));
				}
			}

			foreach (int artifactId in _fieldsArtifactId)
			{
				fieldArtifacts.Add(new ArtifactDTO(artifactId, 0, "", new List<ArtifactFieldDTO>()));
			}

			SetReturnValue(_sourceFieldManager, _SOURCE_WORKSPACE_ARTIFACT_ID, fieldArtifacts);
			SetReturnValue(_targetFieldManager, _TARGET_WORKSPACE_ARTIFACT_ID, fieldArtifacts);
		}

		private void SetReturnValue(IFieldManager fieldManager, int workspaceId, List<ArtifactDTO> fieldArtifacts)
		{
			const int artifactTypeDocument = 10;
			fieldManager.RetrieveFields(artifactTypeDocument,
				new HashSet<string>(new[]
				{
					RelativityProviderValidationMessages.FIELD_MAP_FIELD_NAME,
					RelativityProviderValidationMessages.FIELD_MAP_FIELD_IS_IDENTIFIER
				}))
				.ReturnsForAnyArgs(fieldArtifacts.ToArray());
		}
	}
}
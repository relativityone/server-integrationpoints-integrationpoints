using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
	[TestFixture]
	public class IntegrationPointExecutionValidatorTests
	{
	    private IIntegrationPointExecutionValidator _subjectUnderTest;
	    private ISerializer _serializer;

	    private IRelativityProviderValidatorsFactory _validatorsFactoryMock;
	    private IValidator _savedSearchValidatorMock;

	    private ValidationResult _validationError;


		private const int _SAVED_SEARCH_ARTIFACT_ID = 1038052;
		private const int _SOURCE_WORKSPACE_ARTIFACT_ID = 1074540;

		private const string _SOURCE_CONFIGURATION_OF_RELATIVITY_PROVIDER =
			"{\"SavedSearchArtifactId\":\"1038052\"" +
            ",\"SourceWorkspaceArtifactId\":1074540" +
            ",\"TargetWorkspaceArtifactId\":\"1075642\"" +
            ",\"FolderArtifactId\":\"1039185\",\"FolderArtifactName\":\"Test Folder\",\"TypeOfExport\":\"3\"}";

	    private const string _SOURCE_CONFIGURATION_OF_EXPORT_TO_LOAD_FILE =
	        "{\"SavedSearchArtifactId\":\"1038052\"" +
	        ",\"SourceWorkspaceArtifactId\":1074540" +
	        ",\"ExportType\":\"3\"}";

	    private const string _SOURCE_CONFIGURATION_ENCRYPTED_LDAP =
	        "SkvgTNyRfuPpBK3wl+mDS1tCsGuQDZW/Nefbz/9RX5Sf9nC8" +
	        "TFtrA+4/HzwQ7/WDYCMd28S8rxwHsemyOkni2beNBLhInm6M" +
	        "d8pebzSW3OsQxjiCSpMCxYChhGJYkX2TVY8qkAmAD2gGGEf2" +
	        "KkecMI0BB0nCQLvBatbKR+a8HpHXWZ/mtsRjI7S1SixY1Alc" +
	        "+j+XnWY8FmhZ+lMkgCLWSZU6XegW7lAjv1zDQmd5hWgcZ8jx" +
	        "wBdBtrjkFuR6jXjpS4Z0ZFDPyIr7FVUYuP3zj20QuRCAQoH3" +
	        "Wuqg6qEbrsjbK66YdtZ93IqPjNZk0+z/+fTX39hi3SfvyuAJ" +
	        "oNI94B7HPjOdi1FNQvRJZs4bVl9hMyoU/DKS+PjVV1oEL8bC" +
	        "nFObZ4+7pYaAt8jcVOjmkO28uXhKigTaEhjW7FLwQ9al0u7/" +
	        "G1Ib4zINsjc4xd/ZgD5cJvTEC2z4/WRAAxGSONByrl6AwsbV" +
	        "D5Hz6N6zoDvpHD7r/E2IR0xaJ7+jKlbi72HiqQyeIW2mrXZ4M" +
	        "KaWNy/KgP+rzrYD2rBMDvK0y2o08Qg+A4Ffl1PVdWI8oQTbiJiwuKjKSTec04D8prAAew==";

        [SetUp]
	    public void SetUp()
        {
	        IAPILog logger = Substitute.For<IAPILog>();
	        _serializer = new JSONSerializer();
	        _validatorsFactoryMock = Substitute.For<IRelativityProviderValidatorsFactory>();
	        _savedSearchValidatorMock = Substitute.For<SavedSearchValidator>(logger, Substitute.For<ISavedSearchQueryRepository>(), _SAVED_SEARCH_ARTIFACT_ID );

            _subjectUnderTest = new IntegrationPointExecutionValidator( _serializer, _validatorsFactoryMock);

	        _validatorsFactoryMock.CreateSavedSearchValidator(_SOURCE_WORKSPACE_ARTIFACT_ID, _SAVED_SEARCH_ARTIFACT_ID)
	            .Returns(_savedSearchValidatorMock);
	    }

		[TestCase(_SOURCE_CONFIGURATION_OF_RELATIVITY_PROVIDER)]
		[TestCase(_SOURCE_CONFIGURATION_OF_EXPORT_TO_LOAD_FILE)]
        public void ItShouldValidateSavedSearch(string sourceConfiguration)
		{
            // arrange
		    var model = new IntegrationPointModel()
		    {
		        SourceConfiguration = sourceConfiguration
		    };

            _validationError = new ValidationResult(false, Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NOT_PUBLIC);
		    _savedSearchValidatorMock.Validate(_SAVED_SEARCH_ARTIFACT_ID).Returns(_validationError);

			// act
		    var result = _subjectUnderTest.Validate(model);

			// assert
			Assert.False( result.IsValid );
		    CollectionAssert.AreEquivalent(_validationError.Messages, result.Messages);

            _savedSearchValidatorMock.Received(1).Validate(_SAVED_SEARCH_ARTIFACT_ID);
		}


        [Test]
	    public void ItShouldNotThrowExceptionWhenSourceConfigurationIsEncrypted()
	    {
	        // arrange
	        var model = new IntegrationPointModel()
	        {
	            SourceConfiguration = _SOURCE_CONFIGURATION_ENCRYPTED_LDAP
	        };

	        _validationError = new ValidationResult(false, Constants.IntegrationPoints.PermissionErrors.SAVED_SEARCH_NOT_PUBLIC);
	        _savedSearchValidatorMock.Validate(_SAVED_SEARCH_ARTIFACT_ID).Returns(_validationError);

	        // act
	        var result = _subjectUnderTest.Validate(model);

	        // assert
	        //Assert.False(result.IsValid);
	        //CollectionAssert.AreEquivalent(_validationError.Messages, result.Messages);

	        //_savedSearchValidatorMock.Received(1).Validate(_SAVED_SEARCH_ARTIFACT_ID);
	    }
    }
}
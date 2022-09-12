using System;
using System.Collections.Generic;
using FluentAssertions;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Constants = kCura.IntegrationPoints.Core.Constants;
using Field = kCura.EventHandler.Field;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers.Implementations
{
    [TestFixture]
    [Category("Unit")]
    internal class IntegrationPointViewPreLoadTests
    {
        private IIntegrationPointBaseFieldsConstants _fieldsConstants;

        private Mock<IRelativityProviderConfiguration> _relativityProviderSourceConfigurationFake;
        private Mock<IRelativityProviderConfiguration> _relativityProviderDestinationConfigurationMock;
        private Mock<ICaseServiceContext> _caseServiceContextMock;
        private Mock<IRelativityObjectManagerService> _relativityObjectManagerServiceMock;
        private Mock<IEHHelper> _helperMock;
        private Mock<IAPILog> _loggerMock;
        private Mock<IDBContext> _dbContextMock;
        private Mock<IRelativityObjectManager> _objectManagerMock;

        private Artifact _artifact;

        private IntegrationPointViewPreLoad _sut;

        private const string _INTEGRATION_POINT_NAME = "A7 Integration Point";
        private const string _SAVEDSEARCH_ARTIFACT_ID_KEY = "SavedSearchArtifactId";
        private const string _SAVEDSEARCH_NAME = "A7 SavedSearch";
        private const int _SAVEDSEARCH_ARTIFACT_ID_VALUE = 1234;
        private const string _PRODUCTION_ARTIFACT_ID_KEY = "SourceProductionId";
        private const int _PRODUCTION_ARTIFACT_ID_VALUE = 4321;
        private const string _SOURCE_VIEW_ARTIFACT_ID_KEY = "SourceViewId";
        private const int _SOURCE_VIEW_ARTIFACT_ID_VALUE = 2131;
        private const int _SOURCE_PROVIDER_VALUE = 1038696;
        private const int _WORKSPACE_ID = 432145;

        [SetUp]
        public void SetUp()
        {
            _fieldsConstants = new IntegrationPointFieldsConstants();

            _relativityProviderSourceConfigurationFake = new Mock<IRelativityProviderConfiguration>();
            _relativityProviderDestinationConfigurationMock = new Mock<IRelativityProviderConfiguration>();
            _caseServiceContextMock = new Mock<ICaseServiceContext>();
            _relativityObjectManagerServiceMock = new Mock<IRelativityObjectManagerService>();
            _objectManagerMock = new Mock<IRelativityObjectManager>();
            _helperMock = new Mock<IEHHelper>();

            _caseServiceContextMock.Setup(x => x.RelativityObjectManagerService).Returns(_relativityObjectManagerServiceMock.Object);
            _relativityObjectManagerServiceMock.Setup(x => x.RelativityObjectManager).Returns(_objectManagerMock.Object);
            SourceProvider sourceProvider = new SourceProvider
            {
                Name = Constants.IntegrationPoints.RELATIVITY_PROVIDER_NAME
            };

            _objectManagerMock
                .Setup(x => x.Read<SourceProvider>(_SOURCE_PROVIDER_VALUE, ExecutionIdentity.CurrentUser))
                .Returns(sourceProvider);

            FieldCollection fields = new FieldCollection();
            _artifact = new Artifact(1093775, 1003663, 1000044, "Integration Point", false, fields);

            _sut = new IntegrationPointViewPreLoad(
                _caseServiceContextMock.Object,
                _relativityProviderSourceConfigurationFake.Object,
                _relativityProviderDestinationConfigurationMock.Object,
                _fieldsConstants,
                _objectManagerMock.Object,
                _helperMock.Object);
        }

        [Test]
        public void PreLoad_ShouldUpdateSourceConfigurationNames_WhenRelativitySourceProviderIsSelected()
        {
            // Arrange
            IDictionary<string, object> sourceConfiguration = new Dictionary<string, object>();
            sourceConfiguration[_SAVEDSEARCH_ARTIFACT_ID_KEY] = _SAVEDSEARCH_ARTIFACT_ID_VALUE;

            CreateArtifactFields(sourceConfiguration);

            // Act
            _sut.PreLoad(_artifact);

            // Assert
            _relativityProviderSourceConfigurationFake.Verify(
                x => x.UpdateNames(
                It.Is<IDictionary<string, object>>(
                    y => int.Parse(y[_SAVEDSEARCH_ARTIFACT_ID_KEY].ToString()) == _SAVEDSEARCH_ARTIFACT_ID_VALUE),
                It.Is<Artifact>(
                    y => JsonConvert.DeserializeObject<IDictionary<string, object>>(
                        y.Fields[_fieldsConstants.SourceConfiguration].Value.Value.ToString())[_SAVEDSEARCH_ARTIFACT_ID_KEY].ToString() == _SAVEDSEARCH_ARTIFACT_ID_VALUE.ToString())),
                Times.Once);
        }

        [Test]
        public void PreLoad_ShouldNotUpdateSourceConfigurationNames_WhenSourceProviderIsNotRelativity()
        {
            // Arrange
            IDictionary<string, object> sourceConfiguration = new Dictionary<string, object>();
            sourceConfiguration[_SAVEDSEARCH_ARTIFACT_ID_KEY] = _SAVEDSEARCH_ARTIFACT_ID_VALUE;
            SourceProvider sourceProvider = new SourceProvider
            {
                Name = "Load File"
            };
            _objectManagerMock
                .Setup(x => x.Read<SourceProvider>(_SOURCE_PROVIDER_VALUE, ExecutionIdentity.CurrentUser))
                .Returns(sourceProvider);

            CreateArtifactFields(sourceConfiguration);

            // Act
            _sut.PreLoad(_artifact);

            // Assert
            _relativityProviderSourceConfigurationFake.Verify(
                x => x.UpdateNames(
                    It.Is<IDictionary<string, object>>(
                        y => int.Parse(y[_SAVEDSEARCH_ARTIFACT_ID_KEY].ToString()) == _SAVEDSEARCH_ARTIFACT_ID_VALUE),
                    It.Is<Artifact>(
                        y => JsonConvert.DeserializeObject<IDictionary<string, object>>(
                            y.Fields[_fieldsConstants.SourceConfiguration].Value.Value.ToString())[_SAVEDSEARCH_ARTIFACT_ID_KEY].ToString() == _SAVEDSEARCH_ARTIFACT_ID_VALUE.ToString())),
                Times.Never);
        }

        [Test]
        public void ResetSavedSearch_ShouldUpdateSourceSavedSearchArtifactId_WhenSavedSearchArtifactIdNotFoundButItShould()
        {
            // Arrange
            IDictionary<string, object> sourceConfiguration = new Dictionary<string, object>();
            sourceConfiguration[_SAVEDSEARCH_ARTIFACT_ID_KEY] = 0;
            CreateArtifactFields(sourceConfiguration);
            PrepareResetSavedSearchMocks();

            // Act
            _sut.ResetSavedSearch((artifact) => { }, _artifact);

            // Assert
            ResetSavedSearchAssert(Times.Once(), Times.Once(), _SAVEDSEARCH_ARTIFACT_ID_VALUE.ToString());
        }

        [Test]
        public void ResetSavedSearch_ShouldNotUpdateSourceSavedSearchArtifactId_WhenSourceProviderIsNotRelativity()
        {
            // Arrange
            IDictionary<string, object> sourceConfiguration = new Dictionary<string, object>();
            const int savedSearchArtifactId = 11223344;
            sourceConfiguration[_SAVEDSEARCH_ARTIFACT_ID_KEY] = savedSearchArtifactId;
            CreateArtifactFields(sourceConfiguration);
            PrepareResetSavedSearchMocks();

            SourceProvider sourceProvider = new SourceProvider
            {
                Name = "Load File"
            };
            _objectManagerMock
                .Setup(x => x.Read<SourceProvider>(_SOURCE_PROVIDER_VALUE, ExecutionIdentity.CurrentUser))
                .Returns(sourceProvider);

            // Act
            _sut.ResetSavedSearch((artifact) => { }, _artifact);

            // Assert
            ResetSavedSearchAssert(Times.Never(), Times.Never(), savedSearchArtifactId.ToString());
        }

        [Test]
        public void ResetSavedSearch_ShouldNotUpdateSourceSavedSearchArtifactId_WhenProductionArtifactIdFound()
        {
            // Arrange
            IDictionary<string, object> sourceConfiguration = new Dictionary<string, object>();
            sourceConfiguration[_PRODUCTION_ARTIFACT_ID_KEY] = _PRODUCTION_ARTIFACT_ID_VALUE;
            CreateArtifactFields(sourceConfiguration);
            PrepareResetSavedSearchMocks();

            // Act
            _sut.ResetSavedSearch((artifact) => { }, _artifact);

            // Assert
            ResetSavedSearchAssert(Times.Never(), Times.Never(), _PRODUCTION_ARTIFACT_ID_VALUE.ToString());
        }

        [Test]
        public void ResetSavedSearch_ShouldNotUpdateSourceSavedSearchArtifactId_WhenSourceViewArtifactIdFound()
        {
            // Arrange
            IDictionary<string, object> sourceConfiguration = new Dictionary<string, object>();
            sourceConfiguration[_SOURCE_VIEW_ARTIFACT_ID_KEY] = _SOURCE_VIEW_ARTIFACT_ID_VALUE;
            CreateArtifactFields(sourceConfiguration);
            PrepareResetSavedSearchMocks();

            // Act
            _sut.ResetSavedSearch((artifact) => { }, _artifact);

            // Assert
            ResetSavedSearchAssert(Times.Never(), Times.Never(), _SOURCE_VIEW_ARTIFACT_ID_VALUE.ToString());
        }

        [Test]
        public void ResetSavedSearch_ShouldLogError_WhenFailedToGetSourceConfigurationFromDatabase()
        {
            // Arrange
            IDictionary<string, object> sourceConfiguration = new Dictionary<string, object>();
            sourceConfiguration[_SAVEDSEARCH_ARTIFACT_ID_KEY] = 0;
            CreateArtifactFields(sourceConfiguration);
            PrepareResetSavedSearchMocks();
            _dbContextMock.Setup(x => x.ExecuteSqlStatementAsScalar<string>(It.IsAny<string>())).Throws(new Exception());

            // Act
            Action action = () => _sut.ResetSavedSearch((artifact) => { }, _artifact);

            // Assert
            action.ShouldThrow<Exception>();
            _loggerMock.Verify(
                x =>
                    x.LogError(
                        "Unable to get SavedSearchArtifactId for integrationPoint - {integrationPoint}",
                        _INTEGRATION_POINT_NAME),
                Times.Once);
            ResetSavedSearchAssert(Times.Once(), Times.Never(), _SAVEDSEARCH_ARTIFACT_ID_VALUE.ToString(), false);
        }

        [Test]
        public void ResetSavedSearch_ShouldLogError_WhenFailedToGetSavedSearchWithObjectManager()
        {
            // Arrange
            IDictionary<string, object> sourceConfiguration = new Dictionary<string, object>();
            sourceConfiguration[_SAVEDSEARCH_ARTIFACT_ID_KEY] = 0;
            CreateArtifactFields(sourceConfiguration);
            PrepareResetSavedSearchMocks();
            _objectManagerMock.Setup(x => x.QueryAsync(It.IsAny<QueryRequest>(), ExecutionIdentity.CurrentUser)).Throws(new Exception());

            // Act
            Action action = () => _sut.ResetSavedSearch((artifact) => { }, _artifact);

            // Assert
            action.ShouldThrow<Exception>();
            _loggerMock.Verify(
                x =>
                    x.LogError(
                        "ObjectManager unable to read savedSearch with ArtifactId - {savedSearchArtifactId}.",
                        _SAVEDSEARCH_ARTIFACT_ID_VALUE),
                Times.Once);
            ResetSavedSearchAssert(Times.Once(), Times.Never(), _SAVEDSEARCH_ARTIFACT_ID_VALUE.ToString(), false);
        }

        private void CreateArtifactFields(IDictionary<string, object> sourceConfiguration)
        {
            FieldCollection fields = _artifact.Fields;
            Field sourceConfigurationField = new Field(
                1038570,
                _fieldsConstants.SourceConfiguration,
                _fieldsConstants.SourceConfiguration,
                4,
                null,
                0,
                false,
                true,
                new FieldValue(JsonConvert.SerializeObject(sourceConfiguration)),
                null);
            Field destinationConfigurationField = new Field(
                1038571,
                _fieldsConstants.DestinationConfiguration,
                _fieldsConstants.DestinationConfiguration,
                4,
                null,
                0,
                false,
                true,
                new FieldValue(JsonConvert.SerializeObject(new Dictionary<string, object>())),
                null);
            Field sourceProviderField = new Field(
                1038572,
                _fieldsConstants.SourceProvider,
                _fieldsConstants.SourceProvider,
                10,
                null,
                0,
                false,
                true,
                new FieldValue(_SOURCE_PROVIDER_VALUE),
                null);
            Field nameField = new Field(
                1038480,
                _fieldsConstants.Name,
                _fieldsConstants.Name,
                0,
                null,
                2,
                false,
                true,
                new FieldValue(_INTEGRATION_POINT_NAME),
                null);
            fields.Add(sourceConfigurationField);
            fields.Add(destinationConfigurationField);
            fields.Add(sourceProviderField);
            fields.Add(nameField);
        }

        private void PrepareResetSavedSearchMocks()
        {
            Mock<ILogFactory> loggerFactoryMock = new Mock<ILogFactory>();
            _loggerMock = new Mock<IAPILog>();
            _dbContextMock = new Mock<IDBContext>();

            _helperMock.Setup(x => x.GetDBContext(It.IsAny<int>())).Returns(_dbContextMock.Object);

            _helperMock.Setup(x => x.GetActiveCaseID()).Returns(_WORKSPACE_ID);
            _helperMock.Setup(x => x.GetLoggerFactory()).Returns(loggerFactoryMock.Object);
            loggerFactoryMock.Setup(x => x.GetLogger()).Returns(_loggerMock.Object);

            Dictionary<string, object> dbSourceConfiguration = new Dictionary<string, object>();
            dbSourceConfiguration[_SAVEDSEARCH_ARTIFACT_ID_KEY] = _SAVEDSEARCH_ARTIFACT_ID_VALUE;
            string sqlQuery = $"SELECT [SourceConfiguration] FROM [EDDS{_WORKSPACE_ID}].[EDDSDBO].[IntegrationPoint] WHERE [Name] = '{_INTEGRATION_POINT_NAME}'";
            _dbContextMock.Setup(x => x.ExecuteSqlStatementAsScalar<string>(sqlQuery))
                .Returns(JsonConvert.SerializeObject(dbSourceConfiguration));

            List<RelativityObject> result = new List<RelativityObject>
            {
                new RelativityObject
                {
                    FieldValues = new List<FieldValuePair>
                    {
                        new FieldValuePair
                        {
                            Value = _SAVEDSEARCH_NAME
                        }
                    }
                }
            };

            _objectManagerMock.Setup(
                    x => x.QueryAsync(
                    It.Is<QueryRequest>(
                        y => 
                            y.ObjectType.ArtifactTypeID == (int)ArtifactType.Search &&
                            y.Condition == $"'Artifact ID' == {_SAVEDSEARCH_ARTIFACT_ID_VALUE}"),
                    It.IsAny<ExecutionIdentity>()))
                .ReturnsAsync(result);
        }

        private void ResetSavedSearchAssert(
            Times logWarningInvocationTimes,
            Times logInformationInvocationTimes,
            string artifactIdValue,
            bool isArtifactShouldContain = true)
        {
            if (isArtifactShouldContain)
            {
                _artifact.Fields[_fieldsConstants.SourceConfiguration].Value.Value.ToString()
                    .Should().Contain(artifactIdValue);
            }
            else
            {
                _artifact.Fields[_fieldsConstants.SourceConfiguration].Value.Value.ToString()
                    .Should().NotContain(artifactIdValue);
            }

            _loggerMock.Verify(
                x =>
                    x.LogWarning(
                        "savedSearchArtifactId is 0, trying to read it from database Integration Point settings."),
                logWarningInvocationTimes);
            _loggerMock.Verify(
                x =>
                    x.LogInformation(
                        "PreLoadEventHandler savedSearch configuration reset; savedSearchArtifactId - {savedSearchArtifactId}, savedSearchName - {savedSearchName}.",
                        _SAVEDSEARCH_ARTIFACT_ID_VALUE,
                        _SAVEDSEARCH_NAME),
                logInformationInvocationTimes);
        }
    }
}

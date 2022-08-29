using System;
using System.Collections;
using System.Collections.Generic;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.API;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers.Implementations
{
    [TestFixture]
    [Category("Unit")]
    internal class IntegrationPointViewPreLoadTests
    {
        private IIntegrationPointBaseFieldsConstants _fieldsConstants;
        private Mock<IRelativityProviderConfiguration> _relativityProviderSourceConfigurationMock;
        private Mock<IRelativityProviderConfiguration> _relativityProviderDestinationConfigurationMock;
        private Mock<ICaseServiceContext> _caseServiceContextMock;
        private Mock<IRelativityObjectManagerService> _relativityObjectManagerServiceMock;
        private Mock<IRelativityObjectManager> _relativityObjectManagerMock;

        private Artifact _artifact;

        private IntegrationPointViewPreLoad _sut;

        private const string _SAVEDSEARCH_ARTIFACT_ID_KEY = "SavedSearchArtifactId";
        private const int _SAVEDSEARCH_ARTIFACT_ID_VALUE = 1234;
        private const int _SOURCE_PROVIDER_VALUE = 1038696;

        [SetUp]
        public void SetUp()
        {
            _fieldsConstants = new IntegrationPointFieldsConstants();
            _relativityProviderSourceConfigurationMock = new Mock<IRelativityProviderConfiguration>();
            _relativityProviderDestinationConfigurationMock = new Mock<IRelativityProviderConfiguration>();
            _caseServiceContextMock = new Mock<ICaseServiceContext>();
            _relativityObjectManagerServiceMock = new Mock<IRelativityObjectManagerService>();
            _relativityObjectManagerMock = new Mock<IRelativityObjectManager>();

            _caseServiceContextMock.Setup(x => x.RelativityObjectManagerService).Returns(_relativityObjectManagerServiceMock.Object);
            _relativityObjectManagerServiceMock.Setup(x => x.RelativityObjectManager).Returns(_relativityObjectManagerMock.Object);
            SourceProvider sourceProvider = new SourceProvider
            {
                Name = Constants.IntegrationPoints.RELATIVITY_PROVIDER_NAME
            };

            _relativityObjectManagerMock
                .Setup(x => x.Read<SourceProvider>(_SOURCE_PROVIDER_VALUE, ExecutionIdentity.CurrentUser))
                .Returns(sourceProvider);

            FieldCollection fields = new FieldCollection();
            _artifact = new Artifact(1093775, 1003663, 1000044, "Integration Point", false, fields);

            _sut = new IntegrationPointViewPreLoad(
                _caseServiceContextMock.Object,
                _relativityProviderSourceConfigurationMock.Object,
                _relativityProviderDestinationConfigurationMock.Object,
                _fieldsConstants);
        }

        [Test]
        public void PreLoad_()
        {
            // Arrange
            IDictionary<string, object> sourceConfiguration = new Dictionary<string, object>();
            sourceConfiguration[_SAVEDSEARCH_ARTIFACT_ID_KEY] = _SAVEDSEARCH_ARTIFACT_ID_VALUE;

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
            fields.Add(sourceConfigurationField);
            fields.Add(destinationConfigurationField);
            fields.Add(sourceProviderField);

            // Act
            _sut.PreLoad(_artifact);

            // Assert

        }
    }
}

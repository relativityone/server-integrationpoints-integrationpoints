using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.FieldsMapping;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    [TestFixture, Category("Unit")]
    internal class WorkspaceFieldControllerTests : WebControllerTestBase
    {
        #region Fields

        private WorkspaceFieldController _subjectUnderTest;

        private ISynchronizerFactory _synchronizerFactoryMock;
        private IDataSynchronizer _dataSynchronizerMock;

        private ISerializer _serializer;
        private SynchronizerSettings _synchronizerSettings;


        private const int _ARTIFACT_TYPE_ID = 10;
        private const int _DESTINATION_FOLDER_ARTIFACT_ID = 2;
        private const string _CREDENTIALS = "password";
        private const string _DISPLAY_NAME = "FieldName";
        private const string _IDENTIFIER = "Identifier";

        private readonly List<FieldEntry> _fields = new List<FieldEntry>()
        {
            new FieldEntry()
            {
                DisplayName = _DISPLAY_NAME,
                FieldType = FieldType.File,
                IsIdentifier = true,
                FieldIdentifier = _IDENTIFIER
            }
        };

        #endregion //Fields

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _synchronizerFactoryMock = Substitute.For<ISynchronizerFactory>();
            _dataSynchronizerMock = Substitute.For<IDataSynchronizer>();

            _serializer = new JSONSerializer();

             _synchronizerSettings = new SynchronizerSettings()
            {
                Settings =
                    $"{{ artifactTypeID: {_ARTIFACT_TYPE_ID}, DestinationFolderArtifactId: {_DESTINATION_FOLDER_ARTIFACT_ID} }}",
                Credentials = _CREDENTIALS
            };

            _subjectUnderTest = new WorkspaceFieldController(_synchronizerFactoryMock, _serializer, Helper)
            {
                Request = new HttpRequestMessage()
            };

            _subjectUnderTest.Request.SetConfiguration(new HttpConfiguration());
        }

        [Test]
        public void ItShouldGetFields()
        {
            // Arrange
            _synchronizerFactoryMock.CreateSynchronizer( Guid.Empty, _synchronizerSettings.Settings).Returns(_dataSynchronizerMock);
            _dataSynchronizerMock.GetFields(Arg.Any<DataSourceProviderConfiguration>()).Returns(_fields);

            // Act
            HttpResponseMessage httpResponseMessage = _subjectUnderTest
                .Post(_synchronizerSettings);

            // Assert
            List<ClassifiedFieldDTO> retValue;
            httpResponseMessage.TryGetContentValue(out retValue);

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            CollectionAssert.AreEqual(_fields.Select(x => x.IsIdentifier), retValue.Select(x => x.IsIdentifier));
            CollectionAssert.AreEqual(_fields.Select(x => x.DisplayName), retValue.Select(x => x.Name));
            CollectionAssert.AreEqual(_fields.Select(x => x.FieldIdentifier), retValue.Select(x => x.FieldIdentifier));

            _dataSynchronizerMock.Received(1).GetFields(Arg.Is<DataSourceProviderConfiguration>(x =>
                (_serializer.Deserialize<ImportSettings>(x.Configuration).ArtifactTypeId == _ARTIFACT_TYPE_ID) &&
                (_serializer.Deserialize<ImportSettings>(x.Configuration).DestinationFolderArtifactId == _DESTINATION_FOLDER_ARTIFACT_ID) &&
                (_serializer.Deserialize<ImportSettings>(x.Configuration).FederatedInstanceCredentials == _CREDENTIALS) &&
                (x.SecuredConfiguration == _CREDENTIALS)));
        }

    }
}

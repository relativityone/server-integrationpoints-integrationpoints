using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Synchronizer;
using kCura.IntegrationPoints.Synchronizers.RDO;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
	[TestFixture]
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

        private readonly List<FieldEntry> _fields = new List<FieldEntry>()
        {
            new FieldEntry()
            {
                DisplayName = _DISPLAY_NAME,
                FieldType = FieldType.File,
                IsIdentifier = true
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
		    _synchronizerFactoryMock.CreateSynchronizer( Guid.Empty, _synchronizerSettings.Settings, _synchronizerSettings.Credentials).Returns(_dataSynchronizerMock);
		    _dataSynchronizerMock.GetFields(Arg.Any<string>()).Returns(_fields);

            // Act
            HttpResponseMessage httpResponseMessage = _subjectUnderTest
                .Post(_synchronizerSettings);

            //// Assert
            List<FieldEntry> retValue;
            httpResponseMessage.TryGetContentValue(out retValue);

            Assert.That(httpResponseMessage.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            CollectionAssert.AreEquivalent(_fields, retValue);

		    _dataSynchronizerMock.Received(1).GetFields(Arg.Is<string>(x =>
		        (_serializer.Deserialize<ImportSettings>(x).ArtifactTypeId == _ARTIFACT_TYPE_ID) &&
		        (_serializer.Deserialize<ImportSettings>(x).DestinationFolderArtifactId == _DESTINATION_FOLDER_ARTIFACT_ID) &&
		        (_serializer.Deserialize<ImportSettings>(x).FederatedInstanceCredentials == _CREDENTIALS)));
		}

	}
}

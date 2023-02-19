using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Models;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using Relativity.IntegrationPoints.Contracts.Models;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Web.Tests.Controllers.API
{
    [TestFixture, Category("Unit")]
    public class FtpProviderAPIControllerTests : TestBase
    {
        private FtpProviderAPIController _instance;
        private ISettingsManager _settingsManager;
        private IDataProviderFactory _providerFactory;
        private ISerializer _serializer;

        [SetUp]
        public override void SetUp()
        {
            _settingsManager = Substitute.For<ISettingsManager>();
            _providerFactory = Substitute.For<IDataProviderFactory>();
            _serializer = Substitute.For<ISerializer>();

            _instance = new FtpProviderAPIController(_settingsManager, _providerFactory, _serializer)
            {
                Request = new HttpRequestMessage()
            };

            _instance.Request.SetConfiguration(new HttpConfiguration());
        }

        [Test]
        public void ItShouldGetColumnList()
        {
            // Arrange
            var fields = new List<FieldEntry>() { new FieldEntry() {DisplayName = "A"}, new FieldEntry() {DisplayName = "B"}};
            IDataSourceProvider ftpProvider = Substitute.For<IDataSourceProvider>();
            ftpProvider.GetFields(Arg.Any<DataSourceProviderConfiguration>()).Returns(fields);
            _providerFactory.GetDataProvider(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(ftpProvider);

            // Act
            IHttpActionResult actualResult = _instance.GetColumnList(new SynchronizerSettings());

            // Assert
            Assert.AreEqual(typeof(OkNegotiatedContentResult<List<FieldEntry>>), actualResult.GetType());
            Assert.AreEqual(fields, ((OkNegotiatedContentResult<List<FieldEntry>>)actualResult).Content);
        }

        [TestCase("some data")]
        public void ItShouldGetViewFields([FromBody] object data)
        {
            // Arrange
            var settings = new Settings() {Filename_Prefix = "SettigsFileName", Host = "HostName"};
            _settingsManager.DeserializeSettings(data.ToString()).Returns(settings);
            var expectedModel = new FtpProviderSummaryPageSettingsModel(settings);
            string expectedSerializedModel = JsonConvert.SerializeObject(expectedModel);

            _serializer.Serialize(Arg.Any<FtpProviderSummaryPageSettingsModel>()).Returns(expectedSerializedModel);

            // Act
            HttpResponseMessage response = _instance.GetViewFields(data);

            // Assert
            Assert.IsNotNull(response, "Response should not be null");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "HttpStatusCode should be OK");
            Assert.AreEqual(expectedSerializedModel, JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result), "The HttpContent should be as expected");
        }
    }
}

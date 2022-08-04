using System.Net;
using System.Web.Mvc;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Helpers;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using kCura.IntegrationPoints.FtpProvider.Helpers.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers.Models;
using kCura.IntegrationPoints.Web.Controllers;
using kCura.IntegrationPoints.Web.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using NSubstitute;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Controllers
{
    [TestFixture, Category("Unit")]
    public class FtpProviderControllerTests : TestBase
    {
        private bool _testConnectionResult = false;
        private ISettingsManager _settingsManager;
        private IConnectorFactory _connectorFactory = null;
        private IFtpConnector _ftpConnector = null;
        private ICPHelper _helper = null;
        private ILogFactory _logFactory = null;
        private IAPILog _logger = null;
        private FtpProviderController _instance;

        public override void SetUp()
        {
            _ftpConnector = Substitute.For<IFtpConnector>();
            _ftpConnector.TestConnection().Returns(_testConnectionResult);
            _settingsManager = Substitute.For<ISettingsManager>();
            _connectorFactory = Substitute.For<IConnectorFactory>();
            _connectorFactory.GetConnector("", "", 777, "", "").ReturnsForAnyArgs(_ftpConnector);
            _logger = Substitute.For<IAPILog>();
            _logFactory = Substitute.For<ILogFactory>();
            _logFactory.GetLogger().Returns(_logger);
            _helper = Substitute.For<ICPHelper>();
            _helper.GetLoggerFactory().Returns(_logFactory);

            _instance = new FtpProviderController(_connectorFactory, _settingsManager, _helper);
        }

        [TestCase("", "test.host", "", "", "", 21, true, HttpStatusCode.BadRequest, ErrorMessage.MISSING_CSV_FILE_NAME)]
        [TestCase("AnyFileName", "test.h!@#$%^&*()__+=[]\',./<>?:;ost", "", "", "", 21, true, HttpStatusCode.BadRequest, ErrorMessage.INVALID_HOST_NAME)]
        [TestCase("AnyFileName", "test.host", "", "", "", 88888, true, HttpStatusCode.NoContent, null)]
        [TestCase("AnyFileName", "test.host", "", "", "", 88888, false, HttpStatusCode.Forbidden, "Cannot connect to specified host.")]
        public void ShouldValidateSettingsCorrectness(
            string filename, 
            string host, 
            string username, 
            string password, 
            string protocol, 
            int port, 
            bool testConnectionResult,
            HttpStatusCode expectedStatus, 
            string expectedDescription)
        {
            _testConnectionResult = testConnectionResult;
            // NSubstitute result is passed by value so refreshing TestConnection mockup is needed
            _ftpConnector.TestConnection().Returns(_testConnectionResult);

            var settings = new Settings()
            {
                Filename_Prefix = filename,
                Host = host,
                Protocol = protocol,
                Port = port
            };

            var credentials = new SecuredConfiguration()
            {
                Password = password,
                Username = username,
            };

            JsonSerializerSettings jsonSettings = JSONHelper.GetDefaultSettings();

            SynchronizerSettings synchronizerSettings = new SynchronizerSettings()
            {
                Settings = JsonConvert.SerializeObject(settings, jsonSettings),
                Credentials = JsonConvert.SerializeObject(credentials, jsonSettings)
            };

            _settingsManager.DeserializeSettings(Arg.Is(synchronizerSettings.Settings)).Returns(settings);
            _settingsManager.DeserializeCredentials(Arg.Is(synchronizerSettings.Credentials)).Returns(credentials);

            HttpStatusCodeResult response = _instance.ValidateHostConnection(synchronizerSettings);

            Assert.AreEqual((int)expectedStatus, response.StatusCode);
            Assert.AreEqual(expectedDescription, response.StatusDescription);
        }
    }
}

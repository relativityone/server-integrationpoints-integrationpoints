using System.Net;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.FtpProvider.Connection.Interfaces;
using kCura.IntegrationPoints.FtpProvider.Helpers;
using kCura.IntegrationPoints.Security;
using kCura.IntegrationPoints.Web.Controllers;
using NUnit.Framework;
using NSubstitute;

namespace kCura.IntegrationPoints.Web.Tests.Controllers
{
    [TestFixture]
    public class FtpProviderControllerTests : TestBase
    {
        private bool _testConnectionResult = false;
        private IEncryptionManager _securityManager = null;
        private IConnectorFactory _connectorFactory = null;
        private IFtpConnector _ftpConnector = null;
        private FtpProviderController _instance;

        public override void SetUp()
        {
            _ftpConnector = Substitute.For<IFtpConnector>();
            _ftpConnector.TestConnection().Returns(_testConnectionResult);
            _securityManager = Substitute.For<IEncryptionManager>();
            _connectorFactory = Substitute.For<IConnectorFactory>();
            _connectorFactory.GetConnector("", "", 777, "", "").ReturnsForAnyArgs(_ftpConnector);

            _instance = new FtpProviderController(_securityManager, _connectorFactory);
        }

        [TestCase("", "test.host", "", "", "", 21, true, HttpStatusCode.BadRequest, ErrorMessage.MISSING_CSV_FILE_NAME)]
        [TestCase("AnyFileName", "test.h!@#$%^&*()__+=[]\',./<>?:;ost", "", "", "", 21, true, HttpStatusCode.BadRequest, ErrorMessage.INVALID_HOST_NAME)]
        [TestCase("AnyFileName", "test.host", "", "", "", 88888, true, HttpStatusCode.OK, null)]
        [TestCase("AnyFileName", "test.host", "", "", "", 88888, false, HttpStatusCode.NotImplemented, "Nothing happened")]
        public void ShouldValidateSettingsCorrectness(
            string filename, 
            string host, 
            string username, 
            string password, 
            string protocol, 
            int port, 
            bool testConnectionResult,
            HttpStatusCode expectedStatus, string expectedDescription)
        {
            _testConnectionResult = testConnectionResult;
            // NSubstitute result is passed by value so refreshing TestConnection mockup is needed
            _ftpConnector.TestConnection().Returns(_testConnectionResult);

            var response =_instance.ValidateHostConnection(new FtpProvider.Helpers.Models.Settings()
            {
                Filename_Prefix = filename,
                Host = host,
                Password = password,
                Protocol = protocol,
                Username = username,
                Port = port,
                
            });

            Assert.AreEqual((int)expectedStatus, response.StatusCode);
            Assert.AreEqual(expectedDescription, response.StatusDescription);
        }
    }
}

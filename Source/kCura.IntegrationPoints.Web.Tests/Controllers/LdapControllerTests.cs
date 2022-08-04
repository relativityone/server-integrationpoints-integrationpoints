using System.Web.Http.Results;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Controllers
{
    [TestFixture, Category("Unit"), Category("ImportProvider")]
    [Description("These tests are meant to mimic the way the ajax request will behave on the client side.")]
    public class LdapControllerTests : TestBase
    {
        private const string _SERIALIZED_MODEL = "{ConnectionPath:'oiler.corp/OU=Employees,OU=Accounts,OU=oiler,DC=oiler,DC=oiler', ConnectionAuthenticationType:32}";
        private const string _CONNECTION_PATH = "oiler.corp/OU=Employees,OU=Accounts,OU=oiler,DC=oiler,DC=oiler";

        private ICPHelper _helperMock; 
        private ISerializer _serializerMock;
        private ILDAPSettingsReader _reader;
        private ILDAPServiceFactory _ldapServiceFactory;

        private LdapController _subjectUnderTest;

        [SetUp]
        public override void SetUp()
        {
            _helperMock = Substitute.For<ICPHelper>();
            _serializerMock = Substitute.For<ISerializer>();
            _ldapServiceFactory = Substitute.For<ILDAPServiceFactory>();
            _reader = new LDAPSettingsReader(_helperMock);
            _subjectUnderTest = new LdapController(_helperMock, _reader, _serializerMock, _ldapServiceFactory);
        }

        [Test]
        public void ItShouldGetViewFields()
        {
            //ARRANGE
            ILDAPSettingsReader readerMock = Substitute.For<ILDAPSettingsReader>();
            _subjectUnderTest = new LdapController(_helperMock, _reader, _serializerMock, _ldapServiceFactory);


            var settings = new LDAPSettings()
            {

                ConnectionPath = _CONNECTION_PATH,
                ConnectionAuthenticationType = AuthenticationTypesEnum.FastBind,
            };

            readerMock.GetSettings(_SERIALIZED_MODEL).Returns(settings);
            _serializerMock.Serialize(Arg.Any<LdapProviderSummaryPageSettingsModel>()).Returns(_SERIALIZED_MODEL);

            //ACT
            var result = _subjectUnderTest.GetViewFields(_SERIALIZED_MODEL) as OkNegotiatedContentResult<string>;

            //ASSERT
            string value = result.Content;
            Assert.AreEqual(_SERIALIZED_MODEL, value);
            _serializerMock.Received(1).Serialize(Arg.Is<LdapProviderSummaryPageSettingsModel>(arg =>
                ( arg.ConnectionPath == settings.ConnectionPath && arg.ConnectionAuthenticationType == settings.ConnectionAuthenticationType.ToString() ) ));

        }
    }
}

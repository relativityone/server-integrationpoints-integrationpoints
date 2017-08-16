using System;
using System.Web.Http.Results;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Security;
using kCura.IntegrationPoints.Web.Controllers.API;
using kCura.IntegrationPoints.Web.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Controllers
{
	
	[TestFixture(Description = "These tests are meant to mimic the way the ajax request will behave on the client side.")]
	public class LdapControllerTests : TestBase
	{
	    private const string _MESSAGE = "{\"a\":\"Asdf\"}";
        private const string _ENCRYPTED_MESSAGE = "\"zQ/bTBWY00FKnJih9VYK/DYGiHbKukxj4jAPuDO3v4H+W/Oc+Doh8vQZGpkwB8StnOVz9XXJ4Mp/OazQ4zA07b0wVpYKh6/zcljWcAPK8C9fgI9bP0+Ec95W0BqC32YXd8qaXfLqZQ73mp2VdlnMu4WYJFJunW/d+uJoNuOaCr7KyvgTlqlBs4EST51MOi4PzsXyzzbF2RWg2MKSnMvBYPEcLaj01akIKAPQWB2vjXraeTSh1P7ixKAKZAsQnKUBXdWalv/LZiJDQMkRvE1/lkjHt1txzjQd15nzTXn8WRbXASnSrZ5ROfxRdof8+QdK\"";
	    private const string _SERIALIZED_MODEL = "{ConnectionPath:;'oiler.corp/OU=Employees,OU=Accounts,OU=oiler,DC=oiler,DC=oiler'";
	    private const string _CONNECTION_PATH = "oiler.corp/OU=Employees,OU=Accounts,OU=oiler,DC=oiler,DC=oiler";
	    private const string _ENCRYPTED_CONFIGURATION = "#mySecretConfiguration#";

	    private IEncryptionManager _managerMock; 
	    private IHelper _helperMock; 
	    private ISerializer _serializerMock;
	    private ILDAPSettingsReader _reader;

	    private LdapController _subjectUnderTest;

        [SetUp]
		public override void SetUp()
		{
            _managerMock = Substitute.For<IEncryptionManager>();
            _helperMock = Substitute.For<IHelper>();
		    _serializerMock = Substitute.For<ISerializer>();
            _reader = new LDAPSettingsReader(_managerMock, _helperMock);
            _subjectUnderTest = new LdapController(_reader,_managerMock,_serializerMock);
        }

		[Test]
		public void ItShouldThrowLdapProviderExceptionWhenDecryptJson()
		{
			//ARRANGE
		    _managerMock.Decrypt(Arg.Any<string>()).ThrowsForAnyArgs(new Exception());

			//ACT, ASSERT
			Assert.Throws<LDAPProviderException>(() => _subjectUnderTest.Decrypt(_MESSAGE));
		}

		[Test]
		public void ItShouldDecrypt()
		{
			//ARRANGE
			_managerMock.Decrypt(Arg.Any<string>()).Returns(_MESSAGE);

			//ACT
			var result = _subjectUnderTest.Decrypt(_ENCRYPTED_MESSAGE) as OkNegotiatedContentResult<string>;

			//ASSERT
			string value = result.Content;
			Assert.AreEqual(_MESSAGE, value);
		}

	    [Test]
	    public void ItShouldEncrypt()
	    {
	        //ARRANGE
	        _managerMock.Encrypt(_MESSAGE).Returns(_ENCRYPTED_MESSAGE );

	        //ACT
	        var result = _subjectUnderTest.Encrypt(_MESSAGE) as OkNegotiatedContentResult<string>;

	        //ASSERT
	        string value = result.Content;
	        Assert.AreEqual(_ENCRYPTED_MESSAGE, value);
	    }

	    [Test]
	    public void ItShouldEncryptNullStringAsEmptyString()
	    {
	        //ACT
	        var result = _subjectUnderTest.Encrypt(null) as OkNegotiatedContentResult<string>;

	        //ASSERT
	        string value = result.Content;
	        Assert.AreEqual(string.Empty, value);
	        _managerMock.Received(0).Encrypt(Arg.Any<string>());
	    }

        [Test]
	    public void ItShouldGetViewFields()
	    {
            //ARRANGE
	        ILDAPSettingsReader readerMock = Substitute.For<ILDAPSettingsReader>();;
            _subjectUnderTest = new LdapController(readerMock, _managerMock, _serializerMock);


	        var settings = new LDAPSettings()
	        {

	            ConnectionPath = _CONNECTION_PATH,
	            ConnectionAuthenticationType = AuthenticationTypesEnum.FastBind,
	        };

	        string encryptedConfiguration = _ENCRYPTED_CONFIGURATION;
	        readerMock.GetSettings(encryptedConfiguration).Returns(settings);
	        _serializerMock.Serialize(Arg.Any<LdapProviderSummaryPageSettingsModel>()).Returns(_SERIALIZED_MODEL);

            //ACT
            var result = _subjectUnderTest.GetViewFields(encryptedConfiguration) as OkNegotiatedContentResult<string>;

            //ASSERT
            string value = result.Content;
            Assert.AreEqual(_SERIALIZED_MODEL, value);
	        _serializerMock.Received(1).Serialize(Arg.Is<LdapProviderSummaryPageSettingsModel>(arg =>
                ( arg.ConnectionPath == settings.ConnectionPath && arg.ConnectionAuthenticationType == settings.ConnectionAuthenticationType.ToString() ) ));

	    }
    }
}

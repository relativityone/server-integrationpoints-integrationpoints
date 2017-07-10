using System;
using System.Web.Http.Results;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.LDAPProvider;
using kCura.IntegrationPoints.Security;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Web.Tests.Controllers
{
	
	[TestFixture(Description = "These tests are meant to mimic the way the ajax request will behave on the client side.")]
	public class LdapControllerTests : TestBase
	{
		[SetUp]
		public override void SetUp()
		{
			
		}

		[Test]
		public void Decrypt_stringJustJSON_ThrowsLDAPProviderException()
		{
			//ARRANGE
			var manager = Substitute.For<IEncryptionManager>();
		    manager.Decrypt(Arg.Any<string>()).ThrowsForAnyArgs(new Exception());
			var helper = Substitute.For<IHelper>();
			var serializer = Substitute.For<ISerializer>();
            var reader = new LDAPSettingsReader(manager, helper);
			var controller = new LdapController(reader, manager, serializer);
			var message = "{\"a\":\"Asdf\"}";

			//ACT, ASSERT
			Assert.Throws<LDAPProviderException>(() => controller.Decrypt(message));
		}

		[Test]
		public void Decrypt_stringEncrypted_returnsDecryptedString()
		{
			//ARRANGE
			var manager = Substitute.For<IEncryptionManager>();
			var message = "{\"a\":\"Asdf\"}";
			manager.Decrypt(Arg.Any<string>()).Returns(message);
			var encryptedMessage = "\"zQ/bTBWY00FKnJih9VYK/DYGiHbKukxj4jAPuDO3v4H+W/Oc+Doh8vQZGpkwB8StnOVz9XXJ4Mp/OazQ4zA07b0wVpYKh6/zcljWcAPK8C9fgI9bP0+Ec95W0BqC32YXd8qaXfLqZQ73mp2VdlnMu4WYJFJunW/d+uJoNuOaCr7KyvgTlqlBs4EST51MOi4PzsXyzzbF2RWg2MKSnMvBYPEcLaj01akIKAPQWB2vjXraeTSh1P7ixKAKZAsQnKUBXdWalv/LZiJDQMkRvE1/lkjHt1txzjQd15nzTXn8WRbXASnSrZ5ROfxRdof8+QdK\"";
			var helper = Substitute.For<IHelper>();

			var serializer = Substitute.For<ISerializer>();
		    var reader = new LDAPSettingsReader(manager, helper);
            var controller = new LdapController(reader, manager, serializer);

			//ACT
			var result = controller.Decrypt(encryptedMessage) as OkNegotiatedContentResult<string>;

			//ASSERT
			var value = result.Content;
			Assert.AreEqual(message, value);

		}
	}
}

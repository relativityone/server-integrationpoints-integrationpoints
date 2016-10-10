using System.Web.Http.Results;
using kCura.IntegrationPoints.Security;
using kCura.IntegrationPoints.Web.Controllers.API;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Web.Tests.Controllers
{
	
	[TestFixture(Description = "These tests are meant to mimic the way the ajax request will behave on the client side.")]
	public class LdapControllerTests
	{
		[Test]
		public void Decrypt_stringJustJSON_returnsJSONString()
		{
			//ARRANGE
			var manager = NSubstitute.Substitute.For<IEncryptionManager>();
			var controller = new LdapController(manager);
			var message = "{\"a\":\"Asdf\"}";

			//ACT
			var result = controller.Decrypt(message) as OkNegotiatedContentResult<string>;

			//ASSERT
			var value = result.Content;
			Assert.AreEqual(message, value);

		}

		[Test]
		public void Decrypt_stringEncrypted_returnsDecryptedString()
		{
			//ARRANGE
			var manager = NSubstitute.Substitute.For<IEncryptionManager>();
			var message = "{\"a\":\"Asdf\"}";
			manager.Decrypt(Arg.Any<string>()).Returns(message);
			var encryptedMessage = "\"zQ/bTBWY00FKnJih9VYK/DYGiHbKukxj4jAPuDO3v4H+W/Oc+Doh8vQZGpkwB8StnOVz9XXJ4Mp/OazQ4zA07b0wVpYKh6/zcljWcAPK8C9fgI9bP0+Ec95W0BqC32YXd8qaXfLqZQ73mp2VdlnMu4WYJFJunW/d+uJoNuOaCr7KyvgTlqlBs4EST51MOi4PzsXyzzbF2RWg2MKSnMvBYPEcLaj01akIKAPQWB2vjXraeTSh1P7ixKAKZAsQnKUBXdWalv/LZiJDQMkRvE1/lkjHt1txzjQd15nzTXn8WRbXASnSrZ5ROfxRdof8+QdK\"";

			var controller = new LdapController(manager);

			//ACT
			var result = controller.Decrypt(encryptedMessage) as OkNegotiatedContentResult<string>;

			//ASSERT
			var value = result.Content;
			Assert.AreEqual(message, value);

		}

	}
}

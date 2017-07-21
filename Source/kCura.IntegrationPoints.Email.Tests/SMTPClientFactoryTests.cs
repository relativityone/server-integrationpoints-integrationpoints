using NUnit.Framework;
using kCura.IntegrationPoints.Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;

namespace kCura.IntegrationPoints.Email.Tests
{
	[TestFixture()]
	public class SMTPClientFactoryTests : TestBase
	{
		[Test]
		public void GetClientTest()
		{
			var emailConfiguration = new EmailConfiguration()
			{
				Domain = "testDomain",
				Password = "testPass",
				Port = 1234,
				UseSSL = true,
				UserName = "testUser"
			};
			SMTPClientFactory clientFactory = new SMTPClientFactory();
			var client = clientFactory.GetClient(emailConfiguration);

			Assert.AreEqual(emailConfiguration.Domain, client.Host);
			Assert.That(client.Credentials != null);
			Assert.AreEqual(emailConfiguration.Port, client.Port);
			Assert.AreEqual(emailConfiguration.UseSSL, client.EnableSsl);
		}

		[SetUp]
		public override void SetUp()
		{
			
		}
	}
}
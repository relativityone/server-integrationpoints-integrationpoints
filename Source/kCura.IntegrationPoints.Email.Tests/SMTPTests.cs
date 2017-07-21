using System;
using System.Net.Mail;
using kCura.IntegrationPoints.Email;
using NUnit.Framework;
using kCura.IntegrationPoint.Tests.Core;
using NSubstitute;
using Relativity.API;

namespace kCura.IntegrationPoints.Email.Tests
{
    [TestFixture]
    public class SMTPTests : TestBase
    {
        private IAPILog _logger;
        private IHelper _helper;
	    private ISMTPClientFactory _clientFactory;

        private SMTP _smtp;

        [SetUp]
        public override void SetUp()
        {
            _helper = Substitute.For<IHelper>();
            _logger = Substitute.For<IAPILog>();
	        _clientFactory = Substitute.For<ISMTPClientFactory>();
        }

        [Test]
        public void ShouldValidateMissingConfiguration()
        {
            SMTP smtp = new SMTP(_helper, _clientFactory, null);
            AggregateException ex = Assert.Throws<AggregateException>(() => smtp.Send(new MailMessage()));
            Assert.That(ex.InnerExceptions[0].Message == Properties.Resources.Invalid_SMTP_Settings);
        }


        [Test]
        public void ShouldValdiateEmptyDomain()
        {
            SMTP smtp = new SMTP(_helper, _clientFactory, new EmailConfiguration()
            {
                Domain = "",
                Password = "",
                Port = 0,
                UseSSL = false,
                UserName = ""
            });

            AggregateException ex = Assert.Throws<AggregateException>(() => smtp.Send(new MailMessage()));
			Assert.That(ex.InnerExceptions[0].Message == Properties.Resources.SMTP_Requires_SMTP_Domain);
        }

        [Test]
        public void ShouldValdiateWrongPort()
        {
            SMTP smtp = new SMTP(_helper, _clientFactory, new EmailConfiguration()
            {
                Domain = "domain",
                Password = "",
                Port = -1,
                UseSSL = false,
                UserName = ""
            });

            AggregateException ex = Assert.Throws<AggregateException>(() => smtp.Send(new MailMessage()));
			Assert.That(ex.InnerExceptions[0].Message == Properties.Resources.SMTP_Port_Negative);
        }

        [Test]
        public void ShouldValdiatePortAndDomain()
        {
            SMTP smtp = new SMTP(_helper, _clientFactory, new EmailConfiguration()
            {
                Domain = "",
                Password = "",
                Port = -1,
                UseSSL = false,
                UserName = ""
            });

            AggregateException ex = Assert.Throws<AggregateException>(() => smtp.Send(new MailMessage()));
            Assert.That(ex.InnerExceptions[0].Message == Properties.Resources.SMTP_Port_Negative);
            Assert.That(ex.InnerExceptions[1].Message == Properties.Resources.SMTP_Requires_SMTP_Domain);
        }
    }
}
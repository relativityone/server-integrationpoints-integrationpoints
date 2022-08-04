using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Logging
{
    [TestFixture, Category("Unit")]
    public class ExportUserNotificationTests : TestBase
    {
        private ExportUserNotification _exportUserNotification;

        [SetUp]
        public override void SetUp()
        {
            _exportUserNotification = new ExportUserNotification();
        }

        [Test]
        public void ItShouldRaiseFatalEventOnAlert()
        {
            AssertFatalEventCall(expectedMessage => _exportUserNotification.Alert(expectedMessage));
        }

        [Test]
        public void ItShouldRaiseFatalEventOnAlertCriticalError()
        {
            AssertFatalEventCall(expectedMessage => _exportUserNotification.AlertCriticalError(expectedMessage));
        }

        [Test]
        public void ItShouldRaiseWarningEventOnAlertWarningSkippable()
        {
            AssertWarningEventCall(expectedMessage => _exportUserNotification.AlertWarningSkippable(expectedMessage));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("any_message")]
        public void ItShouldAlwaysReturnTrueOnAlertWarningSkippable(string message)
        {
            Assert.True(_exportUserNotification.AlertWarningSkippable(message));
        }

        private void AssertFatalEventCall(Action<string> webUserNotificationCall)
        {
            const string expectedMessage = "expected_message";
            string actualMessage = null;

            _exportUserNotification.UserFatalMessageEvent += (sender, args) => { actualMessage = args.Message; };

            webUserNotificationCall.Invoke(expectedMessage);

            Assert.AreEqual(expectedMessage, actualMessage);
        }

        private void AssertWarningEventCall(Action<string> webUserNotificationCall)
        {
            const string expectedMessage = "expected_message";
            string actualMessage = null;

            _exportUserNotification.UserWarningMessageEvent += (sender, args) => { actualMessage = args.Message; };

            webUserNotificationCall.Invoke(expectedMessage);

            Assert.AreEqual(expectedMessage, actualMessage);
        }
    }
}
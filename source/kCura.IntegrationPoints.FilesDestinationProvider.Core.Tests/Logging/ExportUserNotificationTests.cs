using System;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Logging
{
    public class ExportUserNotificationTests
    {
        private ExportUserNotification _exportUserNotification;

        [SetUp]
        public void SetUp()
        {
            _exportUserNotification = new ExportUserNotification();
        }

        [Test]
        public void ItShouldRaiseEventOnAlert()
        {
            AssertEventCall(expectedMessage => _exportUserNotification.Alert(expectedMessage));
        }

        [Test]
        public void ItShouldRaiseEventOnAlertCriticalError()
        {
            AssertEventCall(expectedMessage => _exportUserNotification.AlertCriticalError(expectedMessage));
        }

        [Test]
        public void ItShouldRaiseEventOnAlertWarningSkippable()
        {
            AssertEventCall(expectedMessage => _exportUserNotification.AlertWarningSkippable(expectedMessage));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("any_message")]
        public void ItShouldAlwaysReturnFalseOnAlertWarningSkippable(string message)
        {
            Assert.False(_exportUserNotification.AlertWarningSkippable(message));
        }

        private void AssertEventCall(Action<string> webUserNotificationCall)
        {
            const string expectedMessage = "expected_message";
            string actualMessage = null;

            _exportUserNotification.UserMessageEvent += (sender, args) => { actualMessage = args.Message; };

            webUserNotificationCall.Invoke(expectedMessage);

            Assert.AreEqual(expectedMessage, actualMessage);
        }
    }
}
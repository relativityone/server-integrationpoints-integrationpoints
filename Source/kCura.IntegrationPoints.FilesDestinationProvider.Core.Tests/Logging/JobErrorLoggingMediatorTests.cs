using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS;
using NSubstitute;
using NUnit.Framework;
using Relativity.DataExchange.Process;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Logging
{
    [TestFixture, Category("Unit")]
    public class JobErrorLoggingMediatorTests : TestBase
    {
        private ICoreExporterStatusNotification _exporterStatusNotification;
        private IJobHistoryErrorService _historyErrorService;
        private IUserMessageNotification _userMessageNotification;

        [SetUp]
        public override void SetUp()
        {
            _userMessageNotification = Substitute.For<IUserMessageNotification>();
            _historyErrorService = Substitute.For<IJobHistoryErrorService>();
            _exporterStatusNotification = Substitute.For<ICoreExporterStatusNotification>();
            var jobErrorLoggingMediator = new JobErrorLoggingMediator(_historyErrorService);
            jobErrorLoggingMediator.RegisterEventHandlers(_userMessageNotification, _exporterStatusNotification);
        }

        [Test]
        public void ItShouldAddJobErrorOnUserFatalMessage()
        {
            const string expectedMessage = "expected_user_message";
            _userMessageNotification.UserFatalMessageEvent += Raise.EventWith(new UserMessageEventArgs(expectedMessage));

            _historyErrorService.Received()
                .AddError(ErrorTypeChoices.JobHistoryErrorJob, string.Empty, expectedMessage, string.Empty);
        }

        [Test]
        public void ItShouldAddItemErrorOnUserWarningMessage()
        {
            const string expectedMessage = "expected_user_message";
            _userMessageNotification.UserWarningMessageEvent += Raise.EventWith(new UserMessageEventArgs(expectedMessage));

            _historyErrorService.Received()
                .AddError(ErrorTypeChoices.JobHistoryErrorItem, string.Empty, expectedMessage, string.Empty);
        }

        [Test]
        public void IsShouldAddErrorOnFatalError()
        {
            var expectedException = new Exception();
            _exporterStatusNotification.FatalErrorEvent +=
                Raise.Event<IExporterStatusNotification.FatalErrorEventEventHandler>("fatal_message", expectedException);

            _historyErrorService.Received().AddError(ErrorTypeChoices.JobHistoryErrorJob, expectedException);
        }

        [Test]
        public void ItShouldAddErrorOnStatusMessageWithError()
        {
            var exportEventArgs = new ExportEventArgs(0, 0, "status_message", EventType2.Error, null, null);
            _exporterStatusNotification.StatusMessage +=
                Raise.Event<IExporterStatusNotification.StatusMessageEventHandler>(exportEventArgs);

            _historyErrorService.Received()
                .AddError(ErrorTypeChoices.JobHistoryErrorItem, string.Empty, exportEventArgs.Message, string.Empty);
        }

        [Test]
        [TestCase(EventType2.Warning)]
        [TestCase(EventType2.Count)]
        [TestCase(EventType2.End)]
        [TestCase(EventType2.Progress)]
        [TestCase(EventType2.ResetStartTime)]
        [TestCase(EventType2.Status)]
        public void ItShouldSkipLoggingOnStatusMessageWithoutError(EventType2 eventType)
        {
            var exportEventArgs = new ExportEventArgs(0, 0, "status_message", eventType, null, null);
            _exporterStatusNotification.StatusMessage +=
                Raise.Event<IExporterStatusNotification.StatusMessageEventHandler>(exportEventArgs);

            _historyErrorService.DidNotReceiveWithAnyArgs().AddError(null, null);
        }
    }
}
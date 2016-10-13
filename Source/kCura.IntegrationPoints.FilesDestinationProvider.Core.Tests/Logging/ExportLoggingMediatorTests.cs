using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.Windows.Process;
using kCura.WinEDDS;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Logging
{
    public class ExportLoggingMediatorTests
    {
        private readonly string _errorMessageTemplate = "Error occured: {message}. Additional info: {@additionalInfo}.";
        private readonly string _fileTransferTemplateMessage = "File transfer mode has been changed: {mode}";

        private readonly string _progressMessageTemplate = "Progress update: {message}. Additional info: {@additionalInfo}.";

        private readonly string _statusMessageTemplate = "Status update: {message}. Additional info: {@additionalInfo}.";
        private readonly string _unexpectedEventTypeTemplate = "Unexpected EventType.{event}. EventArgs: {@eventArgs}";
        private readonly string _warningMessageTemplate = "Warning: {message}. Additional info: {@additionalInfo}.";

        private IAPILog _apiLog;
        private ICoreExporterStatusNotification _exporterStatusNotification;
        private IUserMessageNotification _userMessageNotification;

        [SetUp]
        public void SetUp()
        {
            _apiLog = Substitute.For<IAPILog>();
            _userMessageNotification = Substitute.For<IUserMessageNotification>();
            _exporterStatusNotification = Substitute.For<ICoreExporterStatusNotification>();
            var exportLogger = new ExportLoggingMediator(_apiLog);
            exportLogger.RegisterEventHandlers(_userMessageNotification, _exporterStatusNotification);
        }

        [Test]
        public void ItShouldLogFileTransferModeChangeAsInfo()
        {
            const string newMode = "new_mode";

            _exporterStatusNotification.FileTransferModeChangeEvent += Raise.Event<IExporterStatusNotification.FileTransferModeChangeEventEventHandler>(newMode);

            _apiLog.Received().LogInformation(_fileTransferTemplateMessage, newMode);
        }

        [Test]
        public void ItShouldLogFatalErrorAsFatal()
        {
            const string message = "Fatal error message";
            var exception = new Exception("Fatal error occured");

            _exporterStatusNotification.FatalErrorEvent += Raise.Event<IExporterStatusNotification.FatalErrorEventEventHandler>(message, exception);

            _apiLog.Received().LogFatal(exception, message);
        }

        [Test]
        [TestCase(EventType.Count)]
        [TestCase(EventType.End)]
        [TestCase(EventType.ResetStartTime)]
        public void ItShouldHandleStatusMessageWithGivenTypeAsUnexpectedEvent(EventType givenType)
        {
            var exportEventArgs = CreateExportEventArgs(givenType);

            RaiseStatusMessage(exportEventArgs);

            _apiLog.Received().LogDebug(_unexpectedEventTypeTemplate, givenType, exportEventArgs);
        }

        [Test]
        public void ItShouldLogStatusMessageWithErrorAsError()
        {
            var exportEventArgs = CreateExportEventArgs(EventType.Error);

            RaiseStatusMessage(exportEventArgs);

            _apiLog.Received().LogError(_errorMessageTemplate, exportEventArgs.Message, exportEventArgs.AdditionalInfo);
        }

        [Test]
        public void ItShouldLogStatusMessageWithWarningAsWarning()
        {
            var exportEventArgs = CreateExportEventArgs(EventType.Warning);

            RaiseStatusMessage(exportEventArgs);

            _apiLog.Received().LogWarning(_warningMessageTemplate, exportEventArgs.Message, exportEventArgs.AdditionalInfo);
        }

        [Test]
        public void ItShouldLogStatusMessageWithStatusAsVerbose()
        {
            var exportEventArgs = CreateExportEventArgs(EventType.Status);

            RaiseStatusMessage(exportEventArgs);

            _apiLog.Received().LogVerbose(_statusMessageTemplate, exportEventArgs.Message, exportEventArgs.AdditionalInfo);
        }

        [Test]
        public void ItShouldLogStatusMessageWithProgressAsVerbose()
        {
            var exportEventArgs = CreateExportEventArgs(EventType.Progress);

            RaiseStatusMessage(exportEventArgs);

            _apiLog.Received().LogVerbose(_progressMessageTemplate, exportEventArgs.Message, exportEventArgs.AdditionalInfo);
        }

        [Test]
        public void ItShouldLogUserFatalMessageAsFatal()
        {
            const string expectedMessage = "expected_user_message";
            _userMessageNotification.UserFatalMessageEvent += Raise.EventWith(null, new UserMessageEventArgs(expectedMessage));

            _apiLog.Received().LogFatal(expectedMessage);
		}

		[Test]
		public void ItShouldLogUserWarningMessageAsWarning()
		{
			const string expectedMessage = "expected_user_message";
			_userMessageNotification.UserWarningMessageEvent += Raise.EventWith(null, new UserMessageEventArgs(expectedMessage));

			_apiLog.Received().LogWarning(expectedMessage);
		}

		[Test]
        public void ItShouldThrowExceptionForUnknownEventType()
        {
            var correctValues = Enum.GetValues(typeof (EventType)).Cast<int>().ToList();
            var incorrectValue = 1;
            while (correctValues.Contains(incorrectValue))
            {
                ++incorrectValue;
            }
            var exportEventArgs = CreateExportEventArgs((EventType) incorrectValue);

            Assert.That(() => RaiseStatusMessage(exportEventArgs),
                Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown EventType ({incorrectValue})"));
        }

        private void RaiseStatusMessage(ExportEventArgs exportEventArgs)
        {
            _exporterStatusNotification.StatusMessage += Raise.Event<IExporterStatusNotification.StatusMessageEventHandler>(exportEventArgs);
        }

        private ExportEventArgs CreateExportEventArgs(EventType eventType)
        {
            const string expectedMessage = "expected_event_message";
            var additionalInfo = new Dictionary<string, string>
            {
                {"info", "additional_info"}
            };
            return new ExportEventArgs(0, 0, expectedMessage, eventType, additionalInfo);
        }
    }
}
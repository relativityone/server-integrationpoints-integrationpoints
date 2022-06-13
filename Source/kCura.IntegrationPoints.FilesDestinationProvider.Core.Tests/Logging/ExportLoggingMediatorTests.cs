using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.WinEDDS;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataExchange.Process;
using Relativity.DataExchange.Transfer;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Logging
{
	[TestFixture, Category("Unit")]
	public class ExportLoggingMediatorTests : TestBase
	{
		private readonly string _errorMessageTemplate = "Error occured: {message}.";
		private readonly string _fileTransferTemplateMessage = "File transfer mode has been changed: {mode}";

		private readonly string _progressMessageTemplate = "Progress update: {message}.";

		private readonly string _statusMessageTemplate = "Status update: {message}.";
		private readonly string _unexpectedEventTypeTemplate = "Unexpected EventType.{event}. EventArgs: {@eventArgs}";
		private readonly string _warningMessageTemplate = "Warning: {message}.";

		private IAPILog _apiLog;
		private ICoreExporterStatusNotification _exporterStatusNotification;
		private IUserMessageNotification _userMessageNotification;

		[SetUp]
		public override void SetUp()
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
			_exporterStatusNotification.FileTransferMultiClientModeChangeEvent += Raise.Event<IExporterStatusNotification.FileTransferMultiClientModeChangeEventEventHandler>(this, new TapiMultiClientEventArgs(TapiClient.Aspera));

			_apiLog.Received().LogInformation(_fileTransferTemplateMessage, TapiClient.Aspera.ToString());
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
		[TestCase(EventType2.Count)]
		[TestCase(EventType2.End)]
		[TestCase(EventType2.ResetStartTime)]
		public void ItShouldHandleStatusMessageWithGivenTypeAsUnexpectedEvent(EventType2 givenType)
		{
			var exportEventArgs = CreateExportEventArgs(givenType);

			RaiseStatusMessage(exportEventArgs);

			_apiLog.Received().LogInformation(_unexpectedEventTypeTemplate, givenType, exportEventArgs);
		}

		[Test]
		public void ItShouldLogStatusMessageWithErrorAsError()
		{
			var exportEventArgs = CreateExportEventArgs(EventType2.Error);

			RaiseStatusMessage(exportEventArgs);

			_apiLog.Received().LogError(_errorMessageTemplate, exportEventArgs.Message);
		}

		[Test]
		public void ItShouldLogStatusMessageWithWarningAsWarning()
		{
			var exportEventArgs = CreateExportEventArgs(EventType2.Warning);

			RaiseStatusMessage(exportEventArgs);

			_apiLog.Received().LogWarning(_warningMessageTemplate, exportEventArgs.Message);
		}

		[Test]
		public void ItShouldLogStatusMessageWithStatusAsVerbose()
		{
			var exportEventArgs = CreateExportEventArgs(EventType2.Status);

			RaiseStatusMessage(exportEventArgs);

			_apiLog.Received().LogInformation(_statusMessageTemplate, exportEventArgs.Message);
		}

		[Test]
		public void ItShouldLogStatusMessageWithProgressAsVerbose()
		{
			var exportEventArgs = CreateExportEventArgs(EventType2.Progress);

			RaiseStatusMessage(exportEventArgs);

			_apiLog.Received().LogInformation(_progressMessageTemplate, exportEventArgs.Message);
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
			var correctValues = Enum.GetValues(typeof(EventType2)).Cast<int>().ToList();
			var incorrectValue = 1;
			while (correctValues.Contains(incorrectValue))
			{
				++incorrectValue;
			}
			var exportEventArgs = CreateExportEventArgs((EventType2)incorrectValue);

			Assert.That(() => RaiseStatusMessage(exportEventArgs),
				Throws.TypeOf<InvalidEnumArgumentException>().With.Message.EqualTo($"Unknown EventType ({incorrectValue})"));
		}

		private void RaiseStatusMessage(ExportEventArgs exportEventArgs)
		{
			_exporterStatusNotification.StatusMessage += Raise.Event<IExporterStatusNotification.StatusMessageEventHandler>(exportEventArgs);
		}

		private ExportEventArgs CreateExportEventArgs(EventType2 eventType)
		{
			const string expectedMessage = "expected_event_message";
			var additionalInfo = new Dictionary<string, string>
			{
				{"info", "additional_info"}
			};
			return new ExportEventArgs(0, 0, expectedMessage, eventType, additionalInfo, new Statistics());
		}
	}
}
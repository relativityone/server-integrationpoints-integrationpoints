﻿using System;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.Windows.Process;
using kCura.WinEDDS;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Logging
{
	public class JobErrorLoggingMediatorTests
	{
		private IExporterStatusNotification _exporterStatusNotification;
		private IJobHistoryErrorService _historyErrorService;
		private IUserMessageNotification _userMessageNotification;

		[SetUp]
		public void SetUp()
		{
			_userMessageNotification = Substitute.For<IUserMessageNotification>();
			_historyErrorService = Substitute.For<IJobHistoryErrorService>();
			_exporterStatusNotification = Substitute.For<IExporterStatusNotification>();
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
			var exportEventArgs = new ExportEventArgs(0, 0, "status_message", EventType.Error, null);
			_exporterStatusNotification.StatusMessage +=
				Raise.Event<IExporterStatusNotification.StatusMessageEventHandler>(exportEventArgs);

			_historyErrorService.Received()
				.AddError(ErrorTypeChoices.JobHistoryErrorItem, string.Empty, exportEventArgs.Message, string.Empty);
		}

		[Test]
		[TestCase(EventType.Warning)]
		[TestCase(EventType.Count)]
		[TestCase(EventType.End)]
		[TestCase(EventType.Progress)]
		[TestCase(EventType.ResetStartTime)]
		[TestCase(EventType.Status)]
		public void ItShouldSkipLoggingOnStatusMessageWithoutError(EventType eventType)
		{
			var exportEventArgs = new ExportEventArgs(0, 0, "status_message", eventType, null);
			_exporterStatusNotification.StatusMessage +=
				Raise.Event<IExporterStatusNotification.StatusMessageEventHandler>(exportEventArgs);

			_historyErrorService.DidNotReceiveWithAnyArgs().AddError(null, null);
		}
	}
}
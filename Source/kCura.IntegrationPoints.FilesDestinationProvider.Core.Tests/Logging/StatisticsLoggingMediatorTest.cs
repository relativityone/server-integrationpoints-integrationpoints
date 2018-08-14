using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.Windows.Process;
using kCura.WinEDDS;
using NSubstitute;
using NUnit.Framework;
using Relativity.DataTransfer.MessageService;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Logging
{
	[TestFixture]
	internal class StatisticsLoggingMediatorTest : TestBase
	{
		private StatisticsLoggingMediator _subjectUnderTest;
		private ICoreExporterStatusNotification _exporterStatusNotificationMock;

		[SetUp]
		public override void SetUp()
		{
			IJobHistoryErrorService jobHistoryErrorService = Substitute.For<IJobHistoryErrorService>();
			jobHistoryErrorService.IntegrationPoint = new Data.IntegrationPoint()
			{
				SourceProvider = 0,
				DestinationProvider = 0
			};
			jobHistoryErrorService.JobHistory = Substitute.For<JobHistory>();

			IIntegrationPointProviderTypeService integrationPointProviderTypeService = Substitute.For<IIntegrationPointProviderTypeService>();

			_subjectUnderTest = new StatisticsLoggingMediator(Substitute.For<IMessageService>(),
				jobHistoryErrorService, Substitute.For<ICaseServiceContext>(),
				integrationPointProviderTypeService, Substitute.For<IDateTimeHelper>());

			_exporterStatusNotificationMock = Substitute.For<ICoreExporterStatusNotification>();

			_subjectUnderTest.RegisterEventHandlers(Substitute.For<IUserMessageNotification>(), _exporterStatusNotificationMock);
		}

		[Test]
		public void ItShouldFireBatchCompleteEvent()
		{
			DateTime expectedStartTime = new DateTime(2000, 1, 1);
			DateTime expectedEndTime = new DateTime(2000, 1, 2);
			DateTime registeredStartTime = DateTime.Now, registeredEndTime = DateTime.Now;

			const int expectedTotalCount = 10, expectedRowCount = 1;
			int registeredTotalRowsCount = 0, registeredErrorRowsCount = 0;

			_subjectUnderTest.OnBatchComplete += (startTime, endTime, totalRows, errorRowCount) =>
			{
				registeredStartTime = startTime;
				registeredEndTime = endTime;
				registeredTotalRowsCount = totalRows;
				registeredErrorRowsCount = errorRowCount;
			};

			_exporterStatusNotificationMock.OnBatchCompleted +=
				Raise.Event<BatchCompleted>(expectedStartTime, expectedEndTime, expectedTotalCount, expectedRowCount);

			Assert.That(registeredStartTime, Is.EqualTo(expectedStartTime));
			Assert.That(registeredEndTime, Is.EqualTo(expectedEndTime));
			Assert.That(registeredTotalRowsCount, Is.EqualTo(expectedTotalCount));
			Assert.That(registeredErrorRowsCount, Is.EqualTo(expectedRowCount));
		}

		[Test]
		public void ItShouldFireStatusUpdateEvent()
		{
			const int totalRecordCount = 12345, exportedRecordCount = 2000;
			int registeredImportedCount = -1, registeredErrorCount = -1;

			ExportEventArgs reisedEventArgs = new ExportEventArgs(exportedRecordCount, totalRecordCount, "", EventType.Progress,
				null, null);


			_subjectUnderTest.OnStatusUpdate += (importedCount, errorCount) =>
			{
				registeredImportedCount = importedCount;
				registeredErrorCount = errorCount;
			};

			_exporterStatusNotificationMock.StatusMessage +=
				Raise.Event<IExporterStatusNotification.StatusMessageEventHandler>(reisedEventArgs);

			Assert.That(registeredImportedCount, Is.EqualTo(1000));
			Assert.That(registeredErrorCount, Is.EqualTo(0));
		}

		[Test]
		[TestCase(EventType.Count, 1000)]
		[TestCase(EventType.End, 1000)]
		[TestCase(EventType.Error, 1000)]
		[TestCase(EventType.ResetStartTime, 1000)]
		[TestCase(EventType.Status, 1000)]
		[TestCase(EventType.Warning, 1000)]
		[TestCase(EventType.Progress, 1001)]
		[TestCase(EventType.Progress, 100)]
		public void ItShouldNotFireStatusUpdateEvent(EventType eventType, int exportedRecordCount)
		{
			const int totalRecordCount = 12345;

			int expectedValue = -1;
			int registeredImportedCount = expectedValue, registeredErrorCount = expectedValue;

			ExportEventArgs reisedEventArgs = new ExportEventArgs(exportedRecordCount, totalRecordCount, "", eventType, new object(), new Statistics());

			_subjectUnderTest.OnStatusUpdate += (importedCount, errorCount) =>
			{
				registeredImportedCount = importedCount;
				registeredErrorCount = errorCount;
			};

			_exporterStatusNotificationMock.StatusMessage +=
				Raise.Event<IExporterStatusNotification.StatusMessageEventHandler>(reisedEventArgs);

			Assert.That(registeredImportedCount, Is.EqualTo(expectedValue));
			Assert.That(registeredErrorCount, Is.EqualTo(expectedValue));
		}

		[Test]
		public void ItShouldFireErrorMessageEvent()
		{
			const int totalRecordCount = 12345, exportedRecordCount = 2000;
			string expectedMessage = "Some message";
			string registeredDocumentIdentifier = "", registeredErrorMessage = "";

			ExportEventArgs reisedEventArgs = new ExportEventArgs(exportedRecordCount, totalRecordCount, expectedMessage, EventType.Error, new object(), new Statistics());

			_subjectUnderTest.OnDocumentError += (documentIdentifier, errorMessage) =>
			{
				registeredDocumentIdentifier = documentIdentifier;
				registeredErrorMessage = errorMessage;
			};

			_exporterStatusNotificationMock.StatusMessage += Raise.Event<IExporterStatusNotification.StatusMessageEventHandler>(reisedEventArgs);

			Assert.That(registeredDocumentIdentifier, Is.EqualTo(expectedMessage));
			Assert.That(registeredErrorMessage, Is.EqualTo(expectedMessage));
		}

		[Test]
		[TestCase(EventType.Count)]
		[TestCase(EventType.End)]
		[TestCase(EventType.ResetStartTime)]
		[TestCase(EventType.Status)]
		[TestCase(EventType.Warning)]
		[TestCase(EventType.Progress)]
		public void ItShouldNotFireErrorMessageEvent(EventType eventType)
		{
			string registeredDocumentIdentifier = "", registeredErrorMessage = "";
			const int totalRecordCount = 12345, exportedRecordCount = 2000;
			string expectedMessage = "Some message";

			ExportEventArgs reisedEventArgs = new ExportEventArgs(exportedRecordCount, totalRecordCount, expectedMessage, eventType, new object(), new Statistics());

			_subjectUnderTest.OnDocumentError += (documentIdentifier, errorMessage) =>
			{
				registeredDocumentIdentifier = documentIdentifier;
				registeredErrorMessage = errorMessage;
			};

			_exporterStatusNotificationMock.StatusMessage += Raise.Event<IExporterStatusNotification.StatusMessageEventHandler>(reisedEventArgs);

			Assert.That(registeredDocumentIdentifier, Is.Not.EqualTo(expectedMessage));
			Assert.That(registeredErrorMessage, Is.Not.EqualTo(expectedMessage));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Contracts.Models;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportManagers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.ScheduleQueue.Core;
using kCura.WinEDDS.Core.Model.Export;
using kCura.WinEDDS.Exporters;
using kCura.WinEDDS.Service.Export;
using NSubstitute;
using NUnit.Framework;
using Relativity;
using Relativity.API;
using ViewFieldInfo = kCura.WinEDDS.ViewFieldInfo;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Process
{
	[TestFixture]
	public class ExportProcessBuilderTests : TestBase
	{
		private readonly string JobName = "Name";
		private readonly DateTime JobStart = new DateTime(2020, 1, 1, 10, 30, 30);

		private class BatchReporterMock : IBatchReporter, ILoggingMediator
		{
			public event BatchCompleted OnBatchComplete { add { } remove { } }
			public event BatchSubmitted OnBatchSubmit { add { } remove { } }
			public event BatchCreated OnBatchCreate { add { } remove { } }
			public event StatusUpdate OnStatusUpdate { add { } remove { } }
			public event JobError OnJobError { add { } remove { } }
			public event RowError OnDocumentError { add { } remove { } }

			public void RegisterEventHandlers(IUserMessageNotification userMessageNotification,
				ICoreExporterStatusNotification exporterStatusNotification)
			{
				
			}
		}

		#region Fields

		private ICredentialProvider _credentialProvider;
		private IExtendedExporterFactory _exporterFactory;
		private ExtendedExportFile _exportFile;
		private ExportDataContext _exportDataContext;
		private IExportFileBuilder _exportFileBuilder;
		private ExportProcessBuilder _exportProcessBuilder;
		private ICompositeLoggingMediator _loggingMediator;
		private IUserMessageNotification _userMessageNotification;
		private IUserNotification _userNotification;
		private IConfigFactory _configFactory;
		private JobStatisticsService _jobStatisticsService;
		private IJobInfoFactory _jobInfoFactoryMock;
		private IJobInfo _jobInfoMock;
		private IDirectoryHelper _directoryHelper;
		private IExportServiceFactory _exportServiceFactory;
		private IExtendedServiceFactory _serviceFactory;

		private Job _job;

		private Dictionary<int, FieldEntry> AllExportableAvfIds => new Dictionary<int, FieldEntry>
		{
			{ 1234, new FieldEntry { DisplayName = "Field1"}},
			{ 5678, new FieldEntry { DisplayName = "Field2"}}
		};
		private Dictionary<int, FieldEntry> SelectedAvfIds => new Dictionary<int, FieldEntry>
		{
			{ 1234, new FieldEntry { DisplayName = "Field1"}},
		};

		#endregion

		#region SetUp

		[SetUp]
		public override void SetUp()
		{
			_credentialProvider = Substitute.For<ICredentialProvider>();
			_exporterFactory = Substitute.For<IExtendedExporterFactory>();
			_exportFileBuilder = Substitute.For<IExportFileBuilder>();
			_loggingMediator = Substitute.For<ICompositeLoggingMediator>();
			_userMessageNotification = Substitute.For<IUserMessageNotification>();
			_userNotification = Substitute.For<IUserNotification>();
			_configFactory = Substitute.For<IConfigFactory>();
			_jobStatisticsService = Substitute.For<JobStatisticsService>();

			_jobInfoFactoryMock = Substitute.For<IJobInfoFactory>();
			_jobInfoMock = Substitute.For<IJobInfo>();
			_directoryHelper = Substitute.For<IDirectoryHelper>();

			_jobInfoFactoryMock.Create(Arg.Any<Job>()).Returns(_jobInfoMock);

			_jobInfoMock.GetStartTimeUtc().Returns(JobStart);
			_jobInfoMock.GetName().Returns(JobName);
			_serviceFactory = Substitute.For<IExtendedServiceFactory>();
			_exportServiceFactory = Substitute.For<IExportServiceFactory>();
			_exportServiceFactory.Create(Arg.Any<ExportDataContext>()).Returns(_serviceFactory);


			var helper = Substitute.For<IHelper>();

			_loggingMediator.LoggingMediators.Returns(new List<ILoggingMediator>());

			MockExportFile();

			MockSearchManagerReturnValue(ViewFieldInfoMockFactory.CreateMockedViewFieldInfoArray(AllExportableAvfIds.Keys.ToList()));

			_exportProcessBuilder = new ExportProcessBuilder(
				_configFactory,
				_loggingMediator,
				_userMessageNotification,
				_userNotification,
				_credentialProvider,
				_exporterFactory,
				_exportFileBuilder,
				helper,
				_jobStatisticsService,
				_jobInfoFactoryMock,
				_directoryHelper,
				_exportServiceFactory
			);

			_job = JobExtensions.CreateJob();
		}

		private void MockExportFile()
		{
			_exportFile = new ExtendedExportFile(1)
			{
				CaseInfo = new CaseInfo
				{
					DocumentPath = "document_path",
					ArtifactID = 2
				}
			};

			var exportSettings = new ExportSettings();
			_exportDataContext = new ExportDataContext()
			{
				ExportFile = _exportFile,
				Settings = exportSettings
			};
			_exportFileBuilder.Create(_exportDataContext.Settings).ReturnsForAnyArgs(_exportFile);
		}

		#endregion

		#region Tests

		[Test]
		public void ItShouldPerformLogin()
		{
			var credential = new NetworkCredential();
			_credentialProvider.Authenticate(new CookieContainer()).ReturnsForAnyArgs(credential);

			_exportProcessBuilder.Create(new ExportSettings()
			{
				SelViewFieldIds = SelectedAvfIds
			}, _job);

			Assert.IsNotNull(_exportFile.CookieContainer);
			Assert.AreEqual(credential, _exportFile.Credential);
		}

		[Test]
		public void ItShouldCreateAndDisposeSearchManager()
		{
			var searchManager = Substitute.For<ISearchManager>();
			searchManager.RetrieveAllExportableViewFields(_exportFile.CaseInfo.ArtifactID, _exportFile.ArtifactTypeID).Returns(
				ViewFieldInfoMockFactory.CreateMockedViewFieldInfoArray(AllExportableAvfIds.Keys.ToList()));

			_serviceFactory.CreateSearchManager().Returns(searchManager);
			_exportProcessBuilder.Create(new ExportSettings()
			{
				SelViewFieldIds = SelectedAvfIds
			}, _job);

			_serviceFactory.Received(1).CreateSearchManager();
			searchManager.Received().Dispose();
		}

		[Test]
		public void ItShouldCreateAndDisposeCaseManager()
		{
			var caseManager = Substitute.For<ICaseManager>();
			_serviceFactory.CreateCaseManager().Returns(caseManager);

			_exportProcessBuilder.Create(new ExportSettings()
			{
				SelViewFieldIds = SelectedAvfIds
			}, _job);

			_serviceFactory.Received(1).CreateCaseManager();
			caseManager.Received().Dispose();
		}

		[Test]
		public void ItShouldPopulateCaseInfoForEmptyDocumentPath()
		{
			_exportFile.CaseInfo.DocumentPath = string.Empty;
			var expectedCaseInfoArtifactId = _exportFile.CaseInfo.ArtifactID;

			var caseManager = Substitute.For<ICaseManager>();
			caseManager.Read(1).ReturnsForAnyArgs(new CaseInfo()
			{
				ArtifactID = _exportFile.CaseInfo.ArtifactID
			});
			_serviceFactory.CreateCaseManager().Returns(caseManager);

			_exportProcessBuilder.Create(new ExportSettings()
			{
				SelViewFieldIds = SelectedAvfIds
			}, _job);

			caseManager.Received().Read(expectedCaseInfoArtifactId);
		}

		[Test]
		public void ItShouldNotPopulateCaseInfoForNotEmptyDocumentPath()
		{
			_exportFile.CaseInfo.DocumentPath = "document_path";

			var caseManager = Substitute.For<ICaseManager>();
			_serviceFactory.CreateCaseManager().Returns(caseManager);

			_exportProcessBuilder.Create(new ExportSettings()
			{
				SelViewFieldIds = SelectedAvfIds
			}, _job);

			caseManager.DidNotReceiveWithAnyArgs().Read(1);
		}

		[Test]
		public void ItShouldAssignAllExportableFields()
		{
			var expectedExportableFields = AllExportableAvfIds;

			_exportProcessBuilder.Create(new ExportSettings()
			{
				SelViewFieldIds = AllExportableAvfIds
			}, _job);

			CollectionAssert.AreEquivalent(expectedExportableFields.Keys, _exportFile.SelectedViewFields.Select(x => x.AvfId));

			Assert.That(expectedExportableFields.Count, Is.EqualTo(_exportFile.AllExportableFields.Length));

			Assert.That(expectedExportableFields.Select(item => item.Key).ToList().Exists(item => _exportFile.AllExportableFields.Any(obj => obj.AvfId == item)));
		}

		[Test]
		public void ItShouldFilterSelectedViewFields()
		{
			var expectedFilteredFields = new Dictionary<int, FieldEntry>
			{
				{ 1, new FieldEntry()},
				{ 2, new FieldEntry()},
				{ 3, new FieldEntry()}
			};
			var notExpectedFilteredFields = new Dictionary <int, FieldEntry>
			{
				{ 4, new FieldEntry()},
				{ 5, new FieldEntry()},
				{ 6, new FieldEntry()}
			};
			var settings = new ExportSettings
			{
				SelViewFieldIds = expectedFilteredFields
			};
			var expected = ViewFieldInfoMockFactory.CreateMockedViewFieldInfoArray(expectedFilteredFields.Keys.Concat(notExpectedFilteredFields.Keys).ToList());

			MockSearchManagerReturnValue(expected);

			_exportProcessBuilder.Create(settings, _job);

			CollectionAssert.AreEquivalent(expectedFilteredFields.Keys, _exportFile.SelectedViewFields.Select(x => x.AvfId));
		}

		[Test]
		public void ItShouldAssignTextPrecedenceViewFields()
		{
			_exportFile.ExportFullTextAsFile = true;

			var textPrecedenceFieldsIdsExpected = new List<int>
			{
				1,
				2,
				3
			};
			var textPrecedenceFieldsIdsNotExpected = new List<int>
			{
				4,
				5,
				6
			};
			var settings = new ExportSettings
			{
				SelViewFieldIds = SelectedAvfIds,
				TextPrecedenceFieldsIds = textPrecedenceFieldsIdsExpected
			};
			settings.SelViewFieldIds.Add(textPrecedenceFieldsIdsExpected[0], new FieldEntry());
			var expected = 
				ViewFieldInfoMockFactory.CreateMockedViewFieldInfoArray(textPrecedenceFieldsIdsExpected.Concat(textPrecedenceFieldsIdsNotExpected).ToList());

			MockSearchManagerReturnValue(expected);

			_exportProcessBuilder.Create(settings, _job);

			CollectionAssert.AreEquivalent(textPrecedenceFieldsIdsExpected, _exportFile.SelectedTextFields.Select(x => x.AvfId));
		}

		[Test]
		public void ItShouldCreateExporterUsingFactory()
		{
			_exportProcessBuilder.Create(new ExportSettings()
			{
				SelViewFieldIds = SelectedAvfIds
			}, _job);

			_exporterFactory.Received().Create(Arg.Is<ExportDataContext>(context => context.ExportFile == _exportDataContext.ExportFile), _serviceFactory);
		}

		[Test]
		public void ItShouldAttachEventHandlers()
		{
			var exporter = Substitute.For<IExporter>();
			_exporterFactory.Create(Arg.Is<ExportDataContext>(item => item.ExportFile == _exportFile ), _serviceFactory).Returns(exporter);

			_exportProcessBuilder.Create(new ExportSettings()
			{
				SelViewFieldIds = SelectedAvfIds
			}, _job);

			_loggingMediator.Received().RegisterEventHandlers(_userMessageNotification, exporter);
			exporter.Received().InteractionManager = _userNotification;
		}

		[Test]
		public void ItShouldSubscribeBatchRepoertToJobStatsService()
		{
			//Arrange
			var exporter = Substitute.For<IExporter>();
			var batchReporterMock = new BatchReporterMock();
			var job = _job;

			_exporterFactory.Create(Arg.Is<ExportDataContext>(item => item.ExportFile == _exportFile), _serviceFactory).Returns(exporter);

			_loggingMediator.LoggingMediators.Returns(
				new List<ILoggingMediator>( new [] { batchReporterMock } ));
			
			//Act
			_exportProcessBuilder.Create(new ExportSettings()
			{
				SelViewFieldIds = SelectedAvfIds
			}, job);

			//Assert
			_loggingMediator.Received().RegisterEventHandlers(_userMessageNotification, exporter);
			exporter.Received().InteractionManager = _userNotification;
			_jobStatisticsService.Received().Subscribe(batchReporterMock, job);
		}

		[Test]
		public void ItShouldMaintainFieldsOrder()
		{
			// arrange
			var exportableFieldIds = new List<int> {1, 2, 3, 4, 5, 6};
			MockSearchManagerReturnValue(ViewFieldInfoMockFactory.CreateMockedViewFieldInfoArray(exportableFieldIds));

			var expectedFieldIds = new Dictionary<int, FieldEntry>
			{
				{ 2, new FieldEntry()},
				{ 3, new FieldEntry()},
				{ 1, new FieldEntry()}
			};

			var settings = new ExportSettings
			{
				SelViewFieldIds = expectedFieldIds
			};

			// act
			_exportProcessBuilder.Create(settings, _job);

			// assert
			CollectionAssert.AreEqual(expectedFieldIds.Keys, _exportFile.SelectedViewFields.Select(x => x.AvfId));
		}

		[Test]
		public void ItShouldSetFileField()
		{
			const int fieldArtifactId = 123456;

			ViewFieldInfo[] fieldInfos = ViewFieldInfoMockFactory.CreateMockedViewFieldInfoArray(AllExportableAvfIds.Keys.ToList(), true, fieldArtifactId);

			MockSearchManagerReturnValue(fieldInfos);

			var settings = new ExportSettings
			{
				SelViewFieldIds = SelectedAvfIds
			};

			// Act
			_exportProcessBuilder.Create(settings, _job);

			// Assert
			Assert.That(_exportFile.FileField.FieldID, Is.EqualTo(fieldArtifactId));
		}

		[Test]
		public void IsShouldPopulateFileNameViewFields()
		{
			const int fieldArtifactId = 123456;

			ViewFieldInfo[] fieldInfos = ViewFieldInfoMockFactory.CreateMockedViewFieldInfoArray(AllExportableAvfIds.Keys.ToList(), true, fieldArtifactId);

		}

		private void MockSearchManagerReturnValue(ViewFieldInfo[] expectedExportableFields)
		{
			var searchManager = Substitute.For<ISearchManager>();
			searchManager.RetrieveAllExportableViewFields(_exportFile.CaseInfo.ArtifactID, _exportFile.ArtifactTypeID).Returns(expectedExportableFields);
			_serviceFactory.CreateSearchManager().Returns(searchManager);
		}

		#endregion
	}
}
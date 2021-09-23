using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Core.Contracts.BatchReporter;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations.DTO;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.ScheduleQueue.Core;
using kCura.WinEDDS.Exporters;
using kCura.WinEDDS.Service.Export;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using kCura.IntegrationPoints.Core.Authentication.WebApi;
using kCura.IntegrationPoints.Data;
using kCura.WinEDDS;
using Relativity.DataExchange.Io;
using Relativity.DataExchange.Service;
using Relativity.IntegrationPoints.Contracts.Models;
using IExporter = kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary.IExporter;
using IServiceFactory = kCura.WinEDDS.Service.Export.IServiceFactory;
using ViewFieldInfo = kCura.WinEDDS.ViewFieldInfo;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Core.Tests.Process
{
	[TestFixture, Category("Unit")]
	public class ExportProcessBuilderTests : TestBase
	{
		private readonly string JobName = "Name";
		private readonly DateTime _jobStart = new DateTime(2020, 1, 1, 10, 30, 30);

		private class BatchReporterMock : IBatchReporter, ILoggingMediator
		{
			public event StatisticsUpdate OnStatisticsUpdate { add { } remove { } }
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

		private IWebApiLoginService _credentialProvider;
		private IExtendedExporterFactory _exporterFactory;
		private ExtendedExportFile _exportFile;
		private ExportDataContext _exportDataContext;
		private IExportFileBuilder _exportFileBuilder;
		private ExportProcessBuilder _exportProcessBuilder;
		private ICompositeLoggingMediator _loggingMediator;
		private IUserMessageNotification _userMessageNotification;
		private IUserNotification _userNotification;
		private JobStatisticsService _jobStatisticsService;
		private IServiceFactory _serviceFactory;
		private IRepositoryFactory _repositoryFactory;

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
			_credentialProvider = Substitute.For<IWebApiLoginService>();
			_exporterFactory = Substitute.For<IExtendedExporterFactory>();
			_exportFileBuilder = Substitute.For<IExportFileBuilder>();
			_loggingMediator = Substitute.For<ICompositeLoggingMediator>();
			_userMessageNotification = Substitute.For<IUserMessageNotification>();
			_userNotification = Substitute.For<IUserNotification>();
			IConfigFactory configFactory = Substitute.For<IConfigFactory>();

			_jobStatisticsService = Substitute.For<JobStatisticsService>();

			IJobInfoFactory jobInfoFactoryMock = Substitute.For<IJobInfoFactory>();
			IJobInfo jobInfoMock = Substitute.For<IJobInfo>();
			IDirectory directoryWrap = Substitute.For<IDirectory>();

			jobInfoFactoryMock.Create(Arg.Any<Job>()).Returns(jobInfoMock);

			jobInfoMock.GetStartTimeUtc().Returns(_jobStart);
			jobInfoMock.GetName().Returns(JobName);
			_serviceFactory = Substitute.For<IServiceFactory>();
			IExportServiceFactory exportServiceFactory = Substitute.For<IExportServiceFactory>();
			exportServiceFactory.Create(Arg.Any<ExportDataContext>()).Returns(_serviceFactory);
			_repositoryFactory = Substitute.For<IRepositoryFactory>();

			var helper = Substitute.For<IHelper>();

			_loggingMediator.LoggingMediators.Returns(new List<ILoggingMediator>());

			MockExportFile();

			MockSearchManagerReturnValue(ViewFieldInfoMockFactory.CreateMockedViewFieldInfoArray(AllExportableAvfIds.Keys.ToList()));

			_exportProcessBuilder = new ExportProcessBuilder(
				configFactory,
				_loggingMediator,
				_userMessageNotification,
				_userNotification,
				_credentialProvider,
				_exporterFactory,
				_exportFileBuilder,
				helper,
				_jobStatisticsService,
				jobInfoFactoryMock,
				directoryWrap,
				exportServiceFactory,
				_repositoryFactory
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
		public void ItShouldPopulateCaseInfoForEmptyDocumentPath()
		{
			// arrange
			_exportFile.CaseInfo.DocumentPath = string.Empty;
			int expectedCaseInfoArtifactId = _exportFile.CaseInfo.ArtifactID;
			const int expectedRootArtifactId = 6345345;

			ICaseRepository caseRepository = Substitute.For<ICaseRepository>();
			caseRepository.Read(1).ReturnsForAnyArgs(new CaseInfoDto(
				expectedCaseInfoArtifactId,
				name: null,
				matterArtifactId: 0,
				statusCodeArtifactId: 0,
				enableDataGrid: false,
				rootFolderId: 0,
				rootArtifactId: expectedRootArtifactId,
				downloadHandlerUrl: null,
				asImportAllowed: false,
				exportAllowed: false,
				documentPath: "C:\\temp"
			));
			_repositoryFactory.GetCaseRepository().Returns(caseRepository);

			// act
			_exportProcessBuilder.Create(new ExportSettings()
			{
				SelViewFieldIds = SelectedAvfIds
			}, _job);

			// assert
			_exporterFactory.Received().Create(
				Arg.Is<ExportDataContext>(x => x.ExportFile.CaseInfo.RootArtifactID == expectedRootArtifactId),
				Arg.Any<IServiceFactory>()
			);
			caseRepository.Received().Dispose();
		}

		[Test]
		public void ItShouldNotPopulateCaseInfoForNotEmptyDocumentPath()
		{
			_exportFile.CaseInfo.DocumentPath = "document_path";

			ICaseRepository caseRepository = Substitute.For<ICaseRepository>();
			_repositoryFactory.GetCaseRepository().Returns(caseRepository);

			_exportProcessBuilder.Create(new ExportSettings()
			{
				SelViewFieldIds = SelectedAvfIds
			}, _job);

			caseRepository.DidNotReceiveWithAnyArgs().Read(1);
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
			var notExpectedFilteredFields = new Dictionary<int, FieldEntry>
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
			_exporterFactory.Create(Arg.Is<ExportDataContext>(item => item.ExportFile == _exportFile), _serviceFactory).Returns(exporter);

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
				new List<ILoggingMediator>(new[] { batchReporterMock }));

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
			var exportableFieldIds = new List<int> { 1, 2, 3, 4, 5, 6 };
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

			ViewFieldInfoMockFactory.CreateMockedViewFieldInfoArray(AllExportableAvfIds.Keys.ToList(), true, fieldArtifactId);

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
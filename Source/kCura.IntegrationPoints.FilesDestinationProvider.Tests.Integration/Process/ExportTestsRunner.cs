﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoint.Tests.Core.Extensions;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Services.JobHistory;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Helpers;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Logging;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.Process;
using kCura.IntegrationPoints.FilesDestinationProvider.Core.SharedLibrary;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Abstract;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Helpers;
using kCura.Relativity.Client;
using kCura.ScheduleQueue.Core;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.TestCategories;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Core.Factories;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process.Internals;
using kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.TestCases;
using kCura.WinEDDS.Core.IO;
using kCura.WinEDDS.Exporters;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using DateTime = System.DateTime;
using Directory = kCura.Utility.Directory;
using ExportSettings = kCura.IntegrationPoints.FilesDestinationProvider.Core.ExportSettings;

namespace kCura.IntegrationPoints.FilesDestinationProvider.Tests.Integration.Process
{
	[TestFixture]
	public class ExportTestsRunner
	{
		private ExportProcessRunner _sut;
		private readonly ExportTestContextProvider _testContextProvider;

		private static readonly ExportTestConfiguration _testConfiguration = new ExportTestConfiguration();
		private static readonly ExportTestContext _testContext = new ExportTestContext { WorkspaceName = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") };
		private static readonly WindsorContainer _windsorContainer = ContainerInstaller.CreateContainer(_testConfiguration, _testContext);

		public ExportTestsRunner()
		{
			_testContextProvider = _windsorContainer.Resolve<ExportTestContextProvider>();
		}

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			_testContextProvider.InitializeContext();
			InitializeSut();
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			_testContextProvider.DeleteContext();
		}

		[Test]
		[SmokeTest]
		[TestCaseSource(nameof(ExportTestCaseSource))]
		public void RunStableTestCase(IExportTestCase testCase)
		{
			RunTestCase(testCase);
		}

		[Test]
		[SmokeTest]
		[TestInQuarantine(TestQuarantineState.FailsContinuously,
			@"REL-307438 - for all failing continuously,
			REL-307464 - instability of ItShouldExportImagesAsPdfs,
			REL-307466 - FailsContinuously of ItShouldLogJobErrorForNegativeVolumeStartNumber")]
		[TestCaseSource(nameof(FlakyExportTestCaseSource))]
		public void RunFlakyTestCase(IExportTestCase testCase)
		{
			RunTestCase(testCase);
		}

		[Test]
		[SmokeTest]
		[TestCaseSource(nameof(InvalidFileshareExportTestCaseSource))]
		public void RunInvalidFileshareTestCase(IInvalidFileshareExportTestCase testCase)
		{
			// Arrange
			ExportSettings settings = ExportTestSettingsBuilder.CreateExportSettings(_testConfiguration, _testContext);
			settings = testCase.Prepare(settings);

			// Act
			_sut.StartWith(settings, JobExtensions.CreateJob());

			// Assert
			testCase.Verify();
		}

		private void RunTestCase(IExportTestCase testCase)
		{
			// Arrange
			ExportSettings settings = ExportTestSettingsBuilder.CreateExportSettings(_testConfiguration, _testContext);
			settings = testCase.Prepare(settings);
			var directory = new DirectoryInfo(settings.ExportFilesLocation);

			Directory.Instance.CreateDirectoryIfNotExist(directory.FullName);

			// Act
			_sut.StartWith(settings, JobExtensions.CreateJob());

			// Assert
			testCase.Verify(directory, _testContext.DocumentsTestData);
		}

		private static IEnumerable<IExportTestCase> ExportTestCaseSource()
		{
			return _windsorContainer
				.ResolveAll<IExportTestCase>()
				.Where(x => !IsFlakyTest(x));
		}

		private static IEnumerable<IExportTestCase> FlakyExportTestCaseSource()
		{
			return _windsorContainer
				.ResolveAll<IExportTestCase>()
				.Where(IsFlakyTest);
		}

		private static IEnumerable<IInvalidFileshareExportTestCase> InvalidFileshareExportTestCaseSource()
		{
			return _windsorContainer.ResolveAll<IInvalidFileshareExportTestCase>();
		}

		private static bool IsFlakyTest(IExportTestCase testCase)
		{
			return testCase is ItShouldExportFilesStartingFromGivenRecord
				   || testCase is ItShouldExportFolderAndSubfoldersWithNatives
				   || testCase is ItShouldExportFolderWithNatives
				   || testCase is ItShouldExportImagesAsMultiplePages
				   || testCase is ItShouldExportImagesAsPdfs
				   || testCase is ItShouldExportImagesAsSinglePages
				   || testCase is ItShouldExportOriginalImagesPrecendence
				   || testCase is ItShouldExportProducedImagesPrecedence
				   || testCase is ItShouldExportProductionSetWithImages
				   || testCase is ItShouldExportProductionSetWithNatives
				   || testCase is ItShouldExportSavedSearch
				;
		}

		private void InitializeSut()
		{
			IHelper helper = _windsorContainer.Resolve<IHelper>();
			ExportProcessBuilder exportProcessBuilder = CreateExportProcessBuilder(helper);
			var exportSettingsBuilder = new ExportSettingsBuilder(helper, descriptorPartsBuilder: null);

			_sut = new ExportProcessRunner(exportProcessBuilder, exportSettingsBuilder, helper);
		}

		private ExportProcessBuilder CreateExportProcessBuilder(IHelper helper)
		{
			IUserNotification userNotification = _windsorContainer.Resolve<IUserNotification>();
			IUserMessageNotification exportUserNotification = _windsorContainer.Resolve<IUserMessageNotification>();
			LoggingMediatorForTestsFactory loggingMediatorFactory = _windsorContainer.Resolve<LoggingMediatorForTestsFactory>();
			ICompositeLoggingMediator loggingMediator = loggingMediatorFactory.Create();
			JobStatisticsService jobStats = Substitute.For<JobStatisticsService>();
			IConfig configMock = Substitute.For<IConfig>();
			configMock.WebApiPath.Returns(SharedVariables.RelativityWebApiUrl);

			IConfigFactory configFactoryMock = Substitute.For<IConfigFactory>();
			configFactoryMock.Create().Returns(configMock);

			IRepositoryFactory repositoryFactory = _windsorContainer.Resolve<IRepositoryFactory>();

			IExportServiceFactory exportServiceFactory = _windsorContainer.Resolve<IExportServiceFactory>();

			return new ExportProcessBuilder(
				configFactoryMock,
				loggingMediator,
				exportUserNotification,
				userNotification,
				new UserPasswordCredentialProvider(),
				_windsorContainer.Resolve<IExtendedExporterFactory>(),
				new ExportFileBuilder(new DelimitersBuilder(), new VolumeInfoBuilder(),
					new ExportedObjectBuilder(new ExportedArtifactNameRepository(_windsorContainer.Resolve<IRSAPIClient>(), _windsorContainer.Resolve<IServiceManagerProvider>()))
					),
				helper,
				jobStats,
				CreateJobInfoFactoryMock(),
				new LongPathDirectoryHelper(),
				exportServiceFactory,
				repositoryFactory
			);
		}

		private IJobInfoFactory CreateJobInfoFactoryMock()
		{
			IJobInfoFactory jobInfoFactory = Substitute.For<IJobInfoFactory>();
			IJobInfo jobInfo = Substitute.For<IJobInfo>();

			jobInfoFactory.Create(Arg.Any<Job>()).Returns(jobInfo);

			jobInfo.GetStartTimeUtc().Returns(_testConfiguration.JobStart);
			jobInfo.GetName().Returns(_testConfiguration.JobName);

			return jobInfoFactory;
		}
	}
}
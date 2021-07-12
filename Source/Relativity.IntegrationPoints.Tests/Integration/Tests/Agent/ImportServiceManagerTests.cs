using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Validation;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.FileShare;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using System;
using System.Linq;
using SystemInterface.IO;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Queries;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.ScheduleQueue.Core.Core;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Queries;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi.LoadFile;
using Relativity.IntegrationPoints.Tests.Integration.Utils;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
	[TestExecutionCategory.CI, TestLevel.L1]
	public class ImportServiceManagerTests : TestsBase
	{
		private FakeFileInfoFactory _fakeFileInfoFactory;
		private FakeDirectory _fakeDirectory;

		private ImportServiceManager PrepareSut(Action<FakeJobImport> importAction)
		{
			Container.Register(Component.For<IDataSourceProvider>()
				.ImplementedBy<MyFirstProvider.Provider.MyFirstProvider>()
				.Named(MyFirstProvider.Provider.GlobalConstants.FIRST_PROVIDER_GUID));

			Container.Register(Component.For<IJobImport>().Instance(new FakeJobImport(importAction)).LifestyleSingleton());

			return PrepareSut();
		}

		private ImportServiceManager PrepareSut()
		{
			ImportServiceManager sut = Container.Resolve<ImportServiceManager>();
			return sut;
		}

		public override void SetUp()
		{
			base.SetUp();

			_fakeFileInfoFactory = new FakeFileInfoFactory();
			_fakeDirectory = new FakeDirectory();

			Container.Register(Component.For<IFileInfoFactory>().UsingFactoryMethod(c => _fakeFileInfoFactory)
				.LifestyleTransient().Named(nameof(FakeFileInfoFactory)).IsDefault());
			Container.Register(Component.For<IDirectory>().UsingFactoryMethod(c => _fakeDirectory)
				.LifestyleTransient().Named(nameof(FakeDirectory)).IsDefault());
		}

		[IdentifiedTest("F08E46B0-CA04-4D37-9666-1CDEBFF48244")]
		public void Execute_ShouldFailedValidation_WhenLoadFileHasChangedAfterJobRun()
		{
			// Arrange
			const string loadFile = @"DataTransfer\Import\SaltPepper\saltvpepper-no_errors.dat";
			const long size = 1000;
			DateTime modifiedDate = new DateTime(2020, 1, 1);

			const long newSize = size + 10;
			DateTime newModifiedDate = modifiedDate.AddMinutes(5);

			_fakeFileInfoFactory.SetupFile(loadFile, newSize, newModifiedDate);

			IntegrationPointTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper
				.CreateImportDocumentLoadFileIntegrationPoint(loadFile);

			JobTest job = FakeRelativityInstance.Helpers.JobHelper.ScheduleImportIntegrationPointRun(SourceWorkspace, integrationPoint, size, modifiedDate, processedItemsCount: 0);
			RegisterJobContext(job);

			ImportServiceManager sut = PrepareSut();

			// Act
			Action action = () => sut.Execute(job.AsJob());

			// Assert
			action.ShouldThrow<IntegrationPointValidationException>();
		}

		[IdentifiedTest("1C5F2F4E-30C0-4B2B-B43A-282AE2413E37")]
		public void Execute_ShouldDrainStopAndStoreNumberOfProcessedItems()
		{
			// Arrange
			const int numberOfRecords = 10;
			const int drainStopAfterImporting = 3;

			JobTest job = ScheduleIntegrationPointImportJob(numberOfRecords);

			IRemovableAgent agent = Container.Resolve<IRemovableAgent>();

			ImportServiceManager sut = PrepareSut((importJob) =>
			{
				importJob.Complete(drainStopAfterImporting, 0, useDataReader:false);

				agent.ToBeRemoved = true;
			});

			// Act
			sut.Execute(job.AsJob());

			// Assert
			JobHistoryTest jobHistory = SourceWorkspace.JobHistory.Single();

			jobHistory.JobStatus.Guids.Single().Should().Be(JobStatusChoices.JobHistorySuspendedGuid);
			jobHistory.ItemsTransferred.Should().Be(drainStopAfterImporting);
			
			job.StopState.Should().Be(StopState.DrainStopping);
		}

		[IdentifiedTest("7179C321-11C6-4DEE-A252-6970F7441EF6")]
		public void Execute_ShouldResumeDrainStoppedJob()
		{
			// Arrange
			const int itemsToTransfer = 10;
			const int itemLevelErrorsToTransfer = 5;
			const int initialItemsTransferred = 3;
			const int initialItemLevelErrors = 1;

			const int totalDocuments = itemsToTransfer + itemLevelErrorsToTransfer
			                                + initialItemsTransferred + initialItemLevelErrors;

			JobTest job = ScheduleIntegrationPointImportJob(totalDocuments, initialItemsTransferred + initialItemLevelErrors);

			FakeJobStatisticsQuery fakeJobStatisticsQuery = Container.Resolve<IJobStatisticsQuery>() as FakeJobStatisticsQuery;
			fakeJobStatisticsQuery.AlreadyTransferredItems = initialItemsTransferred;
			fakeJobStatisticsQuery.AlreadyFailedItems = initialItemLevelErrors;

			JobHistoryTest jobHistory = SourceWorkspace.JobHistory.Single();
			jobHistory.ItemsTransferred = initialItemsTransferred;
			jobHistory.ItemsWithErrors = initialItemLevelErrors;

			ImportServiceManager sut = PrepareSut(importJob =>
			{
				importJob.Complete(itemsToTransfer, itemLevelErrorsToTransfer, useDataReader:false);
			});

			// Act
			sut.Execute(job.AsJob());

			// Assert
			jobHistory.ItemsTransferred.Should().Be(initialItemsTransferred + itemsToTransfer);
			jobHistory.ItemsWithErrors.Should().Be(initialItemLevelErrors + itemLevelErrorsToTransfer);
			
			jobHistory.ShouldHaveCorrectItemsTransferredUpdateHistory(initialItemsTransferred, initialItemsTransferred + itemsToTransfer);
			jobHistory.ShouldHaveCorrectItemsWithErrorsUpdateHistory(initialItemLevelErrors, initialItemLevelErrors + itemLevelErrorsToTransfer);
		}

		[IdentifiedTest("98287D79-8940-4DDB-90B1-4EE4604763A7")]
		public void Execute_ShouldHandleDrainStop_WhenAllRecordsWereProcessed()
		{
			// Arrange
			const int numberOfRecords = 10;
			const int drainStopAfterImporting = 10;

			JobTest job = ScheduleIntegrationPointImportJob(numberOfRecords);

			IRemovableAgent agent = Container.Resolve<IRemovableAgent>();

			ImportServiceManager sut = PrepareSut((importJob) =>
			{
				importJob.Complete(drainStopAfterImporting, 0, useDataReader: false);

				agent.ToBeRemoved = true;
			});

			// Act
			sut.Execute(job.AsJob());

			// Assert
			JobHistoryTest jobHistory = SourceWorkspace.JobHistory.Single();

			jobHistory.JobStatus.Guids.Single().Should().Be(JobStatusChoices.JobHistoryCompletedGuid);
			jobHistory.ItemsTransferred.Should().Be(drainStopAfterImporting);

			job.StopState.Should().Be(StopState.None);
		}

		private JobTest ScheduleIntegrationPointImportJob(int numberOfRecords, int processedItemsCount = 0)
		{
			const string loadFile = @"DataTransfer\Import\SaltPepper\saltvpepper-no_errors.dat";
			const long size = 1024;
			DateTime modifiedDate = new DateTime(2020, 1, 1);

			Container.Register(Component.For<IWinEddsFileReaderFactory>().UsingFactoryMethod(c => new FakeWinEddsFileReaderFactory(numberOfRecords))
				.LifestyleTransient().Named(nameof(FakeWinEddsFileReaderFactory)).IsDefault());
			_fakeFileInfoFactory.SetupFile(loadFile, size, modifiedDate);

			IntegrationPointTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper
				.CreateImportDocumentLoadFileIntegrationPoint(loadFile);

			JobTest job = FakeRelativityInstance.Helpers.JobHelper.ScheduleImportIntegrationPointRun(SourceWorkspace, integrationPoint, size, modifiedDate, processedItemsCount: processedItemsCount);
			RegisterJobContext(job);

			SourceWorkspace.Helpers.JobHistoryHelper.CreateJobHistory(job, integrationPoint);

			return job;
		}
	}
}

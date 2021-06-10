using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Validation;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.FileShare;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Testing.Identification;
using System;
using SystemInterface.IO;
using kCura.IntegrationPoints.Common.Agent;
using kCura.IntegrationPoints.Config;
using kCura.IntegrationPoints.Core.Authentication.WebApi.LoginHelperFacade;
using kCura.IntegrationPoints.Domain.Authentication;
using kCura.IntegrationPoints.ImportProvider.Parser.Interfaces;
using kCura.IntegrationPoints.Synchronizers.RDO.JobImport;
using kCura.ScheduleQueue.Core;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi.LoadFile;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.Services.ImportApi.WebApi;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
	[IdentifiedTestFixture("DF85E997-C5E0-4B77-B687-E88545CC9F7B")]
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

			Container.Register(Component.For<IWebApiConfig>().UsingFactoryMethod(c => new FakeWebApiConfig())
				.LifestyleTransient().Named(nameof(FakeWebApiConfig)).IsDefault());
			Container.Register(Component.For<IAuthTokenGenerator>().UsingFactoryMethod(c => new FakeAuthTokenGenerator())
				.LifestyleTransient().Named(nameof(FakeAuthTokenGenerator)).IsDefault());
			Container.Register(Component.For<ILoginHelperFacade>().UsingFactoryMethod(c => new FakeLoginHelperFacade())
				.LifestyleTransient().Named(nameof(FakeLoginHelperFacade)).IsDefault());
			Container.Register(Component.For<IWinEddsBasicLoadFileFactory>().UsingFactoryMethod(c => new FakeWinEddsBasicLoadFileFactory())
				.LifestyleTransient().Named(nameof(FakeWinEddsBasicLoadFileFactory)).IsDefault());
			Container.Register(Component.For<IWinEddsFileReaderFactory>().UsingFactoryMethod(c => new FakeWinEddsFileReaderFactory())
				.LifestyleTransient().Named(nameof(FakeLoadFileArtifactReader)).IsDefault());
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

			JobTest job = FakeRelativityInstance.Helpers.JobHelper.ScheduleImportIntegrationPointRun(SourceWorkspace, integrationPoint, size, modifiedDate);
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
			const string loadFile = @"DataTransfer\Import\SaltPepper\saltvpepper-no_errors.dat";
			const long size = 100;
			DateTime modifiedDate = new DateTime(2020, 1, 1);
			
			_fakeFileInfoFactory.SetupFile(loadFile, size, modifiedDate);

			IntegrationPointTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper
				.CreateImportDocumentLoadFileIntegrationPoint(loadFile);

			JobTest job = FakeRelativityInstance.Helpers.JobHelper.ScheduleImportIntegrationPointRun(SourceWorkspace, integrationPoint, size, modifiedDate);

			IRemovableAgent agent = Container.Resolve<IRemovableAgent>();

			ImportServiceManager sut = PrepareSut((importJob) =>
			{
				importJob.Complete(50);

				agent.ToBeRemoved = true;
			});

			// Act
			sut.Execute(job.AsJob());

			// Assert
			sut.Result.Status.Should().Be(TaskStatusEnum.Success); // TODO .DrainStopped
		}
	}
}

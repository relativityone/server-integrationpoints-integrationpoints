using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Agent.Tasks;
using kCura.IntegrationPoints.Core.Validation;
using NUnit.Framework;
using Relativity.IntegrationPoints.Tests.Integration.Mocks.FileShare;
using Relativity.IntegrationPoints.Tests.Integration.Models;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Identification;
using System;
using System.Linq;
using SystemInterface.IO;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.Agent
{
	[IdentifiedTestFixture("DF85E997-C5E0-4B77-B687-E88545CC9F7B")]
	[TestExecutionCategory.CI, TestLevel.L1]
	public class ImportServiceManagerTests : TestsBase
	{
		private ImportServiceManager _sut;

		private FakeFileInfoFactory _fakeFileInfoFactory;

		public override void SetUp()
		{
			base.SetUp();

			_fakeFileInfoFactory = new FakeFileInfoFactory();

			Container.Register(Component.For<IFileInfoFactory>().UsingFactoryMethod(c => _fakeFileInfoFactory)
				.LifestyleTransient().Named(nameof(FakeFileInfoFactory)).IsDefault());

			_sut = Container.Resolve<ImportServiceManager>();
		}

		[IdentifiedTest("F08E46B0-CA04-4D37-9666-1CDEBFF48244")]
		public void Execute_ShouldFailedValidation_WhenLoadFileHasChangedAfterJobRun()
		{
			// Arrange
			const string loadFile = "DataTransfer\\Import\\SaltPepper\\saltvpepper-no_errors.dat";
			const long size = 1000;
			DateTime modifiedDate = new DateTime(2020, 1, 1);

			const long newSize = size + 10;
			DateTime newModifiedDate = modifiedDate.AddMinutes(5);

			_fakeFileInfoFactory.SetupFile(loadFile, newSize, newModifiedDate);

			IntegrationPointTest integrationPoint = SourceWorkspace.Helpers.IntegrationPointHelper
				.CreateImportDocumentLoadFileIntegrationPoint(loadFile);

			JobTest job = FakeRelativityInstance.Helpers.JobHelper.ScheduleImportIntegrationPointRun(SourceWorkspace, integrationPoint, size, modifiedDate);

			// Act
			Action action = () => _sut.Execute(job.AsJob());

			// Assert
			action.ShouldThrow<IntegrationPointValidationException>();
		}
	}
}

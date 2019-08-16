using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Core.Installers.Registrations;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Data.Repositories;
using NUnit.Framework;
using static kCura.IntegrationPoint.Tests.Core.TestHelpers.WindsorContainerTestHelpers;

namespace kCura.IntegrationPoints.Core.Tests.Installers.Registrations
{
	[TestFixture]
	public class ExportSanitizerRegistrationTests
	{
		private IWindsorContainer _sut;

		[SetUp]
		public void SetUp()
		{
			_sut = new WindsorContainer();
			_sut.AddExportSanitizer();
		}

		[Test]
		public void ExportSanitizerComponents_ShouldBeRegisteredWithProperLifestyle()
		{
			// assert
			_sut.Should()
				.HaveRegisteredSingleComponent<IChoiceTreeToStringConverter>()
				.Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
			_sut.Should()
				.HaveRegisteredSingleComponent<IChoiceCache>()
				.Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
			_sut.Should()
				.HaveRegisteredSingleComponent<IExportFieldSanitizerProvider>()
				.Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
			_sut.Should()
				.HaveRegisteredSingleComponent<IExportDataSanitizer>()
				.Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
		}

		[Test]
		public void ExportSanitizerComponents_ShouldBeRegisteredWithProperImplementation()
		{
			// assert
			_sut.Should()
				.HaveRegisteredProperImplementation<IChoiceTreeToStringConverter, ChoiceTreeToStringConverter>();
			_sut.Should()
				.HaveRegisteredProperImplementation<IChoiceCache, ChoiceCache>();
			_sut.Should()
				.HaveRegisteredProperImplementation<IExportFieldSanitizerProvider, ExportFieldSanitizerProvider>();
			_sut.Should()
				.HaveRegisteredProperImplementation<IExportDataSanitizer, ExportDataSanitizer>();
		}

		[Test]
		public void ExportSanitizerComponents_ShouldBeResolvedWithoutThrowing()
		{
			// arrange
			RegisterDependencies(_sut);

			// assert
			_sut.Should()
				.ResolveWithoutThrowing<IChoiceTreeToStringConverter>();
			_sut.Should()
				.ResolveWithoutThrowing<IChoiceCache>();
			_sut.Should()
				.ResolveWithoutThrowing<IExportFieldSanitizerProvider>();
			_sut.Should()
				.ResolveWithoutThrowing<IExportDataSanitizer>();
		}

		private static void RegisterDependencies(IWindsorContainer container)
		{
			IRegistration[] dependencies =
			{
				CreateDummyObjectRegistration<IRelativityObjectManager>(),
				CreateDummyObjectRegistration<ISerializer>()
			};

			container.Register(dependencies);
		}
	}
}

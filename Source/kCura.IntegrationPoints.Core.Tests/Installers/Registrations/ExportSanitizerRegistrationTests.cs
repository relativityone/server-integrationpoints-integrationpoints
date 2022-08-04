using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Core.Installers.Registrations;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using kCura.IntegrationPoints.Data.Repositories;
using NUnit.Framework;
using Relativity.API;
using static kCura.IntegrationPoint.Tests.Core.TestHelpers.WindsorContainerTestHelpers;

namespace kCura.IntegrationPoints.Core.Tests.Installers.Registrations
{
    [TestFixture, Category("Unit")]
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
                .HaveRegisteredSingleComponent<ISanitizationDeserializer>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
            _sut.Should()
                .HaveRegisteredMultipleComponents<IExportFieldSanitizer>()
                .And.AllWithLifestyle(LifestyleType.Transient);
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
                .HaveRegisteredProperImplementation<ISanitizationDeserializer, SanitizationDeserializer>();
            _sut.Should()
                .HaveRegisteredMultipleComponents<IExportFieldSanitizer>()
                .And.OneOfThemWithImplementation<SingleObjectFieldSanitizer>()
                .And.OneOfThemWithImplementation<MultipleObjectFieldSanitizer>()
                .And.OneOfThemWithImplementation<SingleChoiceFieldSanitizer>()
                .And.OneOfThemWithImplementation<MultipleChoiceFieldSanitizer>()
                .And.OneOfThemWithImplementation<UserFieldSanitizer>();
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
                .ResolveWithoutThrowing<ISanitizationDeserializer>();
            _sut.Should()
                .ResolveWithoutThrowing<IExportDataSanitizer>();
        }

        private static void RegisterDependencies(IWindsorContainer container)
        {
            container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel));

            IRegistration[] dependencies =
            {
                CreateDummyObjectRegistration<IChoiceRepository>(),
                CreateDummyObjectRegistration<ISerializer>(),
                CreateDummyObjectRegistration<IHelper>()
            };

            container.Register(dependencies);
        }
    }
}

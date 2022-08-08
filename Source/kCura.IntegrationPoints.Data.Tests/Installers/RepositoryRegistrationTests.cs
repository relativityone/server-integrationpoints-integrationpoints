using System;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Facades.SecretStore;
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using kCura.IntegrationPoints.Data.Installers;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using NUnit.Framework;
using Relativity.API;
using static kCura.IntegrationPoint.Tests.Core.TestHelpers.WindsorContainerTestHelpers;

namespace kCura.IntegrationPoints.Data.Tests.Installers
{
    [TestFixture, Category("Unit")]
    public class RepositoryRegistrationTests
    {
        private IWindsorContainer _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new WindsorContainer();
            _sut.AddRepositories();
        }

        [Test]
        public void IntegrationPointRepository_ShouldBeRegisteredWithProperLifestyle()
        {
            // assert
            _sut.Should()
                .HaveRegisteredSingleComponent<IIntegrationPointRepository>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void IntegrationPointRepository_ShouldBeRegisteredWithProperImplementation()
        {
            // assert
            _sut.Should()
                .HaveRegisteredProperImplementation<IIntegrationPointRepository, IntegrationPointRepository>();
        }

        [Test]
        public void IntegrationPointRepository_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            RegisterDependencies(_sut);

            // assert
            _sut.Should()
                .ResolveWithoutThrowing<IIntegrationPointRepository>();
        }

        [Test]
        public void SourceProviderRepository_ShouldBeRegisteredWithProperLifestyle()
        {
            _sut.Should()
                .HaveRegisteredSingleComponent<ISourceProviderRepository>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void SourceProviderRepository_ShouldBeRegisteredWithProperImplementation()
        {
            // assert
            _sut.Should()
                .HaveRegisteredProperImplementation<ISourceProviderRepository, SourceProviderRepository>();
        }

        [Test]
        public void SourceProviderRepository_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            RegisterDependencies(_sut);

            // assert
            _sut.Should()
                .ResolveWithoutThrowing<ISourceProviderRepository>();
        }

        [Test]
        public void DestinationProviderRepository_ShouldBeRegisteredWithProperLifestyle()
        {
            _sut.Should()
                .HaveRegisteredSingleComponent<IDestinationProviderRepository>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void DestinationProviderRepository_ShouldBeRegisteredWithProperImplementation()
        {
            // assert
            _sut.Should()
                .HaveRegisteredProperImplementation<IDestinationProviderRepository, DestinationProviderRepository>();
        }

        [Test]
        public void DestinationProviderRepository_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            RegisterDependencies(_sut);

            // assert
            _sut.Should()
                .ResolveWithoutThrowing<IDestinationProviderRepository>();
        }

        [Test]
        public void DocumentRepository_ShouldBeRegisteredWithProperLifestyle()
        {
            // assert
            _sut.Should()
                .HaveRegisteredSingleComponent<IDocumentRepository>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void ChoiceRepository_ShouldBeRegisteredWithProperLifestyle()
        {
            // assert
            _sut.Should()
                .HaveRegisteredMultipleComponents<IChoiceRepository>()
                .And.AllWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void DocumentRepository_ShouldBeRegisteredWithProperImplementation()
        {
            // assert
            _sut.Should()
                .HaveRegisteredProperImplementation<IDocumentRepository, KeplerDocumentRepository>();
        }

        [Test]
        public void ChoiceRepository_ShouldBeRegisteredWithProperImplementation()
        {
            // assert
            _sut.Should()
                .HaveRegisteredMultipleComponents<IChoiceRepository>()
                .And.OneOfThemWithImplementation<ChoiceRepository>()
                .And.OneOfThemWithImplementation<CachedChoiceRepository>();
        }

        [Test]
        public void DocumentRepository_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            RegisterDependencies(_sut);

            // assert
            _sut.Should()
                .ResolveWithoutThrowing<IDocumentRepository>();
        }

        [Test]
        public void ChoiceRepository_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            RegisterDependencies(_sut);

            // assert
            _sut.Should()
                .ResolveImplementationWithoutThrowing<IChoiceRepository, CachedChoiceRepository>();
        }

        [Test]
        public void SecretsRepository_ShouldBeRegisteredWithProperLifestyle()
        {
            // assert
            _sut.Should()
                .HaveRegisteredSingleComponent<ISecretsRepository>()
                .Which.Should().BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void SecretsRepository_ShouldBeRegisteredWithProperImplementation()
        {
            // assert
            _sut.Should()
                .HaveRegisteredProperImplementation<ISecretsRepository, SecretsRepository>();
        }

        [Test]
        public void SecretsRepository_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            RegisterDependencies(_sut);

            // assert
            _sut.Should()
                .ResolveWithoutThrowing<ISecretsRepository>();
        }

        [Test]
        public void SecretStoreFacade_ShouldBeRegisteredWithSecretStoreFacadeRetryDecorator()
        {
            // assert
            _sut.Should()
                .HaveRegisteredMultipleComponents<ISecretStoreFacade>()
                .And.OneOfThemWithImplementation<SecretStoreFacadeRetryDecorator>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void SecretStoreFacade_ShouldBeRegisteredWithSecretStoreFacadeInstrumentationDecorator()
        {
            // assert
            _sut.Should()
                .HaveRegisteredMultipleComponents<ISecretStoreFacade>()
                .And.OneOfThemWithImplementation<SecretStoreFacadeInstrumentationDecorator>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void SecretStoreFacade_ShouldBeRegisteredWithSecretStoreFacade()
        {
            // assert
            _sut.Should()
                .HaveRegisteredMultipleComponents<ISecretStoreFacade>()
                .And.OneOfThemWithImplementation<SecretStoreFacade>()
                .Which.Should()
                .BeRegisteredWithLifestyle(LifestyleType.Transient);
        }

        [Test]
        public void SecretStoreFacade_ShouldBeResolvedWithoutThrowing()
        {
            // arrange
            RegisterDependencies(_sut);

            // assert
            _sut.Should()
                .ResolveWithoutThrowing<ISecretStoreFacade>();
        }

        [Test]
        public void SecretStoreFacade_ShouldBeRegisteredInCorrectOrder()
        {
            // assert
            Type[] implementationsOrder =
            {
                typeof(SecretStoreFacadeRetryDecorator),
                typeof(SecretStoreFacadeInstrumentationDecorator),
                typeof(SecretStoreFacade)
            };

            _sut.Should()
                .HaveRegisteredMultipleComponents<ISecretStoreFacade>()
                .And.AllRegisteredInFollowingOrder(implementationsOrder);
        }

        private static void RegisterDependencies(IWindsorContainer container)
        {
            IRegistration[] dependencies =
            {
                CreateDummyObjectRegistration<IRelativityObjectManager>(),
                CreateDummyObjectRegistration<IIntegrationPointSerializer>(),
                CreateDummyObjectRegistration<IExternalServiceInstrumentationProvider>(),
                CreateDummyObjectRegistration<IRetryHandlerFactory>(),
                CreateDummyObjectRegistration<IAPILog>()
            };

            container.Register(dependencies);

            container.Register(Component
                .For<ILazyComponentLoader>()
                .ImplementedBy<LazyOfTComponentLoader>()
            );
        }
    }
}

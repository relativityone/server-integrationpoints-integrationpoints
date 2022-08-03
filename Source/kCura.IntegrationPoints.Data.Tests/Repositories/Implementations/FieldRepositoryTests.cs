using System;
using FluentAssertions;
using kCura.IntegrationPoints.Common.Monitoring.Instrumentation;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.Implementations;
using kCura.IntegrationPoints.Domain.Exceptions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.API.Foundation;

namespace kCura.IntegrationPoints.Data.Tests.Repositories.Implementations
{
    [TestFixture, Category("Unit")]
    public class FieldRepositoryTests
    {
        private IServicesMgr _servicesMgr;
        private IHelper _helper;
        private IFoundationRepositoryFactory _foundationRepositoryFactory;
        private global::Relativity.API.Foundation.Repositories.IFieldRepository _apiFieldRepository;
        private IExternalServiceInstrumentationProvider _instrumentationProvider;
        private IExternalServiceInstrumentation _instrumentation;
        private IExternalServiceInstrumentationStarted _startedInstrumentation;

        private const int WORKSPACE_ID = 10001;
        private const string API_FOUNDATION = "API.Foundation";
        private const string IFIELD_REPOSITORY = "IFieldRepository";
        private const string READ = "Read";

        [SetUp]
        public void SetUp()
        {
            _servicesMgr = Substitute.For<IServicesMgr>();
            _helper = Substitute.For<IHelper>();
            _apiFieldRepository = Substitute.For<global::Relativity.API.Foundation.Repositories.IFieldRepository>();
            _foundationRepositoryFactory = Substitute.For<IFoundationRepositoryFactory>();
            _foundationRepositoryFactory
                .GetRepository<global::Relativity.API.Foundation.Repositories.IFieldRepository>(Arg.Any<int>())
                .Returns(_apiFieldRepository);
            _startedInstrumentation = Substitute.For<IExternalServiceInstrumentationStarted>();
            _instrumentation = Substitute.For<IExternalServiceInstrumentation>();
            _instrumentation.Started().Returns(_startedInstrumentation);
            _instrumentationProvider = Substitute.For<IExternalServiceInstrumentationProvider>();
            _instrumentationProvider.Create(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>())
                .Returns(_instrumentation);
        }

        [Test]
        public void ShouldCallStartedAndCompletedWhenReadExecutedSuccessfully()
        {
            //arrange
            const int fieldId = 100;
            IField field = Substitute.For<IField>();
            field.ArtifactID.Returns(fieldId);
            _apiFieldRepository.Read(Arg.Any<IArtifactRef>()).Returns(field);
            var sut = new FieldRepository(
                _servicesMgr, 
                _helper,
                _foundationRepositoryFactory,
                _instrumentationProvider,
                WORKSPACE_ID);

            //act
            IField result = sut.Read(fieldId);

            //assert
            result.ArtifactID.Should().Be(fieldId);
            _instrumentationProvider.Received()
                .Create(API_FOUNDATION, IFIELD_REPOSITORY, READ);
            _instrumentation.Received().Started();
            _startedInstrumentation.Received().Completed();
        }

        [Test]
        public void ShouldCallStartedAndFailedWhenReadThrowsException()
        {
            //arrange
            const int fieldId = 100;
            var exception = new InvalidOperationException();
            _apiFieldRepository.Read(Arg.Any<IArtifactRef>()).Throws(exception);
            var sut = new FieldRepository(
                _servicesMgr,
                _helper,
                _foundationRepositoryFactory,
                _instrumentationProvider,
                WORKSPACE_ID);

            //act
            Action act = () => sut.Read(fieldId);

            //assert
            act.ShouldThrow<IntegrationPointsException>()
               .WithMessage($"An error occured while reading field {fieldId} from workspace {WORKSPACE_ID}")
               .WithInnerExceptionExactly<InvalidOperationException>();
            _instrumentationProvider.Received()
                .Create(API_FOUNDATION, IFIELD_REPOSITORY, READ);
            _instrumentation.Received().Started();
            _startedInstrumentation.Received().Failed(exception);
        }
    }
}
